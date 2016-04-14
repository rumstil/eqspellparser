using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;


namespace Everquest
{
    public static class SpellParser
    {
        public const int MAX_LEVEL = 105;

        /// <summary>
        /// Load all spells from spells_us.txt.
        /// This method only takes the path to the main spells_us.txt. It will attempt to load supporting files as long as they are in the same folder and use the same naming convention.
        /// </summary>
        static public List<Spell> LoadFromFile(string spellPath)
        {
            // the spell file is required. the other files are optional 
            if (!File.Exists(spellPath))
                throw new FileNotFoundException("Could not open spell file.", spellPath);

            var list = new List<Spell>(50000);
            var listById = new Dictionary<int, Spell>(50000);

            // load description strings file
            var desc = new Dictionary<string, string>(50000);
            var descPath = spellPath.Replace("spells_us", "dbstr_us");
            foreach (var fields in LoadFields(descPath))
            {
                desc[fields[1] + "/" + fields[0]] = fields[2].Trim();
                // 0 = id within type
                // 1 = type
                // 2 = description
                // type 1 = AA names
                // type 4 = AA desc
                // type 5 = spell categories
                // type 6 = spell desc
                // type 7 = lore groups
                // type 11 = illusions
                // type 12 = body type
                // type 16 = aug slot desc
                // type 18 = currency
                // type 40 = spell stacking group
            }

            // load spell definition file
            foreach (var fields in LoadFields(spellPath))
            {
                // file format has changed over time - pick the best parser
                Func<string[], Spell> parser = ParseSpell;
                if (fields.Length == 179)
                    parser = ParseSpell20160413;
                if (fields.Length > 220)
                    parser = ParseSpell20160210;

                // parser the spell
                var spell = parser(fields);

                list.Add(spell);
                listById[spell.ID] = spell;

                if (!desc.TryGetValue("6/" + spell.DescID, out spell.Desc))
                    spell.Desc = null;

#if !LimitMemoryUse
                // all spells can be grouped into up to 2 categories (type 5 in db_str)
                // ignore the "timer" categories because they are frequently wrong
                var cat = new List<string>(3);
                string c1;
                string c2;
                if (desc.TryGetValue("5/" + spell.CategoryDescID[0], out c1))
                {
                    // sub category 1
                    if (desc.TryGetValue("5/" + spell.CategoryDescID[1], out c2) && !c2.StartsWith("Timer"))
                        cat.Add(c1 + "/" + c2);

                    // sub category 2
                    if (desc.TryGetValue("5/" + spell.CategoryDescID[2], out c2) && !c2.StartsWith("Timer"))
                        cat.Add(c1 + "/" + c2);

                    // general category if no subcategories are defined
                    if (cat.Count == 0)
                        cat.Add(c1);
                }

                // add a timer category 
                if (spell.TimerID > 0)
                    cat.Add("Timer " + spell.TimerID.ToString("D2"));
                spell.Categories = cat.ToArray();
#endif
            }

            // load spell stacking file
            // my guess is that this was made a separate file because a single spell can be part of multiple spell stacking groups
            var stackPath = spellPath.Replace("spells_us", "SpellStackingGroups");
            foreach (var fields in LoadFields(stackPath))
            {
                var id = ParseInt(fields[0]);
                var group = fields[1];
                var rank = fields[2];
                var type = fields[3];

                Spell spell = null;
                if (!listById.TryGetValue(id, out spell))
                    continue;

                if (desc.ContainsKey("40/" + group))
                    group = desc["40/" + group];

                group += " " + rank;

                // type 6 = non-override stacking group; only one can be active at a time and new procs won't overwrite old ones that are still active
                if (type == "6")
                    group += " (Non-Overide)";

                spell.Stacking.Add(group);
            }


            return list;
        }

        /// <summary>
        /// Load fields from a delimited text file.
        /// </summary>
        static IEnumerable<string[]> LoadFields(string path)
        {
            if (File.Exists(path))
                using (var text = PossiblyCompressedFile.OpenText(path))
                    while (!text.EndOfStream)
                    {
                        var line = text.ReadLine();
                        var fields = line.Split('^');
                        if (fields.Length < 2 || line.StartsWith("#"))
                            continue;
                        yield return fields;
                    }
        }

        /// <summary>
        /// Load a single spell from the spells file.
        /// This handles the current file format.
        /// This format started with 174 fields.
        /// </summary>
        static Spell ParseSpell(string[] fields)
        {
            var spell = new Spell();

            // 0 SPELLINDEX
            spell.ID = Convert.ToInt32(fields[0]);
            // 1 SPELLNAME
            spell.Name = fields[1].Trim();
            // 2 ACTORTAG
            // 3 NPC_FILENAME
            spell.Extra = fields[3];
            // 4 CASTERMETXT
            // 5 CASTEROTHERTXT
            // 6 CASTEDMETXT
            spell.LandOnSelf = fields[6];
            // 7 CASTEDOTHERTXT
            spell.LandOnOther = fields[7];
            // 8 SPELLGONE
            // 9 RANGE
            spell.Range = ParseInt(fields[9]);
            // 10 IMPACTRADIUS
            spell.AERange = ParseInt(fields[10]);
            // 11 OUTFORCE
            spell.PushBack = ParseFloat(fields[11]);
            // 12 UPFORCE
            spell.PushUp = ParseFloat(fields[12]);
            // 13 CASTINGTIME
            spell.CastingTime = ParseFloat(fields[13]) / 1000f;
            // 14 RECOVERYDELAY
            spell.RestTime = ParseFloat(fields[14]) / 1000f;
            // 15 SPELLDELAY
            spell.RecastTime = ParseFloat(fields[15]) / 1000f;
            // 16 DURATIONBASE
            // 17 DURATIONCAP
            spell.DurationTicks = Spell.CalcDuration(ParseInt(fields[16]), ParseInt(fields[17]), MAX_LEVEL);
            // 18 IMPACTDURATION
            spell.AEDuration = ParseInt(fields[18]);
            // 19 MANACOST
            spell.Mana = ParseInt(fields[19]);
            // 20 IMAGENUMBER
            // 21 MEMIMAGENUMBER
            // 22 EXPENDREAGENT1 .. 25 EXPENDREAGENT4
            // 26 EXPENDQTY1 .. 29 EXPENDQTY4
            // 30 NOEXPENDREAGENT1 .. 33 NOEXPENDREAGENT4
            for (int i = 0; i < 3; i++)
            {
                spell.ConsumeItemID[i] = ParseInt(fields[22 + i]);
                spell.ConsumeItemCount[i] = ParseInt(fields[26 + i]);
                spell.FocusID[i] = ParseInt(fields[30 + i]);
            }
            // 34 LIGHTTYPE
            // 35 BENEFICIAL
            spell.Beneficial = ParseBool(fields[35]);
            // 36 ACTIVATED
            // 37 RESISTTYPE
            spell.ResistType = (SpellResist)ParseInt(fields[37]);
            // 38 TYPENUMBER
            spell.Target = (SpellTarget)ParseInt(fields[38]);
            // 39 BASEDIFFICULTY = fizzle?
            // 40 CASTINGSKILL
            spell.Skill = (SpellSkill)ParseInt(fields[40]);
            // 41 ZONETYPE
            spell.Zone = (SpellZoneRestrict)ParseInt(fields[41]);
            // 42 ENVIRONMENTTYPE
            // 43 TIMEOFDAY
            // 44 WARRIORMIN .. BERSERKERMIN
            for (int i = 0; i < spell.Levels.Length; i++)
                spell.Levels[i] = (byte)ParseInt(fields[44 + i]);
            // 60 CASTINGANIM
            // 61 TARGETANIM
            // 62 TRAVELTYPE
            // 63 SPELLAFFECTINDEX
            // 64 CANCELONSIT
            spell.CancelOnSit = ParseBool(fields[64]);
            // 65 DIETY_AGNOSTIC .. 81 DIETY_VEESHAN
            string[] gods = {
                "Agnostic", "Bertox", "Brell", "Cazic", "Erollisi", "Bristlebane", "Innoruuk", "Karana", "Mithanial", "Prexus",
                "Quellious", "Rallos", "Rodcet", "Solusek", "Tribunal", "Tunare", "Veeshan" };
            for (int i = 0; i < gods.Length; i++)
                if (ParseBool(fields[65 + i]))
                    spell.Deity += gods[i] + " ";
            // 82 NPC_NO_CAST
            // 83 AI_PT_BONUS
            // 84 NEW_ICON
            spell.Icon = ParseInt(fields[84]);
            // 85 SPELL_EFFECT_INDEX
            // 86 NO_INTERRUPT
            spell.Interruptable = !ParseBool(fields[86]);
            // 87 RESIST_MOD
            spell.ResistMod = ParseInt(fields[87]);
            // 88 NOT_STACKABLE_DOT
            // 89 DELETE_OK
            // 90 REFLECT_SPELLINDEX
            spell.RecourseID = ParseInt(fields[90]);
            if (spell.RecourseID != 0)
                spell.Recourse = String.Format("[Spell {0}]", spell.RecourseID);
            // 91 NO_PARTIAL_SAVE = used to prevent a nuke from being partially resisted. it also prevents or allows a player to resist a spell fully if they resist "part" of its components.
            spell.PartialResist = ParseBool(fields[91]);
            // 92 SMALL_TARGETS_ONLY
            // 93 USES_PERSISTENT_PARTICLES
            // 94 BARD_BUFF_BOX
            spell.SongWindow = ParseBool(fields[94]);
            // 95 DESCRIPTION_INDEX
            spell.DescID = ParseInt(fields[95]);
            // 96 PRIMARY_CATEGORY
            spell.CategoryDescID[0] = ParseInt(fields[96]);
            // 97 SECONDARY_CATEGORY_1
            spell.CategoryDescID[1] = ParseInt(fields[97]);
            // 98 SECONDARY_CATEGORY_2
            spell.CategoryDescID[2] = ParseInt(fields[98]);
            // 99 NO_NPC_LOS - NPC Does not Require LoS
            // 100 FEEDBACKABLE - Triggers spell damage shield. This is mostly used on procs and non nukes, so it's not that useful to show
            spell.Feedbackable = ParseBool(fields[100]);
            // 101 REFLECTABLE
            spell.Reflectable = ParseBool(fields[101]);
            // 102 HATE_MOD
            spell.HateMod = ParseInt(fields[102]);
            // 103 RESIST_PER_LEVEL
            spell.ResistPerLevel = ParseInt(fields[103]);
            // 104 RESIST_CAP
            spell.ResistCap = ParseInt(fields[104]);
            // 105 AFFECT_INANIMATE - Can be cast on objects
            // 106 STAMINA_COST
            spell.Endurance = ParseInt(fields[106]);
            // 107 TIMER_INDEX
            spell.TimerID = ParseInt(fields[107]);
            // 108 IS_SKILL
            spell.CombatSkill = ParseBool(fields[108]);
            // 109 SPELL_HATE_GIVEN
            spell.HateOverride = ParseInt(fields[109]);
            // 110 ENDUR_UPKEEP
            spell.EnduranceUpkeep = ParseInt(fields[110]);
            // 111 LIMITED_USE_TYPE
            spell.MaxHitsType = (SpellMaxHits)ParseInt(fields[111]);
            // 112 LIMITED_USE_COUNT
            spell.MaxHits = ParseInt(fields[112]);
            // 113 PVP_RESIST_MOD
            // 114 PVP_RESIST_PER_LEVEL
            // 115 PVP_RESIST_CAP
            // 116 GLOBAL_GROUP
            // 117 PVP_DURATION
            // 118 PVP_DURATION_CAP
            // 119 PCNPC_ONLY_FLAG
            // 120 CAST_NOT_STANDING
            // 121 CAN_MGB
            spell.MGBable = ParseBool(fields[121]);
            // 122 NO_DISPELL
            spell.Dispelable = !ParseBool(fields[122]);
            // 123 NPC_MEM_CATEGORY
            // 124 NPC_USEFULNESS
            // 125 MIN_RESIST
            spell.MinResist = ParseInt(fields[125]);
            // 126 MAX_RESIST
            spell.MaxResist = ParseInt(fields[126]);
            // 127 MIN_SPREAD_TIME
            spell.MinViralTime = ParseInt(fields[127]);
            // 128 MAX_SPREAD_TIME
            spell.MaxViralTime = ParseInt(fields[128]);
            // 129 DURATION_PARTICLE_EFFECT
            // 130 CONE_START_ANGLE
            spell.ConeStartAngle = ParseInt(fields[130]);
            // 131 CONE_END_ANGLE
            spell.ConeEndAngle = ParseInt(fields[131]);
            // 132 SNEAK_ATTACK
            spell.Sneaking = ParseBool(fields[132]);
            // 133 NOT_FOCUSABLE
            spell.Focusable = !ParseBool(fields[133]);
            // 134 NO_DETRIMENTAL_SPELL_AGGRO
            // 135 SHOW_WEAR_OFF_MESSAGE
            // 136 IS_COUNTDOWN_HELD
            spell.DurationFrozen = ParseBool(fields[136]);
            // 137 SPREAD_RADIUS
            spell.ViralRange = ParseInt(fields[137]);
            // 138 BASE_EFFECTS_FOCUS_CAP
            spell.SongCap = ParseInt(fields[138]);
            // 139 STACKS_WITH_SELF
            // 140 NOT_SHOWN_TO_PLAYER
            // 141 NO_BUFF_BLOCK
            spell.BeneficialBlockable = !ParseBool(fields[141]);
            // 142 ANIM_VARIATION
            // 143 SPELL_GROUP
            spell.GroupID = ParseInt(fields[143]);
            // 144 SPELL_GROUP_RANK
            spell.Rank = ParseInt(fields[144]); // rank 1/5/10. a few auras do not have this set properly
            if (spell.Rank == 5 || spell.Name.EndsWith("II") || spell.Name.EndsWith("02"))
                spell.Rank = 2;
            if (spell.Rank == 10 || spell.Name.EndsWith("III") || spell.Name.EndsWith("03"))
                spell.Rank = 3;
            // 145 NO_RESIST - ignore SPA 120 resist?
            // 146 ALLOW_SPELLSCRIBE
            // 147 SPELL_REQ_ASSOCIATION_ID
            spell.TargetRestrict = (SpellTargetRestrict)ParseInt(fields[147]);
            // 148 BYPASS_REGEN_CHECK
            spell.AllowFastRegen = ParseBool(fields[148]);
            // 149 CAN_CAST_IN_COMBAT
            spell.CastOutOfCombat = !ParseBool(fields[149]);
            // 150 CAN_CAST_OUT_OF_COMBAT
            // 151 SHOW_DOT_MESSAGE
            // 152 OVERRIDE_CRIT_CHANCE
            spell.CritOverride = ParseInt(fields[152]);
            // 153 MAX_TARGETS
            spell.MaxTargets = ParseInt(fields[153]);
            // 154 NO_HEAL_DAMAGE_ITEM_MOD
            // 155 CASTER_REQUIREMENT_ID
            spell.CasterRestrict = (SpellTargetRestrict)ParseInt(fields[155]);
            // 156 SPELL_CLASS
            // 157 SPELL_SUBCLASS
            // 158 AI_VALID_TARGETS
            // 159 NO_STRIP_ON_DEATH
            spell.PersistAfterDeath = ParseBool(fields[159]);
            // 160 BASE_EFFECTS_FOCUS_SLOPE
            // 161 BASE_EFFECTS_FOCUS_OFFSET
            // 162 DISTANCE_MOD_CLOSE_DIST
            spell.RangeModCloseDist = ParseInt(fields[162]);
            // 163 DISTANCE_MOD_CLOSE_MULT
            spell.RangeModCloseMult = ParseInt(fields[163]);
            // 164 DISTANCE_MOD_FAR_DIST
            spell.RangeModFarDist = ParseInt(fields[164]);
            // 165 DISTANCE_MOD_FAR_MULT
            spell.RangeModFarMult = ParseInt(fields[165]);
            // 166 MIN_RANGE
            spell.MinRange = ParseInt(fields[166]);
            // 167 NO_REMOVE
            spell.CannotRemove = ParseBool(fields[167]);
            // 168 SPELL_RECOURSE_TYPE
            // 169 ONLY_DURING_FAST_REGEN
            spell.CastInFastRegen = ParseBool(fields[169]);
            // 170 IS_BETA_ONLY
            spell.BetaOnly = ParseBool(fields[170]);
            // 171 SPELL_SUBGROUP
            // 172 NO_OVERWRITE
            // 173 SPA_SLOTS
            var slotlist = fields[173].Split('$').Select(x => x.Split('|'));
            foreach (var slotfields in slotlist)
            {
                // SPELL_SLOT|EFFECT_ID|BASE_EFFECT_1|BASE_EFFECT_2|LEVEL_EFFECT_MOD|EFFECT_CAP
                if (slotfields.Length < 5)
                    break;

                int i = ParseInt(slotfields[0]) - 1;
                int spa = ParseInt(slotfields[1]);
                int base1 = ParseInt(slotfields[2]);
                int base2 = ParseInt(slotfields[3]);
                int calc = ParseInt(slotfields[4]);
                int max = ParseInt(slotfields[5]);
                string desc = spell.ParseEffect(spa, base1, base2, max, calc, MAX_LEVEL);

                // unused slot, no more to follow
                if (spa == 254)
                    break;

                // unused slot, but there are more to follow
                if (desc == null)
                    continue;

                // make sure there is space for this slot (and any others that may have been skipped)
                while (spell.Slots.Count <= i)
                    spell.Slots.Add(null);

#if DEBUG
                desc = String.Format("SPA {0} Base1={1} Base2={2} Max={3} Calc={4} --- ", spa, base1, base2, max, calc) + desc;
#endif

                spell.Slots[i] = new SpellSlot
                {
                    SPA = spa,
                    Base1 = base1,
                    Base2 = base2,
                    Max = max,
                    Calc = calc,
                    Desc = desc
                };
            }

            // debug stuff
            //spell.Unknown = ParseFloat(fields[209]);
            //if (spell.ID == 21683) for (int i = 0; i < fields.Length; i++) Console.WriteLine("{0}: {1}", i, fields[i]);
            //if (fields[198] != "0") Console.WriteLine("\n\n===\n{0} {1}", fields[198], String.Join("\n", spell.Details()));

            spell.Prepare();
            return spell;
        }

        /// <summary>
        /// Load a single spell from the spells file.
        /// This handles the old file format which became obsolete with the 2016-04-13 test server patch.
        /// This format only had 179 fields.
        /// </summary>
        static Spell ParseSpell20160413(string[] fields)
        {
            var spell = new Spell();

            // 0 SPELLINDEX
            spell.ID = Convert.ToInt32(fields[0]);
            // 1 SPELLNAME
            spell.Name = fields[1].Trim();
            // 2 ACTORTAG
            // 3 NPC_FILENAME
            spell.Extra = fields[3];
            // 4 CASTERMETXT
            // 5 CASTEROTHERTXT
            // 6 CASTEDMETXT
            spell.LandOnSelf = fields[6];
            // 7 CASTEDOTHERTXT
            spell.LandOnOther = fields[7];
            // 8 SPELLGONE
            // 9 RANGE
            spell.Range = ParseInt(fields[9]);
            // 10 IMPACTRADIUS
            spell.AERange = ParseInt(fields[10]);
            // 11 OUTFORCE
            spell.PushBack = ParseFloat(fields[11]);
            // 12 UPFORCE
            spell.PushUp = ParseFloat(fields[12]);
            // 13 CASTINGTIME
            spell.CastingTime = ParseFloat(fields[13]) / 1000f;
            // 14 RECOVERYDELAY
            spell.RestTime = ParseFloat(fields[14]) / 1000f;
            // 15 SPELLDELAY
            spell.RecastTime = ParseFloat(fields[15]) / 1000f;
            // 16 DURATIONBASE
            // 17 DURATIONCAP
            spell.DurationTicks = Spell.CalcDuration(ParseInt(fields[16]), ParseInt(fields[17]), MAX_LEVEL);
            // 18 IMPACTDURATION
            spell.AEDuration = ParseInt(fields[18]);
            // 19 MANACOST
            spell.Mana = ParseInt(fields[19]);
            // 20 IMAGENUMBER
            // 21 MEMIMAGENUMBER
            // 22 EXPENDREAGENT1 .. 25 EXPENDREAGENT4
            // 26 EXPENDQTY1 .. 29 EXPENDQTY4
            // 30 NOEXPENDREAGENT1 .. 33 NOEXPENDREAGENT4
            for (int i = 0; i < 3; i++)
            {
                spell.ConsumeItemID[i] = ParseInt(fields[22 + i]);
                spell.ConsumeItemCount[i] = ParseInt(fields[26 + i]);
                spell.FocusID[i] = ParseInt(fields[30 + i]);
            }
            // 34 LIGHTTYPE
            // 35 BENEFICIAL
            spell.Beneficial = ParseBool(fields[35]);
            // 36 ACTIVATED
            // 37 RESISTTYPE
            spell.ResistType = (SpellResist)ParseInt(fields[37]);
            // 38 TYPENUMBER
            spell.Target = (SpellTarget)ParseInt(fields[38]);
            // 39 BASEDIFFICULTY = fizzle?
            // 40 CASTINGSKILL
            spell.Skill = (SpellSkill)ParseInt(fields[40]);
            // 41 ZONETYPE
            spell.Zone = (SpellZoneRestrict)ParseInt(fields[41]);
            // 42 ENVIRONMENTTYPE
            // 43 TIMEOFDAY
            // 44 WARRIORMIN .. BERSERKERMIN
            for (int i = 0; i < spell.Levels.Length; i++)
                spell.Levels[i] = (byte)ParseInt(fields[44 + i]);
            // 60 CASTINGANIM
            // 61 TARGETANIM
            // 62 TRAVELTYPE
            // 63 SPELLAFFECTINDEX
            // 64 CANCELONSIT
            spell.CancelOnSit = ParseBool(fields[64]);
            // 65 DIETY_AGNOSTIC .. 81 DIETY_VEESHAN
            string[] gods = {
                "Agnostic", "Bertox", "Brell", "Cazic", "Erollisi", "Bristlebane", "Innoruuk", "Karana", "Mithanial", "Prexus",
                "Quellious", "Rallos", "Rodcet", "Solusek", "Tribunal", "Tunare", "Veeshan" };
            for (int i = 0; i < gods.Length; i++)
                if (ParseBool(fields[65 + i]))
                    spell.Deity += gods[i] + " ";
            // 82 NPC_NO_CAST
            // 83 AI_PT_BONUS
            // 84 NEW_ICON
            spell.Icon = ParseInt(fields[84]);
            // 85 SPELL_EFFECT_INDEX
            // 86 NO_INTERRUPT
            spell.Interruptable = !ParseBool(fields[86]);
            // 87 RESIST_MOD
            spell.ResistMod = ParseInt(fields[87]);
            // 88 NOT_STACKABLE_DOT
            // 89 DELETE_OK
            // 90 REFLECT_SPELLINDEX
            spell.RecourseID = ParseInt(fields[90]);
            if (spell.RecourseID != 0)
                spell.Recourse = String.Format("[Spell {0}]", spell.RecourseID);
            // 91 NO_PARTIAL_SAVE = used to prevent a nuke from being partially resisted. it also prevents or allows a player to resist a spell fully if they resist "part" of its components.
            spell.PartialResist = ParseBool(fields[91]);
            // 92 SMALL_TARGETS_ONLY
            // 93 USES_PERSISTENT_PARTICLES
            // 94 BARD_BUFF_BOX
            spell.SongWindow = ParseBool(fields[94]);
            // 95 DESCRIPTION_INDEX
            spell.DescID = ParseInt(fields[95]);
            // 96 PRIMARY_CATEGORY
            spell.CategoryDescID[0] = ParseInt(fields[96]);
            // 97 SECONDARY_CATEGORY_1
            spell.CategoryDescID[1] = ParseInt(fields[97]);
            // 98 SECONDARY_CATEGORY_2
            spell.CategoryDescID[2] = ParseInt(fields[98]);
            // 99 NO_NPC_LOS - NPC Does not Require LoS
            // 100 FEEDBACKABLE - Triggers spell damage shield. This is mostly used on procs and non nukes, so it's not that useful to show
            spell.Feedbackable = ParseBool(fields[100]);
            // 101 REFLECTABLE
            spell.Reflectable = ParseBool(fields[101]);
            // 102 HATE_MOD
            spell.HateMod = ParseInt(fields[102]);
            // 103 RESIST_PER_LEVEL
            spell.ResistPerLevel = ParseInt(fields[103]);
            // 104 RESIST_CAP
            spell.ResistCap = ParseInt(fields[104]);
            // 105 AFFECT_INANIMATE - Can be cast on objects
            // 106 STAMINA_COST
            spell.Endurance = ParseInt(fields[106]);
            // 107 TIMER_INDEX
            spell.TimerID = ParseInt(fields[107]);
            // 108 IS_SKILL
            spell.CombatSkill = ParseBool(fields[108]);
            // 109 ATTACK_OPENING
            // 110 DEFENSE_OPENING
            // 111 SKILL_OPENING
            // 112 NPC_ERROR_OPENING
            // 113 SPELL_HATE_GIVEN
            spell.HateOverride = ParseInt(fields[113]);
            // 114 ENDUR_UPKEEP
            spell.EnduranceUpkeep = ParseInt(fields[114]);
            // 115 LIMITED_USE_TYPE
            spell.MaxHitsType = (SpellMaxHits)ParseInt(fields[115]);
            // 116 LIMITED_USE_COUNT
            spell.MaxHits = ParseInt(fields[116]);
            // 117 PVP_RESIST_MOD
            // 118 PVP_RESIST_PER_LEVEL
            // 119 PVP_RESIST_CAP
            // 120 GLOBAL_GROUP
            // 121 PVP_DURATION
            // 122 PVP_DURATION_CAP
            // 123 PCNPC_ONLY_FLAG
            // 124 CAST_NOT_STANDING
            // 125 CAN_MGB
            spell.MGBable = ParseBool(fields[125]);
            // 126 NO_DISPELL
            spell.Dispelable = !ParseBool(fields[126]);
            // 127 NPC_MEM_CATEGORY
            // 128 NPC_USEFULNESS
            // 129 MIN_RESIST
            spell.MinResist = ParseInt(fields[129]);
            // 130 MAX_RESIST
            spell.MaxResist = ParseInt(fields[130]);
            // 131 MIN_SPREAD_TIME
            spell.MinViralTime = ParseInt(fields[131]);
            // 132 MAX_SPREAD_TIME
            spell.MaxViralTime = ParseInt(fields[132]);
            // 133 DURATION_PARTICLE_EFFECT
            // 134 CONE_START_ANGLE
            spell.ConeStartAngle = ParseInt(fields[134]);
            // 135 CONE_END_ANGLE
            spell.ConeEndAngle = ParseInt(fields[135]);
            // 136 SNEAK_ATTACK
            spell.Sneaking = ParseBool(fields[136]);
            // 137 NOT_FOCUSABLE
            spell.Focusable = !ParseBool(fields[137]);
            // 138 NO_DETRIMENTAL_SPELL_AGGRO
            // 139 SHOW_WEAR_OFF_MESSAGE
            // 140 IS_COUNTDOWN_HELD
            spell.DurationFrozen = ParseBool(fields[140]);
            // 141 SPREAD_RADIUS
            spell.ViralRange = ParseInt(fields[141]);
            // 142 BASE_EFFECTS_FOCUS_CAP
            spell.SongCap = ParseInt(fields[142]);
            // 143 STACKS_WITH_SELF
            // 144 NOT_SHOWN_TO_PLAYER
            // 145 NO_BUFF_BLOCK
            spell.BeneficialBlockable = !ParseBool(fields[145]);
            // 146 ANIM_VARIATION
            // 147 SPELL_GROUP
            spell.GroupID = ParseInt(fields[147]);
            // 148 SPELL_GROUP_RANK
            spell.Rank = ParseInt(fields[148]); // rank 1/5/10. a few auras do not have this set properly
            if (spell.Rank == 5 || spell.Name.EndsWith("II") || spell.Name.EndsWith("02"))
                spell.Rank = 2;
            if (spell.Rank == 10 || spell.Name.EndsWith("III") || spell.Name.EndsWith("03"))
                spell.Rank = 3;
            // 149 NO_RESIST - ignore SPA 120 resist?
            // 150 ALLOW_SPELLSCRIBE
            // 151 SPELL_REQ_ASSOCIATION_ID
            spell.TargetRestrict = (SpellTargetRestrict)ParseInt(fields[151]);
            // 152 BYPASS_REGEN_CHECK
            spell.AllowFastRegen = ParseBool(fields[152]);
            // 153 CAN_CAST_IN_COMBAT
            spell.CastOutOfCombat = !ParseBool(fields[153]);
            // 154 CAN_CAST_OUT_OF_COMBAT
            // 155 SHOW_DOT_MESSAGE
            // 156 INVALID
            // 157 OVERRIDE_CRIT_CHANCE
            spell.CritOverride = ParseInt(fields[157]);
            // 158 MAX_TARGETS
            spell.MaxTargets = ParseInt(fields[158]);
            // 159 NO_HEAL_DAMAGE_ITEM_MOD
            // 160 CASTER_REQUIREMENT_ID
            spell.CasterRestrict = (SpellTargetRestrict)ParseInt(fields[160]);
            // 161 SPELL_CLASS
            // 162 SPELL_SUBCLASS
            // 163 AI_VALID_TARGETS
            // 164 NO_STRIP_ON_DEATH
            spell.PersistAfterDeath = ParseBool(fields[164]);
            // 165 BASE_EFFECTS_FOCUS_SLOPE
            // 166 BASE_EFFECTS_FOCUS_OFFSET
            // 167 DISTANCE_MOD_CLOSE_DIST
            spell.RangeModCloseDist = ParseInt(fields[167]);
            // 168 DISTANCE_MOD_CLOSE_MULT
            spell.RangeModCloseMult = ParseInt(fields[168]);
            // 169 DISTANCE_MOD_FAR_DIST
            spell.RangeModFarDist = ParseInt(fields[169]);
            // 170 DISTANCE_MOD_FAR_MULT
            spell.RangeModFarMult = ParseInt(fields[170]);
            // 171 MIN_RANGE
            spell.MinRange = ParseInt(fields[171]);
            // 172 NO_REMOVE
            spell.CannotRemove = ParseBool(fields[172]);
            // 173 SPELL_RECOURSE_TYPE
            // 174 ONLY_DURING_FAST_REGEN
            spell.CastInFastRegen = ParseBool(fields[174]);
            // 175 IS_BETA_ONLY
            spell.BetaOnly = ParseBool(fields[175]);
            // 176 SPELL_SUBGROUP
            // 177 NO_OVERWRITE
            // 178 SPA_SLOTS
            var slotlist = fields[178].Split('$').Select(x => x.Split('|'));
            foreach (var slotfields in slotlist)
            {
                // SPELL_SLOT|EFFECT_ID|BASE_EFFECT_1|BASE_EFFECT_2|LEVEL_EFFECT_MOD|EFFECT_CAP
                if (slotfields.Length < 5)
                    break;

                int i = ParseInt(slotfields[0]) - 1;
                int spa = ParseInt(slotfields[1]);
                int base1 = ParseInt(slotfields[2]);
                int base2 = ParseInt(slotfields[3]);
                int calc = ParseInt(slotfields[4]);
                int max = ParseInt(slotfields[5]);
                string desc = spell.ParseEffect(spa, base1, base2, max, calc, MAX_LEVEL);

                // unused slot, no more to follow
                if (spa == 254)
                    break;

                // unused slot, but there are more to follow
                if (desc == null)
                    continue;

                // make sure there is space for this slot (and any others that may have been skipped)
                while (spell.Slots.Count <= i)
                    spell.Slots.Add(null);

#if DEBUG
                desc = String.Format("SPA {0} Base1={1} Base2={2} Max={3} Calc={4} --- ", spa, base1, base2, max, calc) + desc;
#endif

                spell.Slots[i] = new SpellSlot
                {
                    SPA = spa,
                    Base1 = base1,
                    Base2 = base2,
                    Max = max,
                    Calc = calc,
                    Desc = desc
                };
            }

            // debug stuff
            //spell.Unknown = ParseFloat(fields[209]);
            //if (spell.ID == 21683) for (int i = 0; i < fields.Length; i++) Console.WriteLine("{0}: {1}", i, fields[i]);
            //if (fields[198] != "0") Console.WriteLine("\n\n===\n{0} {1}", fields[198], String.Join("\n", spell.Details()));

            spell.Prepare();
            return spell;
        }

        /// <summary>
        /// Load a single spell from the spell file.
        /// This handles the old file format which became obsolete with the 2016-02-10 test server patch.
        /// This format grew to 239 fields at it's final iteration.
        /// </summary>
        static Spell ParseSpell20160210(string[] fields)
        {
            var spell = new Spell();

            // 0 SPELLINDEX
            spell.ID = Convert.ToInt32(fields[0]);
            // 1 SPELLNAME
            spell.Name = fields[1].Trim();
            // 2 ACTORTAG
            // 3 NPC_FILENAME
            spell.Extra = fields[3];
            // 4 CASTERMETXT
            // 5 CASTEROTHERTXT
            // 6 CASTEDMETXT
            spell.LandOnSelf = fields[6];
            // 7 CASTEDOTHERTXT
            spell.LandOnOther = fields[7];
            // 8 SPELLGONE
            // 9 RANGE
            spell.Range = ParseInt(fields[9]);
            // 10 IMPACTRADIUS
            spell.AERange = ParseInt(fields[10]);
            // 11 OUTFORCE
            spell.PushBack = ParseFloat(fields[11]);
            // 12 UPFORCE
            spell.PushUp = ParseFloat(fields[12]);
            // 13 CASTINGTIME
            spell.CastingTime = ParseFloat(fields[13]) / 1000f;
            // 14 RECOVERYDELAY
            spell.RestTime = ParseFloat(fields[14]) / 1000f;
            // 15 SPELLDELAY
            spell.RecastTime = ParseFloat(fields[15]) / 1000f;
            // 16 DURATIONBASE
            // 17 DURATIONCAP
            spell.DurationTicks = Spell.CalcDuration(ParseInt(fields[16]), ParseInt(fields[17]), MAX_LEVEL);
            // 18 IMPACTDURATION
            spell.AEDuration = ParseInt(fields[18]);
            // 19 MANACOST
            spell.Mana = ParseInt(fields[19]);
            // 20 BASEAFFECT1 .. BASEAFFECT12
            // 32 BASE_EFFECT2_1 .. BASE_EFFECT2_12
            // 44 AFFECT1CAP .. AFFECT12CAP
            // 56 IMAGENUMBER
            // 57 MEMIMAGENUMBER
            // 58 EXPENDREAGENT1 .. 61 EXPENDREAGENT4
            // 62 EXPENDQTY1 .. 65 EXPENDQTY4
            // 66 NOEXPENDREAGENT1 .. 69 NOEXPENDREAGENT4
            for (int i = 0; i < 3; i++)
            {
                spell.ConsumeItemID[i] = ParseInt(fields[58 + i]);
                spell.ConsumeItemCount[i] = ParseInt(fields[62 + i]);
                spell.FocusID[i] = ParseInt(fields[66 + i]);
            }
            // 82 LIGHTTYPE
            // 83 BENEFICIAL
            spell.Beneficial = ParseBool(fields[83]);
            // 84 ACTIVATED
            // 85 RESISTTYPE
            spell.ResistType = (SpellResist)ParseInt(fields[85]);
            // 86 SPELLAFFECT1 .. SPELLAFFECT12
            // 98 TYPENUMBER
            spell.Target = (SpellTarget)ParseInt(fields[98]);
            // 99 BASEDIFFICULTY = fizzle?
            // 100 CASTINGSKILL
            spell.Skill = (SpellSkill)ParseInt(fields[100]);
            // 101 ZONETYPE
            spell.Zone = (SpellZoneRestrict)ParseInt(fields[101]);
            // 102 ENVIRONMENTTYPE
            // 103 TIMEOFDAY
            // 104 WARRIORMIN .. BERSERKERMIN
            for (int i = 0; i < spell.Levels.Length; i++)
                spell.Levels[i] = (byte)ParseInt(fields[104 + i]);
            // 120 CASTINGANIM
            // 121 TARGETANIM
            // 122 TRAVELTYPE
            // 123 SPELLAFFECTINDEX
            // 124 CANCELONSIT
            spell.CancelOnSit = ParseBool(fields[124]);
            // 125 DIETY_AGNOSTIC .. 141 DIETY_VEESHAN
            string[] gods = { 
                "Agnostic", "Bertox", "Brell", "Cazic", "Erollisi", "Bristlebane", "Innoruuk", "Karana", "Mithanial", "Prexus", 
                "Quellious", "Rallos", "Rodcet", "Solusek", "Tribunal", "Tunare", "Veeshan" };
            for (int i = 0; i < gods.Length; i++)
                if (ParseBool(fields[125 + i]))
                    spell.Deity += gods[i] + " ";
            // 142 NPC_NO_CAST
            // 143 AI_PT_BONUS
            // 144 NEW_ICON
            spell.Icon = ParseInt(fields[144]);
            // 145 SPELL_EFFECT_INDEX
            // 146 NO_INTERRUPT
            spell.Interruptable = !ParseBool(fields[146]);
            // 147 RESIST_MOD
            spell.ResistMod = ParseInt(fields[147]);
            // 148 NOT_STACKABLE_DOT
            // 149 DELETE_OK
            // 150 REFLECT_SPELLINDEX
            spell.RecourseID = ParseInt(fields[150]);
            if (spell.RecourseID != 0)
                spell.Recourse = String.Format("[Spell {0}]", spell.RecourseID);
            // 151 NO_PARTIAL_SAVE = used to prevent a nuke from being partially resisted. it also prevents or allows a player to resist a spell fully if they resist "part" of its components.
            spell.PartialResist = ParseBool(fields[151]);
            // 152 SMALL_TARGETS_ONLY
            // 153 USES_PERSISTENT_PARTICLES
            // 154 BARD_BUFF_BOX
            spell.SongWindow = ParseBool(fields[154]);
            // 155 DESCRIPTION_INDEX
            spell.DescID = ParseInt(fields[155]);
            // 156 PRIMARY_CATEGORY
            spell.CategoryDescID[0] = ParseInt(fields[156]);
            // 157 SECONDARY_CATEGORY_1
            spell.CategoryDescID[1] = ParseInt(fields[157]);
            // 158 SECONDARY_CATEGORY_2
            spell.CategoryDescID[2] = ParseInt(fields[158]);
            // 159 NO_NPC_LOS - NPC Does not Require LoS
            // 160 FEEDBACKABLE - Triggers spell damage shield. This is mostly used on procs and non nukes, so it's not that useful to show
            spell.Feedbackable = ParseBool(fields[160]);
            // 161 REFLECTABLE
            spell.Reflectable = ParseBool(fields[161]);
            // 162 HATE_MOD
            spell.HateMod = ParseInt(fields[162]);
            // 163 RESIST_PER_LEVEL
            spell.ResistPerLevel = ParseInt(fields[163]);
            // 164 RESIST_CAP
            spell.ResistCap = ParseInt(fields[164]);
            // 165 AFFECT_INANIMATE - Can be cast on objects
            // 166 STAMINA_COST
            spell.Endurance = ParseInt(fields[166]);
            // 167 TIMER_INDEX
            spell.TimerID = ParseInt(fields[167]);
            // 168 IS_SKILL
            spell.CombatSkill = ParseBool(fields[168]);
            // 169 ATTACK_OPENING
            // 170 DEFENSE_OPENING
            // 171 SKILL_OPENING
            // 172 NPC_ERROR_OPENING
            // 173 SPELL_HATE_GIVEN
            spell.HateOverride = ParseInt(fields[173]);
            // 174 ENDUR_UPKEEP
            spell.EnduranceUpkeep = ParseInt(fields[174]);
            // 175 LIMITED_USE_TYPE
            spell.MaxHitsType = (SpellMaxHits)ParseInt(fields[175]);
            // 176 LIMITED_USE_COUNT
            spell.MaxHits = ParseInt(fields[176]);
            // 177 PVP_RESIST_MOD
            // 178 PVP_RESIST_PER_LEVEL
            // 179 PVP_RESIST_CAP
            // 180 GLOBAL_GROUP
            // 181 PVP_DURATION
            // 182 PVP_DURATION_CAP
            // 183 PCNPC_ONLY_FLAG
            // 184 CAST_NOT_STANDING
            // 185 CAN_MGB
            spell.MGBable = ParseBool(fields[185]);
            // 186 NO_DISPELL
            spell.Dispelable = !ParseBool(fields[186]);
            // 187 NPC_MEM_CATEGORY
            // 188 NPC_USEFULNESS
            // 189 MIN_RESIST
            spell.MinResist = ParseInt(fields[189]);
            // 190 MAX_RESIST
            spell.MaxResist = ParseInt(fields[190]);
            // 191 MIN_SPREAD_TIME
            spell.MinViralTime = ParseInt(fields[191]);
            // 192 MAX_SPREAD_TIME
            spell.MaxViralTime = ParseInt(fields[192]);
            // 193 DURATION_PARTICLE_EFFECT
            // 194 CONE_START_ANGLE
            spell.ConeStartAngle = ParseInt(fields[194]);
            // 195 CONE_END_ANGLE
            spell.ConeEndAngle = ParseInt(fields[195]);
            // 196 SNEAK_ATTACK
            spell.Sneaking = ParseBool(fields[196]);
            // 197 NOT_FOCUSABLE
            spell.Focusable = !ParseBool(fields[197]);
            // 198 NO_DETRIMENTAL_SPELL_AGGRO
            // 199 SHOW_WEAR_OFF_MESSAGE
            // 200 IS_COUNTDOWN_HELD
            spell.DurationFrozen = ParseBool(fields[200]);
            // 201 SPREAD_RADIUS
            spell.ViralRange = ParseInt(fields[201]);
            // 202 BASE_EFFECTS_FOCUS_CAP
            spell.SongCap = ParseInt(fields[202]);
            // 203 STACKS_WITH_SELF
            // 204 NOT_SHOWN_TO_PLAYER
            // 205 NO_BUFF_BLOCK
            spell.BeneficialBlockable = !ParseBool(fields[205]);
            // 206 ANIM_VARIATION
            // 207 SPELL_GROUP
            spell.GroupID = ParseInt(fields[207]);
            // 208 SPELL_GROUP_RANK
            spell.Rank = ParseInt(fields[208]); // rank 1/5/10. a few auras do not have this set properly
            if (spell.Rank == 5 || spell.Name.EndsWith("II") || spell.Name.EndsWith("02"))
                spell.Rank = 2;
            if (spell.Rank == 10 || spell.Name.EndsWith("III") || spell.Name.EndsWith("03"))
                spell.Rank = 3;
            // 209 NO_RESIST - ignore SPA 180 resist?
            // 210 ALLOW_SPELLSCRIBE
            // 211 SPELL_REQ_ASSOCIATION_ID
            spell.TargetRestrict = (SpellTargetRestrict)ParseInt(fields[211]);
            // 212 BYPASS_REGEN_CHECK
            spell.AllowFastRegen = ParseBool(fields[212]);
            // 213 CAN_CAST_IN_COMBAT
            spell.CastOutOfCombat = !ParseBool(fields[213]);
            // 214 CAN_CAST_OUT_OF_COMBAT
            // 215 SHOW_DOT_MESSAGE
            // 216 INVALID
            // 217 OVERRIDE_CRIT_CHANCE
            spell.CritOverride = ParseInt(fields[217]);
            // 218 MAX_TARGETS
            spell.MaxTargets = ParseInt(fields[218]);
            // 219 NO_HEAL_DAMAGE_ITEM_MOD
            // 220 CASTER_REQUIREMENT_ID
            spell.CasterRestrict = (SpellTargetRestrict)ParseInt(fields[220]);
            // 221 SPELL_CLASS
            // 222 SPELL_SUBCLASS
            // 223 AI_VALID_TARGETS
            // 224 NO_STRIP_ON_DEATH
            spell.PersistAfterDeath = ParseBool(fields[224]);
            // 225 BASE_EFFECTS_FOCUS_SLOPE
            // 226 BASE_EFFECTS_FOCUS_OFFSET
            // 227 DISTANCE_MOD_CLOSE_DIST
            spell.RangeModCloseDist = ParseInt(fields[227]);
            // 228 DISTANCE_MOD_CLOSE_MULT
            spell.RangeModCloseMult = ParseInt(fields[228]);
            // 229 DISTANCE_MOD_FAR_DIST
            spell.RangeModFarDist = ParseInt(fields[229]);
            // 230 DISTANCE_MOD_FAR_MULT
            spell.RangeModFarMult = ParseInt(fields[230]);
            // 231 MIN_RANGE
            spell.MinRange = ParseInt(fields[231]);
            // 232 NO_REMOVE
            spell.CannotRemove = ParseBool(fields[232]);
            // 233 SPELL_RECOURSE_TYPE
            // 234 ONLY_DURING_FAST_REGEN
            spell.CastInFastRegen = ParseBool(fields[234]);
            // 235 IS_BETA_ONLY
            spell.BetaOnly = ParseBool(fields[235]);
            // 236 SPELL_SUBGROUP

            // each spell has 12 effect slots which have 5 attributes each
            // 20..31 - slot 1..12 base1 effect
            // 32..43 - slot 1..12 base2 effect
            // 44..55 - slot 1..12 max effect
            // 70..81 - slot 1..12 calc forumla data
            // 86..97 - slot 1..12 spa/type
            for (int i = 0; i < 12; i++)
            {
                int spa = ParseInt(fields[86 + i]);
                int calc = ParseInt(fields[70 + i]);
                int max = ParseInt(fields[44 + i]);
                int base1 = ParseInt(fields[20 + i]);
                int base2 = ParseInt(fields[32 + i]);
                string desc = spell.ParseEffect(spa, base1, base2, max, calc, MAX_LEVEL);

                // unused slot, no more to follow
                if (spa == 254)
                    break;

                // unused slot, but there are more to follow
                if (desc == null)
                {
                    spell.Slots.Add(null);
                    continue;
                }

#if DEBUG
                desc = String.Format("SPA {0} Base1={1} Base2={2} Max={3} Calc={4} --- ", spa, base1, base2, max, calc) + desc;
#endif

                spell.Slots.Add(new SpellSlot
                {
                    SPA = spa,
                    Base1 = base1,
                    Base2 = base2,
                    Max = max,
                    Calc = calc,
                    Desc = desc
                });
            }

            // debug stuff
            //spell.Unknown = ParseFloat(fields[209]);
            //if (spell.ID == 21683) for (int i = 0; i < fields.Length; i++) Console.WriteLine("{0}: {1}", i, fields[i]);
            //if (fields[198] != "0") Console.WriteLine("\n\n===\n{0} {1}", fields[198], String.Join("\n", spell.Details()));

            spell.Prepare();
            return spell;
        }

        static float ParseFloat(string s)
        {
            if (String.IsNullOrEmpty(s))
                return 0f;
            return Single.Parse(s, CultureInfo.InvariantCulture);
        }

        static int ParseInt(string s)
        {
            if (s == "" || s == "0" || s[0] == '.')
                return 0;
            return (int)Single.Parse(s, CultureInfo.InvariantCulture);
        }

        static bool ParseBool(string s)
        {
            return !String.IsNullOrEmpty(s) && (s != "0");
        }

        public static int ParseClass(string text)
        {
            if (String.IsNullOrEmpty(text))
                return 0;

            if (text.Length == 3 && Enum.IsDefined(typeof(SpellClasses), text.ToUpper()))
                return (int)Enum.Parse(typeof(SpellClasses), text.ToUpper());

            string[] names = Enum.GetNames(typeof(SpellClassesLong));
            for (int i = 0; i < names.Length; i++)
                if (String.Compare(names[i], text, true) == 0)
                    return (int)Enum.Parse(typeof(SpellClassesLong), names[i]);

            return 0;
        }

        /// <summary>
        /// Parse roman numerals up to 50. Returns 0 if the numeral could not be parsed.
        /// </summary>
        private static int ParseRomanNumeral(string num)
        {
            var roman = new string[] { 
                "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" ,
                "XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX", "XX",
                "XXI", "XXII", "XXIII", "XXIV", "XXV", "XXVI", "XXVII", "XXVIII", "XXIX", "XXX",
                "XXXI", "XXXII", "XXXIII", "XXXIV", "XXXV", "XXXVI", "XXXVII", "XXXVIII", "XXXIX", "XL",
                "XLI", "XLII", "XLIII", "XLIV", "XLV", "XLVI", "XLVII", "XLVIII", "XLIX", "L",
            };

            return Array.IndexOf(roman, num.ToUpper()) + 1;
        }

    }

    /// <summary>
    /// Provides transparent gzip decompression on files that may compressed.
    /// The game doesn't ever store spells files in gzip format but I added this in case someone wants to keep lots of versioned spell files around and save space.
    /// </summary>
    public static class PossiblyCompressedFile
    {
        public static StreamReader OpenText(string path)
        {
            if (path.EndsWith(".gz"))
            {
                // todo: should I buffer the uncompressed stream?
                var stream = new GZipStream(File.OpenRead(path), CompressionMode.Decompress);
                return new StreamReader(stream);
            }

            return File.OpenText(path);
        }
    }

}
