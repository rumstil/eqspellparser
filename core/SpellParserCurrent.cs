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
        /// This format has 165 fields
        /// </summary>
        static Spell ParseSpellCurrent(string[] fields, int version)
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
            // 13 IMAGENUMBER
            // 14 MEMIMAGENUMBER
            // 15 EXPENDREAGENT1 .. 18 EXPENDREAGENT4
            // 19 EXPENDQTY1 .. 22 EXPENDQTY4
            // 23 NOEXPENDREAGENT1 .. 26 NOEXPENDREAGENT4
            for (int i = 0; i < 3; i++)
            {
                spell.ConsumeItemID[i] = ParseInt(fields[15 + i]);
                spell.ConsumeItemCount[i] = ParseInt(fields[19 + i]);
                spell.FocusID[i] = ParseInt(fields[23 + i]);
            }
            // 27 LIGHTTYPE
            // 28 BENEFICIAL
            spell.Beneficial = ParseBool(fields[28]);
            // 29 RESISTTYPE
            spell.ResistType = (SpellResist)ParseInt(fields[29]);
            // 30 TYPENUMBER
            spell.Target = (SpellTarget)ParseInt(fields[30]);
            // 31 BASEDIFFICULTY = fizzle?
            // 32 CASTINGSKILL
            spell.Skill = (SpellSkill)ParseInt(fields[32]);
            // 33 ZONETYPE
            spell.Zone = (SpellZoneRestrict)ParseInt(fields[33]);
            // 34 ENVIRONMENTTYPE
            // 35 TIMEOFDAY
            // 36 WARRIORMIN .. BERSERKERMIN
            for (int i = 0; i < spell.Levels.Length; i++)
                spell.Levels[i] = (byte)ParseInt(fields[36 + i]);
            // 52 CASTINGANIM
            // 53 TARGETANIM
            // 54 TRAVELTYPE
            // 55 SPELLAFFECTINDEX
            // 56 CANCELONSIT
            spell.CancelOnSit = ParseBool(fields[56]);
            // 57 DIETY_AGNOSTIC .. 73 DIETY_VEESHAN
            string[] gods = {
                "Agnostic", "Bertox", "Brell", "Cazic", "Erollisi", "Bristlebane", "Innoruuk", "Karana", "Mithanial", "Prexus",
                "Quellious", "Rallos", "Rodcet", "Solusek", "Tribunal", "Tunare", "Veeshan" };
            for (int i = 0; i < gods.Length; i++)
                if (ParseBool(fields[57 + i]))
                    spell.Deity += gods[i] + " ";
            // 74 NPC_NO_CAST
            // 75 NEW_ICON
            spell.Icon = ParseInt(fields[75]);
            // 76 SPELL_EFFECT_INDEX
            // 77 NO_INTERRUPT
            spell.Interruptable = !ParseBool(fields[77]);
            // 78 RESIST_MOD
            spell.ResistMod = ParseInt(fields[78]);
            // 79 NOT_STACKABLE_DOT
            // 80 DELETE_OK
            // 81 REFLECT_SPELLINDEX
            spell.RecourseID = ParseInt(fields[81]);
            // 82 NO_PARTIAL_SAVE = used to prevent a nuke from being partially resisted. it also prevents or allows a player to resist a spell fully if they resist "part" of its components.
            spell.PartialResist = ParseBool(fields[82]);
            // 83 SMALL_TARGETS_ONLY (removed)
            // 83 USES_PERSISTENT_PARTICLES
            // 84 BARD_BUFF_BOX
            spell.SongWindow = ParseBool(fields[84]);
            // 85 DESCRIPTION_INDEX
            spell.DescID = ParseInt(fields[85]);
            // 86 PRIMARY_CATEGORY
            spell.CategoryDescID[0] = ParseInt(fields[86]);
            // 87 SECONDARY_CATEGORY_1
            spell.CategoryDescID[1] = ParseInt(fields[87]);
            // 88 SECONDARY_CATEGORY_2
            spell.CategoryDescID[2] = ParseInt(fields[88]);
            // 89 NO_NPC_LOS - NPC Does not Require LoS
            // 90 FEEDBACKABLE - Triggers spell damage shield. This is mostly used on procs and non nukes, so it's not that useful to show
            spell.Feedbackable = ParseBool(fields[90]);
            // 91 REFLECTABLE
            spell.Reflectable = ParseBool(fields[91]);
            // 92 HATE_MOD
            spell.HateMod = ParseInt(fields[92]);
            // 93 RESIST_PER_LEVEL
            spell.ResistPerLevel = ParseInt(fields[93]);
            // 94 RESIST_CAP
            spell.ResistCap = ParseInt(fields[94]);
            // 95 AFFECT_INANIMATE - Can be cast on objects
            // 96 STAMINA_COST
            spell.Endurance = ParseInt(fields[96]);
            // 97 TIMER_INDEX
            spell.TimerID = ParseInt(fields[97]);
            // 98 IS_SKILL
            spell.CombatSkill = ParseBool(fields[98]);
            // 99 SPELL_HATE_GIVEN
            spell.HateOverride = ParseInt(fields[99]);
            // 100 ENDUR_UPKEEP
            spell.EnduranceUpkeep = ParseInt(fields[100]);
            // 101 LIMITED_USE_TYPE
            spell.MaxHitsType = (SpellMaxHits)ParseInt(fields[101]);
            // 102 LIMITED_USE_COUNT
            spell.MaxHits = ParseInt(fields[102]);
            // 103 PVP_RESIST_MOD
            // 104 PVP_RESIST_PER_LEVEL
            // 105 PVP_RESIST_CAP
            // 106 PVP_DURATION
            // 107 PVP_DURATION_CAP
            // 108 PCNPC_ONLY_FLAG
            // 109 CAST_NOT_STANDING
            // 110 CAN_MGB
            spell.MGBable = ParseBool(fields[110]);
            // 111 NO_DISPELL
            spell.Dispelable = !ParseBool(fields[111]);
            // 112 NPC_MEM_CATEGORY
            // 113 NPC_USEFULNESS
            // 114 MIN_RESIST
            spell.MinResist = ParseInt(fields[114]);
            // 115 MAX_RESIST
            spell.MaxResist = ParseInt(fields[115]);
            // 116 MIN_SPREAD_TIME
            spell.MinViralTime = ParseInt(fields[116]);
            // 117 MAX_SPREAD_TIME
            spell.MaxViralTime = ParseInt(fields[117]);
            // 118 DURATION_PARTICLE_EFFECT
            // 119 CONE_START_ANGLE
            spell.ConeStartAngle = ParseInt(fields[119]);
            // 120 CONE_END_ANGLE
            spell.ConeEndAngle = ParseInt(fields[120]);
            // 121 SNEAK_ATTACK
            spell.Sneaking = ParseBool(fields[121]);
            // 122 NOT_FOCUSABLE
            spell.Focusable = !ParseBool(fields[122]);
            // 123 NO_DETRIMENTAL_SPELL_AGGRO
            // 124 SHOW_WEAR_OFF_MESSAGE
            // 125 IS_COUNTDOWN_HELD
            spell.DurationFrozen = ParseBool(fields[125]);
            // 126 SPREAD_RADIUS
            spell.ViralRange = ParseInt(fields[126]);
            // 127 BASE_EFFECTS_FOCUS_CAP
            spell.SongCap = ParseInt(fields[127]);
            // 128 STACKS_WITH_SELF
            // 129 NOT_SHOWN_TO_PLAYER
            // 130 NO_BUFF_BLOCK
            spell.BeneficialBlockable = !ParseBool(fields[130]);
            // 131 ANIM_VARIATION
            // 132 SPELL_GROUP
            spell.GroupID = ParseInt(fields[132]);
            // 133 SPELL_GROUP_RANK
            spell.Rank = ParseInt(fields[133]); // rank 1/5/10. a few auras do not have this set properly
            // 134 NO_RESIST - ignore SPA 177 resist
            spell.NoSanctification = ParseBool(fields[134]);
            // 135 ALLOW_SPELLSCRIBE
            // 136 SPELL_REQ_ASSOCIATION_ID
            spell.TargetRestrict = (SpellTargetRestrict)ParseInt(fields[136]);
            // 137 BYPASS_REGEN_CHECK
            spell.AllowFastRegen = ParseBool(fields[137]);
            // 138 CAN_CAST_IN_COMBAT
            spell.CastOutOfCombat = !ParseBool(fields[138]);
            // 139 CAN_CAST_OUT_OF_COMBAT
            // 140 SHOW_DOT_MESSAGE
            // 141 OVERRIDE_CRIT_CHANCE
            spell.CritOverride = ParseInt(fields[141]);
            // 142 MAX_TARGETS
            spell.MaxTargets = ParseInt(fields[142]);
            // 143 NO_HEAL_DAMAGE_ITEM_MOD
            // 144 CASTER_REQUIREMENT_ID
            spell.CasterRestrict = (SpellTargetRestrict)ParseInt(fields[144]);
            // 145 SPELL_CLASS
            // 146 SPELL_SUBCLASS
            // 147 AI_VALID_TARGETS
            // 148 NO_STRIP_ON_DEATH
            spell.PersistAfterDeath = ParseBool(fields[148]);
            // 149 BASE_EFFECTS_FOCUS_SLOPE
            // 150 BASE_EFFECTS_FOCUS_OFFSET
            // 151 DISTANCE_MOD_CLOSE_DIST
            spell.RangeModCloseDist = ParseInt(fields[151]);
            // 152 DISTANCE_MOD_CLOSE_MULT
            spell.RangeModCloseMult = ParseInt(fields[152]);
            // 153 DISTANCE_MOD_FAR_DIST
            spell.RangeModFarDist = ParseInt(fields[153]);
            // 154 DISTANCE_MOD_FAR_MULT
            spell.RangeModFarMult = ParseInt(fields[154]);
            // 155 MIN_RANGE
            spell.MinRange = ParseInt(fields[155]);
            // 156 NO_REMOVE
            spell.CannotRemove = ParseBool(fields[156]);
            // 157 SPELL_RECOURSE_TYPE
            // 158 ONLY_DURING_FAST_REGEN
            spell.CastInFastRegen = ParseBool(fields[158]);
            // 159 IS_BETA_ONLY
            spell.BetaOnly = ParseBool(fields[159]);
            // 160 SPELL_SUBGROUP
            // 161 NO_OVERWRITE
            // 164 SPA_SLOTS
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

    }
}


