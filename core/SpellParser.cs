using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;


namespace EQSpellParser
{
    /// <summary>
    /// SpellParser reads spells_us.txt and related files to generate Spell data instances for every spell.
    /// Most of the interesting SPA and calc parsing is handled by the Spell class.
    /// </summary>
    public static partial class SpellParser
    {
        /// <summary>
        /// Load all spells from spells_us.txt.
        /// This method only takes the path to the main spells_us.txt. It will attempt to load supporting files 
        /// as long as they are in the same folder and use the same naming convention.
        /// </summary>
        public static List<Spell> LoadFromFile(string spellPath)
        {
            // the spell file is required. the other files are optional 
            if (!File.Exists(spellPath))
                throw new FileNotFoundException("Could not open spell file.", spellPath);

            var version = GetFileVersion(spellPath);

            var list = new List<Spell>(60000);
            var listById = new Dictionary<int, Spell>(list.Capacity);

            // load description strings file
            var desc = new Dictionary<string, string>(80000);
            var descPath = spellPath.Replace("spells_us", "dbstr_us");
            foreach (var fields in LoadAllLines(descPath))
            {
                // currently 4 fields - older files are 3 fields
                if (fields.Length >= 3)
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
                // type 45 = faction 
            }

            // load spell definition file
            foreach (var fields in LoadAllLines(spellPath))
            {
                // debug
                //for (int i = 0; i < fields.Length; i++) Console.WriteLine("{0}: {1}", i, fields[i]);

                // file format has changed over time - pick the best parser
                Func<string[], int, Spell> parser = ParseSpell;
                if (fields.Length == 167 && version < 20191008)
                    parser = ParseSpell20190108;
                if (fields.Length == 168 && version < 20180509)
                    parser = ParseSpell20180509;
                if (fields.Length == 169 && version < 20180410)
                    parser = ParseSpell20180410;
                if (fields.Length == 174 && version < 20180204)
                    parser = ParseSpell20180214;
                if (fields.Length == 179 && version < 20160413)
                    parser = ParseSpell20160413;
                if (fields.Length > 220)
                    parser = ParseSpell20160210;

                // parser the spell
                var spell = parser(fields, version);

                list.Add(spell);
                listById[spell.ID] = spell;

                // update faction references with actual names
                for (int i = 0; i < spell.Slots.Count; i++)
                    if (spell.Slots[i] != null)
                    {
                        var slot = spell.Slots[i];

                        slot.Desc = Spell.FactionRefExpr.Replace(slot.Desc, m => {
                            string fname = null;
                            if (desc.TryGetValue("45/" + m.Groups[1].Value, out fname))
                                return fname;
                            return m.Groups[0].Value;
                        });
                    }

                // update description
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

            // load spell string file (starting 2018-2-14)
            // *SPELLINDEX^CASTERMETXT^CASTEROTHERTXT^CASTEDMETXT^CASTEDOTHERTXT^SPELLGONE^
            var strPath = spellPath.Replace("spells_us", "spells_us_str");
            foreach (var fields in LoadAllLines(strPath))
            {
                var id = ParseInt(fields[0]);
                var landself = fields[3];
                var landother = fields[4];

                Spell spell = null;
                if (!listById.TryGetValue(id, out spell))
                    continue;

                spell.LandOnSelf = landself;
                //spell.LandOnOther = landother; // this doesn't really get used so saving memory by excluding it
            }

            // load spell stacking file
            // my guess is that this was made a separate file because a single spell can be part of multiple spell stacking groups
            var stackPath = spellPath.Replace("spells_us", "SpellStackingGroups");
            foreach (var fields in LoadAllLines(stackPath))
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
        /// Load a single spell from the spells file.
        /// This handles the current file format.
        /// This format has 166 or 168 fields.
        /// </summary>
        static Spell ParseSpell(string[] fields, int version)
        {
            var spell = new Spell();
            spell.Version = version;

            // 0 SPELLINDEX
            spell.ID = Convert.ToInt32(fields[0]);
            // 1 SPELLNAME
            spell.Name = fields[1].Trim();
            // 2 ACTORTAG
            // 3 NPC_FILENAME
            spell.Extra = fields[3];
            //spell.LandOnSelf = fields[?];
            //spell.LandOnOther = fields[?];
            // 4 RANGE
            spell.Range = ParseInt(fields[4]);
            // 5 IMPACTRADIUS
            spell.AERange = ParseInt(fields[5]);
            // 6 OUTFORCE
            spell.PushBack = ParseFloat(fields[6]);
            // 7 UPFORCE
            spell.PushUp = ParseFloat(fields[7]);
            // 8 CASTINGTIME
            spell.CastingTime = ParseFloat(fields[8]) / 1000f;
            // 9 RECOVERYDELAY
            spell.RestTime = ParseFloat(fields[9]) / 1000f;
            // 10 SPELLDELAY
            spell.RecastTime = ParseFloat(fields[10]) / 1000f;
            // 11 DURATIONBASE
            // 12 DURATIONCAP
            spell.DurationTicks = Spell.CalcDuration(ParseInt(fields[11]), ParseInt(fields[12]));
            // 13 IMPACTDURATION
            spell.AEDuration = ParseInt(fields[13]);
            // 14 MANACOST
            spell.Mana = ParseInt(fields[14]);
            // 15 IMAGENUMBER
            // 16 MEMIMAGENUMBER
            // 17 EXPENDREAGENT1 .. 20 EXPENDREAGENT4
            // 21 EXPENDQTY1 .. 24 EXPENDQTY4
            // 25 NOEXPENDREAGENT1 .. 28 NOEXPENDREAGENT4
            for (int i = 0; i < 3; i++)
            {
                spell.ConsumeItemID[i] = ParseInt(fields[17 + i]);
                spell.ConsumeItemCount[i] = ParseInt(fields[21 + i]);
                spell.FocusID[i] = ParseInt(fields[25 + i]);
            }
            // 29 LIGHTTYPE
            // 30 BENEFICIAL
            spell.Beneficial = ParseBool(fields[30]);
            // 31 RESISTTYPE
            spell.ResistType = (SpellResist)ParseInt(fields[31]);
            // 32 TYPENUMBER
            spell.Target = (SpellTarget)ParseInt(fields[32]);
            // 33 BASEDIFFICULTY = fizzle?
            // 34 CASTINGSKILL
            spell.Skill = (SpellSkill)ParseInt(fields[34]);
            // 35 ZONETYPE
            spell.Zone = (SpellZoneRestrict)ParseInt(fields[35]);
            // 36 ENVIRONMENTTYPE
            // 37 TIMEOFDAY
            // 38 WARRIORMIN .. BERSERKERMIN
            for (int i = 0; i < spell.Levels.Length; i++)
                spell.Levels[i] = (byte)ParseInt(fields[38 + i]);
            // 54 CASTINGANIM
            // 55 TARGETANIM
            // 56 TRAVELTYPE
            // 57 SPELLAFFECTINDEX
            // 58 CANCELONSIT
            spell.CancelOnSit = ParseBool(fields[58]);
            // 59 DIETY_AGNOSTIC .. 75 DIETY_VEESHAN
            string[] gods = {
                "Agnostic", "Bertox", "Brell", "Cazic", "Erollisi", "Bristlebane", "Innoruuk", "Karana", "Mithanial", "Prexus",
                "Quellious", "Rallos", "Rodcet", "Solusek", "Tribunal", "Tunare", "Veeshan" };
            for (int i = 0; i < gods.Length; i++)
                if (ParseBool(fields[59 + i]))
                    spell.Deity += gods[i] + " ";
            // 76 NPC_NO_CAST
            // 77 NEW_ICON
            spell.Icon = ParseInt(fields[77]);
            // 78 SPELL_EFFECT_INDEX
            // 79 NO_INTERRUPT
            spell.Interruptable = !ParseBool(fields[79]);
            // 80 RESIST_MOD
            spell.ResistMod = ParseInt(fields[80]);
            // 81 NOT_STACKABLE_DOT
            // 82 DELETE_OK
            // 83 REFLECT_SPELLINDEX
            spell.RecourseID = ParseInt(fields[83]);
            // 84 NO_PARTIAL_SAVE = used to prevent a nuke from being partially resisted. it also prevents or allows a player to resist a spell fully if they resist "part" of its components.
            spell.PartialResist = ParseBool(fields[84]);
            // 85 SMALL_TARGETS_ONLY
            // 86 USES_PERSISTENT_PARTICLES
            // 87 BARD_BUFF_BOX
            spell.SongWindow = ParseBool(fields[87]);
            // 88 DESCRIPTION_INDEX
            spell.DescID = ParseInt(fields[88]);
            // 89 PRIMARY_CATEGORY
            spell.CategoryDescID[0] = ParseInt(fields[89]);
            // 90 SECONDARY_CATEGORY_1
            spell.CategoryDescID[1] = ParseInt(fields[90]);
            // 91 SECONDARY_CATEGORY_2
            spell.CategoryDescID[2] = ParseInt(fields[91]);
            // 92 NO_NPC_LOS - NPC Does not Require LoS
            // 93 FEEDBACKABLE - Triggers spell damage shield. This is mostly used on procs and non nukes, so it's not that useful to show
            spell.Feedbackable = ParseBool(fields[93]);
            // 94 REFLECTABLE
            spell.Reflectable = ParseBool(fields[94]);
            // 95 HATE_MOD
            spell.HateMod = ParseInt(fields[95]);
            // 96 RESIST_PER_LEVEL
            spell.ResistPerLevel = ParseInt(fields[96]);
            // 97 RESIST_CAP
            spell.ResistCap = ParseInt(fields[97]);
            // 98 AFFECT_INANIMATE - Can be cast on objects
            // 99 STAMINA_COST
            spell.Endurance = ParseInt(fields[99]);
            // 100 TIMER_INDEX
            spell.TimerID = ParseInt(fields[100]);
            // 101 IS_SKILL
            spell.CombatSkill = ParseBool(fields[101]);
            // 102 SPELL_HATE_GIVEN
            spell.HateOverride = ParseInt(fields[102]);
            // 103 ENDUR_UPKEEP
            spell.EnduranceUpkeep = ParseInt(fields[103]);
            // 104 LIMITED_USE_TYPE
            spell.MaxHitsType = (SpellMaxHits)ParseInt(fields[104]);
            // 105 LIMITED_USE_COUNT
            spell.MaxHits = ParseInt(fields[105]);
            // 106 PVP_RESIST_MOD
            // 107 PVP_RESIST_PER_LEVEL
            // 108 PVP_RESIST_CAP
            // 109 PVP_DURATION
            // 110 PVP_DURATION_CAP
            // 111 PCNPC_ONLY_FLAG
            // 112 CAST_NOT_STANDING
            // 113 CAN_MGB
            spell.MGBable = ParseBool(fields[113]);
            // 114 NO_DISPELL
            spell.Dispelable = !ParseBool(fields[114]);
            // 115 NPC_MEM_CATEGORY
            // 116 NPC_USEFULNESS
            // 117 MIN_RESIST
            spell.MinResist = ParseInt(fields[117]);
            // 118 MAX_RESIST
            spell.MaxResist = ParseInt(fields[118]);
            // 119 MIN_SPREAD_TIME
            spell.MinViralTime = ParseInt(fields[119]);
            // 120 MAX_SPREAD_TIME
            spell.MaxViralTime = ParseInt(fields[120]);
            // 121 DURATION_PARTICLE_EFFECT
            // 122 CONE_START_ANGLE
            spell.ConeStartAngle = ParseInt(fields[122]);
            // 123 CONE_END_ANGLE
            spell.ConeEndAngle = ParseInt(fields[123]);
            // 124 SNEAK_ATTACK
            spell.Sneaking = ParseBool(fields[124]);
            // 125 NOT_FOCUSABLE
            spell.Focusable = !ParseBool(fields[125]);
            // 126 NO_DETRIMENTAL_SPELL_AGGRO
            // 127 SHOW_WEAR_OFF_MESSAGE
            // 128 IS_COUNTDOWN_HELD
            spell.DurationFrozen = ParseBool(fields[128]);
            // 129 SPREAD_RADIUS
            spell.ViralRange = ParseInt(fields[129]);
            // 130 BASE_EFFECTS_FOCUS_CAP
            spell.SongCap = ParseInt(fields[130]);
            // 131 STACKS_WITH_SELF
            // 132 NOT_SHOWN_TO_PLAYER
            // 133 NO_BUFF_BLOCK
            spell.BeneficialBlockable = !ParseBool(fields[133]);
            // 134 ANIM_VARIATION
            // 135 SPELL_GROUP
            spell.GroupID = ParseInt(fields[135]);
            // 136 SPELL_GROUP_RANK
            spell.Rank = ParseInt(fields[136]); // rank 1/5/10. a few auras do not have this set properly
            // 137 NO_RESIST - ignore SPA 178 resist
            spell.NoSanctification = ParseBool(fields[137]);
            // 138 ALLOW_SPELLSCRIBE
            // 139 SPELL_REQ_ASSOCIATION_ID
            spell.TargetRestrict = (SpellTargetRestrict)ParseInt(fields[139]);
            // 140 BYPASS_REGEN_CHECK
            spell.AllowFastRegen = ParseBool(fields[140]);
            // 141 CAN_CAST_IN_COMBAT
            spell.CastOutOfCombat = !ParseBool(fields[141]);
            // 142 CAN_CAST_OUT_OF_COMBAT
            // 143 SHOW_DOT_MESSAGE
            // 144 OVERRIDE_CRIT_CHANCE
            spell.CritOverride = ParseInt(fields[144]);
            // 145 MAX_TARGETS
            spell.MaxTargets = ParseInt(fields[145]);
            // 146 NO_HEAL_DAMAGE_ITEM_MOD
            // 147 CASTER_REQUIREMENT_ID
            spell.CasterRestrict = (SpellTargetRestrict)ParseInt(fields[147]);
            // 148 SPELL_CLASS
            // 149 SPELL_SUBCLASS
            // 150 AI_VALID_TARGETS
            // 151 NO_STRIP_ON_DEATH
            spell.PersistAfterDeath = ParseBool(fields[151]);
            // 152 BASE_EFFECTS_FOCUS_SLOPE
            // 153 BASE_EFFECTS_FOCUS_OFFSET
            // 154 DISTANCE_MOD_CLOSE_DIST
            spell.RangeModCloseDist = ParseInt(fields[154]);
            // 155 DISTANCE_MOD_CLOSE_MULT
            spell.RangeModCloseMult = ParseInt(fields[155]);
            // 156 DISTANCE_MOD_FAR_DIST
            spell.RangeModFarDist = ParseInt(fields[156]);
            // 157 DISTANCE_MOD_FAR_MULT
            spell.RangeModFarMult = ParseInt(fields[157]);
            // 158 MIN_RANGE
            spell.MinRange = ParseInt(fields[158]);
            // 159 NO_REMOVE
            spell.CannotRemove = ParseBool(fields[159]);
            // 160 SPELL_RECOURSE_TYPE
            // 161 ONLY_DURING_FAST_REGEN
            spell.CastInFastRegen = ParseBool(fields[161]);
            // 162 IS_BETA_ONLY
            spell.BetaOnly = ParseBool(fields[162]);
            // 163 SPELL_SUBGROUP
            // 164 NO_OVERWRITE
            // 165 SPA_SLOTS (to 2019-09-13)
            // 167 SPA_SLOTS (from 2019-10-08)
            var slotlist = fields[fields.Length - 1].Split('$').Select(x => x.Split('|'));
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

                // unused slot, no more to follow
                if (spa == 254)
                    break;

                // unused slot, but there are more to follow
                //if (desc == null)
                //    continue;

                // make sure there is space for this slot (and any others that may have been skipped)
                while (spell.Slots.Count <= i)
                    spell.Slots.Add(null);

                spell.Slots[i] = new SpellSlot
                {
                    SPA = spa,
                    Base1 = base1,
                    Base2 = base2,
                    Max = max,
                    Calc = calc,
                };
            }

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
        static int ParseRomanNumeral(string num)
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

        /// <summary>
        /// Open a spell file and perform gzip decompression if needed.
        /// The game doesn't ever store spells files in gzip format but I added this in case someone wants to keep lots of versioned spell files around and save space.
        /// </summary>
        public static StreamReader OpenFileReader(string path)
        {
            var f = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (path.EndsWith(".gz"))
            {
                // todo: should I buffer the uncompressed stream?
                var gzip = new GZipStream(f, CompressionMode.Decompress);
                return new StreamReader(gzip);
            }

            return new StreamReader(f);
        }

        /// <summary>
        /// Load all lines from a delimited spell data file and split each up into individual fields.
        /// </summary>
        public static IEnumerable<string[]> LoadAllLines(string path)
        {
            if (File.Exists(path))
                using (var text = OpenFileReader(path))
                    while (!text.EndOfStream)
                    {
                        var line = text.ReadLine();
                        if (line.StartsWith("#"))
                            continue;
                        var fields = line.Split('^');
                        if (fields.Length < 2 || line.StartsWith("#"))
                            continue;
                        yield return fields;
                    }
        }

        public static int CountFields(string path)
        {
            if (path.EndsWith(".gz"))
                return 0;

            using (var f = File.OpenText(path))
            {
                return f.ReadLine().Split('^').Length;
            }
        }

        /// <summary>
        /// Return file date as an integer. e.g. Feb 1, 2016 -> 20160201
        /// </summary>
        public static int GetFileVersion(string path)
        {
            // use the file timestamp as the version string 
            // (this is not perfect because file timestamps can be accidentally changed)
            var version = File.GetCreationTime(path).ToString("yyyyMMdd");

            // also check the filename for a date version string. e.g. spells_us-2016-01-01.txt
            var versionInName = Regex.Match(Path.GetFileName(path), @"\d\d\d\d-\d\d-\d\d");
            if (versionInName.Success)
                version = versionInName.Groups[0].Value.Replace("-", "");

            return Int32.Parse(version);
        }

    }

}
