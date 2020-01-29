using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQSpellParser
{
    public static partial class SpellParser
    {
        /// <summary>
        /// Load a single spell from the spells file.
        /// This handles the old file format which became obsolete with the 2018-02-14 test server patch.
        /// This format had 174 fields.
        /// </summary>
        static Spell ParseSpell20180214(string[] fields, int version)
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
            spell.DurationTicks = Spell.CalcDuration(ParseInt(fields[16]), ParseInt(fields[17]));
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
            // 145 NO_RESIST - ignore SPA 180 resist
            spell.NoSanctification = ParseBool(fields[145]);
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

    }
}
