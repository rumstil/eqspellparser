﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace parser
{
    [FlagsAttribute]
    public enum SpellClasses
    {
        BER = 32768, BRD = 128, BST = 16384, CLR = 2, DRU = 32, ENC = 8192, MAG = 4096, MNK = 64,
        NEC = 1024, PAL = 4, RNG = 8, ROG = 256, SHD = 16, SHM = 512, WAR = 1, WIZ = 2048
    };

    class Spell
    {
        // this shorthand may not compile in vs2005?
        public int ID { get; set; }  
        public string Name { get; set; }
        public string[] Slots { get; set; }
        public SpellClasses Classes { get; set; }

        public Spell()
        {
            Slots = new string[12];
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

            // each spell has 12 effect slots:
            // 86..97 - slot 1..12 type
            // 20..31 - slot 1..12 base effect
            // 32..43 - slot 1..12 base_2 effect
            // 44..55 - slot 1..12 max effect
            // 70..81 - slot 1..12 calc forumla data
            for (int i = 0; i < spell.Slots.Length; i++)
            {
                int type;
                Int32.TryParse(fields[86 + i], out type);
                int value;
                Int32.TryParse(fields[20 + i], out value);
                int value2;
                Int32.TryParse(fields[32 + i], out value2);
                int max;
                Int32.TryParse(fields[44 + i], out max);
                int calc;
                Int32.TryParse(fields[70 + i], out calc);
                //spell.Slots[i] = ParseSlot(Convert.ToInt32(fields[86 + i]), Convert.ToInt32(fields[20 + i]), Convert.ToInt32(fields[32 + i]), Convert.ToInt32(fields[44 + i]), Convert.ToInt32(fields[70 + i]));
                spell.Slots[i] = ParseSlot(type, value, value2, max, calc);                
            }

            return spell;
        }

        static string ParseSlot(int type, int value, int value2, int max, int calc)
        {
            int level = MaxLevel;


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
                    value += level / 6;
                    break;
                default:
                    if (calc < 100)
                        value = level * calc;                    
                    break;
            }
            if (max > 0 && value > max) 
                value = max;

            switch (type)
            {
                case 0: 
                    return FormatCount("HP", value);
                case 1:
                    return FormatCount("AC", (int)Math.Floor(value / (10f / 3f))); 
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
                    // haste is given as a percentage of 100. so 85 = 15% slow, 130 = 30% haste 
                    if (value < 100)
                        value = 100 - value;
                    else
                        value = value - 100;
                    return FormatPercent("Melee Haste", value);

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

                case 69:
                    // max hp
                    return FormatCount("HP", value);
                case 79:
                    // current hp for heal/nuke, non-repeating if with duration
                    return FormatCount("Current HP", value);


                case 148:
                    return String.Format("Block new spell if slot {0} is {1} and < {2}", calc % 100, ParseSlotType(value), max);
                case 149:
                    return String.Format("Overwrite existing spell if slot {0} is {1} and < {2}", calc % 100, ParseSlotType(value), max);

                case 254:
                    // 254 is used for unused slots
                    return null;
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
