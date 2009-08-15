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
            const string path = "spells_us.txt";

            if (!File.Exists(path))
                SpellParser.DownloadFile(path);
            List<Spell> list = SpellParser.LoadFromFile(path);

            
            Console.WriteLine("============================================================================");
            foreach(Spell spell in list)
            {   
                if (args.Length == 0                   
                    || (args.Length == 1 && spell.DebugEffectList.Contains(";" + args[0] + ";"))      
                    || (args.Length == 2 && args[0] == "id" && spell.ID.ToString() == args[1])
                    || (args.Length == 2 && args[0] == "name" && spell.Name.ToLower().Contains(args[1].ToLower())))

                    Console.WriteLine("\n[{0}] {1}\n{2}", spell.ID, spell.Name, String.Join("\n", spell.Details().ToArray()));
            }

        }

    }
}
