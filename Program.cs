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
                var spells = SpellParser.LoadFromFile(SpellFilename, SpellFilename.Replace("spells_us", "dbstr_us"));
                var lookup = spells.ToDictionary(x => x.ID);     
                Console.Error.WriteLine("{0} spells", spells.Count);
                
                // perform search based on command line parameters                
                string type = args.Length >= 1 ? args[0].ToLower() : null;
                string value = args.Length >= 2 ? args[1] : null;
                Console.Error.Write("Searching for {0} {1}... ", type, value);
                var results = Search(spells , type, value);

                // expand results to include referenced spells 
                results = Expand(results, lookup);
                if (results != null)
                {
                    Console.Error.WriteLine("{0} results", results.Count);
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
        static IList<Spell> Search(IEnumerable<Spell> list, string field, string value)
        {
            IEnumerable<Spell> results = null;

            if (field == "all")
                results = list;
            //results = list.Where(x => (int)x.TargetRestrict > 0).OrderBy(x => x.TargetRestrict);

            // search by effect type
            if (field == "type" || field == "spa")
                results = list.Where(x => x.HasEffect(value) >= 0);

            // search by id
            if (field == "id")
                results = list.Where(x => x.ID.ToString() == value);

            // search by group
            if (field == "group")
                results = list.Where(x => x.GroupID.ToString() == value);

            // search by name
            if (field == "name")
                results = list.Where(x => x.Name.ToLower().Contains(value.ToLower())).OrderBy(x => x.Name);

            // search by class
            if (field == "class")
            {
                int i = SpellParser.ParseClass(value) - 1;
                if (i < 0)
                    throw new Exception("Unknown class: " + value);
                results = list.Where(x => x.ExtLevels[i] > 0 && x.ExtLevels[i] < 255).OrderBy(x => x.Levels[i]).ThenBy(x => x.ID);
            }

            // search by target
            if (field == "target")
                results = list.Where(x => x.Target.ToString().ToLower() == value.ToLower());

            // debugging: search the unknown field 
            if (field == "unknown")
                results = list.Where(x => x.Unknown > 0).OrderBy(x => x.Unknown);


            return results.ToList();
        }

        /// <summary>
        /// Recursively expand the spell list to include referenced spells.
        /// </summary>                
        static IList<Spell> Expand(IEnumerable<Spell> list, IDictionary<int, Spell> lookup)
        {
            List<Spell> results = list.ToList();

            // keep a hash based index of existing results to avoid doing a linear search on results
            // when checking if a spell is already included
            HashSet<int> included = new HashSet<int>();
            foreach (Spell spell in results)
                included.Add(spell.ID);

            Func<string, string> expand = text => Spell.SpellRefExpr.Replace(text, delegate(Match m)
                {
                    Spell spellref;
                    if (lookup.TryGetValue(Int32.Parse(m.Groups[1].Value), out spellref))
                    {
                        if (!included.Contains(spellref.ID))
                        {
                            included.Add(spellref.ID);
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
            var files = LaunchpadPatcher.DownloadManifest(server);

            LaunchpadPatcher.DownloadFile(files[SpellFilename].Url, SpellFilename);
            LaunchpadPatcher.DownloadFile(files[DescFilename].Url, DescFilename);
        }

    }
}
