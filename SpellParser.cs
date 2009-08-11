using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace parser
{
    //[FlagsAttribute]
    //public enum SpellClasses
    //{
    //    BER = 32768, BRD = 128, BST = 16384, CLR = 2, DRU = 32, ENC = 8192, MAG = 4096, MNK = 64,
    //    NEC = 1024, PAL = 4, RNG = 8, ROG = 256, SHD = 16, SHM = 512, WAR = 1, WIZ = 2048
    //};

    //public enum SpellEffect
    //{
    //    Current_HP = 0,
    //    AC = 1
    //    ATK = 2;
    //}

    class Spell
    {
        public int ID; 
        public string Name; 
        public string Duration; 
        public string[] Slots;
        public int[] Levels;
        

        public string DebugEffectList;

        public Spell()
        {
            Slots = new string[12];
            Levels = new int[16];
        }

        public override string ToString()
        {
            return String.Format("[{0}] {1}", ID, Name);
        }
    }

    class SpellCache
    {
        Dictionary<int, Spell> list = new Dictionary<int, Spell>(25000);

        public Dictionary<int, Spell> List { get { return list; } }


        public void LoadFromFile(string path)
        {
            using (StreamReader f = File.OpenText(path))
                while (!f.EndOfStream)
                {
                    string line = f.ReadLine();
                    Spell spell = SpellParser.Parse(line.Split('^'));
                    list[spell.ID] = spell;
                    //list.Add(spell);
                }
        }

        
    }

    //delegate string SpellSlotParser(int type);

    static class SpellParser
    {
        const int MaxLevel = 85;

        public static Spell Parse(string[] fields)
        {
            Spell spell = new Spell();
            spell.ID = Convert.ToInt32(fields[0]);
            spell.Name = fields[1];

            int dur = ParseInt(fields[17]);
            int durcalc = ParseInt(fields[16]);
            if (dur > 0 || durcalc > 0)
                spell.Duration = dur.ToString();

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
                spell.Slots[i] = ParseSlot(spell,
                    ParseInt(fields[86 + i]), 
                    ParseInt(fields[20 + i]), 
                    ParseInt(fields[32 + i]), 
                    ParseInt(fields[44 + i]), 
                    ParseInt(fields[70 + i]));

                spell.DebugEffectList += ";" + fields[86 + i].ToString();
            }
            spell.DebugEffectList += ";";

            return spell;
        }

        static int ParseInt(string s)
        {
            if (String.IsNullOrEmpty(s))
                return 0;
            return (int)Single.Parse(s);
        }

        /// <summary>
        /// Parses a spell effect slot. Each slot has a series of attributes associated with it.
        /// </summary>
        /// <returns>A description of the slot effect</returns>
        static string ParseSlot(Spell spell, int type, int value, int value2, int max, int calc)
        {
            int level = MaxLevel;
            int minlevel = 255;
            string repeating = !String.IsNullOrEmpty(spell.Duration) ? " per tick" : null;

            for (int i = 0; i < spell.Levels.Length; i++)
                if (spell.Levels[i] > 0 && spell.Levels[i] < minlevel)
                    minlevel = spell.Levels[i];
            if (minlevel > MaxLevel)
                minlevel = 0;
            //if (minlevel == 0)
            //    minlevel = 1;

            // negate level for calculations
            if (value < 0)
                level = -level;

            // the default calculation (100) leaves the base value as is.
            // all other calculations alter the base value
            switch (calc)
            {
                case 101:
                    value += level / 2;
                    break;
                case 102:
                    value += level;
                    break;
                case 103:
                    value += level * 2;
                    break;
                case 104:
                    value += level * 3;
                    break;
                case 105:
                    value += level * 4;
                    break;
                case 106:
                    value += level * 5;
                    break;
                case 107:
                    value += level / 2;
                    break;
                case 108:
                    value += level / 3;
                    break;
                case 109:
                    value += level / 4;
                    break;
                case 110:
                    value += level / 5;
                    break;
                case 111:
                    value += (level - minlevel) * 6; 
                    break;
                case 112:
                    value += (level - minlevel) * 8;
                    break;
                case 113:
                    value += (level - minlevel) * 10; 
                    break;
                case 114:
                    value += (level - minlevel) * 15;
                    break;
                case 115:
                    value += (level - minlevel) * 6;
                    break;
                case 116:
                    value += (level - minlevel) * 8;
                    break;
                case 117:
                    value += (level - minlevel) * 12;
                    break;
                case 118:
                    value += (level - minlevel) * 20;
                    break;


                default:
                    if (calc > 0 && calc < 100)
                        value = level * calc;
                    break;
            }

            //if (calc == 115) Console.WriteLine("------  " + spell + " " + String.Format("Eff={0} Val={1} Val2={2} Max={3} Calc={4}", type, value, value2, max, calc));


            if (max > 0 && value > max)
                value = max;
            if (max > 0 && value < -max)
                value = -max;



            switch (type)
            {                
                case 0:
                    // delta hp for heal/nuke, repeating if with duration
                    return FormatCount("Current HP", value) + repeating;
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
                    // 10 is also used as a magic value for unused slots
                    if (value > 0)
                        return FormatCount("CHA", value);                    
                    return null; 
                case 11:
                    // haste is given as a factor of 100. so 85 = 15% slow, 130 = 30% haste 
                    if (value < 100)
                        value = 100 - value;
                    else
                        value = value - 100;
                    return FormatPercent("Melee Haste", value);
                case 12:
                    return "Invisibility (Unstable)";
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
                case 30:
                    return String.Format("Decrease Aggro Radius up to level {0}", max);

                case 35:
                    return FormatCount("Disease Counter", value);
                case 36:
                    return FormatCount("Poison Counter", value);

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

                case 55:
                    return FormatCount("Rune", value);

                    
                case 64:
                    return "SpinStun";
                case 65:
                    return "Infravision";
                case 66:
                    return "Ultravision";
                case 67:
                    return "Eye of Zomm";
                case 69:
                    // max hp
                    return FormatCount("HP", value);
                case 79:
                    // delta hp for heal/nuke, non repeating
                    return FormatCount("Current HP", value);

                case 86:
                    return String.Format("Decrease Assist Radius up to level {0}", max);

                case 91:
                    return String.Format("Summon corpse up to level {0}", value);


                case 148:
                    return String.Format("Block new spell if slot {0} is '{1}' and < {2}", calc % 100, ParseSlotType(value), max);
                case 149:
                    return String.Format("Overwrite existing spell if slot {0} is '{1}' and < {2}", calc % 100, ParseSlotType(value), max);

                case 254:
                    // 254 is used for unused slots
                    return null;
                case 314:
                    return "Invisibility (Permanent)";
            }

            return String.Format("Unknown Effect: {0} Val={1} Val2={2} Max={3} Calc={4}", type, value, value2, max, calc);
        }

        static string ParseSlotType(int type)
        {
            switch (type)
            {
                case 1: return "AC";
                case 2: return "ATK";
                case 69: return "HP"; // max hp              
            }

            return String.Format("Unknown Effect: {0}", type);
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
