using System;
using System.Collections.Generic;
using System.Globalization;
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
        static string SpellFilename = "spells_us.txt";
        static string DescFilename = "dbstr_us.txt";

        static void Main(string[] args)
        {
            int timer = System.Environment.TickCount;

            int dec = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalDigits;
            if (dec < 2)
                Console.Error.WriteLine("Your system is set to display {0} decimal digits. Some values will be rounded.", dec);

            if (args.Length == 0)
            {
                Console.Error.WriteLine("No search parameters specified!");
                Thread.Sleep(2000);
                return;
            }

            if (args[0] == "update")
            {
                DownloadPatchFiles(args.Length >= 2 ? args[1] : null);
                return;
            }

            if (File.Exists(args[0]))
            {
                SpellFilename = args[0];
                args[0] = "all";
            }

            if (!File.Exists(SpellFilename) || !File.Exists(DescFilename))
                DownloadPatchFiles(null);

            var spells = SpellParser.LoadFromFile(SpellFilename, DescFilename).ToList();

            Func<int, Spell> lookup = id => spells.FirstOrDefault(x => x.ID == id);

            var results = Search(spells, args);
            results = Expand(results, lookup);

            if (results != null)
            {
                Show(results);
                Console.Error.WriteLine();
                Console.Error.WriteLine("{0} results in {1}s", results.Count(), (System.Environment.TickCount - timer) / 1000);
            }
        }

        /// <summary>
        /// Search the spell list for matching spells.
        /// </summary>
        static IEnumerable<Spell> Search(IEnumerable<Spell> list, string[] args)
        {
            IEnumerable<Spell> results = null;

            string type = args.Length >= 1 ? args[0].ToLower() : null;
            string param1 = args.Length >= 2 ? args[1] : null;

            if (type == "all")
                results = list;
            //results = list.Where(x => (int)x.TargetRestrict > 0).OrderBy(x => x.TargetRestrict);

            // search by effect type
            if (type == "type" || type == "spa")
                results = list.Where(x => x.HasEffect(param1));

            // search by id
            if (type == "id")
                results = list.Where(x => x.ID.ToString() == param1);

            // search by group
            if (type == "group")
                results = list.Where(x => x.GroupID.ToString() == param1);

            // search by name
            if (type == "name")
                results = list.Where(x => x.Name.ToLower().Contains(param1.ToLower())).OrderBy(x => x.Name);

            // search by class
            if (type == "class")
            {
                int i = (int)Enum.Parse(typeof(SpellClasses), param1.ToUpper()) - 1;
                results = list.Where(x => x.Levels[i] > 0 && x.Levels[i] < 255).OrderBy(x => x.Levels[i]).ThenBy(x => x.ID);
            }

            // search by target
            if (type == "target")
                results = list.Where(x => x.Target.ToString().ToLower() == param1.ToLower());

            // debugging: search the unknown field 
            if (type == "unknown")
                results = list.Where(x => x.Unknown != 0).OrderBy(x => x.Unknown);


            return results;
        }

        /// <summary>
        /// Recursively expand the spell list to include referenced spells.
        /// </summary>                
        static IEnumerable<Spell> Expand(IEnumerable<Spell> list, Func<int, Spell> lookup)
        {
            List<Spell> results = list.ToList();

            // keep a hash based indx of existing results to avoid doing a linear search on results
            HashSet<int> resultsIndex = new HashSet<int>();
            foreach (Spell spell in results)
                resultsIndex.Add(spell.ID);


            Func<string, string> expand = text => Spell.SpellRefExpr.Replace(text, delegate(Match m)
                {
                    Spell spellref = lookup(Int32.Parse(m.Groups[1].Value));
                    if (spellref != null)
                    {
                        if (!resultsIndex.Contains(spellref.ID))
                        {
                            resultsIndex.Add(spellref.ID);
                            results.Add(spellref);
                        }
                        //return spellref.Name;
                        return String.Format("{1} [Spell {0}]", spellref.ID, spellref.Name);
                    }
                    return m.Groups[0].Value;
                });


            // scan each spell in the queue for spell references. if a new reference is found
            // then add it to the queue so that it can also be checked
            int i = 0;
            while (i < results.Count)
            {
                Spell spell = results[i++];
 
                if (spell.Recourse != null)
                    spell.Recourse = expand(spell.Recourse);

                // check effects slots for the [Spell 1234] references
                for (int j = 0; j < spell.Slots.Length; j++)
                    if (spell.Slots[j] != null)
                        spell.Slots[j] = expand(spell.Slots[j]);
            }

            return results;
        }

        /// <summary>
        /// Show list on console.
        /// </summary>
        static void Show(IEnumerable<Spell> list)
        {
            foreach (Spell spell in list)
            {
                Console.WriteLine(spell);
                foreach (string s in spell.Details())
                    Console.WriteLine(s);
                if (!String.IsNullOrEmpty(spell.Desc))
                    Console.WriteLine(spell.Desc);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Download spell files from the patch server.
        /// </summary>
        /// <param name="server">Null for the live server.</param>
        static void DownloadPatchFiles(string server)
        {
            string host = "http://eq.patch.station.sony.com";

            string path = host + "/patch/everquest/en" + server + "/everquest-update.xml.gz";
            DownloadFile(path, "update.xml");

            XmlDocument doc = new XmlDocument();
            doc.Load("update.xml");
            path = host + "/" + doc.SelectSingleNode("//Product[@Name='EverQuest']/Distribution[@Name='Main Distribution']/Directory[@LocalPath='::HomeDirectory::']/@RemotePath").Value + "/";
            DownloadFile(path + SpellFilename + ".gz", SpellFilename);
            DownloadFile(path + DescFilename + ".gz", DescFilename);
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
              
                    Console.Error.WriteLine("   {0} bytes - {1}", output.Length, web.ResponseHeaders["Last-Modified"]);
                    Console.Error.WriteLine();
                }

                DateTime lastMod;
                if (DateTime.TryParse(web.ResponseHeaders["Last-Modified"], out lastMod))
                    File.SetLastWriteTimeUtc(path, lastMod);
            }
        }

    }
}
