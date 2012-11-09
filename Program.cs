using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        static string SpellFilename = "spells_us.txt";
        static string DescFilename = "dbstr_us.txt";

        static SpellCache spells;

        static void Main(string[] args)
        {
            try
            {
                int dec = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalDigits;
                if (dec < 2)
                    Console.Error.WriteLine("Your system has been configured to display {0} decimal digits. Some values will be rounded.", dec);

                if (args.Length == 0)
                {
                    Console.Error.WriteLine("You must include a search parameter on the command line.");
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

                if (!File.Exists(SpellFilename))
                    DownloadPatchFiles(null);

                Console.Error.Write("Loading {0}... ", SpellFilename);
                spells = new SpellCache(SpellParser.LoadFromFile(SpellFilename, SpellFilename.Replace("spells_us", "dbstr_us")));
                Console.Error.WriteLine("{0} spells", spells.Count);

                // perform search based on command line parameters                
                string type = args.Length >= 1 ? args[0].ToLower() : null;
                string value = args.Length >= 2 ? args[1] : null;
                Console.Error.Write("Searching for {0} {1}... ", type, value);
                var results = Search(type, value);

                if (results != null)
                {
                    Console.Error.WriteLine("{0} results", results.Count);
                    Console.Error.WriteLine();
                    spells.Expand(results, false);
                    Show(results);
                }

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

        }

        /// <summary>
        /// Search the spell list for matching spells.
        /// </summary>
        static List<Spell> Search(string field, string value)
        {
            IEnumerable<Spell> results = null;

            if (field == "all")
                results = spells;
            //results = list.Where(x => (int)x.TargetRestrict > 0).OrderBy(x => x.TargetRestrict);

            // search by effect type
            if (field == "type" || field == "spa")
                results = spells.Where(x => x.HasEffect(value) >= 0);

            // search by id
            if (field == "id")
                results = spells.Where(x => x.ID.ToString() == value);

            // search by group
            if (field == "group")
                results = spells.Where(x => x.GroupID.ToString() == value);

            // search by name
            if (field == "name")
                results = spells.Where(x => x.Name.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) >= 0).OrderBy(x => x.Name);

            // search by class
            if (field == "class")
            {
                int i = SpellParser.ParseClass(value) - 1;
                if (i < 0)
                    throw new Exception("Unknown class: " + value);
                results = spells.Where(x => x.ExtLevels[i] > 0 && x.ExtLevels[i] < 255).OrderBy(x => x.Levels[i]).ThenBy(x => x.ID);
            }

            // search by target
            if (field == "target")
                results = spells.Where(x => x.Target.ToString().Equals(value, StringComparison.CurrentCultureIgnoreCase));

            // debugging: search the unknown field 
            if (field == "unknown")
                results = spells.Where(x => x.Unknown > 0).OrderBy(x => x.Unknown);


            return results.ToList();
        }

        /// <summary>
        /// Show list to console.
        /// </summary>
        static void Show(IEnumerable<Spell> list)
        {
            foreach (Spell spell in list)
            {
                Console.WriteLine(spell);
                foreach (string s in spell.Details())
                    Console.WriteLine(spells.InsertSpellNames(s));
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
            var files = LaunchpadPatcher.DownloadManifest(server);

            LaunchpadPatcher.DownloadFile(files[SpellFilename].Url, SpellFilename);
            LaunchpadPatcher.DownloadFile(files[DescFilename].Url, DescFilename);
        }

    }
}
