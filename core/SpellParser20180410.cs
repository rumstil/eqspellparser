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
        /// This handles the old file format which became obsolete with the 2018-04-10 test server patch.
        /// This format had 169 fields.
        /// </summary>
        static Spell ParseSpell20180410(string[] fields, int version)
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
            // 31 ACTIVATED
            // 32 RESISTTYPE
            spell.ResistType = (SpellResist)ParseInt(fields[32]);
            // 33 TYPENUMBER
            spell.Target = (SpellTarget)ParseInt(fields[33]);
            // 34 BASEDIFFICULTY = fizzle?
            // 35 CASTINGSKILL
            spell.Skill = (SpellSkill)ParseInt(fields[35]);
            // 36 ZONETYPE
            spell.Zone = (SpellZoneRestrict)ParseInt(fields[36]);
            // 37 ENVIRONMENTTYPE
            // 38 TIMEOFDAY
            // 39 WARRIORMIN .. BERSERKERMIN
            for (int i = 0; i < spell.Levels.Length; i++)
                spell.Levels[i] = (byte)ParseInt(fields[39 + i]);
            // 55 CASTINGANIM
            // 56 TARGETANIM
            // 57 TRAVELTYPE
            // 58 SPELLAFFECTINDEX
            // 59 CANCELONSIT
            spell.CancelOnSit = ParseBool(fields[59]);
            // 60 DIETY_AGNOSTIC .. 76 DIETY_VEESHAN
            string[] gods = {
                "Agnostic", "Bertox", "Brell", "Cazic", "Erollisi", "Bristlebane", "Innoruuk", "Karana", "Mithanial", "Prexus",
                "Quellious", "Rallos", "Rodcet", "Solusek", "Tribunal", "Tunare", "Veeshan" };
            for (int i = 0; i < gods.Length; i++)
                if (ParseBool(fields[60 + i]))
                    spell.Deity += gods[i] + " ";
            // 77 NPC_NO_CAST
            // 78 AI_PT_BONUS
            // 79 NEW_ICON
            spell.Icon = ParseInt(fields[79]);
            // 80 SPELL_EFFECT_INDEX
            // 81 NO_INTERRUPT
            spell.Interruptable = !ParseBool(fields[81]);
            // 82 RESIST_MOD
            spell.ResistMod = ParseInt(fields[82]);
            // 83 NOT_STACKABLE_DOT
            // 84 DELETE_OK
            // 85 REFLECT_SPELLINDEX
            spell.RecourseID = ParseInt(fields[85]);
            // 86 NO_PARTIAL_SAVE = used to prevent a nuke from being partially resisted. it also prevents or allows a player to resist a spell fully if they resist "part" of its components.
            spell.PartialResist = ParseBool(fields[86]);
            // 87 SMALL_TARGETS_ONLY
            // 88 USES_PERSISTENT_PARTICLES
            // 89 BARD_BUFF_BOX
            spell.SongWindow = ParseBool(fields[89]);
            // 90 DESCRIPTION_INDEX
            spell.DescID = ParseInt(fields[90]);
            // 91 PRIMARY_CATEGORY
            spell.CategoryDescID[0] = ParseInt(fields[91]);
            // 92 SECONDARY_CATEGORY_1
            spell.CategoryDescID[1] = ParseInt(fields[92]);
            // 93 SECONDARY_CATEGORY_2
            spell.CategoryDescID[2] = ParseInt(fields[93]);
            // 94 NO_NPC_LOS - NPC Does not Require LoS
            // 95 FEEDBACKABLE - Triggers spell damage shield. This is mostly used on procs and non nukes, so it's not that useful to show
            spell.Feedbackable = ParseBool(fields[95]);
            // 96 REFLECTABLE
            spell.Reflectable = ParseBool(fields[96]);
            // 97 HATE_MOD
            spell.HateMod = ParseInt(fields[97]);
            // 98 RESIST_PER_LEVEL
            spell.ResistPerLevel = ParseInt(fields[98]);
            // 99 RESIST_CAP
            spell.ResistCap = ParseInt(fields[99]);
            // 100 AFFECT_INANIMATE - Can be cast on objects
            // 101 STAMINA_COST
            spell.Endurance = ParseInt(fields[101]);
            // 102 TIMER_INDEX
            spell.TimerID = ParseInt(fields[102]);
            // 103 IS_SKILL
            spell.CombatSkill = ParseBool(fields[103]);
            // 104 SPELL_HATE_GIVEN
            spell.HateOverride = ParseInt(fields[104]);
            // 105 ENDUR_UPKEEP
            spell.EnduranceUpkeep = ParseInt(fields[105]);
            // 106 LIMITED_USE_TYPE
            spell.MaxHitsType = (SpellMaxHits)ParseInt(fields[106]);
            // 107 LIMITED_USE_COUNT
            spell.MaxHits = ParseInt(fields[107]);
            // 108 PVP_RESIST_MOD
            // 109 PVP_RESIST_PER_LEVEL
            // 110 PVP_RESIST_CAP
            // 111 GLOBAL_GROUP
            // 112 PVP_DURATION
            // 113 PVP_DURATION_CAP
            // 114 PCNPC_ONLY_FLAG
            // 115 CAST_NOT_STANDING
            // 116 CAN_MGB
            spell.MGBable = ParseBool(fields[116]);
            // 117 NO_DISPELL
            spell.Dispelable = !ParseBool(fields[117]);
            // 118 NPC_MEM_CATEGORY
            // 119 NPC_USEFULNESS
            // 120 MIN_RESIST
            spell.MinResist = ParseInt(fields[120]);
            // 121 MAX_RESIST
            spell.MaxResist = ParseInt(fields[121]);
            // 122 MIN_SPREAD_TIME
            spell.MinViralTime = ParseInt(fields[122]);
            // 123 MAX_SPREAD_TIME
            spell.MaxViralTime = ParseInt(fields[123]);
            // 124 DURATION_PARTICLE_EFFECT
            // 125 CONE_START_ANGLE
            spell.ConeStartAngle = ParseInt(fields[125]);
            // 126 CONE_END_ANGLE
            spell.ConeEndAngle = ParseInt(fields[126]);
            // 127 SNEAK_ATTACK
            spell.Sneaking = ParseBool(fields[127]);
            // 128 NOT_FOCUSABLE
            spell.Focusable = !ParseBool(fields[128]);
            // 129 NO_DETRIMENTAL_SPELL_AGGRO
            // 130 SHOW_WEAR_OFF_MESSAGE
            // 131 IS_COUNTDOWN_HELD
            spell.DurationFrozen = ParseBool(fields[131]);
            // 132 SPREAD_RADIUS
            spell.ViralRange = ParseInt(fields[132]);
            // 133 BASE_EFFECTS_FOCUS_CAP
            spell.SongCap = ParseInt(fields[133]);
            // 134 STACKS_WITH_SELF
            // 135 NOT_SHOWN_TO_PLAYER
            // 136 NO_BUFF_BLOCK
            spell.BeneficialBlockable = !ParseBool(fields[136]);
            // 137 ANIM_VARIATION
            // 138 SPELL_GROUP
            spell.GroupID = ParseInt(fields[138]);
            // 139 SPELL_GROUP_RANK
            spell.Rank = ParseInt(fields[139]); // rank 1/5/10. a few auras do not have this set properly
            // 140 NO_RESIST - ignore SPA 180 resist
            spell.NoSanctification = ParseBool(fields[140]);
            // 141 ALLOW_SPELLSCRIBE
            // 142 SPELL_REQ_ASSOCIATION_ID
            spell.TargetRestrict = (SpellTargetRestrict)ParseInt(fields[142]);
            // 143 BYPASS_REGEN_CHECK
            spell.AllowFastRegen = ParseBool(fields[143]);
            // 144 CAN_CAST_IN_COMBAT
            spell.CastOutOfCombat = !ParseBool(fields[144]);
            // 145 CAN_CAST_OUT_OF_COMBAT
            // 146 SHOW_DOT_MESSAGE
            // 147 OVERRIDE_CRIT_CHANCE
            spell.CritOverride = ParseInt(fields[147]);
            // 148 MAX_TARGETS
            spell.MaxTargets = ParseInt(fields[148]);
            // 149 NO_HEAL_DAMAGE_ITEM_MOD
            // 150 CASTER_REQUIREMENT_ID
            spell.CasterRestrict = (SpellTargetRestrict)ParseInt(fields[150]);
            // 151 SPELL_CLASS
            // 152 SPELL_SUBCLASS
            // 153 AI_VALID_TARGETS
            // 154 NO_STRIP_ON_DEATH
            spell.PersistAfterDeath = ParseBool(fields[154]);
            // 155 BASE_EFFECTS_FOCUS_SLOPE
            // 156 BASE_EFFECTS_FOCUS_OFFSET
            // 157 DISTANCE_MOD_CLOSE_DIST
            spell.RangeModCloseDist = ParseInt(fields[157]);
            // 158 DISTANCE_MOD_CLOSE_MULT
            spell.RangeModCloseMult = ParseInt(fields[158]);
            // 159 DISTANCE_MOD_FAR_DIST
            spell.RangeModFarDist = ParseInt(fields[159]);
            // 160 DISTANCE_MOD_FAR_MULT
            spell.RangeModFarMult = ParseInt(fields[160]);
            // 161 MIN_RANGE
            spell.MinRange = ParseInt(fields[161]);
            // 162 NO_REMOVE
            spell.CannotRemove = ParseBool(fields[162]);
            // 163 SPELL_RECOURSE_TYPE
            // 164 ONLY_DURING_FAST_REGEN
            spell.CastInFastRegen = ParseBool(fields[164]);
            // 165 IS_BETA_ONLY
            spell.BetaOnly = ParseBool(fields[165]);
            // 166 SPELL_SUBGROUP
            // 167 NO_OVERWRITE
            // 168 SPA_SLOTS
            var slotlist = fields[168].Split('$').Select(x => x.Split('|'));
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
