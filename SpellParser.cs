using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;



/*
 * EQEmu has parsed a lot of the spell effects and calculations
 * http://code.google.com/p/projecteqemu/source/browse/trunk/EQEmuServer/zone/spdat.h
 * http://sourceforge.net/projects/eqemulator/files/OpenZone/OpenSpell_2.0/OpenSpell_2.0.zip/download
 *
 */



namespace parser
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
        Current_Mana_Repeating = 15,
        Charm = 22,
        Fear = 23,
        Mesmerize = 31,
        Summon_Item = 32,
        Summon_Pet = 33,
        Disease_Counter = 35,
        Poison_Counter = 36,
        Invulnerability = 40,
        Promised_Heal_Marker = 44,
        Fire_Resist = 46,
        Cold_Resist = 47,
        Poison_Resist = 48,
        Disease_Resist = 49,
        Magic_Resist = 50,
        Rune = 55,
        Levitate = 57,
        Damage_Shield = 59,
        Summon_Skeleton_Pet = 71,
        Assist_Radius = 86,
        Max_HP = 69,
        Hate = 92,
        Max_Mana = 97,
        Current_HP_Repeating = 100,
        Current_HP_Donals = 101,
        All_Resists = 111,
        Current_HP_Percent = 147,
        XP_Gain = 337,
        Mana_Burn = 350,
        Current_Mana = 358,
        Corruption_Counter = 369,
        Corruption_Resist = 370
    }

    public enum SpellSkill
    {
        Melee = -1,
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
        Target_Group = 41,
        Directional = 42,
        Single_In_Group = 43,
        Targets_Target = 46
    }

    public enum SpellTargetRestrict
    {
        Animal_Humanoid = 100,
        Dragon = 101,
        Animal_Insect = 102,
        Animal = 104,
        Plant = 105,
        Giant = 106,
        Bixie = 109,
        Harpy = 110,
        Sporali = 112,
        Kobald = 113,
        Shade = 114,
        Drakkin = 115,
        Animal_Plant = 117,
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
        HP_Below_35_Percent = 250,
        Chain_Plate_Classes = 304,
        HP_Between_55_65_Percent = 404,
        HP_Between_45_55_Percent = 403,
        HP_Between_35_45_Percent = 402,
        HP_Between_25_35_Percent = 401,
        HP_Between_1_25_Percent = 400,
        HP_Between_1_35_Percent = 507, // between or below?
        Undead2 = 603,
        Summoned2 = 624
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
        Hideous_Harpy = 527
    }

    public class SpellSlot
    {
        public int Effect;
        public int Value;
        public int Value2;
        public int Calc;
        public int Max;
        public string Desc;

        public override string ToString()
        {
            return Desc;
        }
    }

    public class Spell
    {
        public int ID;
        public int GroupID;
        public string Name;
        public int Icon;
        public int Mana;
        public int Endurance;
        public int Ticks;
        public string[] Slots;
        public int[] SlotEffects;
        
        public int MinLevel, MaxLevel;
        public int[] Levels;
        public string Classes;
        public SpellSkill Skill;
        public bool Beneficial;
        public SpellTarget Target;
        public SpellResist ResistType;
        public int ResistMod;
        public string Extra;
        public int Hate;
        public int Range;
        public int AERange;
        public float CastingTime;
        public float QuietTime;
        public float RecastTime;
        public float PushBack;
        public int DescID;
        public string Desc;
        public int MaxHits;
        public int MaxTargets;
        public int RecourseID; 
        public int TimerID;
        public int ViralPulse;
        public int ViralRange;
        public SpellTargetRestrict TargetRestrict;
        public int Reg1ID;
        public int Reg1Count;

        public int Unknown;

        //public string Focus;        

        public Spell()
        {
            Slots = new string[12];
            SlotEffects = new int[12];
            Levels = new int[16];
        }

        /// <summary>
        /// Get a full description of the spell. This is mostly useful as a debug dump.
        /// </summary>        
        public string[] Details()
        {
            List<string> result = new List<string>();
     
            if (!String.IsNullOrEmpty(Classes))
                result.Add("Classes: " + Classes);

            if (Mana > 0)
                result.Add("Mana: " + Mana);

            if (Endurance > 0)
                result.Add("Endurance: " + Endurance);

            if (Reg1ID > 0)
                result.Add("Regeant: [Item " + Reg1ID + "] x " + Reg1Count);

            //result.Add("Skill: " + Skill);

            if (TargetRestrict > 0)
                result.Add("Target: " + Target + " (" + TargetRestrict + ")");
            else
                result.Add("Target: " + Target);

            if (Range > 0)
                result.Add("Range: " + Range);

            //if (AERange > 0)
            //    result.Add("AE Range: " + AERange);

            if (ViralRange > 0)
                result.Add("Viral Range: " + ViralRange + ", Recast: " + ViralPulse + "s");
                        
            if (ResistType != SpellResist.Unresistable && ResistMod != 0)
                result.Add("Resist: " + ResistType + " " + ResistMod);
            else if (ResistType != SpellResist.Unresistable)
                result.Add("Resist: " + ResistType);

            if (TimerID > 0)
                result.Add("Casting: " + CastingTime.ToString() + "s, Recast: " + FormatTime(RecastTime) + ", Timer: " + TimerID);
            else if (RecastTime > 0)
                result.Add("Casting: " + CastingTime.ToString() + "s, Recast: " + FormatTime(RecastTime));
            else
                result.Add("Casting: " + CastingTime.ToString() + "s");

            if (Ticks > 0)
                result.Add("Duration: " + FormatTime(Ticks * 6) + " (" + Ticks + " ticks)");

            if (PushBack != 0)
                result.Add("Push: " + PushBack);

            if (Hate != 0)
                result.Add("Hate: " + Hate);

            if (MaxHits > 0)
                result.Add("Max Hits: " + MaxHits);

            if (MaxTargets > 0)
                result.Add("Max Targets: " + MaxTargets);

            if (RecourseID > 0)
                result.Add("Recourse: [Spell " + RecourseID + "]");

            if (Unknown > 0)
                result.Add("Unknown: " + Unknown);


            for (int i = 0; i < Slots.Length; i++)
                if (Slots[i] != null)
                    result.Add(String.Format("{0}: {1}", i + 1, Slots[i]));

            //if (!String.IsNullOrEmpty(Desc))
            //    result.Add(Desc);

            return result.ToArray();
        }

        public override string ToString()
        {
            if (GroupID <= 0)
                return String.Format("[{0}] {1}", ID, Name);
            return String.Format("[{0}/{2}] {1}", ID, Name, GroupID);
        }

        static string FormatTime(float seconds)
        {
            if (seconds <= 60)
                return seconds.ToString() + "s";
            return new TimeSpan(0, 0, (int)seconds).ToString();
        }
    }

    public static class SpellParser
    {
        const int MaxLevel = 85;

        /// <summary>
        /// Load spell list from a stream in spell_us.txt format.
        /// </summary>
        public static List<Spell> LoadFromFile(string spellPath, string descPath)
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
                            desc[Int32.Parse(fields[0])] = fields[2];
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
                        desc.TryGetValue(spell.DescID, out spell.Desc);
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
            spell.GroupID = ParseInt(fields[207]);
            spell.Name = fields[1];
            spell.Icon = ParseInt(fields[144]);
            spell.Mana = ParseInt(fields[19]);
            spell.Skill = (SpellSkill)ParseInt(fields[100]);
            spell.Target = (SpellTarget)ParseInt(fields[98]);
            spell.ResistType = (SpellResist)ParseInt(fields[85]);
            spell.ResistMod = ParseInt(fields[147]);
            spell.Beneficial = ParseInt(fields[83]) != 0;
            if (spell.Beneficial)
                spell.ResistType = SpellResist.Unresistable;
            //Console.WriteLine(" Calc: " + fields[16] +" Value: " + fields[17] + "   " + spell.ID + " " + spell.Name);
            spell.Ticks = ParseDurationForumula(ParseInt(fields[16]), ParseInt(fields[17]));
            spell.Extra = fields[3];
            spell.Hate = ParseInt(fields[173]);
            spell.Endurance = ParseInt(fields[166]);
            spell.Range = ParseInt(fields[9]);
            spell.AERange = ParseInt(fields[10]);
            spell.CastingTime = ParseFloat(fields[13]) / 1000f;
            spell.QuietTime = ParseFloat(fields[14]) / 1000f;
            spell.RecastTime = ParseFloat(fields[15]) / 1000f;
            spell.PushBack = ParseFloat(fields[11]);
            spell.DescID = ParseInt(fields[155]);
            spell.MaxHits = ParseInt(fields[176]);
            spell.RecourseID = ParseInt(fields[150]);
            spell.TimerID = ParseInt(fields[167]);
            spell.ViralPulse = ParseInt(fields[191]);
            //spell.Viral2 = ParseInt(fields[192]);
            spell.ViralRange = ParseInt(fields[201]);
            spell.MaxTargets = ParseInt(fields[218]);
            spell.TargetRestrict = (SpellTargetRestrict)ParseInt(fields[211]);
            spell.Reg1ID = ParseInt(fields[58]);
            spell.Reg1Count = ParseInt(fields[62]);

            
            //spell.Unknown = ParseInt(fields[222]);

            // fix up data fields the ignored for self targeted spells
            if (spell.Target == SpellTarget.Self)
            {
                spell.Range = 0;
                spell.MaxTargets = 0;
            }

            // each class can have a different level to cast the spell at
            spell.Classes = String.Empty;
            for (int i = 0; i < spell.Levels.Length; i++)
            {
                spell.Levels[i] = ParseInt(fields[104 + i]);
                if (spell.Levels[i] != 0 && spell.Levels[i] != 255)
                    spell.Classes += " " + (SpellClasses)(i + 1) + "/" + spell.Levels[i];
            }           
            spell.Classes = spell.Classes.TrimStart();

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

                // some debug stuff
                //if (calc == 110 && max == 0) 
                //if (calc == 100 && max < value && max > 0)
                //    Console.WriteLine("---  " + spell + " " + String.Format("Eff={0} Val={1} Val2={2} Max={3} Calc={4}, {5}", type, value, value2, max, calc, calc & 255));
                      

                spell.SlotEffects[i] = type;
                spell.Slots[i] = ParseSlot(spell, type, value, value2, max, calc);
            }

            // some debug stuff
            //if (spell.ID == 15044)
            //    for (int i = 0; i < fields.Length; i++)
            //        Console.WriteLine("{0}: {1}", i, fields[i]);

            return spell;
        }


        /// <summary>
        /// Parse a spell duration formula.
        /// </summary>
        /// <returns>Numbers of ticks (6 second units)</returns>        
        static int ParseDurationForumula(int calc, int max)
        {
            // show scaled spells at the max level strength
            int level = MaxLevel;
            
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
        /// Parse a spell slot value formula.
        /// </summary>
        static int ParseValueFormula(int calc, int value, int max, int duration)
        {
            // the default calculation (100) leaves the base value as is            
            if (calc == 0 || calc == 100)
                return value;

            // show scaled spells at the max level strength
            int level = MaxLevel;

            // show decaying/growing spells at their avg strength
            int ticks = duration / 2;

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
                    change = -1 * ticks;
                    break;
                case 108:
                    change = -2 * ticks;
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
                    change = -5 * ticks;
                    break;
                case 121:
                    change = level / 3;
                    break;
                case 122:
                    change = -12 * ticks;
                    break;
                case 123:
                    // random in range
                    change = (max - Math.Abs(value)) / 2;
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
                    // TODO: future calcs may be added in the < 1000 range.
                    if (calc > 0 && calc < 1000)
                        change = level * calc;

                    // variable over duration
                    // splort (growing): Current_HP Unknown Calc: Val=1 Val2=0 Max=0 Calc=1035
                    // 34 - 69 - 104 - 139 - 174 - 209 - 244 - 279 - 314 - 349 - 384 - 419 - 454 - 489 - 524 - 559 - 594 - 629 - 664 - 699 - 699 
                    // venonscale (decaying): Current_HP Unknown Calc: Val=-822 Val2=0 Max=822 Calc=1018
                    if (calc >= 1000 && calc < 2000)
                        change = ticks * (calc - 1000) * -1;

                    // more level based calcs
                    if (calc >= 2000)
                        change = level * (calc - 2000);
                    break;
            }

            // if the base value was negative then the change should be subtracted rather then added
            if (value < 0)
                change = -change;            
            value += change;            

            // the max is sometimes given as a positive number even though the value is negative
            if (max < 0)
                max = -max;
            if (max > 0 && value > max)
                value = max;
            if (max > 0 && value < -max)
                value = -max;

            return value;
        }

        /// <summary>
        /// Parses a spell effect slot. Each slot has a series of attributes associated with it.
        /// Effects can reference other spells or items via square bracket notation. e.g.
        /// [Spell 123]    is a reference to spell 123
        /// [Group 123]    is a reference to spell group 123
        /// [Item 123]     is a reference to item 123
        /// </summary>
        /// <returns>A description of the slot effect or a null if the slot has no effect.</returns>
        static string ParseSlot(Spell spell, int type, int value, int value2, int max, int calc)
        {
            // type 254 is used for empty slots
            // type 10 is sometimes used for empty slots
            if (type == 254 || (type == 10 && (value == 0 || value > 255)))                
                return null;

            // type 32 and 109 (summon item) misuse the calc field as a count value
            // type 148 and 149 use the calc field as a effect slot index
            string variable = "";
            if (type != 32 && type != 109 && type != 148 && type != 149)
            {
                value = ParseValueFormula(calc, value, max, spell.Ticks);

                if (calc == 123)
                    variable = " (random/avg)";

                if (calc == 107 || calc == 108 || calc == 120 || calc == 122)
                    variable = " (growing/avg)";

                if (calc > 1000 && calc < 2000)
                    variable = " (variable/avg)";

                //if (calc > 141 && calc < 200)
                //if (calc > 212 && calc < 1000)
                //    return String.Format("{0} Unknown Calc: Val={1} Val2={2} Max={3} Calc={4}", (SpellEffect)type, value, value2, max, calc);
            }

            // some types are repeating if they have a duration. in these cases there is one type that doesn't
            // repeat (hp 79) and one type that does repeat (hp 0). but the repeating types are sometimes used as
            // an instant boost on spells without a duration so we still have to check for the duration.            
            string repeating = (spell.Ticks > 0) ? " per tick" : null;

            switch (type)
            {                
                case 0:
                    // delta hp for heal/nuke, repeating if with duration
                    if (value2 > 0)
                        return FormatCount("Current HP", value) + repeating + variable + " (if " + (SpellTargetRestrict)value2 + ")";
                    return FormatCount("Current HP", value) + repeating + variable;
                case 1:
                    return FormatCount("AC", (int)(value / (10f / 3f))); 
                case 2: 
                    return FormatCount("ATK", value);
                case 3:
                    return FormatPercent("Movement Speed", value);
                case 4:
                    return FormatCount("STR", value);
                case 5:
                    return FormatCount("DEX", value);
                case 6:
                    return FormatCount("AGI", value);
                case 7:
                    return FormatCount("STA", value);
                case 8:
                    return FormatCount("INT", value);
                case 9:
                    return FormatCount("WIS", value);
                case 10:
                    return FormatCount("CHA", value); 
                case 11:
                    // haste is given as a factor of 100. so 85 = 15% slow, 130 = 30% haste 
                    if (value < 100)
                        value = -100 + value;
                    else
                        value = value - 100;
                    return FormatPercent("Melee Haste", value);
                case 12:
                    return "Invisible (Unstable)";
                case 13:
                    if (value > 1)
                        return "See Invisible (Enhanced)";
                    return "See Invisible";
                case 14:
                    return "Enduring Breath";
                case 15:
                    return FormatCount("Current Mana", value) + repeating;
                case 18:
                    return "Pacify";
                case 19:
                    return FormatCount("Faction", value);  
                case 20:
                    return "Blind";
                case 21:
                    if (max == 0)
                        max = 55;
                    return String.Format("Stun for {0}s up to level {1}", value / 1000f, max);
                case 22:
                    return String.Format("Charm up to level {0}", max);
                case 23:
                    return String.Format("Fear up to level {0}", max);
                case 24:
                    return FormatCount("Stamina Loss", value);
                case 25:
                    return "Bind";
                case 26:
                    if (value2 > 1)
                        return "Gate to Secondary Bind";
                    return "Gate";
                case 27:
                    return String.Format("Dispell ({0})", value);
                case 28:
                    return "Invisible to Undead (Unstable)";
                case 29:
                    return "Invisible to Animals (Unstable)";
                case 30:
                    return String.Format("Limit Aggro Radius to {1} up to level {0}", max, value);
                case 31:
                    return String.Format("Mesmerize up to level {0}", max);
                case 32:
                    return String.Format("Summon: [Item {0}]", value);
                case 33:
                    return String.Format("Summon Pet: {0}", spell.Extra);
                case 35:
                    return FormatCount("Disease Counter", value);
                case 36:
                    return FormatCount("Poison Counter", value);
                case 40:
                    return "Invulnerability";
                case 41:
                    return "Destroy";
                case 42:
                    return "Shadowstep";                
                case 44:
                    return String.Format("Stacking: Promised Heal Marker ({0})", value);
                case 46:
                    return FormatCount("Fire Resist", value);
                case 47:
                    return FormatCount("Cold Resist", value);
                case 48:
                    return FormatCount("Poison Resist", value);
                case 49:
                    return FormatCount("Disease Resist", value);
                case 50:
                    return FormatCount("Magic Resist", value);
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
                    return String.Format("Illusion: {0} ({1})", TrimEnum((SpellIllusion)value), value2);
                case 59:
                    return FormatCount("Damage Shield", -value);
                case 61:
                    return "Identify Item";
                case 63:
                    return String.Format("Memory Blur ({0})", value);               
                case 64:
                    if (max == 0)
                        max = 55;
                    return String.Format("SpinStun for {0}s up to level {1}", value / 1000f, max);
                case 65:
                    return "Infravision";
                case 66:
                    return "Ultravision";
                case 67:
                    return "Eye of Zomm";
                case 68:
                    return "Reclaim Pet";
                case 69:                    
                    return FormatCount("Max HP", value);
                case 71:
                    return String.Format("Summon Pet: {0}", spell.Extra);
                case 73:
                    return "Bind Sight";
                case 74:
                    return "Feign Death";
                case 75:
                    return "Voice Graft";
                case 76:
                    return "Sentinel";
                case 77:
                    return "Locate Corpse";
                case 78:
                    // absorbs raw value rather than %. see also 296
                    return FormatCount("Spell Damage Taken", -value);                    
                case 79:
                    // delta hp for heal/nuke, non repeating
                    if (value2 > 0)
                        return FormatCount("Current HP", value) + " (if " + (SpellTargetRestrict)value2 + ")";
                    return FormatCount("Current HP", value);                    
                case 81:
                    return String.Format("Resurrection: {0}%", value); 
                case 82:
                    return "Call of Hero";
                case 83:
                    return String.Format("Teleport to {0}", spell.Extra);
                case 84:
                    return "Gravity Flux";
                case 85:
                    if (value2 > 0)
                        return String.Format("Add Proc: [Spell {0}] with {1}% Rate Mod", value, value2);
                    return String.Format("Add Proc: [Spell {0}]", value);
                case 86:
                    return String.Format("Limit Social Radius to {1} up to level {0}", max, value);
                case 87:
                    return FormatPercent("Magnification", value);
                case 88:
                    return String.Format("Evacuate to {0}", spell.Extra);
                case 89:
                    return FormatPercent("Player Size", value - 100);
                case 91:
                    return String.Format("Summon Corpse up to level {0}", value);
                case 92:
                    return FormatCount("Hate", value);
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
                    return FormatCount("Max Mana", value);
                case 98:
                    // yet another super turbo haste. only on 3 bard songs
                    return FormatPercent("Melee Haste v2", value - 100);
                case 99:
                    return "Root";
                case 100:
                    // heal over time
                    if (value2 > 0)
                        return FormatCount("Current HP", value) + repeating + variable + " (if " + (SpellTargetRestrict)value2 + ")";
                   return FormatCount("Current HP", value) + repeating + variable;
                case 101:
                    // only castable via Donal's BP. creates a buf that blocks recasting
                    return "Donal's Heal"; 
                case 102:
                    return "Fear Immunity";
                case 103:
                    return "Summon Pet";
                case 104:
                    return String.Format("Translocate to {0}", spell.Extra);
                case 105:
                    return "Prevent Gate";
                case 106:
                    return String.Format("Summon Warder: {0}", spell.Extra);
                case 108:
                    return String.Format("Summon Familiar: {0}", spell.Extra);
                case 109:                    
                    return String.Format("Summon: [Item {0}]", value);
                case 111:
                    return FormatCount("All Resists", value);
                case 112:
                    return FormatCount("Effective Casting Level", value);
                case 113:
                    return String.Format("Summon Mount: {0}", spell.Extra);
                case 114:
                    return FormatPercent("Aggro Multiplier", value);
                case 115:
                    return "Feed Hunger";
                case 116:
                    return FormatCount("Curse Counter", value);
                case 119:
                    return FormatPercent("Melee Haste v3", value);
                case 120:
                    // works like healing focus. no idea why it is a separate effect
                    return FormatPercent("Healing Effectiveness", value);
                case 121:
                    // damages the target whenever it hits something
                    return FormatCount("Reverse Damage Shield", -value);
                case 123:
                    return "Screech";
                case 124:
                    return String.Format("{0} Spell Damage by {1}% to {2}%", value >= 0 ? "Increase" : "Decrease", value, value2);
                case 125:                    
                    return String.Format("{0} Healing by {1}% to {2}%", value >= 0 ? "Increase" : "Decrease", value, value2);
                case 126:
                    return FormatPercent("Spell Resist Rate", -value);
                case 127:
                    return FormatPercent("Spell Haste", value);
                case 128:
                    return FormatPercent("Spell Duration", value);
                case 129:
                    return FormatPercent("Spell Range", value);
                case 130:
                    return FormatPercent("Spell and Bash Hate", value);
                case 131:
                    return FormatPercent("Chance of Using Reagent", -value);
                case 132:
                    return String.Format("{0} Spell Mana Cost {1}% to {2}%", value < 0 ? "Increase" : "Decrease", Math.Abs(value), Math.Abs(value2));
                case 134:
                    return String.Format("Limit Max Level: {0} (lose {1}% per level)", value, value2);
                case 135:
                    return String.Format("Limit Resist: {0}", (SpellResist)value);
                case 136:
                    return String.Format("Limit Target: {1}{0}", TrimEnum((SpellTarget)Math.Abs(value)), value >= 0 ? "" : "Exclude ");
                case 137:
                    return String.Format("Limit Effect: {1}{0}", TrimEnum((SpellEffect)Math.Abs(value)), value >= 0 ? "" : "Exclude ");
                case 138:
                    return String.Format("Limit Type: {0}", value == 0 ? "Detrimental" : "Beneficial");
                case 139:
                    return String.Format("Limit Spell: {1}[Spell {0}]", Math.Abs(value), value >= 0 ? "" : "Exclude ");
                case 140:
                    return String.Format("Limit Min Duration: {0}s", value * 6);
                case 141:
                    return String.Format("Limit Max Duration: {0}s", 0); 
                case 142:
                    return String.Format("Limit Min Level: {0}", value);
                case 143:
                    return String.Format("Limit Min Casting Time: {0}s", value / 1000f);
                case 144:
                    return String.Format("Limit Max Casting Time: {0}s", value / 1000f);
                case 145:
                    return String.Format("Teleport to {0}", spell.Extra);
                case 147:
                    return String.Format("Increase Current HP by {1} Max: {0}% ", value, max);
                case 148:
                    //if (max > 1000) max -= 1000;
                    return String.Format("Stacking: Block new spell if slot {0} is '{1}' and < {2}", calc % 100, TrimEnum((SpellEffect)value), max);
                case 149:
                    //if (max > 1000) max -= 1000;
                    return String.Format("Stacking: Overwrite existing spell if slot {0} is '{1}' and < {2}", calc % 100, TrimEnum((SpellEffect)value), max);
                case 150:
                    return "Divine Intervention";
                case 151:
                    return "Suspend Pet";
                case 152:
                    return String.Format("Summon Pet: {0} x {1} for {2}s", spell.Extra, value, max);
                case 153:
                    return String.Format("Balance Group HP with {0}% Penalty", value);                       
                case 154:
                    return String.Format("Remove Detrimental ({0})", value);
                case 156:
                    return "Illusion: Target";
                case 157:
                    return FormatCount("Spell Damage Shield", -value);
                case 158:
                    if (max < value)
                        value = max;
                    return FormatPercent("Chance to Reflect Spell", value);
                case 159:
                    return FormatCount("Stats", value);
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
                    return String.Format("Immunity for {0} Hits/Spells", value);
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
                    return FormatPercent("Melee Mitigation", -value);
                case 169:
                    return FormatPercent("Chance to Critical Hit with " + TrimEnum((SpellSkill)value2), value);
                case 170:
                    return FormatPercent("Chance to Critical Cast", value);
                case 171:
                    return FormatPercent("Chance to Crippling Blow", value);
                case 172:
                    return FormatPercent("Chance to Avoid Melee", value);
                case 173:
                    return FormatPercent("Chance to Riposte", value);
                case 174:
                    return FormatPercent("Chance to Dodge", value);
                case 175:
                    return FormatPercent("Chance to Parry", value);
                case 176:
                    return FormatPercent("Chance to Dual Wield", value);
                case 177:
                    return FormatPercent("Chance to Double Attack", value);
                case 178:
                    return String.Format("Lifetap from Weapon Damage: {0}%", value);
                case 179:
                    return String.Format("Instrument Modifier: {0} {1}", spell.Skill, value);
                case 180:
                    return FormatPercent("Chance to Resist Spell", value);
                case 181:
                    return FormatPercent("Chance to Resist Fear Spell", value);
                case 182:
                    return FormatPercent("Weapon Delay", value);
                case 183:
                    return FormatPercent("Skill Check for " + TrimEnum((SpellSkill)value2), value);
                case 184:
                    return FormatPercent("Chance to Hit with " + TrimEnum((SpellSkill)value2), value);
                case 185:
                    return FormatPercent(TrimEnum((SpellSkill)value2) + " Damage", value);
                case 186:
                    return FormatPercent(TrimEnum((SpellSkill)value2) + " Min Damage", value);
                case 188:
                    return FormatPercent("Chance to Block", value);
                case 189:
                    return FormatCount("Current Endurance", value) + repeating;
                case 190:
                    return FormatCount("Max Endurance", value);
                case 191:
                    return "Prevent Combat";
                case 192:
                    return FormatCount("Hate", value) + repeating;
                case 193:
                    return String.Format("{0} Attack for {1} with {2}% Accuracy Mod", TrimEnum(spell.Skill), value, value2);
                case 194:
                    return "Remove All Aggro";
                case 195:
                    // 100 is full resist. not sure why some spells have more
                    return String.Format("Stun Resist ({0})", value);
                case 196:
                    return String.Format("Srikethrough ({0})", value);
                case 197:
                    return FormatPercent(TrimEnum((SpellSkill)value2) + " Damage Taken", value);
                case 198:
                    return FormatCount("Current Endurance", value);
                case 199:
                    return String.Format("Taunt ({0})", value);
                case 200:
                    return FormatPercent("Proc Rate", value);
                case 201:
                    return String.Format("Add Range Proc: [Spell {0}] with {1}% Rate Mod", value, value2);
                case 202:
                    return "Project Illusion";
                case 203:
                    return "Mass Group Buff";
                case 204:
                    return String.Format("Group Fear Immunity ({0})", value);
                case 206:
                    return "AE Taunt";
                case 209:
                    return String.Format("Dispell Beneficial ({0})", value);
                case 214:
                    return FormatPercent("Max HP", value / 100);
                case 216:
                    return FormatPercent("Accuracy", value);
                case 220:                    
                    return FormatCount(TrimEnum((SpellSkill)value2) + " Damage Bonus", value);
                case 227:
                    return String.Format("Reduce {0} Timer by {1}s", TrimEnum((SpellSkill)value2), value);
                case 232:
                    return String.Format("Divine Save: [Spell {0}] Chance: {1}%", value2, value);
                case 233:
                    return FormatPercent("Food Consumption", -value);
                case 243:
                    return FormatPercent("Chance of Charm Breaking", -value);
                case 258:
                    return String.Format("Triple Backstab ({0})", value);
                case 262:
                    return FormatCount(TrimEnum((SpellSkillCap)value2) + " Cap", value);
                case 272:
                    return FormatPercent("Spell Casting Skill", value);
                case 273:
                    return FormatPercent("Chance to Critical DoT", value);
                case 274:
                    return FormatPercent("Chance to Critical Heal", value);
                case 279:
                    return FormatPercent("Chance to Flurry", value);
                case 286:
                    // similar to damage focus, but adds a raw amount
                    // how is this different than 303?
                    return FormatCount("Spell Damage", value);
                case 287:
                    return String.Format("Increase Duration by {0}s", value * 6);
                case 289: 
                    // how is this different than 373?
                    return String.Format("Cast on Fade: [Spell {0}]", value);
                case 291:
                    return String.Format("Purify ({0})", value);
                case 294:
                    if (value > 0 && value2 > 100)
                        return FormatPercent("Chance to Critical Nuke", value) + " and " + FormatPercent("Critical Nuke Damage", value2 - 100);
                    else if (value > 0)
                        return FormatPercent("Chance to Critical Nuke", value);
                    else                    
                        return FormatPercent("Critical Nuke Damage", value2 - 100);
                case 296:
                    return FormatPercent("Spell Damage Taken", value);
                case 297:
                    return FormatCount("Spell Damage Taken", value);
                case 298:
                    return FormatPercent("Pet Size", value - 100);
                case 299:
                    return String.Format("Wake the Dead ({0})", max);
                case 300:
                    return "Doppelganger";
                case 303:
                    return FormatCount("Spell Damage", value);
                case 305:
                    return FormatCount("Damage Shield Taken", -Math.Abs(value));
                case 310:
                    return String.Format("Reduce Timer by {0}s", value / 1000);
                case 311:
                    // does this affect procs that the caster can also cast as spells?
                    return "Limit Type: Exclude Procs";
                case 314:
                    return "Invisible (Permanent)";
                case 315:
                    return "Invisible to Undead (Permanent)";
                case 319:
                    return FormatPercent("Chance to Critical HoT", value);
                case 320:                   
                    return String.Format("Shield Block ({0})", value);
                case 322:
                    return "Gate to Starting City";
                case 323:
                    return String.Format("Add Defensive Proc: [Spell {0}] with {1}% Rate Mod", value, value2);
                case 324:
                    // blood magic. uses HP as mana
                    return String.Format("Cast from HP with {0}% penalty", value);
                case 328:
                    return FormatCount("Max Negative HP", value);
                case 329:
                    return String.Format("Absorb Damage Using Mana: {0}%", value);
                case 330:
                    return FormatPercent("Critical Damage for " + TrimEnum((SpellSkill)value2), value);
                case 333:
                    // so far this is only used on spells that have a rune
                    return String.Format("Cast on Fade/Cancel: [Spell {0}]", value);
                case 335:
                    // prevent spells that match limit rules from landing
                    return "Prevent Spell Landing On Match";
                case 337:
                    return FormatPercent("Experience Gain", value);
                case 339:
                    return String.Format("Add Spell Proc: [Spell {0}] Chance: {1}%", value2, value);
                case 340:
                    return String.Format("Cast: [Spell {0}] Chance: {1}%", value2, value);
                case 342:
                    return "Prevent Fleeing";
                case 343:
                    // is this persistent or instant?
                    return String.Format("Interrupt Spell Chance: {0}%", value); 
                case 348:
                    return String.Format("Limit Min Mana Cost: {0}", value); 
                case 350:
                    return String.Format("Mana Burn: {0}", value); 
                case 351:
                    // the +3 is just a guess that's correct most of the time since spells have 3 ranks
                    // and the effects are placed after the spells
                    return String.Format("Aura Effect: [Spell {0}]", spell.ID + 3);
                case 353:
                    return FormatCount("Max Aura Effects", value);
                case 358:
                    return FormatCount("Current Mana", value);
                case 360:
                    return String.Format("Add Killshot Proc: [Spell {0}] Chance: {1}%", value2, value);
                case 361:
                    return String.Format("Cast on Death: [Spell {0}] Chance: {1}%", value2, value);
                case 365:
                    return String.Format("Cast on Killshot: [Spell {0}] Chance: {1}%", value2, value);
                case 367:
                    return String.Format("Transform Body Type ({0})", value);
                case 368:
                    return String.Format("Faction {0} Modifier: {1}", value, value2);
                case 369:
                    return FormatCount("Corruption Counter", value);
                case 370:
                    return FormatCount("Corruption Resist", value);
                case 371:                    
                    // this may be used to prevent overwritting melee haste
                    // no idea if this is also mitigated
                    return FormatPercent("Melee Delay", Math.Abs(value));
                case 373:
                    return String.Format("Cast on Fade: [Spell {0}]", value);
                case 374:       
                    // i think this is used when several effects need to be placed in a slot. 
                    // i.e. multiple spells are needed but a single cast is required                    
                    return String.Format("Cast: [Spell {0}]", value2);
                case 375:
                    return FormatPercent("Critical DoT Damage", value - 100);
                case 377:
                    return String.Format("Cast if Not Cured: [Spell {0}]", value);
                case 380:
                    return String.Format("Knockback for {0} and up for {1}", value, value2);
                case 381:
                    return "Call of Hero";
                case 382:
                    return String.Format("Inhibit Buff Effect: {0}", TrimEnum((SpellEffect)value2));
                case 383:
                    return String.Format("Cast on Match: [Spell {0}] Chance: {1}%", value2, value);
                case 385:
                    return String.Format("Limit Spells: {1}[Group {0}]", Math.Abs(value), value >= 0 ? "" : "Exclude ");
                case 386:
                    return String.Format("Cast on Curer: [Spell {0}]", value);
                case 387:
                    return String.Format("Cast if Cured: [Spell {0}]", value);
                case 392:
                    // similar to heal focus, but adds a raw amount
                    return FormatCount("Healing", value);
                case 399:
                    return FormatPercent("Chance to Twincast", value);
                case 401:
                    // e.g. Drains up to 401 mana from your target. For each point of mana drained, the target will take damage.
                    return String.Format("Inflict Damage from Mana Tap ({0})", value);
                case 406:
                    return String.Format("Cast if Attacked: [Spell {0}]", value);
                case 408:
                    // how is this different than 214?
                    return FormatPercent("Max HP", -value);
                case 418:
                    // how is this different than 220 bonus? setting it to regular damage for now
                    return FormatCount(TrimEnum((SpellSkill)value2) + " Damage", value);
                case 419:
                    // this is used for potions. how is it different than 85?
                    // value2 looks like a calc value
                    return String.Format("Add Proc: [Spell {0}]", value);
                case 424:
                    return "Gravity Flux";


            }

            return String.Format("Unknown Effect: {0} Val={1} Val2={2} Max={3} Calc={4}", type, value, value2, max, calc);
        }

        static float ParseFloat(string s)
        {
            if (String.IsNullOrEmpty(s))
                return 0f;
            return Single.Parse(s);
        }

        static int ParseInt(string s)
        {
            if (String.IsNullOrEmpty(s))
                return 0;
            return (int)Single.Parse(s);
        }

        static string TrimEnum(object o)
        {
            return o.ToString().Replace("_", " ").Trim();
        }

        static string FormatCount(string name, int count)
        {
            if (count >= 0)
                return String.Format("Increase {0} by {1}", name, count);
            else
                return String.Format("Decrease {0} by {1}", name, -count);
        }

        static string FormatPercent(string name, int count)
        {
            return FormatCount(name, count) + "%";
        }



    }


}
