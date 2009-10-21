using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace parser
{
    class Program
    {
        static void Main(string[] args)
        {
            const string spellFile = "spells_us.txt";
            const string descFile = "dbstr_us.txt";

            if (!File.Exists(spellFile))
                DownloadPatchFile(spellFile);

            if (!File.Exists(descFile))
                DownloadPatchFile(descFile);

            List<Spell> list = SpellParser.LoadFromFile(spellFile, null);
            IEnumerable<Spell> results = null;
            
            Console.WriteLine("============================================================================");

            if (args.Length == 0)
                results = list;

            // search by effect type
            if (args.Length == 1)
                results = list.Where(x => x.DebugEffectList.Contains(";" + args[0] + ";"));
            
            // search by id
            if (args.Length == 2 && args[0] == "id")
                results = list.Where(x => x.ID.ToString() == args[1]);

            // search by name
            if (args.Length == 2 && args[0] == "name")
                results = list.Where(x => x.Name.ToLower().Contains(args[1].ToLower())).OrderBy(x => x.Name);

            // search by class
            if (args.Length == 2 && args[0] == "class")
            {
                int i = (int)Enum.Parse(typeof(SpellClasses), args[1].ToUpper()) - 1;                
                results = list.Where(x => x.Levels[i] > 0 && x.Levels[i] < 255).OrderBy(x => x.Levels[i]).ThenBy(x => x.ID);
            }


            /*
            // add referenced spells
            //Regex linkex = new Regex(@"\[Group\s\d+\]");
            Regex linkex = new Regex(@"\[Spell\s(\d+)\]");
            List<Spell> related = new List<Spell>();
            foreach (Spell spell in results)
            {
                foreach (string s in spell.Slots)
                    if (s != null)
                    {
                        Match link = linkex.Match(s);
                        if (link.Success)
                        {
                            int id = Int32.Parse(link.Groups[1].Value);
                            //if (!results.Exists(x => x.ID == id)) 
                            related.Add(list.First(x => x.ID == id));
                        }
                    }
            }
            foreach (Spell spell in related)
                if (!results.Contains(spell))
                    results.Add(spell);
            */

            // show results
            foreach (Spell spell in results)
                Console.WriteLine("\r\n{0}\r\n{1}", spell, String.Join("\r\n", spell.Details()));

        }

        /// <summary>
        /// Download a file.
        /// </summary>
        static void DownloadFile(string url, string path)
        {
            using (WebClient web = new WebClient())
            {
                byte[] data = web.DownloadData(url);

                Stream datastream = new MemoryStream(data);
                GZipStream input = new GZipStream(datastream, CompressionMode.Decompress);
                using (FileStream output = new FileStream(path, FileMode.Create))
                {
                    byte[] buffer = new byte[32768];
                    while (true)
                    {
                        int read = input.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                            return;
                        output.Write(buffer, 0, read);
                    }
                }
            }
        }

        /// <summary>
        /// Download a file from the patch server.
        /// </summary>
        static void DownloadPatchFile(string filename)
        {
            Console.Error.WriteLine("Downloading " + filename);

            // http://eqitems.13th-floor.org/phpBB2/viewtopic.php?t=316
            // Newest Patcher path: http://patch.everquest.com:7000/patch/lp2/eq/en/patch1/en-main/
            // Older Patcher path: http://patch.everquest.com:7000/patch/everquest/en/patch1/main/
            // Looking at the patcher logs it seems they are switching back and forth between patch0 and patch1. 
            DownloadFile("http://patch.station.sony.com:7000/patch/everquest/en/patch1/main/" + filename + ".gz", filename);
        }

    }
}
