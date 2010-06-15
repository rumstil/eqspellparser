using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;



/* 
 * 
 * http://code.google.com/p/projecteqemu/source/browse/trunk/EQEmuServer/zone/spdat.h
 * http://sourceforge.net/projects/eqemulator/files/OpenZone/OpenSpell_2.0/OpenSpell_2.0.zip/download
 * http://forums.station.sony.com/eq/posts/list.m?start=150&topic_id=162971
 *
 */

namespace Everquest
{
    public enum SpellClasses
    {
        WAR = 1, CLR, PAL, RNG, SHD, DRU, MNK, BRD, ROG, SHM, NEC, WIZ, MAG, ENC, BST, BER
    }

    public enum SpellClassesLong
    {
        Warrior = 1, Cleric, Paladin, Ranger, ShadowKnight, Druid, Monk, Bard, Rogue, Shaman,
        Necro, Wizard, Mage, Enchanter, Beastlord, Berserker
    }

    [Flags]
    public enum SpellClassesMask
    {
        WAR = 1, CLR = 2, PAL = 4, RNG = 8, SHD = 16, DRU = 32, MNK = 64, BRD = 128, ROG = 256,
        SHM = 512, NEC = 1024, WIZ = 2048, MAG = 4096, ENC = 8192, BST = 16384, BER = 32768
    }

    public enum SpellEffect
    {
        Current_HP = 0,
        AC = 1,
        ATK = 2,
        Movement_Speed = 3,
        STR = 4,
        DEX = 5,
        AGI = 6,
        STA = 7,
        WIS = 8,
        INT = 9,
        CHA = 10,
        Melee_Haste = 11,
        Invis = 12,
        See_Invis = 13,
        Enduring_Breath = 14,
        Current_Mana_Repeating = 15,
        Stun = 21,
        Charm = 22,
        Fear = 23,
        Mesmerize = 31,
        Summon_Item = 32,
        Summon_Pet = 33,
        Disease_Counter = 35,
        Poison_Counter = 36,
        Invulnerability = 40,
        Delayed_Heal_Marker = 44,
        Fire_Resist = 46,
        Cold_Resist = 47,
        Poison_Resist = 48,
        Disease_Resist = 49,
        Magic_Resist = 50,
        Rune = 55,
        Levitate = 57,
        Damage_Shield = 59,
        Summon_Skeleton_Pet = 71,
        Melee_Proc = 85,
        Assist_Radius = 86,
        Max_HP = 69,
        Hate = 92,
        Max_Mana = 97,
        Current_HP_Repeating = 100,
        Donals_Heal = 101,
        All_Resists = 111,
        Current_HP_Percent = 147,
        Spell_Rune = 161,
        Melee_Rune = 162,
        Absorb_Hits = 163,
        Melee_Mitigation = 168,
        Lifetap_From_Weapon = 178,
        Hate_Repeating = 192,
        Taunt = 199,
        Proc_Rate = 200,
        Weapon_Damage_Bonus = 220,
        Spell_Damage_Bonus = 286,
        XP_Gain = 337,
        Add_Spell_Proc = 339,
        Mana_Burn = 350,
        Current_Mana = 358,
        Corruption_Counter = 369,
        Corruption_Resist = 370,
        Twincast = 399
    }

    public enum SpellSkill
    {
        Hit = -1, // weapons/archery/backstab/frenzy/kick/etc..
        _1H_Blunt = 0,
        _1H_Slash = 1,
        _2H_Blunt = 2,
        _2H_Slash = 3,
        Abjuration = 4,
        Alteration = 5,
        Apply_Poison = 6,
        Archery = 7,
        Backstab = 8,
        Bind_Wound = 9,
        Bash = 10,
        Block = 11,
        Brass_Instruments = 12,
        Channeling = 13,
        Conjuration = 14,
        Defense = 15,
        Disarm = 16,
        Disarm_Traps = 17,
        Divination = 18,
        Dodge = 19,
        Double_Attack = 20,
        Dragon_Punch = 21,
        Dual_Wield = 22,
        Eagle_Strike = 23,
        Evocation = 24,
        Feign_Death = 25,
        Flying_Kick = 26,
        Forage = 27,
        Hand_to_Hand = 28,
        Hide = 29,
        Kick = 30,
        Meditate = 31,
        Mend = 32,
        Offense = 33,
        Parry = 34,
        Pick_Lock = 35,
        Piercing = 36,
        Riposte = 37,
        Round_Kick = 38,
        Safe_Fall = 39,
        Sense_Heading = 40,
        Singing = 41,
        Sneak = 42,
        Stringed_Instruments = 49,
        Throwing = 51,
        Tiger_Claw = 52,
        Tracking = 53,
        Wind_Instruments = 54,
        Alcohol_Tolerance = 66,
        Percusion_Instruments = 70,
        Berserking = 72,
        Taunt = 73,
        Frenzy = 74
    }

    public enum SpellSkillCap
    {
        STR = 0,
        STA = 1,
        AGI = 2,
        DEX = 3,
        WIS = 4,
        INT = 5,
        CHA = 6,
        Magic_Resist = 7,
        Fire_Resist = 8,
        Cold_Resist = 9,
        Poison_Resist = 10,
        Disease_Resist = 11
    }

    public enum SpellResist
    {
        Unresistable = 0, // only for detrimental spells
        Magic = 1,
        Fire = 2,
        Cold = 3,
        Poison = 4,
        Disease = 5,
        Lowest = 6, // Chromatic/lowest
        Average = 7, // Prismatic/average
        Physical = 8,
        Corruption = 9
    }

    public enum SpellTarget
    {
        Line_of_Sight = 1,
        Caster_AE = 2,
        Caster_Group = 3,
        Caster_PB = 4,
        Single = 5,
        Self = 6,
        Target_AE = 8,
        Animal = 9,
        Undead = 10,
        Summoned = 11,
        Lifetap = 13,
        Pet = 14,
        Corpse = 15,
        Plant = 16,
        Old_Giants = 17,
        Old_Dragons = 18,
        Undead_AE = 24,
        Summoned_AE = 25,
        Hatelist = 33,
        Chest = 34,
        Nearby_Players = 40, // bard AE hits all players
        Target_Group = 41,
        Directional_AE = 42, // see degree fields
        Frontal_AE = 44,
        Single_In_Group = 43,
        Target_Ring_AE = 45,
        Targets_Target = 46,
        Pet_Owner = 47
    }

    public enum SpellTargetRestrict
    {
        Animal_or_Humanoid = 100,
        Dragon = 101,
        Animal_or_Insect = 102,
        Animal = 104,
        Plant = 105,
        Giant = 106,
        Bixie = 109,
        Harpy = 110,
        Sporali = 112,
        Kobald = 113,
        Shade = 114,
        Drakkin = 115,
        Animal_or_Plant = 117,
        Summoned = 118,
        Fire_Pet = 119,
        Undead = 120,
        Living = 121,
        Fairy = 122,
        Humanoid = 123,
        HP_Below_10_Percent = 124,
        Clockwork = 125,
        Wisp = 126,
        HP_Above_75_Percent = 201,
        HP_Below_20_Percent = 203,
        Not_In_Combat = 216,
        At_Least_1_Pet_On_Hatelist = 221,
        At_Least_2_Pets_On_Hatelist = 222,
        At_Least_3_Pets_On_Hatelist = 223,
        At_Least_4_Pets_On_Hatelist = 224,
        At_Least_5_Pets_On_Hatelist = 225,
        At_Least_6_Pets_On_Hatelist = 226,
        At_Least_7_Pets_On_Hatelist = 227,
        At_Least_8_Pets_On_Hatelist = 228,
        At_Least_9_Pets_On_Hatelist = 229,
        At_Least_10_Pets_On_Hatelist = 230,
        HP_Below_35_Percent = 250,
        Chain_Plate_Classes = 304,
        HP_Between_55_and_65_Percent = 404,
        HP_Between_45_and_55_Percent = 403,
        HP_Between_35_and_45_Percent = 402,
        HP_Between_25_and_35_Percent = 401,
        HP_Between_1_and_25_Percent = 400,
        HP_Between_1_and_35_Percent = 507, // between or below?
        HP_Below_75_Percent = 515,
        HP_Below_90_Percent = 518,
        Undead2 = 603,
        Undead3 = 608,
        Summoned2 = 624,
        Exclude_Pets = 701
    }

    public enum SpellZoneRestrict
    {
        Outdoors = 1,
        Indoors = 2,
        None = 0
    }

    public enum SpellIllusion
    {
        Human = 1,
        Barbarian = 2,
        Erudite = 3,
        Wood_Elf = 4,
        High_Elf = 5,
        Dark_Elf = 6,
        Half_Elf = 7,
        Dwarf = 8,
        Troll = 9,
        Ogre = 10,
        Halfling = 11,
        Gnome = 12,
        Aviak = 13,
        Werewolf = 14,
        Brownie = 15,
        Centaur = 16,
        Froglok = 26,
        Froglok_Ghoul = 27,
        Gargoyle = 29,
        Wolf = 42,
        Bear = 43,
        Imp = 46,
        Elemental = 75, // has subtypes
        Scarecrow = 82,
        //Skeleton = 85,
        Iksar = 128,
        Kunark_Goblin = 137,
        Tree = 143,
        Iksar_Skeleton = 161,
        Guktan = 330,
        Scaled_Wolf = 356,
        Skeleton = 367,
        Golem = 374, // has subtypes
        Pyrilen = 411,
        Gelidran = 417,
        Goblin = 433, // has subtypes
        Basilisk = 436,
        Gnomework = 457,
        Orc = 458,
        Stone_Gargoyle = 464,
        Evil_Eye = 469,
        Minotaur = 470,
        Zombie = 471,
        Fairy = 473,
        Spectre = 485,
        Banshee = 487,
        Scrykin = 495, // has subtypes
        Bixie = 520,
        Drakkin = 522,
        Hideous_Harpy = 527,
        Crystal_Sphere = 616
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
        public bool DurationExtendable;
        public string[] Slots;
        public int[] SlotEffects;
        public int[] Levels;
        public string ClassesLevels;
        public SpellClassesMask ClassesMask;
        public SpellSkill Skill;
        public bool Beneficial;
        public bool BeneficialBlockable;
        public SpellTarget Target;
        public SpellResist ResistType;
        public int ResistMod;
        public string Extra;
        public int HateOverride;
        public int Range;
        public int AERange;
        public int AEDuration; // rain spells
        public float CastingTime;
        public float QuietTime;
        public float RecastTime;
        public float PushBack;
        public float PushUp;
        public int DescID;
        public string Desc;
        public int MaxHits;
        public int MaxTargets;
        public int RecourseID;
        public int TimerID;
        public int ViralPulse;
        public int ViralRange;
        public SpellTargetRestrict TargetRestrict;
        public SpellTargetRestrict ProcRestrict;
        public int[] RegID;
        public int[] RegCount;
        public int[] FocusID;
        //public string LandOnSelf;
        //public string LandOnOther;
        public int StartDegree;
        public int EndDegree;
        public bool MGBable;
        public int Rank;
        public bool OutOfCombat;
        public SpellZoneRestrict Zone;

        public float Unknown;


        /// Effects can reference other spells or items via square bracket notation. e.g.
        /// [Spell 123]    is a reference to spell 123
        /// [Group 123]    is a reference to spell group 123
        /// [Item 123]     is a reference to item 123
        static public readonly Regex SpellRef = new Regex(@"\[Spell\s(\d+)\]");
        static public readonly Regex GroupRef = new Regex(@"\[Group\s(\d+)\]");
        static public readonly Regex ItemRef = new Regex(@"\[Item\s(\d+)\]");

        public Spell()
        {
            Slots = new string[12];
            SlotEffects = new int[12];
            Levels = new int[16];
            RegID = new int[4];
            RegCount = new int[4];
            FocusID = new int[4];
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
            List<string> result = new List<string>();

            if (!String.IsNullOrEmpty(ClassesLevels))
                result.Add("Classes: " + ClassesLevels);

            if (Mana > 0)
                result.Add("Mana: " + Mana);

            if (EnduranceUpkeep > 0)
                result.Add("Endurance: " + Endurance + ", Upkeep: " + EnduranceUpkeep + " per tick");
            else if (Endurance > 0)
                result.Add("Endurance: " + Endurance);

            for (int i = 0; i < RegID.Length; i++)
                if (RegID[i] > 0)
                    result.Add("Regeant: [Item " + RegID[i] + "] x " + RegCount[i]);

            for (int i = 0; i < FocusID.Length; i++)
                if (FocusID[i] > 0)
                    result.Add("Focus: [Item " + FocusID[i] + "]");

            //result.Add("Skill: " + Skill);

            if (OutOfCombat)
                result.Add("Restriction: Out of Combat");

            if (Zone != SpellZoneRestrict.None)
                result.Add("Restriction: " + Zone + " Only");

            if (Target == SpellTarget.Directional_AE)
                result.Add("Target: " + FormatEnum(Target) + " (" + StartDegree + " to " + EndDegree + " Degrees)");
            else if (TargetRestrict > 0)
                result.Add("Target: " + FormatEnum(Target) + " (" + FormatEnum(TargetRestrict) + ")");
            else if ((Target == SpellTarget.Caster_Group || Target == SpellTarget.Target_Group) && (ClassesMask != 0 && ClassesMask != SpellClassesMask.BRD) && DurationTicks > 0)
                result.Add("Target: " + FormatEnum(Target) + ", MGB: " + (MGBable ? "Yes" : "No"));
            else
                result.Add("Target: " + FormatEnum(Target));

            if (AERange > 0)
                result.Add("Range: " + Range + ", AE Range: " + AERange);
            else if (Range > 0)
                result.Add("Range: " + Range);

            if (ViralRange > 0)
                result.Add("Viral Range: " + ViralRange + ", Recast: " + ViralPulse + "s");

            if (!Beneficial && ResistType != SpellResist.Unresistable && ResistMod != 0)
                result.Add("Resist: " + ResistType + " " + ResistMod);
            else if (!Beneficial)
                result.Add("Resist: " + ResistType);
            else
                result.Add("Beneficial: " + (BeneficialBlockable ? "Blockable" : "Not Blockable"));

            if (TimerID > 0)
                result.Add("Casting: " + CastingTime.ToString() + "s, Recast: " + FormatTime(RecastTime) + ", Timer: " + TimerID);
            else if (RecastTime > 0)
                result.Add("Casting: " + CastingTime.ToString() + "s, Recast: " + FormatTime(RecastTime));
            else
                result.Add("Casting: " + CastingTime.ToString() + "s");

            if (DurationTicks > 0 && Beneficial && ClassesMask != SpellClassesMask.BRD)
                result.Add("Duration: " + FormatTime(DurationTicks * 6) + " (" + DurationTicks + " ticks)" + ", Extend: " + (DurationExtendable ? "Yes" : "No"));
            else if (DurationTicks > 0)
                result.Add("Duration: " + FormatTime(DurationTicks * 6) + " (" + DurationTicks + " ticks)");
            else if (AEDuration >= 2500)
                result.Add("AE Waves: " + AEDuration / 2500);

            if (PushUp != 0)
                result.Add("Push: " + PushBack + ", Up: " + PushUp);
            else if (PushBack != 0)
                result.Add("Push: " + PushBack);

            if (HateOverride != 0)
                result.Add("Hate: " + HateOverride);

            if (MaxHits > 0)
                result.Add("Max Hits: " + MaxHits);

            if (MaxTargets > 0)
                result.Add("Max Targets: " + MaxTargets);

            if (RecourseID > 0)
                result.Add("Recourse: [Spell " + RecourseID + "]");

            if (Unknown != 0)
                result.Add("Unknown: " + Unknown);


            for (int i = 0; i < Slots.Length; i++)
                if (Slots[i] != null)
                    result.Add(String.Format("{0}: {1}", i + 1, Slots[i]));

            return result.ToArray();
        }

        /// <summary>
        /// Fix fields that do not make sense in the current context.
        /// This may happen when spells are not properly zeroed by the EQ spell editor
        /// </summary>
        public void Clean()
        {
            ClassesLevels = String.Empty;
            ClassesMask = 0;
            for (int i = 0; i < Levels.Length; i++)
            {
                if (Levels[i] == 255)
                    Levels[i] = 0;
                if (Levels[i] != 0)
                {
                    ClassesMask |= (SpellClassesMask)(1 << i);
                    ClassesLevels += " " + (SpellClasses)(i + 1) + "/" + Levels[i];
                }
            }
            ClassesLevels = ClassesLevels.TrimStart();

            if (Target == SpellTarget.Self)
            {
                Range = 0;
                MaxTargets = 0;
                HateOverride = 0; // a bunch of self only AAs have 1 hate
            }

            if (Target == SpellTarget.Single)
            {
                AERange = 0;
                MaxTargets = 0;
            }

            if (Zone != SpellZoneRestrict.Indoors && Zone != SpellZoneRestrict.Outdoors)
                Zone = SpellZoneRestrict.None;
        }

        /// <summary>
        /// Parse a spell effect slot. Each spell has 12 of effect slots with associated attributes.
        /// Base spell attributes like ID, Skill, Extra, Ticks are referenced and should be set before
        /// calling this function.
        /// </summary>        
        public string ParseSlot(int type, int base1, int base2, int max, int calc, int level)
        {
            // type 254 indicates an unused slot
            if (type == 254)
                return null;

            // type 10 sometimes indicates an unused slot
            if (type == 10 && (base1 <= 1 || base1 > 255))
                return null;

            // many spells use a scaled value based on either current tick or caster level
            // the switch(type) below determines if an effect uses the scaled value, or the original base1 value
            // decaying/growing spells are shown at their average strength (i.e. ticks / 2)
            int value = CalcValue(calc, base1, max, DurationTicks / 2, level);

            // prepare a comment for effects that do not have a constant value
            // this is only used by hp/mana effects
            string variable = "";

            if (calc == 123)
                variable = String.Format(" (Random: {0} to {1})", base1, max * ((value >= 0) ? 1 : -1));

            if (calc == 107 || calc == 108 || calc == 120 || calc == 122 || (calc > 1000 && calc < 2000))
            {
                int start = CalcValue(calc, base1, max, 1, level);
                int finish = CalcValue(calc, base1, max, DurationTicks, level);
                variable = String.Format(" ({2}: {0} to {1})", start, finish, Math.Abs(start) < Math.Abs(finish) ? "Growing" : "Decaying");
                // +String.Format("Unknown Effect: {0} Base1={1} Base2={2} Max={3} Calc={4} Value={5}", type, base1, base2, max, calc, value); 
            }

            // prepare a comment for effects that repeat for each tick of the duration
            // this is only used by effects that modify hp/mana/end/hate 
            string repeating = (DurationTicks > 0) ? " per tick" : null;

            switch (type)
            {
                case 0:
                    // delta hp for heal/nuke, repeating if with duration
                    if (base2 > 0)
                        return Spell.FormatCount("Current HP", value) + repeating + variable + " (If " + Spell.FormatEnum((SpellTargetRestrict)base2) + ")";
                    return Spell.FormatCount("Current HP", value) + repeating + variable;
                case 1:
                    return Spell.FormatCount("AC", (int)(value / (10f / 3f)));
                case 2:
                    return Spell.FormatCount("ATK", value);
                case 3:
                    return Spell.FormatPercent("Movement Speed", value);
                case 4:
                    return Spell.FormatCount("STR", value);
                case 5:
                    return Spell.FormatCount("DEX", value);
                case 6:
                    return Spell.FormatCount("AGI", value);
                case 7:
                    return Spell.FormatCount("STA", value);
                case 8:
                    return Spell.FormatCount("INT", value);
                case 9:
                    return Spell.FormatCount("WIS", value);
                case 10:
                    return Spell.FormatCount("CHA", value);
                case 11:
                    // base attack speed is 100. so 85 = 15% slow, 130 = 30% haste 
                    return Spell.FormatPercent("Melee Haste", value - 100);
                case 12:
                    return "Invisible (Unstable)";
                case 13:
                    if (value > 1)
                        return "See Invisible (Enhanced)";
                    return "See Invisible";
                case 14:
                    return "Enduring Breath";
                case 15:
                    return Spell.FormatCount("Current Mana", value) + repeating + variable;
                case 18:
                    return "Pacify";
                case 19:
                    return Spell.FormatCount("Faction", value);
                case 20:
                    return "Blind";
                case 21:
                    if (max == 0 && ClassesMask != 0)
                        max = 55;
                    return String.Format("Stun for {0}s", value / 1000f) + (max > 0 ? String.Format(" up to level {0}", max) : "");
                case 22:
                    return String.Format("Charm up to level {0}", max);
                case 23:
                    return String.Format("Fear up to level {0}", max);
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
                    return "Invisible to Undead (Unstable)";
                case 29:
                    return "Invisible to Animals (Unstable)";
                case 30:
                    return String.Format("Decrease Aggro Radius to {0} up to level {1}", value, max);
                case 31:
                    return String.Format("Mesmerize up to level {0}", max);
                case 32:
                    //return String.Format("Summon: [Item {0}] x {1}", base1, calc);
                    return String.Format("Summon: [Item {0}]", base1);
                case 33:
                    return String.Format("Summon Pet: {0}", Extra);
                case 35:
                    return Spell.FormatCount("Disease Counter", value);
                case 36:
                    return Spell.FormatCount("Poison Counter", value);
                case 40:
                    return "Invulnerability";
                case 41:
                    return "Destroy";
                case 42:
                    // TODO: how this this work for highsun?
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
                    return String.Format("Absorb Damage: 100% Total: {0}", value);
                case 56:
                    return "True North";
                case 57:
                    return "Levitate";
                case 58:
                    return String.Format("Illusion: {0} ({1})", Spell.FormatEnum((SpellIllusion)base1), base2);
                case 59:
                    return Spell.FormatCount("Damage Shield", -value);
                case 61:
                    return "Identify Item";
                case 63:
                    return String.Format("Memory Blur ({0})", value);
                case 64:
                    if (max == 0 && ClassesMask != 0)
                        max = 55;
                    return String.Format("SpinStun for {0}s", value / 1000f) + Spell.FormatLevel(max);
                case 65:
                    return "Infravision";
                case 66:
                    return "Ultravision";
                case 67:
                    return "Eye of Zomm";
                case 68:
                    return "Reclaim Pet";
                case 69:
                    return Spell.FormatCount("Max HP", value);
                case 71:
                    return String.Format("Summon Pet: {0}", Extra);
                case 73:
                    return "Bind Sight";
                case 74:
                    if (value < 100)
                        return String.Format("Feign Death (Success: {0}%)", value);
                    return "Feign Death";
                case 75:
                    return "Voice Graft";
                case 76:
                    return "Sentinel";
                case 77:
                    return "Locate Corpse";
                case 78:
                    //return Spell.FormatCount("Spell Damage Taken", -value);
                    return String.Format("Absorb Spell Damage: 100% Total: {0}", value);
                case 79:
                    // delta hp for heal/nuke, non repeating
                    if (base2 > 0)
                        return Spell.FormatCount("Current HP", value) + variable + " (If " + Spell.FormatEnum((SpellTargetRestrict)base2) + ")";
                    return Spell.FormatCount("Current HP", value) + variable;
                case 81:
                    return String.Format("Resurrection: {0}%", value);
                case 82:
                    return "Call of Hero";
                case 83:
                    return String.Format("Teleport to {0}", Extra);
                case 84:
                    return "Gravity Flux";
                case 85:
                    if (base2 > 0)
                        return String.Format("Add Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                    return String.Format("Add Proc: [Spell {0}]", base1);
                case 86:
                    return String.Format("Decrease Social Radius to {0} up to level {1}", value, max);
                case 87:
                    return Spell.FormatPercent("Magnification", value);
                case 88:
                    return String.Format("Evacuate to {0}", Extra);
                case 89:
                    return Spell.FormatPercent("Player Size", value - 100);
                case 91:
                    return String.Format("Summon Corpse up to level {0}", value);
                case 92:
                    return Spell.FormatCount("Hate", value);
                case 93:
                    return "Stop Rain";
                case 94:
                    return "Fade If Combat Initiated";
                case 95:
                    return "Sacrifice";
                case 96:
                    // silence, but named this way to match melee version
                    return "Prevent Spell Casting";
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
                        return Spell.FormatCount("Current HP", value) + repeating + variable + " (If " + Spell.FormatEnum((SpellTargetRestrict)base2) + ")";
                    return Spell.FormatCount("Current HP", value) + repeating + variable;
                case 101:
                    // only castable via Donal's BP. creates a buf that blocks recasting
                    return "Donal's Heal";
                case 102:
                    return "Fear Immunity";
                case 103:
                    return "Summon Pet";
                case 104:
                    return String.Format("Translocate to {0}", Extra);
                case 105:
                    return "Prevent Gate";
                case 106:
                    return String.Format("Summon Warder: {0}", Extra);
                case 108:
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
                    return Spell.FormatPercent("Aggro Multiplier", value);
                case 115:
                    return "Feed Hunger";
                case 116:
                    return Spell.FormatCount("Curse Counter", value);
                case 119:
                    return Spell.FormatPercent("Melee Haste v3", value);
                case 120:
                    return Spell.FormatPercent("Healing Taken", value);
                case 121:
                    // damages the target whenever it hits something
                    return Spell.FormatCount("Reverse Damage Shield", -value);
                case 123:
                    return "Screech";
                case 124:
                    return String.Format("{0} Spell Damage by {1}% to {2}%", value >= 0 ? "Increase" : "Decrease", value, base2);
                case 125:
                    return String.Format("{0} Healing by {1}% to {2}%", value >= 0 ? "Increase" : "Decrease", value, base2);
                case 126:
                    return Spell.FormatPercent("Spell Resist Rate", -value);
                case 127:
                    return Spell.FormatPercent("Spell Haste", value);
                case 128:
                    return Spell.FormatPercent("Spell Duration", value);
                case 129:
                    return Spell.FormatPercent("Spell Range", value);
                case 130:
                    return Spell.FormatPercent("Spell and Bash Hate", value);
                case 131:
                    return Spell.FormatPercent("Chance of Using Reagent", -value);
                case 132:
                    return String.Format("{0} Spell Mana Cost by {1}% to {2}%", value < 0 ? "Increase" : "Decrease", Math.Abs(value), Math.Abs(base2));
                case 134:
                    return String.Format("Limit Max Level: {0} (lose {1}% per level)", base1, base2);
                case 135:
                    return String.Format("Limit Resist: {0}", (SpellResist)value);
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
                case 147:
                    return String.Format("Increase Current HP by {1} Max: {0}% ", value, max);
                case 148:
                    //if (max > 1000) max -= 1000;                    
                    return String.Format("Stacking: Block new spell if slot {0} is '{1}' and < {2}", calc % 100, Spell.FormatEnum((SpellEffect)base1), max);
                case 149:
                    //if (max > 1000) max -= 1000;                    
                    return String.Format("Stacking: Overwrite existing spell if slot {0} is '{1}' and < {2}", calc % 100, Spell.FormatEnum((SpellEffect)base1), max);
                case 150:
                    return "Divine Intervention";
                case 151:
                    return "Suspend Pet";
                case 152:
                    return String.Format("Summon Pet: {0} x {1} for {2}s", Extra, value, max);
                case 153:
                    return String.Format("Balance Group HP with {0}% Penalty", value);
                case 154:
                    return String.Format("Cure Detrimental ({0})", value);
                case 156:
                    return "Illusion: Target";
                case 157:
                    return Spell.FormatCount("Spell Damage Shield", -value);
                case 158:
                    if (max < value)
                        value = max;
                    return Spell.FormatPercent("Chance to Reflect Spell", value);
                case 159:
                    return Spell.FormatCount("Base Stats", value);
                case 160:
                    return String.Format("Intoxicate if Tolerance < {0}", value);
                case 161:
                    if (max > 0)
                        return String.Format("Absorb Spell Damage: {0}% Total: {1}", value, max);
                    return String.Format("Absorb Spell Damage: {0}%", value);
                case 162:
                    if (max > 0)
                        return String.Format("Absorb Melee Damage: {0}% Total: {1}", value, max);
                    return String.Format("Absorb Melee Damage: {0}%", value);
                case 163:
                    return String.Format("Absorb {0} Hits or Spells", value);
                case 164:
                    return "Appraise Chest";
                case 165:
                    return "Disarm Chest";
                case 166:
                    return "Unlock Chest";
                case 167:
                    return String.Format("Pet Power ({0})", value);
                case 168:
                    // how is this different than an endless rune?
                    return Spell.FormatPercent("Melee Mitigation", -value);
                case 169:
                    if ((SpellSkill)base2 != SpellSkill.Hit)
                        return Spell.FormatPercent("Chance to Critical Hit with " + Spell.FormatEnum((SpellSkill)base2), value);
                    return Spell.FormatPercent("Chance to Critical Hit", value);
                case 170:
                    return Spell.FormatPercent("Chance to Critical Cast", value);
                case 171:
                    return Spell.FormatPercent("Chance to Crippling Blow", value);
                case 172:
                    return Spell.FormatPercent("Chance to Avoid Melee", value);
                case 173:
                    return Spell.FormatPercent("Chance to Riposte", value);
                case 174:
                    return Spell.FormatPercent("Chance to Dodge", value);
                case 175:
                    return Spell.FormatPercent("Chance to Parry", value);
                case 176:
                    return Spell.FormatPercent("Chance to Dual Wield", value);
                case 177:
                    return Spell.FormatPercent("Chance to Double Attack", value);
                case 178:
                    return String.Format("Lifetap from Weapon Damage: {0}%", value);
                case 179:
                    return String.Format("Instrument Modifier: {0} {1}", Skill, value);
                case 180:
                    return Spell.FormatPercent("Chance to Resist Spell", value);
                case 181:
                    return Spell.FormatPercent("Chance to Resist Fear Spell", value);
                case 182:
                    return Spell.FormatPercent("Weapon Delay", value);
                case 183:
                    return Spell.FormatPercent("Skill Check for " + Spell.FormatEnum((SpellSkill)base2), value);
                case 184:
                    if ((SpellSkill)base2 != SpellSkill.Hit)
                        return Spell.FormatPercent("Chance to Hit with " + Spell.FormatEnum((SpellSkill)base2), value);
                    return Spell.FormatPercent("Chance to Hit", value);
                case 185:
                    return Spell.FormatPercent(Spell.FormatEnum((SpellSkill)base2) + " Damage", value);
                case 186:
                    return Spell.FormatPercent("Min " + Spell.FormatEnum((SpellSkill)base2) + " Damage", value); // only DI1?
                case 188:
                    return Spell.FormatPercent("Chance to Block", value);
                case 189:
                    return Spell.FormatCount("Current Endurance", value) + repeating + variable;
                case 190:
                    return Spell.FormatCount("Max Endurance", value);
                case 191:
                    return "Prevent Combat";
                case 192:
                    return Spell.FormatCount("Hate", value) + repeating + variable;
                case 193:
                    return String.Format("{0} Attack for {1} with {2}% Accuracy Mod", Spell.FormatEnum(Skill), value, base2);
                case 194:
                    if (value < 100)
                        return String.Format("Wipe Aggro (Success: {0}%)", value);
                    return "Wipe Aggro";
                case 195:
                    // 100 is full resist. not sure why some spells have more
                    return String.Format("Stun Resist ({0})", value);
                case 196:
                    return String.Format("Srikethrough ({0})", value);
                case 197:
                    return Spell.FormatPercent(Spell.FormatEnum((SpellSkill)base2) + " Damage Taken", value);
                case 198:
                    return Spell.FormatCount("Current Endurance", value);
                case 199:
                    return String.Format("Taunt ({0})", value);
                case 200:
                    return Spell.FormatPercent("Proc Rate", value);
                case 201:
                    return String.Format("Add Range Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                case 202:
                    return "Project Illusion";
                case 203:
                    return "Mass Group Buff";
                case 204:
                    return String.Format("Group Fear Immunity ({0})", value);
                case 206:
                    return "AE Taunt";
                case 209:
                    return String.Format("Dispel Beneficial ({0})", value);
                case 214:
                    return Spell.FormatPercent("Max HP", value / 100);
                case 216:
                    return Spell.FormatPercent("Accuracy", value);
                case 220:
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Damage Bonus", value);
                case 227:
                    return String.Format("Reduce {0} Timer by {1}s", Spell.FormatEnum((SpellSkill)base2), value);
                case 232:
                    return String.Format("Divine Save: [Spell {0}] Chance: {1}%", base2, base1);
                case 233:
                    return Spell.FormatPercent("Food Consumption", -value);
                case 243:
                    return Spell.FormatPercent("Chance of Charm Breaking", -value);
                case 258:
                    return String.Format("Triple Backstab ({0})", value);
                case 262:
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkillCap)base2) + " Cap", value);
                case 272:
                    return Spell.FormatPercent("Spell Casting Skill", value);
                case 273:
                    return Spell.FormatPercent("Chance to Critical DoT", value);
                case 274:
                    return Spell.FormatPercent("Chance to Critical Heal", value);
                case 279:
                    return Spell.FormatPercent("Chance to Flurry", value);
                case 286:
                    // this seems to be a total. so if it affects a dot, it should be divided by number of ticks
                    // amount is added after crits. after focus too?
                    return Spell.FormatCount("Spell Damage Bonus", value);
                case 287:
                    return String.Format("Increase Duration by {0}s", value * 6);
                case 289:
                    // how is this different than 373? if base2 > 0, what is base1?                                       
                    return String.Format("Cast on Fade: [Spell {0}]", (base2 > 0) ? base2 : base1);
                case 291:
                    return String.Format("Dispel Detrimental ({0})", value);
                case 294:
                    if (value > 0 && base2 > 100)
                        return Spell.FormatPercent("Chance to Critical Nuke", value) + " and " + Spell.FormatPercent("Critical Nuke Damage", base2 - 100);
                    else if (value > 0)
                        return Spell.FormatPercent("Chance to Critical Nuke", value);
                    else
                        return Spell.FormatPercent("Critical Nuke Damage", base2 - 100);
                case 296:
                    return Spell.FormatPercent("Spell Damage Taken", value);
                case 297:
                    return Spell.FormatCount("Spell Damage Taken", value);
                case 298:
                    return Spell.FormatPercent("Pet Size", value - 100);
                case 299:
                    return String.Format("Wake the Dead ({0})", max);
                case 300:
                    return "Doppelganger";
                case 303:
                    // this seems to be a total. so if it affects a dot, it should be divided by number of ticks
                    return Spell.FormatCount("Spell Damage", value);
                case 305:
                    return Spell.FormatCount("Damage Shield Taken", -Math.Abs(value));
                case 310:
                    return String.Format("Reduce Timer by {0}s", value / 1000f);
                case 311:
                    // does this affect procs that the caster can also cast as spells?
                    return "Limit Type: Exclude Procs";
                case 314:
                    return "Invisible (Permanent)";
                case 315:
                    return "Invisible to Undead (Permanent)";
                case 319:
                    return Spell.FormatPercent("Chance to Critical HoT", value);
                case 320:
                    return String.Format("Shield Block ({0})", value);
                case 322:
                    return "Gate to Home City";
                case 323:
                    if (base2 > 0)
                        return String.Format("Add Defensive Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                    return String.Format("Add Defensive Proc: [Spell {0}]", base1);
                case 324:
                    // blood magic. uses HP as mana
                    return String.Format("Cast from HP with {0}% penalty", value);
                case 328:
                    return Spell.FormatCount("Max Negative HP", value);
                case 329:
                    return String.Format("Absorb Damage Using Mana: {0}%", value);
                case 330:
                    return Spell.FormatPercent("Critical " + Spell.FormatEnum((SpellSkill)base2) + " Damage", value);
                case 333:
                    // so far this is only used on spells that have a rune
                    return String.Format("Cast on Rune Depleted: [Spell {0}]", base1);
                case 335:
                    return "Prevent Matching Spells From Landing";
                case 337:
                    return Spell.FormatPercent("Experience Gain", value);
                case 339:
                    // how is this different than 383? (besides chance)
                    return String.Format("Cast on Matching Spell: [Spell {0}] Chance: {1}%", base2, base1);
                case 340:
                    if (base1 < 100)
                        return String.Format("Cast: [Spell {0}] Chance: {1}%", base2, base1);
                    return String.Format("Cast: [Spell {0}]", base2);
                case 342:
                    return "Prevent Fleeing";
                case 343:
                    return String.Format("Interrupt Spell Chance: {0}%", value);
                case 348:
                    return String.Format("Limit Min Mana Cost: {0}", base1);
                case 350:
                    return String.Format("Mana Burn: {0}", value);
                case 351:
                    // the +3 is just a guess that's correct most of the time since spells have 3 ranks
                    // and the effects are placed after the spells                    
                    int id = (Rank >= 1) || Extra.Contains("Rk") ? ID + 3 : ID + 1;
                    return String.Format("Aura Effect: [Spell {0}] ({1})", id, Extra);
                case 353:
                    return Spell.FormatCount("Aura Slots", value);
                case 358:
                    return Spell.FormatCount("Current Mana", value) + variable;
                case 360:
                    return String.Format("Add Killshot Proc: [Spell {0}] Chance: {1}%", base2, base1);
                case 361:
                    return String.Format("Cast on Death: [Spell {0}] Chance: {1}%", base2, base1);
                case 365:
                    return String.Format("Cast on Killshot: [Spell {0}] Chance: {1}%", base2, base1);
                case 367:
                    return String.Format("Transform Body Type ({0})", value);
                case 368:
                    return String.Format("Faction {0} Modifier: {1}", base1, base2);
                case 369:
                    return Spell.FormatCount("Corruption Counter", value);
                case 370:
                    return Spell.FormatCount("Corruption Resist", value);
                case 371:
                    return Spell.FormatPercent("Melee Delay", Math.Abs(value));
                case 373:
                    // this appears to be used when a spell is removed via any method: times out, cured, rune used up 
                    return String.Format("Cast on Fade: [Spell {0}]", base1);
                case 374:
                    if (base1 < 100)
                        return String.Format("Cast: [Spell {0}] Chance: {1}%", base2, base1);
                    return String.Format("Cast: [Spell {0}]", base2);
                case 375:
                    return Spell.FormatPercent("Critical DoT Damage", value - 100);
                case 377:
                    return String.Format("Cast if Not Cured: [Spell {0}]", base1);
                case 379:
                    if (base2 > 0)
                        return String.Format("Knockback for {0} in Direction: {1}", value, base2);
                    return String.Format("Knockback for {0}", value);
                case 380:
                    return String.Format("Knockback for {0} and up for {1}", value, base2);
                case 381:
                    return "Call of Hero";
                case 382:
                    return String.Format("Inhibit Buff Effect: {0}", Spell.FormatEnum((SpellEffect)base2));
                case 383:
                    // e.g. Has a chance to cast a mana recovering spell every time a spell is cast.
                    return String.Format("Cast on Matching Spell: [Spell {0}] Chance: {1}%", base2, base1 / 10);
                case 385:
                    return String.Format("Limit Spells: {1}[Group {0}]", Math.Abs(value), value >= 0 ? "" : "Exclude ");
                case 386:
                    return String.Format("Cast on Curer: [Spell {0}]", base1);
                case 387:
                    return String.Format("Cast if Cured: [Spell {0}]", base1);
                case 392:
                    return Spell.FormatCount("Healing Taken", value);
                case 393:
                    return Spell.FormatPercent("Healing Taken", value);

                case 396:
                    // used on type 3 augments
                    return Spell.FormatCount("Healing", value);
                case 398:
                    return String.Format("Increase Pet Duration by {0}s", value / 1000);
                case 399:
                    return Spell.FormatPercent("Chance to Twincast", value);
                case 400:
                    // e.g. Channels the power of sunlight, consuming up to #1 mana to heal your group.
                    return String.Format("Use Target's Mana to Heal Group ({0}:{1})", value, Math.Floor(value * base2 / 10f));
                case 401:
                    // e.g. Drains up to 401 mana from your target. For each point of mana drained, the target will take damage.
                    return String.Format("Use Target's Mana to Inflict Damage ({0}:{1})", value, Math.Floor(value * base2 / -10f));
                case 402:
                    // e.g. Consumes up to #6 endurance and inflicts damage for each point of endurance consumed.
                    return String.Format("Use Target's Endurance to Inflict Damage ({0}:{1})", value, Math.Floor(value * base2 / -10f));
                case 404:
                    return String.Format("Limit Skill: {1}{0}", Spell.FormatEnum((SpellSkill)Math.Abs(base1)), base1 >= 0 ? "" : "Exclude ");
                case 406:
                    return String.Format("Cast if Attacked: [Spell {0}]", base1);
                case 408:
                    // unlike 214, this does not show a lower max HP
                    return String.Format("Cap HP at Lowest of {0}% or {1} ", base1, base2);
                case 409:
                    return String.Format("Cap Mana at Lowest of {0}% or {1} ", base1, base2);
                case 410:
                    return String.Format("Cap Endurance at Lowest of {0}% or {1} ", base1, base2);
                case 411:
                    return String.Format("Limit Class: {0}", (SpellClassesMask)(value >> 1));
                case 413:
                    return Spell.FormatPercent("Spell Effectiveness", value);
                case 414:
                    return String.Format("Limit Bard Skill: {0}", Spell.FormatEnum((SpellSkill)base1));
                case 417:
                    // how is this different than 15?
                    return Spell.FormatCount("Current Mana", value) + repeating + variable;
                case 418:
                    // how is this different than 220 bonus? 
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Damage Bonus", value);
                case 419:
                    // this is used for potions. how is it different than 85? maybe proc rate?
                    return String.Format("Add Proc: [Spell {0}] (Unknown: {1})", base1, base2);
                case 424:
                    return String.Format("Gradual Knockback for {0} (Unknown: {1})", base1, base2);
                case 427:
                    return String.Format("Cast on Skill Use: [Spell {0}] Chance: {1}%", base1, base2 / 10);
                case 428:
                    return String.Format("Limit Skill: {0}", Spell.FormatEnum((SpellSkill)value));

            }

            return String.Format("Unknown Effect: {0} Base1={1} Base2={2} Max={3} Calc={4} Value={5}", type, base1, base2, max, calc, value);
        }

        /// <summary>
        /// Parse a spell duration formula.
        /// </summary>
        /// <returns>Numbers of ticks (6 second units)</returns>        
        static public int CalcDuration(int calc, int max, int level)
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
        /// Parse an effect slot value formula.
        /// </summary>
        static public int CalcValue(int calc, int value, int max, int tick, int level)
        {
            // the default calculation (100) leaves the base value as is            
            if (calc == 0 || calc == 100)
                return value;

            int start = value;
            int change = 0;

            switch (calc)
            {
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
                    change = (Math.Abs(max) - Math.Abs(value)) / 2;
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

            //if (Math.Abs(value) >= 100)
                value = Math.Abs(value) + change;

            if (max != 0 && value > Math.Abs(max))            
                value = Math.Abs(max);

            if (start < 0)
                value = -value;

            return value;
        }

        static private string FormatEnum(object o)
        {
            //if (Char.IsDigit(o.ToString()[0])) Console.Error.WriteLine(o);
            return o.ToString().Replace("_", " ").Trim();
        }

        static private string FormatTime(float seconds)
        {
            if (seconds <= 60)
                return seconds.ToString() + "s";
            return new TimeSpan(0, 0, (int)seconds).ToString();
        }

        static private string FormatCount(string name, int count)
        {
            if (count >= 0)
                return String.Format("Increase {0} by {1}", name, count);
            else
                return String.Format("Decrease {0} by {1}", name, -count);
        }

        static private string FormatPercent(string name, int count)
        {
            return Spell.FormatCount(name, count) + "%";
        }

        static private string FormatLevel(int level)
        {
            if (level > 0)
                return String.Format(" up to level {0}", level);
            return null;
        }

    }

    public static class SpellParser
    {
        // the spell file is in US culture (dots are used for decimals)
        private static readonly CultureInfo culture = new CultureInfo("en-US", false);

        // shows all spells as if they were cast by a level 85 player
        public const int MaxLevel = 85;

        /// <summary>
        /// Load spell list from the comma delimitted EQ spell definition files.
        /// </summary>
        static public List<Spell> LoadFromFile(string spellPath, string descPath)
        {
            // load description text file
            Dictionary<int, string> desc = new Dictionary<int, string>();
            if (File.Exists(descPath))
                using (StreamReader text = File.OpenText(descPath))
                    while (!text.EndOfStream)
                    {
                        string line = text.ReadLine();
                        string[] fields = line.Split('^');
                        // type 6 is used for spell descriptions
                        if (fields[1] == "6")
                            desc[Int32.Parse(fields[0])] = fields[2].Trim();
                    }


            // load spell definition file
            List<Spell> list = new List<Spell>();
            if (File.Exists(spellPath))
                using (StreamReader text = File.OpenText(spellPath))
                    while (!text.EndOfStream)
                    {
                        string line = text.ReadLine();
                        string[] fields = line.Split('^');
                        Spell spell = ParseFields(fields);
                        if (!desc.TryGetValue(spell.DescID, out spell.Desc))
                            spell.Desc = spell.DescID.ToString();
                        list.Add(spell);
                    }

            return list;
        }

        /// <summary>
        /// Parse a spell from a set of spell fields. 
        /// </summary>        
        static Spell ParseFields(string[] fields)
        {
            Spell spell = new Spell();

            spell.ID = Convert.ToInt32(fields[0]);
            spell.Name = fields[1];
            spell.Extra = fields[3];
            //spell.LandOnSelf = fields[6];
            //spell.LandOnOther = fields[7];
            spell.DurationTicks = Spell.CalcDuration(ParseInt(fields[16]), ParseInt(fields[17]), MaxLevel);
            spell.Mana = ParseInt(fields[19]);

            spell.Range = ParseInt(fields[9]);
            spell.AERange = ParseInt(fields[10]);
            spell.PushBack = ParseFloat(fields[11]);
            spell.PushUp = ParseFloat(fields[12]);
            spell.CastingTime = ParseFloat(fields[13]) / 1000f;
            spell.QuietTime = ParseFloat(fields[14]) / 1000f;
            spell.RecastTime = ParseFloat(fields[15]) / 1000f;
            spell.AEDuration = ParseInt(fields[18]);

            for (int i = 0; i < 3; i++)
            {
                spell.RegID[i] = ParseInt(fields[58 + i]);
                spell.RegCount[i] = ParseInt(fields[62 + i]);
            }

            for (int i = 0; i < 3; i++)
                spell.FocusID[i] = ParseInt(fields[66 + i]);

            spell.Beneficial = ParseBool(fields[83]);
            spell.ResistType = (SpellResist)ParseInt(fields[85]);
            spell.Target = (SpellTarget)ParseInt(fields[98]);
            spell.Skill = (SpellSkill)ParseInt(fields[100]);
            spell.Zone = (SpellZoneRestrict)ParseInt(fields[101]);
            spell.Icon = ParseInt(fields[145]);
            spell.ResistMod = ParseInt(fields[147]);
            spell.RecourseID = ParseInt(fields[150]);
            spell.DescID = ParseInt(fields[155]);
            spell.Endurance = ParseInt(fields[166]);
            spell.TimerID = ParseInt(fields[167]);
            spell.HateOverride = ParseInt(fields[173]);
            spell.EnduranceUpkeep = ParseInt(fields[174]);
            spell.MaxHits = ParseInt(fields[176]);
            spell.MGBable = ParseBool(fields[185]);
            spell.ViralPulse = ParseInt(fields[191]);
            //spell.ViralMaxSpread = ParseInt(fields[192]);
            spell.StartDegree = ParseInt(fields[194]);
            spell.EndDegree = ParseInt(fields[195]);
            spell.DurationExtendable = !ParseBool(fields[197]);
            spell.ViralRange = ParseInt(fields[201]);
            // 202 = bard related
            spell.BeneficialBlockable = !ParseBool(fields[205]); // for beneficial spells
            spell.GroupID = ParseInt(fields[207]);
            spell.Rank = ParseInt(fields[208]); // rank 1/5/10. a few auras do not have this set properly
            spell.TargetRestrict = (SpellTargetRestrict)ParseInt(fields[211]);
            spell.OutOfCombat = !ParseBool(fields[214]);
            spell.MaxTargets = ParseInt(fields[218]);
            spell.ProcRestrict = (SpellTargetRestrict)ParseInt(fields[220]);  // field 206/216 seems to be related

            // debug stuff
            //spell.Unknown = ParseFloat(fields[18]);

            // each spell has a different casting level for all 16 classes
            for (int i = 0; i < spell.Levels.Length; i++)
                spell.Levels[i] = ParseInt(fields[104 + i]);

            // each spell has 12 effect slots:
            // 86..97 - slot 1..12 type
            // 20..31 - slot 1..12 base effect
            // 32..43 - slot 1..12 base_2 effect
            // 44..55 - slot 1..12 max effect
            // 70..81 - slot 1..12 calc forumla data            
            for (int i = 0; i < spell.Slots.Length; i++)
            {
                int type = ParseInt(fields[86 + i]);
                int calc = ParseInt(fields[70 + i]);
                int max = ParseInt(fields[44 + i]);
                int value = ParseInt(fields[20 + i]);
                int value2 = ParseInt(fields[32 + i]);

                spell.SlotEffects[i] = type;
                spell.Slots[i] = spell.ParseSlot(type, value, value2, max, calc, MaxLevel);

            }

            // debug stuff
            //if (spell.ID == 16548)
            //    for (int i = 0; i < fields.Length; i++)
            //        Console.WriteLine("{0}: {1}", i, fields[i]);

            spell.Clean();

            return spell;
        }

        static float ParseFloat(string s)
        {
            if (String.IsNullOrEmpty(s))
                return 0f;
            return Single.Parse(s, culture);
        }

        static int ParseInt(string s)
        {
            if (String.IsNullOrEmpty(s))
                return 0;
            return (int)Single.Parse(s, culture);
        }

        static bool ParseBool(string s)
        {
            return !String.IsNullOrEmpty(s) && (s != "0");
        }

    }


}
