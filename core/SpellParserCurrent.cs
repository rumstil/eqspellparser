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
        /// This handles the current file format.
        /// This format has 167 fields
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
            // 85 SMALL_TARGETS_ONLY (removed)
            // 85 USES_PERSISTENT_PARTICLES
            // 86 BARD_BUFF_BOX
            spell.SongWindow = ParseBool(fields[86]);
            // 87 DESCRIPTION_INDEX
            spell.DescID = ParseInt(fields[87]);
            // 88 PRIMARY_CATEGORY
            spell.CategoryDescID[0] = ParseInt(fields[88]);
            // 89 SECONDARY_CATEGORY_1
            spell.CategoryDescID[1] = ParseInt(fields[89]);
            // 90 SECONDARY_CATEGORY_2
            spell.CategoryDescID[2] = ParseInt(fields[90]);
            // 91 NO_NPC_LOS - NPC Does not Require LoS
            // 92 FEEDBACKABLE - Triggers spell damage shield. This is mostly used on procs and non nukes, so it's not that useful to show
            spell.Feedbackable = ParseBool(fields[92]);
            // 93 REFLECTABLE
            spell.Reflectable = ParseBool(fields[93]);
            // 94 HATE_MOD
            spell.HateMod = ParseInt(fields[94]);
            // 95 RESIST_PER_LEVEL
            spell.ResistPerLevel = ParseInt(fields[95]);
            // 96 RESIST_CAP
            spell.ResistCap = ParseInt(fields[96]);
            // 97 AFFECT_INANIMATE - Can be cast on objects
            // 98 STAMINA_COST
            spell.Endurance = ParseInt(fields[98]);
            // 99 TIMER_INDEX
            spell.TimerID = ParseInt(fields[99]);
            // 100 IS_SKILL
            spell.CombatSkill = ParseBool(fields[100]);
            // 101 SPELL_HATE_GIVEN
            spell.HateOverride = ParseInt(fields[101]);
            // 102 ENDUR_UPKEEP
            spell.EnduranceUpkeep = ParseInt(fields[102]);
            // 103 LIMITED_USE_TYPE
            spell.MaxHitsType = (SpellMaxHits)ParseInt(fields[103]);
            // 104 LIMITED_USE_COUNT
            spell.MaxHits = ParseInt(fields[104]);
            // 105 PVP_RESIST_MOD
            // 106 PVP_RESIST_PER_LEVEL
            // 107 PVP_RESIST_CAP
            // 108 PVP_DURATION
            // 109 PVP_DURATION_CAP
            // 110 PCNPC_ONLY_FLAG
            // 111 CAST_NOT_STANDING
            // 112 CAN_MGB
            spell.MGBable = ParseBool(fields[112]);
            // 113 NO_DISPELL
            spell.Dispelable = !ParseBool(fields[113]);
            // 114 NPC_MEM_CATEGORY
            // 115 NPC_USEFULNESS
            // 116 MIN_RESIST
            spell.MinResist = ParseInt(fields[116]);
            // 117 MAX_RESIST
            spell.MaxResist = ParseInt(fields[117]);
            // 118 MIN_SPREAD_TIME
            spell.MinViralTime = ParseInt(fields[118]);
            // 119 MAX_SPREAD_TIME
            spell.MaxViralTime = ParseInt(fields[119]);
            // 120 DURATION_PARTICLE_EFFECT
            // 121 CONE_START_ANGLE
            spell.ConeStartAngle = ParseInt(fields[121]);
            // 122 CONE_END_ANGLE
            spell.ConeEndAngle = ParseInt(fields[122]);
            // 123 SNEAK_ATTACK
            spell.Sneaking = ParseBool(fields[123]);
            // 124 NOT_FOCUSABLE
            spell.Focusable = !ParseBool(fields[124]);
            // 125 NO_DETRIMENTAL_SPELL_AGGRO
            // 126 SHOW_WEAR_OFF_MESSAGE
            // 127 IS_COUNTDOWN_HELD
            spell.DurationFrozen = ParseBool(fields[127]);
            // 128 SPREAD_RADIUS
            spell.ViralRange = ParseInt(fields[128]);
            // 129 BASE_EFFECTS_FOCUS_CAP
            spell.SongCap = ParseInt(fields[129]);
            // 130 STACKS_WITH_SELF
            // 131 NOT_SHOWN_TO_PLAYER
            // 132 NO_BUFF_BLOCK
            spell.BeneficialBlockable = !ParseBool(fields[132]);
            // 133 ANIM_VARIATION
            // 134 SPELL_GROUP
            spell.GroupID = ParseInt(fields[134]);
            // 135 SPELL_GROUP_RANK
            spell.Rank = ParseInt(fields[135]); // rank 1/5/10. a few auras do not have this set properly
            // 136 NO_RESIST - ignore SPA 177 resist
            spell.NoSanctification = ParseBool(fields[136]);
            // 137 ALLOW_SPELLSCRIBE
            // 138 SPELL_REQ_ASSOCIATION_ID
            spell.TargetRestrict = (SpellTargetRestrict)ParseInt(fields[138]);
            // 139 BYPASS_REGEN_CHECK
            spell.AllowFastRegen = ParseBool(fields[139]);
            // 140 CAN_CAST_IN_COMBAT
            spell.CastOutOfCombat = !ParseBool(fields[140]);
            // 141 CAN_CAST_OUT_OF_COMBAT
            // 142 SHOW_DOT_MESSAGE
            // 143 OVERRIDE_CRIT_CHANCE
            spell.CritOverride = ParseInt(fields[143]);
            // 144 MAX_TARGETS
            spell.MaxTargets = ParseInt(fields[144]);
            // 145 NO_HEAL_DAMAGE_ITEM_MOD
            // 146 CASTER_REQUIREMENT_ID
            spell.CasterRestrict = (SpellTargetRestrict)ParseInt(fields[146]);
            // 147 SPELL_CLASS
            // 148 SPELL_SUBCLASS
            // 149 AI_VALID_TARGETS
            // 150 NO_STRIP_ON_DEATH
            spell.PersistAfterDeath = ParseBool(fields[150]);
            // 151 BASE_EFFECTS_FOCUS_SLOPE
            // 152 BASE_EFFECTS_FOCUS_OFFSET
            // 153 DISTANCE_MOD_CLOSE_DIST
            spell.RangeModCloseDist = ParseInt(fields[153]);
            // 154 DISTANCE_MOD_CLOSE_MULT
            spell.RangeModCloseMult = ParseInt(fields[154]);
            // 155 DISTANCE_MOD_FAR_DIST
            spell.RangeModFarDist = ParseInt(fields[155]);
            // 156 DISTANCE_MOD_FAR_MULT
            spell.RangeModFarMult = ParseInt(fields[156]);
            // 157 MIN_RANGE
            spell.MinRange = ParseInt(fields[157]);
            // 158 NO_REMOVE
            spell.CannotRemove = ParseBool(fields[158]);
            // 159 SPELL_RECOURSE_TYPE
            // 160 ONLY_DURING_FAST_REGEN
            spell.CastInFastRegen = ParseBool(fields[160]);
            // 161 IS_BETA_ONLY
            spell.BetaOnly = ParseBool(fields[161]);
            // 162 SPELL_SUBGROUP
            // 163 NO_OVERWRITE
            // 166 SPA_SLOTS
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

