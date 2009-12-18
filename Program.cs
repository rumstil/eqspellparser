using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Everquest;

namespace parser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("No search parameters specified!");
                Thread.Sleep(2000);
                return;
            }

            const string spellFile = "spells_us.txt";
            const string descFile = "dbstr_us.txt";

            if (!File.Exists(spellFile))
                DownloadPatchFile(spellFile);

            if (!File.Exists(descFile))
                DownloadPatchFile(descFile);

            List<Spell> list = SpellParser.LoadFromFile(spellFile, descFile);
            Func<int, Spell> lookup = id => list.FirstOrDefault(x => x.ID == id); // TODO: hash table search
                
            var results = Search(list, args);
            results = Expand(results, lookup);
            Show(results); 
        }

        /// <summary>
        /// Search the spell list for matching spells.
        /// </summary>
        static IEnumerable<Spell> Search(IEnumerable<Spell> list, string[] args)
        {
            IEnumerable<Spell> results = null;

            if (args.Length == 1 && args[0] == "all")
                results = list;
                //results = list.Where(x => (int)x.TargetRestrict > 0).OrderBy(x => x.TargetRestrict);

            // search by effect type
            if (args.Length == 2 && args[0] == "type")
                results = list.Where(x => x.SlotEffects.Contains(Int32.Parse(args[1])));

            // search by id
            if (args.Length == 2 && args[0] == "id")
                results = list.Where(x => x.ID.ToString() == args[1]);

            // search by group
            if (args.Length == 2 && args[0] == "group")
                results = list.Where(x => x.GroupID.ToString() == args[1]);

            // search by name
            if (args.Length == 2 && args[0] == "name")
                results = list.Where(x => x.Name.ToLower().Contains(args[1].ToLower())).OrderBy(x => x.Name);

            // search by class
            if (args.Length == 2 && args[0] == "class")
            {
                int i = (int)Enum.Parse(typeof(SpellClasses), args[1].ToUpper()) - 1;
                results = list.Where(x => x.Levels[i] > 0 && x.Levels[i] < 255).OrderBy(x => x.Levels[i]).ThenBy(x => x.ID);
            }

            // search by target
            if (args.Length == 2 && args[0] == "target")
                results = list.Where(x => x.Target.ToString().ToLower() == args[1].ToLower());

            // debugging: search the unknown field 
            if (args.Length == 1 && args[0] == "unknown")
                results = list.Where(x => x.Unknown > 0).OrderBy(x => x.Unknown);

            return results;
        }

        /// <summary>
        /// Recursively expand the spell list to include referenced spells.
        /// </summary>                
        static IEnumerable<Spell> Expand(IEnumerable<Spell> list, Func<int, Spell> lookup)
        {
            List<Spell> queue = list.ToList();

            Regex spellexpr = new Regex(@"\[Spell\s(\d+)\]");

            // scan each spell in the queue for spell references. if a new reference is found
            // then add it to the queue so that it can also be checked
            int i = 0;
            while (i < queue.Count)
            {
                Spell spell = queue[i++];

                if (spell.RecourseID != 0)
                {                    
                    Spell spellref = lookup(spell.RecourseID);
                    if (spellref != null && !queue.Contains(spellref))
                        queue.Add(spellref);
                }

                // check effects slots for the [Spell 1234] references
                foreach (string s in spell.Slots)
                    if (s != null)
                    {
                        Match link = spellexpr.Match(s);
                        if (link.Success)
                        {                            
                            Spell spellref = lookup(Int32.Parse(link.Groups[1].Value));
                            if (spellref != null && !queue.Contains(spellref))
                                queue.Add(spellref);
                        }
                    }
            }

            return queue;
        }

        /// <summary>
        /// Show list on console.
        /// </summary>
        static void Show(IEnumerable<Spell> list)
        {
            if (list != null)
                foreach (Spell spell in list)
                {
                    Console.WriteLine("\r\n{0}\r\n{1}", spell, String.Join("\r\n", spell.Details()));
                    if (!String.IsNullOrEmpty(spell.Desc))
                        Console.WriteLine(spell.Desc);
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
            DownloadFile("http://patch.station.sony.com:7000/patch/everquest/en/patch0/main/" + filename + ".gz", filename);
            //DownloadFile("http://patch.station.sony.com:7000/patch/everquest/en/everquest-update.xml.gz", "update.xml");
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

    }
}
