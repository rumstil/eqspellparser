using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace EQSpellParser
{
    public sealed class SpellSlot
    {
        public int SPA;
        public int Base1;
        public int Base2;
        public int Max;
        public int Calc;
        public string Desc;

        public override string ToString()
        {
            return Desc;
        }
    }

    public sealed class Spell
    {
        public int ID;
        public int GroupID;
        public string Name;
        public int Icon;
        public int Mana;
        public int Endurance;
        public int EnduranceUpkeep;
        public int DurationTicks;
        public bool Focusable;
        public List<SpellSlot> Slots;
        //public byte Level;
        public byte[] Levels; // casting level for each of the 16 classes
        public byte[] ExtLevels; // similar to levels but assigns levels for side effect spells that don't have levels defined (e.g. a proc effect will get the level of it's proc buff)
        public string ClassesLevels;
        public SpellClassesMask ClassesMask;
        public SpellSkill Skill;
        public bool Beneficial;
        public bool BeneficialBlockable;
        public SpellTarget Target;
        public SpellResist ResistType;
        public int ResistMod;
        public bool PartialResist;
        public int MinResist;
        public int MaxResist;
        public string Extra;
        public int HateOverride;
        public int HateMod;
        public int Range;
        public int AERange;
        public int AEDuration; // rain spells
        public float CastingTime;
        public float RestTime; // refresh time
        public float RecastTime;
        public float PushBack;
        public float PushUp;
        public int DescID;
        public string Desc;
        public int MaxHits;
        public SpellMaxHits MaxHitsType;
        public int MaxTargets;
        public int RecourseID;
        //public string Recourse;
        public int TimerID;
        public int ViralRange;
        public int MinViralTime;
        public int MaxViralTime;
        public SpellTargetRestrict TargetRestrict;
        public SpellTargetRestrict CasterRestrict;
        public int[] ConsumeItemID;
        public int[] ConsumeItemCount;
        public int[] FocusID;
        public string LandOnSelf;
        public string LandOnOther;
        public int ConeStartAngle;
        public int ConeEndAngle;
        public bool MGBable;
        public int Rank;
        public bool CastOutOfCombat;
        public SpellZoneRestrict Zone;
        public bool DurationFrozen; // in guildhall/lobby
        public bool Dispelable;
        public bool PersistAfterDeath;
        public bool SongWindow;
        public bool CancelOnSit;
        public bool Sneaking;
        public int[] CategoryDescID; // most AAs don't have these set
        public string Deity;
        public int SongCap;
        public int MinRange;
        public int RangeModCloseDist;
        public int RangeModCloseMult;
        public int RangeModFarDist;
        public int RangeModFarMult;
        public bool Interruptable;
        public bool Feedbackable; // triger spell DS
        public bool Reflectable;
        public int SpellClass;
        public int SpellSubclass;
        public bool CastInFastRegen;
        public bool AllowFastRegen;
        public bool BetaOnly;
        public bool CannotRemove;
        public int CritOverride; // when set the spell has this max % crit chance and mod 
        public bool CombatSkill;
        public int ResistPerLevel;
        public int ResistCap;
        public bool NoSanctification;
        public List<string> Stacking;
        public int Version; // Int32.Parse(yyyyMMdd) - non standard date format but convenient for comparisons

        public int[] LinksTo;
        public int RefCount; // number of spells that link to this

#if !LimitMemoryUse
        public string[] Categories;
        //public string[] RawData;
#endif

        public float Unknown;


        /// Effects can reference other spells or items via square bracket notation. e.g.
        /// [Spell 123]    is a reference to spell 123
        /// [Group 123]    is a reference to spell group 123
        /// [Item 123]     is a reference to item 123
        /// [AA 123]     is a reference to AA group 123
        public static readonly Regex SpellRefExpr = new Regex(@"\[Spell\s(\d+)\]");
        public static readonly Regex GroupRefExpr = new Regex(@"\[Group\s(\d+)\]");
        public static readonly Regex ItemRefExpr = new Regex(@"\[Item\s(\d+)\]");
        public static readonly Regex AARefExpr = new Regex(@"\[AA\s(\d+)\]");

        public Spell()
        {
            Slots = new List<SpellSlot>(6); // first grow will make it a list of 12
            Levels = new byte[16];
            ExtLevels = new byte[16];
            ConsumeItemID = new int[4];
            ConsumeItemCount = new int[4];
            FocusID = new int[4];
            CategoryDescID = new int[3];
            Stacking = new List<string>();
        }

        public string Recourse { get { if (RecourseID != 0) return String.Format("[Spell {0}]", RecourseID); return null; } }

        public string ParseEffect(SpellSlot slot, int level)
        {
            return ParseEffect(slot.SPA, slot.Base1, slot.Base2, slot.Max, slot.Calc, level);
        }

        /// <summary>
        /// Each spell can have a number of slots for variable spell effects. The game developers call these "SPAs".
        /// TODO: this should be a static function but it makes references to spell attributes like ID, Skill, Extra, DurationTicks and in a few cases even modifies the Mana attribute.
        /// </summary>
        public string ParseEffect(int spa, int base1, int base2, int max, int calc, int level)
        {
            // type 254 indicates end of slots (i.e. if there are any others they will also be 254)
            if (spa == 254)
                return null;

            // many SPAs use a scaled value based on either current tick or caster level
            var value = CalcValue(calc, base1, max, 1, level);
            var range = CalcValueRange(calc, base1, max, DurationTicks, level);
            //Func<int> base1_or_value = delegate() { Debug.WriteLineIf(base1 != value, "SPA " + spa + " value uncertain"); return base1; };

            // some hp/mana/end/hate effects repeat for each tick of the duration
            string repeating = (DurationTicks > 0) ? " per tick" : null;

            // some effects are capped at a max level
            string absmax = (max > 0) ? String.Format(" up to level {0}", max) : null;

            string varmax = null;
            if (Version != 0 && Version < 20170912)
                varmax = absmax;
            else if (max > 1000)
                varmax = String.Format(" up to level {0}", max - 1000);
            else if (max != 0)
                varmax = String.Format(" up to level {0}", max.ToString("+ #;- #;0"));

            //Func<string, string> FormatCount = delegate(string name) { return ((value > 0) ? "Increase " : "Decrease ") + name + " by " + Math.Abs(value); };

            switch (spa)
            {
                case 0:
                    if (base2 > 0)
                        return Spell.FormatCount("Current HP", value) + repeating + range + " (If " + Spell.FormatEnum((SpellTargetRestrict)base2) + ")";
                    return Spell.FormatCount("Current HP", value) + repeating + range;
                case 1:
                    return Spell.FormatCount("AC", (int)(value / (10f / 3f)));
                case 2:
                    return Spell.FormatCount("ATK", value) + range;
                case 3:
                    // SPA 3 (found on run-speed buffs) is only 70% effective compared to SPA 271 as the two SPAs
                    // affect the overall movement rate of an entity at different points in the calculation. -Dzarn
                    return Spell.FormatPercent("Movement Speed", value);
                case 4:
                    return Spell.FormatCount("STR", value) + range;
                case 5:
                    return Spell.FormatCount("DEX", value) + range;
                case 6:
                    return Spell.FormatCount("AGI", value) + range;
                case 7:
                    return Spell.FormatCount("STA", value) + range;
                case 8:
                    return Spell.FormatCount("INT", value) + range;
                case 9:
                    return Spell.FormatCount("WIS", value) + range;
                case 10:
                    // type 10 is sometimes an unused placeholder slot
                    if (base1 <= 1 || base1 > 255)
                        return null;
                    return Spell.FormatCount("CHA", value) + range;
                case 11:
                    // base attack speed is 100. so 85 = 15% slow, 130 = 30% haste
                    // reverse check on max value for slow spells
                    if (value < 100 && max > 0 && value < max)
                        value = max;
                    return Spell.FormatPercent("Melee Haste", value - 100);
                case 12:
                    if (base1 > 1)
                        return String.Format("Invisibility (Enhanced {0})", base1);
                    return "Invisibility (Unstable)";
                case 13:
                    if (base1 > 1)
                        return String.Format("See Invisible (Enhanced {0})", base1);
                    return "See Invisible";
                case 14:
                    return "Enduring Breath";
                case 15:
                    return Spell.FormatCount("Current Mana", value) + repeating + range;
                case 18:
                    return "Pacify";
                case 19:
                    return Spell.FormatCount("Faction", value);
                case 20:
                    return "Blind";
                case 21:
                    //if (base2 != base1 && base2 != 0)
                    //    return String.Format("Stun for {0:0.##}s ({1:0.##}s in PvP)", base1 / 1000f, base2 / 1000f) + maxlevel;
                    return String.Format("Stun for {0:0.##}s", base1 / 1000f) + absmax;
                case 22:
                    if (base2 > 0)
                        return "Charm" + absmax + String.Format(" with {0}% Chance of Memory Blur", base2);
                    return "Charm" + absmax;
                case 23:
                    return "Fear" + absmax;
                case 24:
                    return Spell.FormatCount("Stamina Loss", value);
                case 25:
                    return "Bind";
                case 26:
                    if (base2 > 1)
                        return "Gate to Secondary Bind";
                    return "Gate";
                case 27:
                    return String.Format("Dispel ({0})", value);
                case 28:
                    return "Invisibility to Undead (Unstable)";
                case 29:
                    return "Invisibility to Animals (Unstable)";
                case 30:
                    // how close can you get before the mob aggroes you
                    return String.Format("Decrease Aggro Radius to {0}", value) + absmax;
                case 31:
                    return "Mesmerize" + absmax;
                case 32:
                    // calc 100 = summon a stack? (based on item stack size) Pouch of Quellious, Quiver of Marr
                    //return String.Format("Summon: [Item {0}] x {1} {2} {3}", base1, calc, max, base2);
                    return String.Format("Summon: [Item {0}]", base1);
                case 33:
                    return String.Format("Summon Pet: {0}", Extra);
                case 35:
                    return Spell.FormatCount("Disease Counter", value);
                case 36:
                    return Spell.FormatCount("Poison Counter", value);
                case 39:
                    // this doesn't actually block twincast by itself. 
                    // twincast excludes spells that have this marker
                    return "Stacking: Twincast Blocker";
                case 40:
                    return "Invulnerability";
                case 41:
                    return "Destroy";
                case 42:
                    // TODO: does shadowstep always gate an NPC? e.g. highsun
                    return "Shadowstep";
                case 44:
                    return String.Format("Stacking: Delayed Heal Marker ({0})", value);
                case 46:
                    return Spell.FormatCount("Fire Resist", value);
                case 47:
                    return Spell.FormatCount("Cold Resist", value);
                case 48:
                    return Spell.FormatCount("Poison Resist", value);
                case 49:
                    return Spell.FormatCount("Disease Resist", value);
                case 50:
                    return Spell.FormatCount("Magic Resist", value);
                case 52:
                    return "Sense Undead";
                case 53:
                    return "Sense Summoned";
                case 54:
                    return "Sense Animal";
                case 55:
                    return String.Format("Absorb Damage: 100%, Total: {0}", value);
                case 56:
                    return "True North";
                case 57:
                    return "Levitate";
                case 58:
                    value = (base1 << 16) + base2;
                    if (Enum.IsDefined(typeof(SpellIllusion), value))
                        return String.Format("Illusion: {0}", Spell.FormatEnum((SpellIllusion)value));
                    if (base2 > 0)
                        return String.Format("Illusion: {0} ({1})", Spell.FormatEnum((SpellIllusion)base1), base2);
                    return String.Format("Illusion: {0}", Spell.FormatEnum((SpellIllusion)base1));
                case 59:
                    return Spell.FormatCount("Damage Shield", -value);
                case 61:
                    return "Identify Item";
                case 63:
                    // +25 if over level 53, +(cha - 150)/10 max:15. so base is 40 + whatever the value is
                    //return String.Format("Memory Blur ({0})", value);
                    return String.Format("Memory Blur ({0}% Chance)", Math.Min(value + 40, 100));
                case 64:
                    if (base2 != base1 && base2 != 0)
                        return String.Format("Stun and Spin NPC for {0:0.##}s (PC for {1:0.##}s)", base1 / 1000f, base2 / 1000f) + absmax;
                    return String.Format("Stun and Spin for {0:0.##}s", base1 / 1000f) + absmax;
                case 65:
                    return "Infravision";
                case 66:
                    return "Ultravision";
                case 67:
                    return "Eye of Zomm";
                case 68:
                    return "Reclaim Pet Mana";
                case 69:
                    return Spell.FormatCount("Max HP", value) + range;
                case 71:
                    return String.Format("Summon Pet: {0}", Extra);
                case 73:
                    return "Bind Sight";
                case 74:
                    if (value < 100)
                        return String.Format("Feign Death ({0}% Chance)", value);
                    return "Feign Death";
                case 75:
                    return "Project Voice";
                case 76:
                    return "Sentinel";
                case 77:
                    return "Locate Corpse";
                case 78:
                    return String.Format("Absorb Spell Damage: 100%, Total: {0}", value);
                case 79:
                    // delta hp for heal/nuke, non repeating
                    if (base2 > 0)
                        return Spell.FormatCount("Current HP", value) + range + " (If " + Spell.FormatEnum((SpellTargetRestrict)base2) + ")";
                    return Spell.FormatCount("Current HP", value) + range;
                case 81:
                    return String.Format("Resurrect with {0}% XP", value);
                case 82:
                    // call of the hero
                    return "Summon Player";
                case 83:
                    return String.Format("Teleport to {0}", Extra);
                case 84:
                    return "Gravity Flux";
                case 85:
                    if (base2 != 0)
                        return String.Format("Add Melee Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                    return String.Format("Add Melee Proc: [Spell {0}]", base1);
                case 86:
                    return String.Format("Decrease Social Radius to {0}", value) + absmax;
                case 87:
                    return Spell.FormatPercent("Magnification", value);
                case 88:
                    return String.Format("Evacuate to {0}", Extra);
                case 89:
                    return Spell.FormatPercent("Player Size", base1 - 100);
                case 90:
                    // aka pet invis
                    return "Ignore Pet";
                case 91:
                    return String.Format("Summon Corpse up to level {0}", base1);
                case 92:
                    // calming strike spells are all capped at 100. so base1 would be more appropriate for those
                    // but most other hate spells seem to imply scaled value is used
                    return Spell.FormatCount("Hate", value);
                case 93:
                    return "Stop Rain";
                case 94:
                    return "Cancel if Combat Initiated";
                case 95:
                    return "Sacrifice";
                case 96:
                    // aka silence, but named this way to match melee version
                    return "Inhibit Spell Casting";
                case 97:
                    return Spell.FormatCount("Max Mana", value);
                case 98:
                    // yet another super turbo haste. only on 3 bard songs
                    return Spell.FormatPercent("Melee Haste v2", value - 100);
                case 99:
                    return "Root";
                case 100:
                    // heal over time
                    if (base2 > 0)
                        return Spell.FormatCount("Current HP", value) + repeating + range + " (If " + Spell.FormatEnum((SpellTargetRestrict)base2) + ")";
                    return Spell.FormatCount("Current HP", value) + repeating + range;
                case 101:
                    // only castable via Donal's BP. creates a buf that blocks recasting
                    return "Increase Current HP by 7500";
                case 102:
                    return "Fear Immunity";
                case 103:
                    return "Summon Pet";
                case 104:
                    return String.Format("Translocate to {0}", Extra);
                case 105:
                    return "Inhibit Gate";
                case 106:
                    return String.Format("Summon Warder: {0}", Extra);
                case 108:
                    if (base2 > 0)
                        return String.Format("Summon Familiar: {0} (Ignore Auto Leave)", Extra);
                    return String.Format("Summon Familiar: {0}", Extra);
                case 109:
                    return String.Format("Summon: [Item {0}]", base1);
                case 111:
                    return Spell.FormatCount("All Resists", value);
                case 112:
                    return Spell.FormatCount("Effective Casting Level", value);
                case 113:
                    return String.Format("Summon Mount: {0}", Extra);
                case 114:
                    return Spell.FormatPercent("Hate Generated", value);
                case 115:
                    return "Reset Hunger Counter";
                case 116:
                    return Spell.FormatCount("Curse Counter", value);
                case 117:
                    // fear me now wil o wisps
                    return "Make Weapon Magical";
                case 118:
                    return Spell.FormatCount("Singing Skill", value);
                case 119:
                    return Spell.FormatPercent("Melee Haste v3", value);
                case 120:
                    return Spell.FormatPercent("Healing Taken", base1) + " (Before Crit)"; // no min/max range
                case 121:
                    // damages the target whenever it hits something
                    return Spell.FormatCount("Reverse Damage Shield", -value);
                case 122:
                    return String.Format("Decrease {0} Skill ({1})", Spell.FormatEnum((SpellSkill)base1), calc);
                case 123:
                    return "Screech";
                case 124:
                    // crits for DoTs, but not DDs.
                    return Spell.FormatPercentRange("Spell Damage", base1, base2) + " (Before DoT Crit)";
                case 125:
                    return Spell.FormatPercentRange("Healing", base1, base2) + " (Before Crit)";
                case 126:
                    return Spell.FormatPercentRange("Spell Resist Rate", base1, base2, true);
                case 127:
                    return Spell.FormatPercent("Spell Haste", base1);
                case 128:
                    return Spell.FormatPercent("Spell Duration", base1);
                case 129:
                    return Spell.FormatPercent("Spell Range", base1);
                case 130:
                    // i think this affects all special attacks (bash/kick/frenzy/etc...) and unlike 114 it checks focus limit rules
                    return Spell.FormatPercentRange("Spell and Bash Hate", base1, base2);
                case 131:
                    return Spell.FormatPercentRange("Chance of Using Reagent", base1, base2, true);
                case 132:
                    return Spell.FormatPercentRange("Spell Mana Cost", base1, base2, true);
                case 134:
                    if (base2 == 0)
                        base2 = 100; // just to make it obvious that 0 means the focus stops functioning
                    return String.Format("Limit Max Level: {0} (lose {1}% per level)", base1, base2);
                case 135:
                    return String.Format("Limit Resist: {1}{0}", (SpellResist)Math.Abs(base1), base1 >= 0 ? "" : "Exclude ");
                case 136:
                    return String.Format("Limit Target: {1}{0}", Spell.FormatEnum((SpellTarget)Math.Abs(base1)), base1 >= 0 ? "" : "Exclude ");
                case 137:
                    return String.Format("Limit Effect: {1}{0}", Spell.FormatEnum((SpellEffect)Math.Abs(base1)), base1 >= 0 ? "" : "Exclude ");
                case 138:
                    return String.Format("Limit Type: {0}", base1 == 0 ? "Detrimental" : "Beneficial");
                case 139:
                    return String.Format("Limit Spell: {1}[Spell {0}]", Math.Abs(base1), base1 >= 0 ? "" : "Exclude ");
                case 140:
                    return String.Format("Limit Min Duration: {0}s", base1 * 6);
                case 141:
                    return String.Format("Limit Max Duration: {0}s", 0);
                case 142:
                    return String.Format("Limit Min Level: {0}", base1);
                case 143:
                    return String.Format("Limit Min Casting Time: {0}s", base1 / 1000f);
                case 144:
                    return String.Format("Limit Max Casting Time: {0}s", base1 / 1000f);
                case 145:
                    return String.Format("Teleport to {0}", Extra);
                case 146:
                    // teleport SPAs spread their location data over several slots using SPA 146
                    //return String.Format("Set position to {0}", base1);
                    return null;
                case 147:
                    //return String.Format("Increase Current HP by {1} Max: {0}% ", value, max);
                    return FormatPercent("Current HP", value) + String.Format(" up to {0}", max);
                case 148:
                    //if (max > 1000) max -= 1000;
                    return String.Format("Stacking: Block new spell if slot {0} is '{1}' and < {2}", calc % 100, Spell.FormatEnum((SpellEffect)base1), max);
                case 149:
                    //if (max > 1000) max -= 1000;
                    return String.Format("Stacking: Overwrite existing spell if slot {0} is '{1}' and < {2}", calc % 100, Spell.FormatEnum((SpellEffect)base1), max);
                case 150:
                    return String.Format("Divine Intervention with {0} Heal", max);
                case 151:
                    if (base1 == 1)
                        return "Suspend Pet with Buffs";
                    return "Suspend Pet";
                case 152:
                    return String.Format("Summon Pet: {0} x {1} for {2}s", Extra, base1, max);
                case 153:
                    return String.Format("Balance Group HP with {0}% Penalty", value);
                case 154:
                    // +0.5% per level difference
                    if (base2 != 0)
                        return String.Format("Decrease Detrimental Duration by 50% ({0}% Chance)", base1 / 10) + absmax;
                    return String.Format("Dispel Detrimental ({0}% Chance)", base1 / 10) + absmax;
                case 156:
                    return "Illusion: Target";
                case 157:
                    return Spell.FormatCount("Spell Damage Shield", -value);
                case 158:
                    if (max != 0)
                        return Spell.FormatPercent("Chance to Reflect Spell", base1) + String.Format(" with up to {0}% Damage", max);
                    return Spell.FormatPercent("Chance to Reflect Spell", base1);
                case 159:
                    return Spell.FormatCount("Base Stats", value);
                case 160:
                    return String.Format("Intoxicate if Tolerance under {0}", base1);
                case 161:
                    return String.Format("Absorb Spell Damage: {0}%", base1) + (base2 > 0 ? String.Format(", Max Per Hit: {0}", base2) : "") + (max > 0 ? String.Format(", Total: {0}", max) : "");
                case 162:
                    // reduces incoming melee damage by Base1% up to Base2 points of damage.
                    return String.Format("Absorb Melee Damage: {0}%", base1) + (base2 > 0 ? String.Format(", Max Per Hit: {0}", base2) : "") + (max > 0 ? String.Format(", Total: {0}", max) : "");
                case 163:
                    return String.Format("Absorb {0} Hits or Spells", base1) + (max > 0 ? String.Format(", Max Per Hit: {0}", max) : "");
                case 164:
                    return String.Format("Appraise Chest ({0})", value);
                case 165:
                    return String.Format("Disarm Chest ({0})", value);
                case 166:
                    return String.Format("Unlock Chest ({0})", value);
                case 167:
                    return String.Format("Increase Pet Power ({0})", value);
                case 168:
                    // defensive disc. how is this different than an endless rune?
                    // maybe it only mitigates the DI portion of the hit?
                    return Spell.FormatPercent("Melee Mitigation", -value);
                case 169:
                    if ((SpellSkill)base2 != SpellSkill.Hit)
                        return Spell.FormatPercent("Chance to Critical Hit with " + Spell.FormatEnum((SpellSkill)base2), value);
                    return Spell.FormatPercent("Chance to Critical Hit", value);
                case 170:
                    // stacks with itself in other slots
                    return Spell.FormatPercent("Critical Nuke Damage", base1) + " of Base Damage";
                case 171:
                    return Spell.FormatPercent("Chance to Crippling Blow", value);
                case 172:
                    // combat agility AA
                    return Spell.FormatPercent("Chance to Avoid Melee", base1);
                case 173:
                    return Spell.FormatPercent("Chance to Riposte", value);
                case 174:
                    return Spell.FormatPercent("Chance to Dodge", value);
                case 175:
                    return Spell.FormatPercent("Chance to Parry", value);
                case 176:
                    return Spell.FormatPercent("Chance to Dual Wield", value);
                case 177:
                    // this is multiplicative
                    return Spell.FormatPercent("Chance to Double Attack", base1);
                case 178:
                    if (Version != 0 && Version < 20160816)
                        return String.Format("Lifetap from Weapon Damage: {0}%", base1);

                    return String.Format("Lifetap from Weapon Damage: {0}%", base1 / 10f);
                // this has been renamed to resource tap. how does it differ from 457?
                //return string.Format("Return {0}% of Damage as {1}", base1 / 10f, new[] { "HP", "Mana", "Endurance" }[base2 % 3]);
                case 179:
                    return String.Format("Instrument Modifier: {0} {1}", Skill, value);
                case 180:
                    // devs call this Sanctification
                    // mystical shielding AA is 5%, fervor of the dark reign / sanctity of the keepers is 2%.
                    return Spell.FormatPercent("Chance to Resist Spell", value);
                case 181:
                    return Spell.FormatPercent("Chance to Resist Fear Spell", value);
                case 182:
                    // hundred hands effect. how is this different than 371?
                    return Spell.FormatPercent("Weapon Delay", base1 / 10f);
                case 183:
                    // according to prathun this effect does nothing
                    //return Spell.FormatPercent("Skill Check for " + Spell.FormatEnum((SpellSkill)base2), value);
                    return null;
                case 184:
                    if ((SpellSkill)base2 != SpellSkill.Hit)
                        return Spell.FormatPercent("Chance to Hit with " + Spell.FormatEnum((SpellSkill)base2), value);
                    return Spell.FormatPercent("Chance to Hit", value);
                case 185:
                    return Spell.FormatPercent(Spell.FormatEnum((SpellSkill)base2) + " Damage", value);
                case 186:
                    return Spell.FormatPercent("Min " + Spell.FormatEnum((SpellSkill)base2) + " Damage", value); // only DI1?
                case 187:
                    return String.Format("Balance Group Mana with {0}% Penalty", value);
                case 188:
                    return Spell.FormatPercent("Chance to Block", value);
                case 189:
                    return Spell.FormatCount("Current Endurance", value) + repeating + range;
                case 190:
                    return Spell.FormatCount("Max Endurance", value);
                case 191:
                    // melee and special skills
                    return "Inhibit Combat";
                case 192:
                    return Spell.FormatCount("Hate", value) + repeating + range;
                case 193:
                    //if (max != 0)
                    //    return String.Format("{0} Attack for {1} ({2} in PvP) with {3}% Accuracy Mod", Spell.FormatEnum(Skill), base1, max, base2);
                    return String.Format("{0} Attack for {1} with {2}% Accuracy Mod", Spell.FormatEnum(Skill), base1, base2);
                case 194:
                    // aka Fade
                    // if successful and target is outside the hardcoded 30' distance restriction the linked spell will be cast
                    // base=75 was invalid data before base2 was used as a spell id link (ignored in case we are parsing an old spell file)
                    if (base2 > 0 && base2 != 75)
                        return String.Format("Cancel Aggro {2} ({0}% Chance) and Cast: [Spell {1}] on Success", base1, base2, varmax);
                    return String.Format("Cancel Aggro {1} ({0}% Chance)", base1, varmax);
                case 195:
                    // melee + spell
                    // 100 is full resist. not sure why some spells have more
                    return Spell.FormatPercent("Chance to Resist Any Stun", base1);
                case 196:
                    // no longer used
                    return String.Format("Srikethrough ({0})", value);
                case 197:
                    return Spell.FormatPercent(Spell.FormatEnum((SpellSkill)base2) + " Damage Taken", value);
                case 198:
                    return Spell.FormatCount("Current Endurance", value);
                case 199:
                    // base1 is the success rate but only the mercenary taunt is not 100%
                    // base2 is the extra aggro at the top of the hate list
                    return String.Format("Taunt with {0} Hate Mod", base2);
                case 200:
                    // affects worn melee/range weapon procs
                    // doesn't affect 85, 429. pretty sure 201 is also unaffected
                    return Spell.FormatPercent("Worn Proc Rate", base1);
                case 201:
                    return String.Format("Add Range Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                case 202:
                    return "Project Illusion on Next Spell";
                case 203:
                    return "Mass Group Buff on Next Spell";
                case 204:
                    return String.Format("Group Fear Immunity for {0}s", base1 * 10);
                case 205:
                    // strike everything in a radius with a single primary hand combat round
                    return String.Format("Rampage ({0})", base1);
                case 206:
                    // places you [base1] points of hate higher than all of the taunted targets around you - Dzarn
                    return String.Format("AE Taunt with {0} Hate Mod", base1);
                case 207:
                    return "Flesh to Bone Chips";
                case 209:
                    // +0.5% per level difference
                    if (base2 != 0)
                        return String.Format("Decrease Beneficial Duration by 50% ({0}% Chance)", base1 / 10) + absmax;
                    return String.Format("Dispel Beneficial ({0}% Chance)", base1 / 10) + absmax;
                case 210:
                    return String.Format("Pet Shielding for {0}s", base1 * 12);
                case 211:
                    // % chance applies individually to each mob in radius
                    return Spell.FormatPercent("Chance to AE Attack", base1) + (base2 != 100 ? String.Format(" with {0}% Damage", base2) : "");
                case 212:
                    return Spell.FormatPercent("Chance to Critical Nuke", base1) + " and " + Spell.FormatPercent("Spell Mana Cost v2", base2);
                case 213:
                    return Spell.FormatPercent("Pet Max HP", base1);
                case 214:
                    return Spell.FormatPercent("Max HP", base1 / 100f);
                case 215:
                    return Spell.FormatPercent("Pet Chance to Avoid Melee", base1);
                case 216:
                    // should be a count rather than a percent (it's always called "points")
                    if ((SpellSkill)base2 != SpellSkill.Hit)
                        return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Accuracy", value);
                    return Spell.FormatCount("Accuracy", value);
                case 217:
                    return String.Format("Add Headshot Proc with up to {0} Damage", base2);
                case 218:
                    return Spell.FormatPercent("Pet Chance to Critical Hit", value);
                case 219:
                    // Gives [Base1 <= 10000] chance to do (damage * [Base2/100]) to undead targets
                    // [Properties: 3, 8, 12] on a critical hit. For stacking, Base1 is cumulative while Base2 
                    // takes the highest value between spells or AAs. If successful, earlies out of the crit 
                    // melee function before SPA 330 or 170 are considered. - Dzarn
                    return Spell.FormatPercent("Chance to Slay Undead", base1 / 100f) + String.Format(" with {0} Damage Mod", base2);
                case 220:
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Damage Bonus", base1);
                case 221:
                    return Spell.FormatPercent("Weight", -base1);
                case 222:
                    return Spell.FormatPercent("Chance to Block from Back", base1);
                case 223:
                    return Spell.FormatPercent("Chance to Double Riposte", base1);
                case 224:
                    if (base2 > 0)
                        return Spell.FormatPercent("Chance of Additional Riposte with " + Spell.FormatEnum((SpellSkill)base2), base1);
                    return Spell.FormatPercent("Chance of Additional Riposte", base1);
                case 225:
                    // this is additive. 100 = 100% chance
                    return Spell.FormatPercent("Chance to Double Attack", base1) + " (Additive)";
                case 226:
                    // allows bash while weilding a 2h weapon
                    return "Add Two-Handed Bash Ability";
                case 227:
                    return String.Format("Decrease {0} Timer by {1}", Spell.FormatEnum((SpellSkill)base2), FormatTime(base1));
                case 228:
                    return Spell.FormatPercent("Falling Damage", -base1);
                case 229:
                    return Spell.FormatPercent("Chance to Cast Through Stun", base1);
                case 230:
                    // warrior /shield ability
                    return Spell.FormatPercent("Shielding Range", base1);
                case 231:
                    return Spell.FormatPercent("Chance to Stun Bash", base1);
                case 232:
                    return Spell.FormatPercent("Chance to Trigger Divine Intervention", base1);
                case 233:
                    return Spell.FormatPercent("Food Consumption", -base1);
                case 234:
                    return String.Format("Decrease Poison Application Time by {0}s", 10f - base1 / 1000f);
                case 238:
                    if (base1 == 3)
                        return "Permanent Illusion (Persist After Death)";
                    return "Permanent Illusion";
                case 237:
                    return "Enable Pet Ability: Receive Group Buffs";
                case 239:
                    return Spell.FormatPercent("Chance to Feign Death Through Spell Hit", base1);
                case 241:
                    // returns a random amount of mana spent to summon the pet
                    return String.Format("Reclaim Pet Mana (Return 75% to {0}%)", base1);
                case 242:
                    return Spell.FormatPercent("Chance to Memory Blur", base1);
                case 243:
                    return Spell.FormatPercent("Chance of Charm Breaking", -base1);
                case 244:
                    return Spell.FormatPercent("Chance of Root Breaking", -100 + base1);
                case 245:
                    return Spell.FormatPercent("Chance of Trap Circumvention", base1);
                case 246:
                    return Spell.FormatCount("Lung Capacity", base1);
                case 247:
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Skill Cap", base1);
                case 248:
                    // ability to train spell skills over 200 is limited to 1
                    return Spell.FormatCount("Magic Specialization Ability", base1);
                case 249:
                    // 1-Handed weapon - Primary Hand: (damage * delay * level * 80) / 400000
                    // 1-Handed weapon - Secondary Hand: [Primary Hand Formula * Base1 of Sinister Strike SPA]
                    return Spell.FormatPercent("Offhand Weapon Damage Bonus", base1);
                case 250:
                    // increase chance of spa 85 
                    // compare with 200 which increase worn proc rate
                    return Spell.FormatPercent("Melee Proc Rate", base1);
                case 251:
                    // endless quiver AA
                    return Spell.FormatPercent("Chance of Using Ammo", -base1);
                case 252:
                    return Spell.FormatPercent("Chance to Backstab From Front", base1);
                case 253:
                    return String.Format("Chaotic Stab ({0})", base1);
                case 255:
                    return String.Format("Increase Shielding Duration by {0}", FormatTime(base1));
                case 256:
                    // Invisibility, and its counter, see-invisibility are ranked.
                    // Standard invisibility is 1, existing Shroud of Stealth is 2, new Shroud of Stealth is 3. -Dzarn
                    return String.Format("Shroud of Stealth ({0})", base1);
                case 257:
                    // no longer used
                    return "Enable Pet Ability: Hold";
                case 258:
                    return Spell.FormatPercent("Chance to Triple Backstab", value);
                case 259:
                    // combat stability AA
                    return Spell.FormatPercent("AC Soft Cap", base1);
                case 260:
                    return String.Format("Instrument Modifier: {0} {1}", Spell.FormatEnum((SpellSkill)base2), value);
                case 262:
                    // affects worn cap
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkillCap)base2) + " Cap", value);
                case 263:
                    // ability to train tradeskills over 200 is limited to 1 by default
                    return Spell.FormatCount("Ability to Specialize Tradeskills", base1);
                case 264:
                    return String.Format("Reduce [AA {0}] Timer by {1}", base2, FormatTime(base1));
                case 265:
                    // value of zero should negate effects of Mastery of the Past
                    return String.Format("No Fizzle up to level {0}", base1);
                case 266:
                    // only works when first attack is successful?
                    return Spell.FormatPercent("Chance of Additional 2H Attack", base1);
                case 267:
                    // 2 = /pet attack
                    // 3 = /pet qattack
                    // 4 = /pet guard?
                    // 5 = /pet follow?
                    // 34 = /pet swarm - like /pet attack for swarm pets
                    // 35 = /pet qswarm
                    return String.Format("Enable Pet Ability: {0}", base2);
                case 268:
                    return Spell.FormatPercent("Chance to Fail " + Spell.FormatEnum((SpellSkill)base2) + " Combine", -base1);
                case 269:
                    return Spell.FormatPercent("Bandage HP Cap", base1);
                case 270:
                    return Spell.FormatCount("Beneficial Song Range", base1);
                case 271:
                    // each 0.7 points seems to equal 10% normal run speed
                    return Spell.FormatPercent("Innate Movement Speed", base1 / 0.7f);
                case 272:
                    return Spell.FormatPercent("Song Casting Skill", value);
                case 273:
                    if (base2 == 0)
                        base2 = 100;
                    if (max > 0)
                        absmax += String.Format(" (lose {0}% per level)", base2);
                    return Spell.FormatPercent("Chance to Critical DoT", base1) + absmax;
                case 274:
                    if (base2 == 0)
                        base2 = 100;
                    if (max > 0)
                        absmax += String.Format(" (lose {0}% per level)", base2);
                    return Spell.FormatPercent("Chance to Critical Heal", base1) + absmax;
                case 275:
                    return Spell.FormatPercent("Chance to Critical Mend", base1);
                case 276:
                    return String.Format("Dual Wield Amount ({0})", base1);
                case 277:
                    return Spell.FormatPercent("Chance to Trigger Divine Intervention", base1);
                case 278:
                    return String.Format("Add Finishing Blow Proc ({0}% Chance) with up to {1} Damage", base1 / 10, base2);
                case 279:
                    return Spell.FormatPercent("Chance to Flurry", value);
                case 280:
                    return Spell.FormatPercent("Pet Chance to Flurry", value);
                case 281:
                    return Spell.FormatPercent("Pet Chance to Feign Death", base1);
                case 282:
                    return Spell.FormatPercent("Bandage Amount", base1);
                case 283:
                    // only special monks attack skills?
                    return Spell.FormatPercent("Chance to Double Special Attack", base1);
                case 285:
                    return Spell.FormatPercent("Chance to Evade", base1);
                case 286:
                    // is added after all other multipliers (focus, crit, etc..)
                    // for DoTs it adds base1/ticks to each tick.
                    // SPA 286 and 303 work the same way but 286 does not crit and 303 does. - Beimeith
                    return Spell.FormatCount("Spell Damage", base1) + " (After Crit)";
                case 287:
                    return String.Format("Increase Duration by {0}s", base1 * 6);
                case 288:
                    // this procs the spell associated with the AA
                    // the rate on this seems to be an absolute %
                    return String.Format("Add " + Spell.FormatEnum((SpellSkill)base2) + " Proc ({1}% Chance)", base2, base1 / 10f);
                case 289:
                    // this only triggers if the spell times out. compare with 373
                    // 2017-04-19 set max=2 on some spells (e.g. Chill of the Visionary) 
                    return String.Format("Cast: [Spell {0}] on Duration Fade", base1);
                case 290:
                    return Spell.FormatCount("Movement Speed Cap", value);
                case 291:
                    return String.Format("Purify ({0})", value);
                case 292:
                    // increases the chance you will strike through your opponent's active defenses, such as dodge, block, parry, and riposte.
                    return Spell.FormatPercent("Chance of Strikethrough", base1);
                case 293:
                    // melee stun only, from any angle
                    return Spell.FormatPercent("Chance to Resist Melee Stun", base1);
                case 294:
                    // the base2 nuke damage increase only appears on 4 spells after the 2015-7-22 patch 
                    if (base2 > 0)
                        return Spell.FormatPercent("Chance to Critical Nuke", base1) + " and " + Spell.FormatPercent("Critical Nuke Damage v2", base2) + " of Base Damage";
                    else
                        return Spell.FormatPercent("Chance to Critical Nuke", base1);
                case 296:
                    // incoming damage % SPAs 296 and 483 multiply against what is essentially (spell-data's base value * spa 413) 
                    // rather than the focused value - Dzarn
                    return Spell.FormatPercentRange("Spell Damage Taken", base1, base2) + " (Before Crit)";
                case 297:
                    // doesn't use focused value
                    return Spell.FormatCount("Spell Damage Taken", base1) + " (Before Crit)";
                case 298:
                    return Spell.FormatPercent("Pet Size", value - 100);
                case 299:
                    return String.Format("Wake the Dead ({0})", max);
                case 300:
                    return "Summon Doppelganger: " + Extra;
                case 301:
                    return Spell.FormatPercent("Archery Damage", base1);
                case 302:
                    // see also 124. only used on a few AA (like chromatic haze)
                    return Spell.FormatPercentRange("Spell Damage", base1, base2) + " (Before Crit)";
                case 303:
                    // is added before crit multipliers, but after SPA 296 and 302 (and maybe 124)?
                    // for DoTs it adds base1/ticks to each tick.
                    // SPA 286 and 303 work the same way but 286 does not crit and 303 does. - Beimeith
                    return Spell.FormatCount("Spell Damage", base1) + " (Before Crit)";
                case 304:
                    // this may just be chance to avoid offhand riposte
                    return Spell.FormatPercent("Chance to Avoid Offhand Riposte", -base1);
                case 305:
                    return Spell.FormatPercent("Offhand Damage Shield Taken", base1);
                case 306:
                    return String.Format("Wake the Dead: {0} x {1} for {2}s", Extra, base1, max);
                case 307:
                    return "Appraisal";
                case 308:
                    return "Suspend Minion";
                case 309:
                    return "Teleport to Caster's Bind";
                case 310:
                    return String.Format("Reduce Timer by {0}", FormatTime(base1 / 1000f));
                case 311:
                    // filter based on field 168 
                    return String.Format("Limit Type: {0} Combat Skills", base1 == 1 ? "Include" : "Exclude");
                case 312:
                    return "Sanctuary";
                case 313:
                    return Spell.FormatPercent("Chance to Double Forage", base1);
                case 314:
                    return "Invisibility";
                case 315:
                    return "Invisibility to Undead";
                case 316:
                    return "Invisibility to Animals";
                case 317:
                    return Spell.FormatCount("HP Regen Cap", base1);
                case 318:
                    return Spell.FormatCount("Mana Regen Cap", base1);
                case 319:
                    if (base2 == 0)
                        base2 = 100;
                    if (max > 0)
                        absmax += String.Format(" (lose {0}% per level)", base2);
                    return Spell.FormatPercent("Chance to Critical HoT", base1) + absmax;
                case 320:
                    // % chance to block incoming melee attacks with your shield
                    return Spell.FormatPercent("Shield Block Chance", base1);
                case 321:
                    return Spell.FormatCount("Target's Target Hate", -value);
                case 322:
                    return "Gate to Home City";
                case 323:
                    // max may be some sort of level limit for reducing the proc rate
                    if (base2 != 0)
                        return String.Format("Add Defensive Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                    return String.Format("Add Defensive Proc: [Spell {0}]", base1);
                case 324:
                    // blood magic. uses HP as mana
                    return String.Format("Cast from HP with {0}% Penalty", value);
                case 325:
                    return Spell.FormatPercent("Chance to Remain Hidden When Hit By AE", base1);
                case 326:
                    return Spell.FormatCount("Spell Memorization Gems", base1);
                case 327:
                    return Spell.FormatCount("Buff Slots", base1);
                case 328:
                    return Spell.FormatCount("Max Negative HP", value);
                case 329:
                    return String.Format("Absorb Damage using Mana: {0}%", value);
                case 330:
                    // additive with innate crit multiplier
                    return Spell.FormatPercent("Critical " + Spell.FormatEnum((SpellSkill)base2) + " Damage", base1) + " of Base Damage";
                case 331:
                    return Spell.FormatPercent("Chance to Salvage Components", value);
                case 332:
                    return "Summon to Corpse";
                case 333:
                    // so far this is only used on spells that have a rune
                    return String.Format("Cast: [Spell {0}] on Rune Fade", base1);
                case 334:
                    return Spell.FormatCount("Current HP", value) + repeating + range + " (If Target Not Moving)";
                case 335:
                    // block next spell that matches limits
                    if (base1 < 100)
                        return String.Format("Block Next Spell ({0}% Chance)", base1);
                    return "Block Next Spell";
                case 337:
                    return Spell.FormatPercent("Experience Gain", value);
                case 338:
                    return "Summon and Resurrect All Corpses";
                case 339:
                    // compare with 383 where chance is modified by casting time
                    return String.Format("Cast: [Spell {0}] on Spell Use ({1}% Chance)", base2, base1);
                case 340:
                    // when a spell has multiple 340 slots, only one has a chance to cast
                    if (base1 < 100)
                        return String.Format("Cast: [Spell {0}] ({1}% Chance)", base2, base1);
                    return String.Format("Cast: [Spell {0}]", base2);
                case 341:
                    return Spell.FormatCount("ATK Cap", base1);
                case 342:
                    return "Inhibit Low Health Fleeing";
                case 343:
                    if (base1 < 100)
                        return String.Format("Interrupt Casting ({0}% Chance)", base1);
                    return "Interrupt Casting";
                case 344:
                    return Spell.FormatPercent("Chance to Channel Item Procs", base1);
                case 345:
                    return String.Format("Limit Assassinate Level: {0} ({1})", base1, base2);
                case 346:
                    return String.Format("Limit Headshot Level: {0} ({1})", base1, base2);
                case 347:
                    return Spell.FormatPercent("Chance of Double Archery Attack", base1);
                case 348:
                    return String.Format("Limit Min Mana Cost: {0}", base1);
                case 349:
                    // increases weapon damage when a shield is equiped
                    return Spell.FormatPercent("Damage When Shield Equiped", base1);
                case 350:
                    if (base1 > Mana)
                        Mana = base1; // hack
                    return String.Format("Mana Burn up to {0} Damage", base1 * -base2 / 10);
                case 351:
                    // the actual aura spell effect reference doesn't seem to be stored in the spell file so we have to handle this SPA
                    // with guesses and some hardcoding. most of the time the effect is placed right after the aura in the spell file
                    int aura = (Rank >= 1) || Extra.Contains("Rk") ? ID + 3 : ID + 1;
                    // hardcoded fixes for failed guesses
                    if (ID == 8629) aura = 8628;
                    if (ID == 8921) aura = 8935;
                    if (ID == 8922) aura = 8936;
                    if (ID == 8923) aura = 8937;
                    if (ID == 8924) aura = 8959;
                    if (ID == 8925) aura = 8938;
                    if (ID == 8926) aura = 8939;
                    if (ID == 8654) aura = 8649;
                    if (ID == 8928) aura = 8940;
                    if (ID == 8929) aura = 8943;
                    if (ID == 8930) aura = 8945;
                    if (ID == 8931) aura = 8946;
                    if (ID == 8932) aura = 8947;
                    if (ID == 8933) aura = 8948;
                    if (ID == 8934) aura = 8949;
                    if (ID == 9000) aura = 9001;
                    if (ID == 9002) aura = 9003;
                    if (ID == 9004) aura = 9005;
                    if (ID == 9006) aura = 9007;
                    if (ID == 9008) aura = 9009;
                    if (ID == 9010) aura = 9011;
                    if (ID == 9012) aura = 9013;
                    if (ID == 9014) aura = 9015;
                    if (ID == 11519) aura = 11539;
                    if (ID == 11520) aura = 11538;
                    if (ID == 11521) aura = 11551;
                    if (ID == 11523) aura = 11540;
                    if (ID == 21827) aura = 21848;
                    if (ID == 32007) aura = 31993;
                    if (ID == 32271) aura = 32257;
                    if (ID == 22510) aura = 22574;
                    if (ID == 22511) aura = 22575;
                    // these 3 auras have different effects on normal and swarm pets
                    if (ID == 49678) aura = 49700;
                    if (ID == 49679) aura = 49701;
                    if (ID == 49680) aura = 49702;
                    //if (ID == 49678) aura = 49736;
                    //if (ID == 49679) aura = 49737;
                    //if (ID == 49680) aura = 49738;
                    
                    if (Extra.StartsWith("IOQuicksandTrap85")) aura = 22655;
                    if (Extra.StartsWith("IOAuraCantataRk")) aura = 19713 + Rank;
                    if (Extra.StartsWith("IORogTrapAggro92Rk")) aura = 26111 + Rank;
                    if (Extra.StartsWith("IOEncEchoProc95Rk")) aura = 30179 + Rank;
                    if (Extra.StartsWith("IOEncEchoProc100Rk")) aura = 36227 + Rank;
                    if (Extra.StartsWith("IOEncEchoProc105Rk")) aura = 45018 + Rank;

                    // 2017-4-19 patch renamed auras
                    if (Extra.StartsWith("PCIObMagS17L085TrapPetAug")) aura = 22655;
                    if (Extra.StartsWith("PCIObBrdS17L082AuraRegenRk")) aura = 19713 + Rank;
                    if (Extra.StartsWith("PCIObRogS19L092TrapAggroRk")) aura = 26111 + Rank;
                    if (Extra.StartsWith("PCIObEncS19L095EchoCastProcRk")) aura = 30179 + Rank;
                    if (Extra.StartsWith("PCIObEncS20L100EchoCastProcRk")) aura = 36227 + Rank;
                    if (Extra.StartsWith("PCIObEncS21L105EchoCastProcRk")) aura = 45018 + Rank;


                    return String.Format("Aura Effect: [Spell {0}] ({1})", aura, Extra);
                case 353:
                    return Spell.FormatCount("Aura Slots", base1);
                case 357:
                    // similar to 96, but i think this prevents casting of spells matching limits
                    return "Inhibit Spell Casting";
                case 358:
                    return Spell.FormatCount("Current Mana", value) + range;
                case 359:
                    return Spell.FormatPercent("Chance to Sense Trap", base1);
                case 360:
                    return String.Format("Add Killshot Proc: [Spell {0}] ({1}% Chance)", base2, base1);
                case 361:
                    return String.Format("Cast: [Spell {0}] on Death ({1}% Chance)", base2, base1);
                case 362:
                    return Spell.FormatCount("Potion Belt Slots", base1);
                case 363:
                    return Spell.FormatCount("Bandolier Slots", base1);
                case 364:
                    return Spell.FormatPercent("Chance to Triple Attack", value);
                case 365:
                    // included on nukes and triggered when the nuke kills the mob
                    return String.Format("Cast: [Spell {0}] on Killshot ({1}% Chance)", base2, base1);
                case 367:
                    return String.Format("Transform Body Type to {0}", FormatEnum((SpellBodyType)base1));
                case 368:
                    return Spell.FormatCount("Faction with " + FormatEnum((SpellFaction)base1), base2);
                case 369:
                    return Spell.FormatCount("Corruption Counter", value);
                case 370:
                    return Spell.FormatCount("Corruption Resist", value);
                case 371:
                    // only used on slows - this lowers haste by a relative amount unlike other slows 
                    // which cancel haste effects and use 100 - slow amount as the new attack speed
                    return Spell.FormatPercent("Melee Haste v4", -value) + " (Incremental)";
                case 372:
                    return Spell.FormatCount("Forage Skill Cap", base1);
                case 373:
                    // this appears to be used when a spell is removed via any method: times out, cured, rune depleted, max hits, mez break, etc...
                    // devs call this a "doom" effect
                    return String.Format("Cast: [Spell {0}] on Fade", base1);
                case 374:
                    // when a spell has multiple 374 slots, each one has a chance to cast
                    // very few spells have base1 < 100
                    if (base1 < 100)
                        return String.Format("Cast: [Spell {0}] ({1}% Chance)", base2, base1);
                    return String.Format("Cast: [Spell {0}]", base2);
                case 375:
                    // additive with innate crit multiplier and same effect in other slots
                    return Spell.FormatPercent("Critical DoT Damage", base1) + " of Base Damage";
                case 376:
                    return "Fling";
                case 377:
                    return String.Format("Cast: [Spell {0}] if Not Cured", base1);
                case 378:
                    return Spell.FormatPercent("Chance to Resist " + Spell.FormatEnum((SpellEffect)base2) + " Effects", base1);
                case 379:
                    if (base2 == 0)
                        return String.Format("Shadowstep Forward {0}'", base1);
                    if (base2 == 90)
                        return String.Format("Shadowstep Right {0}'", base1);
                    if (base2 == 180)
                        return String.Format("Shadowstep Back {0}'", base1);
                    if (base2 == 270)
                        return String.Format("Shadowstep Left {0}'", base1);
                    return String.Format("Shadowstep {0}' and {1} Degrees", base1, base2);
                case 380:
                    return String.Format("Push Back {0}' and Up {1}'", base2, base1);
                case 381:
                    return String.Format("Fling to Self ({0}' away)", base1) + varmax;
                case 382:
                    return String.Format("Inhibit Effect: {0}", Spell.FormatEnum((SpellEffect)base2), base2);
                case 383:
                    // chance % modified by the cast time of the spell cast that triggers the proc, whereas 339 is not
                    // according to Beimeith:
                    // Cast Time < 2.5 then multiplier = 0.25
                    // Cast Time > 2.5 and < 7 then multiplier = 0.167 * (Cast Time - 1)
                    // Cast Time > 7 then multiplier = 1 * Cast Time / 7
                    string sample383 = String.Format(" e.g. Cast Time 2s={0}% 3s={1:F1}% 4s={2:F1}% 5s={3:F1}%", 0.25 * (base1 / 10), 0.334 * (base1 / 10), 0.5 * (base1 / 10), 0.668 * (base1 / 10));
                    return String.Format("Cast: [Spell {0}] on Spell Use (Base1={1})", base2, base1 / 10) + sample383;
                case 384:
                    return "Fling to Target";
                case 385:
                    return String.Format("Limit Spells: {1}[Group {0}]", Math.Abs(base1), base1 >= 0 ? "" : "Exclude ");
                case 386:
                    return String.Format("Cast: [Spell {0}] on Curer", base1);
                case 387:
                    return String.Format("Cast: [Spell {0}] if Cured", base1);
                case 388:
                    return "Summon All Corpses (From Any Zone)";
                case 389:
                    return "Reset Recast Timers";
                case 390:
                    return "Lockout Recast Timers";
                case 391:
                    return String.Format("Limit Max Mana Cost: {0}", base1);
                case 392:
                    // 392 is the nocrit version and 396 is the crit version. - Beimeith
                    return Spell.FormatCount("Healing", base1) + " (After Crit)";
                case 393:
                    // mostly used on war/pal self discs
                    // like 120 which is before crit - maybe this is after crit?
                    return Spell.FormatPercentRange("Healing Taken", base1, base2);
                case 394:
                    return Spell.FormatCount("Healing Taken", base1) + " (Before Crit)";
                case 395:
                    return Spell.FormatPercentRange("Healing", base1, base2) + " (Before Crit)";
                case 396:
                    // 392 is the nocrit version and 396 is the crit version. - Beimeith
                    return Spell.FormatCount("Healing", base1) + " (Before Crit)";
                case 397:
                    // will use the player AC formula for comparison with buffs but maybe they aren't comparable?
                    return Spell.FormatCount("Pet AC", (int)(value / (10f / 3f)));
                case 398:
                    return String.Format("Increase Pet Duration by {0}s", base1 / 1000);
                case 399:
                    return Spell.FormatPercent("Chance to Twincast", value);
                case 400:
                    // e.g. Channels the power of sunlight, consuming up to #1 mana to heal your group.
                    // this effect doesn't cause hate - it was added to divine arb as a non-aggro heal
                    Mana = base1; // a bit misleading since the spell will cast with 0 mana and scale the heal
                    Target = SpellTarget.Caster_Group; // total hack but makes sense for current spells
                    return String.Format("Increase Current HP by up to {0} ({1} HP per 1 Mana)", Math.Floor(base1 * base2 / 10f), base2 / 10f);
                case 401:
                    return String.Format("Decrease Current HP by up to {0} ({1} HP per 1 Target Mana)", Math.Floor(base1 * base2 / -10f), base2 / -10f);
                case 402:
                    return String.Format("Decrease Current HP by up to {0} ({1} HP per 1 Target End)", Math.Floor(base1 * base2 / -10f), base2 / -10f);
                case 403:
                    return String.Format("Limit Spell Class: {0}{1}", base1 >= 0 ? "" : "Exclude ", Spell.FormatEnum((SpellCategory)Math.Abs(base1)));
                case 404:
                    return String.Format("Limit Spell Subclass: {0}{1}", base1 >= 0 ? "" : "Exclude ", Math.Abs(base1));
                case 405:
                    return Spell.FormatPercent("Staff Block Chance", base1);
                case 406:
                    // 2017-04-19 set max=1 on some spells (e.g. Bosquestalker's Alliance) 
                    // perhaps this affects if the target or the caster is credited with the spell?
                    return String.Format("Cast: [Spell {0}] if Max Hits Used", base1);
                case 407:
                    // this is a guess. haven't tested this
                    return String.Format("Cast: [Spell {0}] if Hit By Spell", base1);
                case 408:
                    // target will still have normal max HP but cannot be regen/heal past the cap
                    if (base2 > 0)
                        return String.Format("Cap HP at lowest of {0}% or {1}", base1, base2);
                    return String.Format("Cap HP at {0}%", base1);
                case 409:
                    if (base2 > 0)
                        return String.Format("Cap Mana at lowest of {0}% or {1}", base1, base2);
                    return String.Format("Cap Mana at {0}%", base1);
                case 410:
                    if (base2 > 0)
                        return String.Format("Cap Endurance at lowest of {0}% or {1}", base1, base2);
                    return String.Format("Cap Endurance at {0}%", base1);
                case 411:
                    return String.Format("Limit Class: {0}", (SpellClassesMask)(base1 >> 1));
                case 412:
                    return String.Format("Limit Race: {0}", base1);
                case 413:
                    // this gets applied before all other additive/multiplicative effects
                    return Spell.FormatPercent("Base Spell Effectiveness", value);
                case 414:
                    return String.Format("Limit Casting Skill: {0}", Spell.FormatEnum((SpellSkill)base1));
                case 416:
                    // SPA 416 functions exactly like SPA 1, it was added so that we could avoid stacking conflicts with only 12 spell slots. - Dzarn
                    return Spell.FormatCount("AC v2", (int)(value / (10f / 3f)));
                case 417:
                    // same as 15 and used for stacking
                    return Spell.FormatCount("Current Mana v2", value) + repeating + range;
                case 418:
                    // same as 220 and used for stacking
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Damage Bonus v2", base1);
                case 419:
                    // this is used for potions. how is it different than 85? maybe proc rate?
                    if (base2 != 0)
                        return String.Format("Add Melee Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                    return String.Format("Add Melee Proc: [Spell {0}]", base1);
                case 421:
                    return Spell.FormatCount("Max Hits Counter", base1);
                case 422:
                    return String.Format("Limit Max Hits Min: {0}", base1);
                case 423:
                    return String.Format("Limit Max Hits Type: {0}", Spell.FormatEnum((SpellMaxHits)base1));
                case 424:
                    return String.Format("Gradual {0} to {2}' away (Force={1})", base1 > 0 ? "Push" : "Pull", Math.Abs(base1), base2) + varmax;
                case 425:
                    return "Fly";
                case 426:
                    return Spell.FormatCount("Extended Target Window Slots", base1);
                case 427:
                    // not sure how this works. base2 / 10 doesn't seem to be the correct chance.
                    // raising base2 increases the frequency of the cast.
                    // it may only have an opportunity to fire once a round or maybe once per some timespan?
                    return String.Format("Cast: [Spell {0}] on Skill Use ({1})", base1, base2);
                case 428:
                    return String.Format("Limit Skill: {0}", Spell.FormatEnum((SpellSkill)base1));
                case 429:
                    if (base2 != 0)
                        return String.Format("Add Skill Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                    return String.Format("Add Skill Proc: [Spell {0}]", base1);
                case 430:
                    return String.Format("Alter Vision: Base1={0} Base2={1} Max={2}", base1, base2, max);
                case 431:
                    // base1 tints
                    // base2 blurs?
                    if (base1 < 0)
                        return String.Format("Tint Vision: Red={0} Green={1} Blue={2}", base1 >> 16 & 0xff, base1 >> 8 & 0xff, base1 & 0xff);
                    return String.Format("Alter Vision: Base1={0} Base2={1} Max={2}", base1, base2, max);
                case 432:
                    return Spell.FormatCount("Trophy Slots", base1);
                case 433:
                    // similar to 220 except the values get lowered with faster weapons 
                    // Dzarn: When calculating the delay associated to SPA 433:
                    // Weapon skills: Weapon * Haste
                    // Skill attacks: Hasted delay of the button
                    // SPA 193: 30 second delay is assumed
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Damage Bonus v2", base1);
                case 434:
                    // similar to 220 except the values get lowered with faster weapons 
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Damage Bonus v3", base1);
                case 435:
                    return String.Format("Fragile Defense ({0})", base1);
                case 436:
                    return "Beneficial Countdown Hold";
                case 437:
                    return "Teleport to your " + FormatEnum((SpellTeleport)base1);
                case 438:
                    return "Teleport to their " + FormatEnum((SpellTeleport)base1);
                case 439:
                    return String.Format("Add Assasinate Proc with up to {0} Damage", base2);
                    //return String.Format("Add Assasinate Proc ({0}% Chance) with up to {1} Damage", base1 / 10f, base2);
                case 440:
                    // base2 / 10 is max mob health
                    return String.Format("Limit Finishing Blow Level: {0}", base1);
                case 441:
                    return String.Format("Cancel if Moved {0}'", base1);
                case 442:
                    return String.Format("Cast: [Spell {0}] once if {1}", base1, Spell.FormatEnum((SpellTargetRestrict)base2));
                case 443:
                    return String.Format("Cast: [Spell {0}] once if Caster {1}", base1, Spell.FormatEnum((SpellTargetRestrict)base2));
                case 444:
                    return "Lock Aggro on Caster and " + Spell.FormatPercent("Other Aggro", base2 - 100) + String.Format(" up to level {0}", base1);
                case 445:
                    return String.Format("Grant {0} Mercenary Slots", base1);
                case 446:
                    // no idea how these 4 buff blockers work
                    return String.Format("Buff Blocker A ({0})", base1);
                case 447:
                    return String.Format("Buff Blocker B ({0})", base1);
                case 448:
                    return String.Format("Buff Blocker C ({0})", base1);
                case 449:
                    return String.Format("Buff Blocker D ({0})", base1);
                case 450:
                    return String.Format("Absorb DoT Damage: {0}%", base1) + (base2 > 0 ? String.Format(", Max Per Hit: {0}", base2) : "") + (max > 0 ? String.Format(", Total: {0}", max) : "");
                case 451:
                    return String.Format("Absorb Melee Damage: {0}% over {1}", base1, base2) + (max > 0 ? String.Format(", Total: {0}", max) : "");
                case 452:
                    return String.Format("Absorb Spell Damage: {0}% over {1}", base1, base2) + (max > 0 ? String.Format(", Total: {0}", max) : "");
                case 453:
                    return String.Format("Cast: [Spell {0}] if {1} Melee Damage Taken in Single Hit", base1, base2);
                case 454:
                    return String.Format("Cast: [Spell {0}] if {1} Spell Damage Taken in Single Hit", base1, base2);
                case 455:
                    // adds a % of your own hate using base1. Example: 1000 hate base1 = 50. Means you will be 1500 hate.
                    return Spell.FormatPercent("Current Hate", base1);
                case 456:
                    // adds a % of your own hate using base1, per tick, scalable. Example: 1000 hate base1 = 50. Means you will be 1500 hate @ 1 tick, 2250 @ 2 ticks.
                    return Spell.FormatPercent("Current Hate", base1) + " per tick";
                case 457:
                    // offical name is "Resource Tap"
                    return string.Format("Return {0}% of Damage as {1}", base1 / 10f, new[] { "HP", "Mana", "Endurance" }[base2 % 3]) + (max > 0 ? String.Format(", Max Per Hit: {0}", max) : "");
                case 458:
                    // -100 = no faction hit, 100 = double faction
                    return Spell.FormatPercent("Faction Hit", base1);
                case 459:
                    // same as 185, created to stack
                    return Spell.FormatPercent(Spell.FormatEnum((SpellSkill)base2) + " Damage v2", base1);
                case 460:
                    // some spells are tagged as non focusable - this overrides that
                    return "Limit Type: Include Non-Focusable";
                case 461:
                    // Crits for DoTs and DDs. Calculated AFTER 413 BEFORE 124, 302. - Beimeith 
                    return Spell.FormatPercentRange("Spell Damage v2", base1, base2) + " (Before Crit)";
                case 462:
                    // SPA 462 appears to be the equivalent of SPA286 and added for stacking purposes. - Beimeith 
                    return Spell.FormatCount("Spell Damage v2", base1) + " (After Crit)";
                case 463:
                    // same as /shield command?
                    return Spell.FormatPercent("Melee Shielding: {0}%", base1);
                case 464:
                    return Spell.FormatPercent("Pet Chance to Rampage", base1);
                case 465:
                    return Spell.FormatPercent("Pet Chance to AE Rampage", base1);
                case 466:
                    // this chance is additive with the owner's passive pet flurry chance AA abilities.
                    // how does this differ from 280?
                    return Spell.FormatPercent("Pet Chance to Flurry", base1);
                case 467:
                    return Spell.FormatCount("Damage Shield Taken", base1);
                case 468:
                    return Spell.FormatPercent("Damage Shield Taken", base1);
                case 469:
                    // 469/470 seem to be similar to spa 340/374 except the cast a spell by group ID rather than spell ID
                    // is the chance on this shared with other chance SPAs (i.e. only 1 can be cast)?
                    return String.Format("Cast: [Group {0}] ({1}% Chance)", base2, base1);
                case 470:
                    // cast highest rank of a spell group - if you don't have any in your spellbook, nothing will be cast
                    // is the chance on this independant of other chance SPAs (i.e. each one has it's own chance to cast)?
                    if (base1 < 100)
                        return String.Format("Cast: [Group {0}] (Highest Rank) ({1}% Chance)", base2, base1);
                    return String.Format("Cast: [Group {0}] (Highest Rank) ", base2);
                case 471:
                    // add an extra melee round. i.e. main attack, double attack, triple
                    // this is sort of like 211 AE attack except it was added to nerf DPS by only affecting the current target
                    if (base2 != 100)
                        return Spell.FormatPercent("Chance to Repeat Primary Hand Round", base1) + String.Format(" with {0}% Damage", base2);
                    return Spell.FormatPercent("Chance to Repeat Primary Hand Round", base1);
                case 472:
                    return String.Format("Buy AA Rank ({0})", base1);
                case 473:
                    return Spell.FormatPercent("Chance to Double Backstab From Front", base1);
                case 474:
                    // similar to 330
                    return Spell.FormatPercent("Pet Critical Hit Damage", base1) + " of Base Damage";
                case 475:
                    // only activates if the spell is being cast from memory rather than an item
                    return String.Format("Cast: [Spell {0}] if Not Cast By Item Click", base2);
                case 476:
                    return String.Format("Weapon Stance: [Spell {0}] ({1})", base2, base1);
                case 477:
                    return String.Format("Move to Top of Hatelist ({0}% Chance)", base1);
                case 478:
                    return String.Format("Move to Bottom of Hatelist ({0}% Chance)", base1);
                case 479:
                    return String.Format("Limit Effect: {0} greater than {1}", Spell.FormatEnum((SpellEffect)base2), base1);
                case 480:
                    return String.Format("Limit Effect: {0} less than {1}", Spell.FormatEnum((SpellEffect)base2), base1);
                case 481:
                    // similar to 407 except maybe it checks limits?
                    if (base1 < 100)
                        return String.Format("Cast: [Spell {0}] if Hit By Spell ({1}% Chance)", base2, base1);
                    return String.Format("Cast: [Spell {0}] if Hit By Spell", base2);
                case 482:
                    return Spell.FormatPercent("Base " + Spell.FormatEnum((SpellSkill)base2) + " Damage", base1);
                case 483:
                    // incoming damage % SPAs 296 and 483 multiply against what is essentially (spell-data's base value * spa 413) 
                    // rather than the focused value - Dzarn
                    return Spell.FormatPercentRange("Spell Damage Taken", base1, base2) + " (After Crit)";
                case 484:
                    // Modifies incoming spell damage by Base1 points. Applies post-crit for both instant damage and DoTs.
                    // Differs from 297 which applies pre-crit to both instant damage and DoTs. - Dzarn
                    return Spell.FormatCount("Spell Damage Taken", base1) + " (After Crit)";
                case 485:
                    // this is used on incoming focuses to limit by the caster's class
                    // 411 is used on outgoing focuses
                    return String.Format("Limit Caster Class: {0}", (SpellClassesMask)(base1 >> 1));
                case 486:
                    if (base1 == 0)
                        return "Limit Caster: Exclude Self";
                    return "Limit Caster: Self";
                case 487:
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Skill Cap with Recipes", base1);
                case 488:
                    // reducing how much push incoming melee attacks will have on a PC.
                    return Spell.FormatPercent("Push Taken", -base1);
                case 489:
                    return Spell.FormatCount("Endurance Regen Cap", base1);
                // 490/491 - probably a limit based on recast timer
                case 490:
                    return String.Format("Limit Min Recast {0:0.##}s", base1 / 1000f);
                case 491:
                    return String.Format("Limit Max Recast {0:0.##}s", base1 / 1000f);
                case 492:
                    return String.Format("Limit Min Endurance Cost: {0}", base1);
                case 493:
                    return String.Format("Limit Max Endurance Cost: {0}", base1);
                case 494:
                    return Spell.FormatCount("Pet ATK", base1);
                case 495:
                    // Limit Max Duration Base1 Ticks
                    // is this the same as 141 with a base1 duration?
                    return String.Format("Limit Max Duration: {0}s", base1 * 6);
                case 496:
                    // description calls this "non-cumulative" but it would probably be better described as "non-stacking"
                    return Spell.FormatPercent("Critical " + Spell.FormatEnum((SpellSkill)base2) + " Damage", base1) + " of Base Damage (Non Stacking)";

            }

            return String.Format("Unknown SPA {0} Base1={1} Base2={2} Max={3} Calc={4} Value={5}", spa, base1, base2, max, calc, value);
        }

        /// <summary>
        /// Calculate a duration.
        /// </summary>
        /// <returns>Numbers of ticks (6 second units)</returns>
        public static int CalcDuration(int calc, int max, int level)
        {
            int value = 0;

            switch (calc)
            {
                case 0:
                    value = 0;
                    break;
                case 1:
                    value = level / 2;
                    if (value < 1)
                        value = 1;
                    break;
                case 2:
                    value = (level / 2) + 5;
                    if (value < 6)
                        value = 6;
                    break;
                case 3:
                    value = level * 30;
                    break;
                case 4:
                    value = 50;
                    break;
                case 5:
                    value = 2;
                    break;
                case 6:
                    value = level / 2;
                    break;
                case 7:
                    value = level;
                    break;
                case 8:
                    value = level + 10;
                    break;
                case 9:
                    value = level * 2 + 10;
                    break;
                case 10:
                    value = level * 30 + 10;
                    break;
                case 11:
                    value = (level + 3) * 30;
                    break;
                case 12:
                    value = level / 2;
                    if (value < 1)
                        value = 1;
                    break;
                case 13:
                    value = level * 3 + 10;
                    break;
                case 14:
                    value = (level + 2) * 5;
                    break;
                case 15:
                    value = (level + 10) * 10;
                    break;
                case 50:
                    value = 72000;
                    break;
                case 3600:
                    value = 3600;
                    break;
                default:
                    value = max;
                    break;
            }

            if (max > 0 && value > max)
                value = max;

            return value;
        }

        /// <summary>
        /// Calculate a level/tick scaled value.
        /// </summary>
        public static int CalcValue(int calc, int base1, int max, int tick, int level)
        {
            if (calc == 0)
                return base1;

            if (calc == 100)
            {
                if (max > 0 && base1 > max)
                    return max;
                return base1;
            }

            int change = 0;

            switch (calc)
            {
                case 100:
                    break;
                case 101:
                    change = level / 2;
                    break;
                case 102:
                    change = level;
                    break;
                case 103:
                    change = level * 2;
                    break;
                case 104:
                    change = level * 3;
                    break;
                case 105:
                    change = level * 4;
                    break;
                case 107:
                    change = -1 * tick;
                    break;
                case 108:
                    change = -2 * tick;
                    break;
                case 109:
                    change = level / 4;
                    break;
                case 110:
                    change = level / 6;
                    break;
                case 111:
                    if (level > 16) change = (level - 16) * 6;
                    break;
                case 112:
                    if (level > 24) change = (level - 24) * 8;
                    break;
                case 113:
                    if (level > 34) change = (level - 34) * 10;
                    break;
                case 114:
                    if (level > 44) change = (level - 44) * 15;
                    break;
                case 115:
                    if (level > 15) change = (level - 15) * 7;
                    break;
                case 116:
                    if (level > 24) change = (level - 24) * 10;
                    break;
                case 117:
                    if (level > 34) change = (level - 34) * 13;
                    break;
                case 118:
                    if (level > 44) change = (level - 44) * 20;
                    break;
                case 119:
                    change = level / 8;
                    break;
                case 120:
                    change = -5 * tick;
                    break;
                case 121:
                    change = level / 3;
                    break;
                case 122:
                    change = -12 * tick;
                    break;
                case 123:
                    // random in range
                    change = (Math.Abs(max) - Math.Abs(base1)) / 2;
                    break;
                case 124:
                    if (level > 50) change = (level - 50);
                    break;
                case 125:
                    if (level > 50) change = (level - 50) * 2;
                    break;
                case 126:
                    if (level > 50) change = (level - 50) * 3;
                    break;
                case 127:
                    if (level > 50) change = (level - 50) * 4;
                    break;
                case 128:
                    if (level > 50) change = (level - 50) * 5;
                    break;
                case 129:
                    if (level > 50) change = (level - 50) * 10;
                    break;
                case 130:
                    if (level > 50) change = (level - 50) * 15;
                    break;
                case 131:
                    if (level > 50) change = (level - 50) * 20;
                    break;
                case 132:
                    if (level > 50) change = (level - 50) * 25;
                    break;
                case 139:
                    if (level > 30) change = (level - 30) / 2;
                    break;
                case 140:
                    if (level > 30) change = (level - 30);
                    break;
                case 141:
                    if (level > 30) change = 3 * (level - 30) / 2;
                    break;
                case 142:
                    if (level > 30) change = 2 * (level - 60);
                    break;
                case 143:
                    change = 3 * level / 4;
                    break;

                case 3000:
                    // todo: this appears to be scaled by the targets level
                    // base1 value how it affects a level 100 target
                    return base1;

                default:
                    if (calc > 0 && calc < 1000)
                        change = level * calc;

                    // 1000..1999 variable by tick
                    // e.g. splort (growing): Effect=0 Base1=1 Base2=0 Max=0 Calc=1035
                    //      34 - 69 - 104 - 139 - 174 - 209 - 244 - 279 - 314 - 349 - 384 - 419 - 454 - 489 - 524 - 559 - 594 - 629 - 664 - 699 - 699
                    // e.g. venonscale (decaying): Effect=0 Base1=-822 Base2=0 Max=822 Calc=1018
                    //
                    // e.g. Deathcloth Spore: Base1=-1000 Base2=0 Max=0 Calc=1999
                    // e.g. Bleeding Bite: Base1=-1000 Base2=0 Max=0 Calc=1100 (The damage done will decrease in severity over time.)
                    // e.g. Blood Rites: Base1=-1500 Base2=0 Max=0 Calc=1999
                    if (calc >= 1000 && calc < 2000)
                        change = tick * (calc - 1000) * -1;

                    // 2000..2999 variable by level
                    if (calc >= 2000)
                        change = level * (calc - 2000);
                    break;
            }

            int value = Math.Abs(base1) + change;

            if (max != 0 && value > Math.Abs(max))
                value = Math.Abs(max);

            if (base1 < 0)
                value = -value;

            return value;
        }

        /// <summary>
        /// Calculate the min/max values for a scaled value.
        /// </summary>
        public static string CalcValueRange(int calc, int base1, int max, int duration, int level)
        {
            int start = CalcValue(calc, base1, max, 1, level);
            int finish = Math.Abs(CalcValue(calc, base1, max, duration, level));

            string type = Math.Abs(start) < Math.Abs(finish) ? "Growing" : "Decaying";

            if (calc == 123)
                return String.Format(" (Random: {0} to {1})", base1, max * ((base1 >= 0) ? 1 : -1));

            if (calc == 107)
                return String.Format(" ({0} to {1} @ 1/tick)", type, finish);

            if (calc == 108)
                return String.Format(" ({0} to {1} @ 2/tick)", type, finish);

            if (calc == 120)
                return String.Format(" ({0} to {1} @ 5/tick)", type, finish);

            if (calc == 122)
                return String.Format(" ({0} to {1} @ 12/tick)", type, finish);

            if (calc > 1000 && calc < 2000)
                return String.Format(" ({0} to {1} @ {2}/tick)", type, finish, calc - 1000);

            return null;
        }

        public override string ToString()
        {
            if (GroupID <= 0)
                return String.Format("[{0}] {1}", ID, Name);
            return String.Format("[{0}/{2}] {1}", ID, Name, GroupID);
        }

        /// <summary>
        /// Get a full description of the spell. This is mostly useful as a debug dump.
        /// </summary>
        public string[] Details()
        {
            var result = new List<string>(20);
            //Action<string> Add = delegate(string s) { result.Add(s); };


            // the skill field is full of random values for spells that aren't PC castable so it only makes sense to show it for PC spells
            if (!String.IsNullOrEmpty(ClassesLevels))
            {
                result.Add("Classes: " + ClassesLevels);

                if (CombatSkill)
                    result.Add("Skill: " + FormatEnum(Skill) + " (Combat Skill)");
                else if (SongCap > 0)
                    result.Add("Skill: " + FormatEnum(Skill) + ", Max Focus: " + SongCap + '%');
                else
                    result.Add("Skill: " + FormatEnum(Skill));
            }

            if (!String.IsNullOrEmpty(Deity))
                result.Add("Deity: " + Deity);

            if (Mana > 0)
                result.Add("Mana: " + Mana);

            if (EnduranceUpkeep > 0)
                result.Add("Endurance: " + Endurance + ", Upkeep: " + EnduranceUpkeep + " per tick");
            else if (Endurance > 0)
                result.Add("Endurance: " + Endurance);

            for (int i = 0; i < ConsumeItemID.Length; i++)
                if (ConsumeItemID[i] > 0)
                    result.Add("Consumes: [Item " + ConsumeItemID[i] + "] x " + ConsumeItemCount[i]);

            for (int i = 0; i < FocusID.Length; i++)
                if (FocusID[i] > 0)
                    result.Add("Focus: [Item " + FocusID[i] + "]");

            if (BetaOnly)
                result.Add("Restriction: Beta Only");

            if (CannotRemove)
                result.Add("Restriction: Cannot Remove");

            if (CastOutOfCombat)
                result.Add("Restriction: Out of Combat"); // i.e. no aggro

            if (CastInFastRegen)
                result.Add("Restriction: In Fast Regen");

            if (Zone != SpellZoneRestrict.None)
                result.Add("Restriction: " + Zone + " Only");

            if (Sneaking)
                result.Add("Restriction: Sneaking");

            if (CancelOnSit)
                result.Add("Restriction: Cancel on Sit");

            if ((int)CasterRestrict > 100)
                result.Add("Restriction: " + FormatEnum(CasterRestrict));

            if (Target == SpellTarget.Directional_AE)
                result.Add("Target: " + FormatEnum(Target) + " (" + ConeStartAngle + " to " + ConeEndAngle + " Degrees)");
            else if (TargetRestrict > 0)
                result.Add("Target: " + FormatEnum(Target) + " (If " + FormatEnum(TargetRestrict) + ")");
            else if ((Target == SpellTarget.Caster_Group || Target == SpellTarget.Target_Group) && (ClassesMask != 0 && ClassesMask != SpellClassesMask.BRD) && DurationTicks > 0)
                result.Add("Target: " + FormatEnum(Target) + ", MGB: " + (MGBable ? "Yes" : "No"));
            else
                result.Add("Target: " + FormatEnum(Target));

            if (AERange > 0 && Range == 0)
                result.Add("AE Range: " + (MinRange > 0 ? MinRange + "' to " : "") + AERange + "'");
            else if (AERange > 0)
                result.Add("Range: " + Range + "', AE Range: " + (MinRange > 0 ? MinRange + "' to " : "") + AERange + "'"); // unsure where the min range should be applied
            else if (Range > 0)
                result.Add("Range: " + (MinRange > 0 ? MinRange + "' to " : "") + Range + "'");

            if (RangeModFarDist != 0)
                result.Add("Range Based Mod: " + (RangeModCloseMult * 100) + "% at " + RangeModCloseDist + "' to " + (RangeModFarMult * 100) + "% at " + RangeModFarDist + "'");

            if (ViralRange > 0)
                result.Add("Viral Range: " + ViralRange + "', Recast: " + MinViralTime + "s to " + MaxViralTime + "s");

            if (Beneficial)
                result.Add("Resist: Beneficial, Blockable: " + (BeneficialBlockable ? "Yes" : "No"));
            else
            {
                result.Add("Resist: " + FormatEnum(ResistType) + (ResistMod != 0 ? " " + ResistMod : "")
                    //+ (ResistType != SpellResist.Sanctification && ResistType != SpellResist.Unresistable && !NoSanctification ? " or Sanctification" : "")
                    // NoSanctification is rare so maybe it makes more sense to only show it when it's disabled
                    + (NoSanctification ? ", No Sanctification" : "")
                    + (MinResist > 0 ? ", Min Resist Chance: " + MinResist / 2f + "%" : "")
                    + (MaxResist > 0 ? ", Max Resist Chance: " + MaxResist / 2f + "%" : ""));
                // + (!PartialResist ? ", No Partials" : ""));

                //if (!NoSanctification)
                //    result.Add("Resist: Sanctification");

                result.Add("Reflectable: " + (Reflectable ? "Yes" : "No"));

                // only nukes and DoT can trigger spell damage shields
                // no point showing it for NPC spells since NPCs will never take significant damage from nuking players
                if (HasEffect("Decrease Current HP", 0) && ClassesMask != 0)
                    result.Add("Trigger Spell DS: " + (Feedbackable ? "Yes" : "No"));
            }

            if (Stacking.Count > 0)
                result.Add("Stacking: " + String.Join(", ", Stacking.ToArray()));

            // this includes both spell and AA focuses
            result.Add("Focusable: " + (Focusable ? "Yes" : "No"));

            string rest = ClassesMask == 0 || ClassesMask == SpellClassesMask.BRD || RestTime == 0 ? "" : ", Rest: " + RestTime.ToString() + "s";
            if (TimerID > 0)
                result.Add("Casting: " + CastingTime.ToString() + "s, Recast: " + FormatTime(RecastTime) + ", Timer: " + TimerID + rest);
            else if (RecastTime > 0)
                result.Add("Casting: " + CastingTime.ToString() + "s, Recast: " + FormatTime(RecastTime) + rest);
            else
                result.Add("Casting: " + CastingTime.ToString() + "s" + rest);

            if (DurationTicks > 0)
                result.Add("Duration: " + FormatTime(DurationTicks * 6) + (Focusable ? "+" : "") + " (" + DurationTicks + " ticks)"
                    + (SongWindow ? " Song" : "")
                    //+ (Beneficial && ClassesMask != SpellClassesMask.BRD ? ", Extendable: " + (Focusable ? "Yes" : "No") : "") // already indicated by "+" symbol above and "focusable" line
                    + ", Dispelable: " + (Dispelable ? "Yes" : "No")
                    //+ (!Beneficial && DurationTicks > 10 ? ", Allow Fast Regen: " + (AllowFastRegen ? "Yes" : "No") : "")  // it applies on <10 ticks, but there really is no need to show it for short term debuffs 
                    + (PersistAfterDeath ? ", Persist After Death" : "")); // pretty rare, so only shown when it's used
            else if (AEDuration >= 2500)
                result.Add("AE Waves: " + AEDuration / 2500);

            if (PushUp != 0)
                result.Add("Push: " + PushBack + "' Up: " + PushUp + "'");
            else if (PushBack != 0)
                result.Add("Push: " + PushBack + "'");

            if (HateMod != 0)
                result.Add("Hate Mod: " + HateMod.ToString("+#;-#;0"));

            if (HateOverride != 0)
                result.Add("Hate: " + HateOverride);

            if (CritOverride > 0)
                result.Add("Max Crit Chance: " + CritOverride + "%");

            //if (SongCap > 0)
            //    result.Add("Max Song Focus: " + SongCap + "%");

            if (MaxHits > 0)
                result.Add("Max Hits: " + MaxHits + " " + FormatEnum((SpellMaxHits)MaxHitsType));

            if (MaxTargets > 0)
                result.Add("Max Targets: " + MaxTargets);

            if (Recourse != null)
                result.Add("Recourse: " + Recourse);

            if (Unknown != 0)
                result.Add("Unknown: " + Unknown);

            //if (!String.IsNullOrEmpty(Category))
            //    result.Add("Category: " + Category);

            for (int i = 0; i < Slots.Count; i++)
                if (Slots[i] != null)
                    result.Add(String.Format("{0}: {1}", i + 1, Slots[i].Desc));

            if (!String.IsNullOrEmpty(LandOnSelf))
                result.Add("Text: " + LandOnSelf);

            return result.ToArray();
        }

        /// <summary>
        /// Finalize spell data after all the attributes have been loaded.
        /// </summary>
        public void Prepare()
        {
            // get coordinates for teleport spells (which are spread over several slots)
            var loc = Slots.Where(x => x != null && (x.SPA == 83 || x.SPA == 88 || x.SPA == 104 || x.SPA == 145 || x.SPA == 146)).Select(x => x.Base1.ToString()).ToArray();
            if (loc.Length > 0)
                Extra += " " + String.Join(", ", loc);

            // parse spell effects
            for (int i = 0; i < Slots.Count; i++)
                if (Slots[i] != null)
                {
                    var slot = Slots[i];
                    slot.Desc = ParseEffect(slot, 105);
#if DEBUG
                    if (slot.Desc != null)
                        slot.Desc = String.Format("SPA {0} Base1={1} Base2={2} Max={3} Calc={4} --- ", slot.SPA, slot.Base1, slot.Base2, slot.Max, slot.Calc) + slot.Desc;
#endif
                    // clear slots that weren't parsed (this will mostly be SPA 10) 
                    if (slot.Desc == null)
                        Slots[i] = null;
                }

            // build a string of classes that can use this spell
            ClassesLevels = String.Empty;
            ClassesMask = 0;

            bool All254 = true;
            for (int i = 0; i < Levels.Length; i++)
            {
                if (Levels[i] == 255)
                    Levels[i] = 0;
                if (Levels[i] != 254 && (i + 1) != (int)SpellClasses.BRD)
                    All254 = false;
                if (Levels[i] != 0)
                {
                    ClassesMask |= (SpellClassesMask)(1 << i);
                    ClassesLevels += " " + (SpellClasses)(i + 1) + "/" + Levels[i];
                }
            }
            Array.Copy(Levels, ExtLevels, Levels.Length);
            ClassesLevels = ClassesLevels.TrimStart();
            if (All254)
                ClassesLevels = "ALL/254";



            if (MaxHitsType == SpellMaxHits.None || (DurationTicks == 0 && !Name.Contains("Aura")))
                MaxHits = 0;

            if (Target == SpellTarget.Caster_PB)
            {
                Range = 0;
            }

            if (Target == SpellTarget.Self)
            {
                Range = 0;
                AERange = 0;
                MaxTargets = 0;
            }

            if (Target == SpellTarget.Single)
            {
                AERange = 0;
                MaxTargets = 0;
            }

            if (ResistType == SpellResist.Unresistable)
            {
                ResistMod = 0;
                MinResist = 0;
                MaxResist = 0;
                // if the spell is unresistable except for sanctification then change it's resist
                // don't do this for PC spells because NPCs don't have sanctification
                //if (!NoSanctification && ClassesMask == 0)
                //    ResistType = SpellResist.Sanctification;
            }

            if (Zone != SpellZoneRestrict.Indoors && Zone != SpellZoneRestrict.Outdoors)
                Zone = SpellZoneRestrict.None;

            if (RangeModCloseDist == RangeModFarDist)
            {
                RangeModCloseDist = RangeModFarDist = 0;
                RangeModCloseMult = RangeModFarMult = 0;
            }
        }

        /// <summary>
        /// Search all effect slots using a SPA match.
        /// </summary>
        /// <param name="slot">0 to check all slots, or a non zero value to check a specific slot.</param>
        public bool HasEffect(int spa, int slot)
        {
            if (slot > 0)
                return slot <= Slots.Count && Slots[slot - 1] != null && Slots[slot - 1].SPA == spa;

            for (int i = 0; i < Slots.Count; i++)
                if (Slots[i] != null && Slots[i].SPA == spa)
                    return true;

            return false;
        }

        /// <summary>
        /// Search all effect slots using a text match.
        /// </summary>
        /// <param name="desc">Effect to search for. Can be text or a integer representing an SPA.</param>
        /// <param name="slot">0 to check all slots, or a non zero value to check a specific slot.</param>
        public bool HasEffect(string text, int slot)
        {
            int spa;
            if (Int32.TryParse(text, out spa))
                return HasEffect(spa, slot);

            if (slot > 0)
                return slot <= Slots.Count && Slots[slot - 1] != null && Slots[slot - 1].Desc.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0;

            for (int i = 0; i < Slots.Count; i++)
                if (Slots[i] != null && Slots[i].Desc.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return true;

            return false;
        }

        /// <summary>
        /// Search all effect slots using a regular expression.
        /// </summary>
        /// <param name="slot">0 to check all slots, or a non zero value to check a specific slot.</param>
        public bool HasEffect(Regex re, int slot)
        {
            if (slot > 0)
                return slot <= Slots.Count && Slots[slot - 1] != null && re.IsMatch(Slots[slot - 1].Desc);

            for (int i = 0; i < Slots.Count; i++)
                if (Slots[i] != null && re.IsMatch(Slots[i].Desc))
                    return true;

            return false;
        }

        /// <summary>
        /// Sum all the spell slots effects that match a regex. The regex must have a capturing group for an integer value.
        /// e.g. Increase Current HP by (\d+)
        /// </summary>
        public int ScoreEffect(Regex re)
        {
            int score = 0;
            for (int i = 0; i < Slots.Count; i++)
                if (Slots[i] != null)
                {
                    Match m = re.Match(Slots[i].Desc);
                    if (m.Success)
                        score += Int32.Parse(m.Groups[1].Value);
                }

            return score;
        }

        public static string FormatEnum(Enum e)
        {
            string type = e.ToString().Replace("_", " ").Trim();


            if (Regex.IsMatch(type, @"^-?\d+$"))
                type = "Type " + type; // undefined numeric enum

            // remove numeric suffix on duplicate enums undead3/summoned3/etc
            else if (e is SpellTargetRestrict)
                type = Regex.Replace(type, @"(?<!\d)\d$", "");

            return type;
        }

        public static string FormatTime(float seconds)
        {
            if (seconds < 120)
                return seconds.ToString("0.##") + "s";

            if (seconds < 7200)
                return (seconds / 60f).ToString("0.#") + "m";

            return (seconds / 3600f).ToString("0.#") + "h";

            //return new TimeSpan(0, 0, (int)seconds).ToString();
        }

        private static string FormatCount(string name, int value)
        {
            return String.Format("{0} {1} by {2}", value < 0 ? "Decrease" : "Increase", name, Math.Abs(value));
        }

        private static string FormatPercent(string name, float value)
        {
            return String.Format("{0} {1} by {2:0.#}%", value < 0 ? "Decrease" : "Increase", name, Math.Abs(value));
        }

        private static string FormatPercentRange(string name, int min, int max)
        {
            return FormatPercentRange(name, min, max, false);
        }

        private static string FormatPercentRange(string name, int min, int max, bool negate)
        {
            if (min < 0)
            {
                // for negative min values, min < max is valid but should be swapped
                // e.g. cleric vow spells: min=-50 max=0, decrease healing by 0% to 50% (not sure why they didn't use min=0 max=-50)
                if (min < max)
                {
                    //return String.Format("{0} {1}", min, max);
                    int temp = min;
                    min = max;
                    max = temp;
                }
            }
            else
            {
                // for positive min values, min < max is bad data and max should be ignored
                if (min > max)
                    max = min;
            }

            // some effects like 'increase mana conservation' use negated wording 'decrease mana cost' 
            if (negate)
            {
                min = -min;
                max = -max;
            }

            if (min == max)
                return String.Format("{0} {1} by {2:0.#}%", max < 0 ? "Decrease" : "Increase", name, Math.Abs(min));

            return String.Format("{0} {1} by {2:0.#}% to {3:0.#}%", max < 0 ? "Decrease" : "Increase", name, Math.Abs(min), Math.Abs(max));
        }


        /*
        private static string FormatDesc()
        {
            // Spell descriptions include references to spell slots. e.g.
            // #7 = base1 for slot 7
            // @7 = calc(base1) for slot 7
            // $7 = base2 for slot 7
            return null;
        }
        */
    }

}