using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace parser
{
    class Program
    {
        static void Main(string[] args)
        {
            // the spells file can be downloaded from:
            // http://patch.station.sony.com:7000/patch/everquest/en/patch1/main/spells_us.txt.gz

            SpellCache cache = new SpellCache();            
            cache.LoadFromFile("spells_us.txt");


            
            
            foreach(Spell spell in cache.List.Values)
            {   
                //if (spell.Name.Contains("Swift"))
                //if (String.Join(";", spell.Slots).Contains("Unknown Effect"))
                if (spell.ID < 100) 
                {
                    Console.WriteLine();
                    Console.WriteLine("[{0}] {1}", spell.ID, spell.Name);
                    for (int j = 0; j < spell.Slots.Length; j++)
                        if (!String.IsNullOrEmpty(spell.Slots[j]))
                            Console.WriteLine("{0}: {1}", j + 1, spell.Slots[j]);
                }

            }


        }
    }
}
