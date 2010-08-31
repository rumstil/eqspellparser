using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Everquest;

namespace parser
{
    class Program
    {
        const string SpellFilename = "spells_us.txt";
        const string DescFilename = "dbstr_us.txt";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("No search parameters specified!");
                Thread.Sleep(2000);
                return;
            }

            if (args.Length == 1 && args[0] == "update")
            {
                DownloadPatchFiles();
                return;
            }

            if (!File.Exists(SpellFilename) || !File.Exists(DescFilename))
                DownloadPatchFiles();

            IList<Spell> list = SpellParser.LoadFromFile(SpellFilename, DescFilename);

            Func<int, Spell> lookup = id => list.FirstOrDefault(x => x.ID == id); // TODO: hash table search

            var results = Search(list, args);
            //results = Expand(results, lookup);
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
                results = list.Where(x => x.HasEffect(args[1]));

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
            List<Spell> results = list.ToList();

            // scan each spell in the queue for spell references. if a new reference is found
            // then add it to the queue so that it can also be checked
            int i = 0;
            while (i < results.Count)
            {
                Spell spell = results[i++];

                if (spell.RecourseID != 0)
                {
                    Spell spellref = lookup(spell.RecourseID);
                    if (spellref != null && !results.Contains(spellref))
                        results.Add(spellref);
                }

                // check effects slots for the [Spell 1234] references
                foreach (string s in spell.Slots)
                    if (s != null)
                    {
                        Match link = Spell.SpellRefExpr.Match(s);
                        if (link.Success)
                        {
                            Spell spellref = lookup(Int32.Parse(link.Groups[1].Value));
                            if (spellref != null && !results.Contains(spellref))
                                results.Add(spellref);
                        }
                    }
            }

            return results;
        }

        /// <summary>
        /// Show list on console.
        /// </summary>
        static void Show(IEnumerable<Spell> list)
        {
            if (list == null)
                return;

            int count = 0;

            foreach (Spell spell in list)
            {
                count++;
                Console.WriteLine(spell);
                foreach (string s in spell.Details())
                    Console.WriteLine(s);
                if (!String.IsNullOrEmpty(spell.Desc))
                    Console.WriteLine(spell.Desc);
                Console.WriteLine();
            }

            Console.Error.WriteLine();
            Console.Error.WriteLine("{0} results", count);
        }

        /// <summary>
        /// Download needed files from the patch server.
        /// </summary>
        static void DownloadPatchFiles()
        {
            //string patch = "http://eq.patch.station.sony.com/patch/everquest/en/everquest-update.xml.gz"; // live servers (patch.everquest.com:7000 also works)
            string patch = "http://eq.patch.station.sony.com/patch/everquest/en-test/everquest-update.xml.gz"; // test server 

            DownloadFile(patch, "update.xml");

            XmlDocument doc = new XmlDocument();
            doc.Load("update.xml");
            string server = "http://" + doc.SelectSingleNode("//VerantPatcher/Product[@Name='EverQuest']").Attributes["Server"].Value;
            string path = "/" + doc.SelectSingleNode("//VerantPatcher/Product[@Name='EverQuest']/Distribution[@Name='Main Distribution']/Directory[@LocalPath='::HomeDirectory::']").Attributes["RemotePath"].Value + "/";

            DownloadFile(server + path + SpellFilename + ".gz", SpellFilename);

            DownloadFile(server + path + DescFilename + ".gz", DescFilename);
        }

        /// <summary>
        /// Download a file.
        /// </summary>
        static void DownloadFile(string url, string path)
        {
            Console.Error.WriteLine("=> " + url);
            int timer = System.Environment.TickCount;

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
                            break;
                        output.Write(buffer, 0, read);
                    }

                    int duration = System.Environment.TickCount - timer;
                    Console.Error.WriteLine(" {0} bytes, {1} kB/s", output.Length, output.Length / duration);
                }
            }
        }

    }
}
