using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQSpellParser
{
    public static partial class SpellParser
    {
        /// <summary>
        /// Load a single spell from the spell file.
        /// This handles the old file format which became obsolete with the 2016-02-10 test server patch.
        /// This format grew to 239 fields in it's final iteration.
        /// </summary>
        static Spell ParseSpell20160210(string[] fields, int version)
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
            // 209 NO_RESIST - ignore SPA 180 resist?
            spell.NoSanctification = ParseBool(fields[209]);
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

                // unused slot, no more to follow
                if (spa == 254)
                    break;

                // unused slot, but there are more to follow
                //if (desc == null)
                //{
                //    spell.Slots.Add(null);
                //    continue;
                //}

                spell.Slots.Add(new SpellSlot
                {
                    SPA = spa,
                    Base1 = base1,
                    Base2 = base2,
                    Max = max,
                    Calc = calc,
                });
            }

            spell.Prepare();
            return spell;
        }

    }
}
