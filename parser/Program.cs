using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using EQSpellParser;


namespace parser
{
    class Program
    {
        static SpellCache cache;

        static void Main(string[] args)
        {
            var path = LaunchpadManifest.SPELL_FILE;

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
                    path = args[0];
                    args[0] = "all";
                }

                if (!File.Exists(path))
                    DownloadPatchFiles(null);

                Console.Error.Write("Loading {0}... ", path);
                cache = new SpellCache();
                cache.LoadSpells(path); 
                Console.Error.WriteLine("{0} spells", cache.SpellList.Count());

                // perform search based on command line parameters                
                var type = args.Length >= 1 ? args[0].ToLower() : null;
                var value = args.Length >= 2 ? args[1] : null;
                Console.Error.Write("Searching for {0} {1}... ", type, value);
                var results = Search(type, value);

                if (results != null)
                {
                    Console.Error.WriteLine("{0} results", results.Count);
                    Console.Error.WriteLine();
                    cache.AddForwardRefs(results);
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

            var q = cache.SpellList;

            if (field == "all")
                results = q;
            //results = list.Where(x => (int)x.TargetRestrict > 0).OrderBy(x => x.TargetRestrict);

            // search by effect type
            if (field == "type" || field == "spa")
                results = q.Where(x => x.HasEffect(value, 0));

            // search by id
            if (field == "id")
                results = q.Where(x => x.ID.ToString() == value);

            // search by group
            if (field == "group")
                results = q.Where(x => x.GroupID.ToString() == value);

            // search by name
            if (field == "name")
                results = q.Where(x => x.Name.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0).OrderBy(x => x.Name);

            // search by class
            if (field == "class")
            {
                int i = SpellParser.ParseClass(value) - 1;
                if (i < 0)
                    throw new Exception("Unknown class: " + value);
                results = q.Where(x => x.ExtLevels[i] > 0 && x.ExtLevels[i] < 255).OrderBy(x => x.Levels[i]).ThenBy(x => x.ID);
            }

            // search by target
            if (field == "target")
                results = q.Where(x => x.Target.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase));

            // debugging: search the unknown field 
            if (field == "unknown")
                results = q.Where(x => x.Unknown > 0).OrderBy(x => x.Unknown);


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
                    Console.WriteLine(cache.InsertRefNames(s));
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
            LaunchpadPatcher.DownloadManifest(server, "manifest.dat");
            var manifest = new LaunchpadManifest(File.OpenRead("manifest.dat"));

            var spell = manifest.FindFile(LaunchpadManifest.SPELL_FILE);
            LaunchpadPatcher.DownloadFile(spell.Url, spell.Name);

            var spellstr = manifest.FindFile(LaunchpadManifest.SPELLSTR_FILE);
            if (spellstr != null)
                LaunchpadPatcher.DownloadFile(spellstr.Url, spellstr.Name);

            var desc = manifest.FindFile(LaunchpadManifest.SPELLDESC_FILE);
            if (desc != null)
                LaunchpadPatcher.DownloadFile(desc.Url, desc.Name);

            var stack = manifest.FindFile(LaunchpadManifest.SPELLSTACK_FILE);
            if (stack != null)
                LaunchpadPatcher.DownloadFile(stack.Url, stack.Name);
        }

    }
}
