/*

This is the command-line version of the parser. Mostly intended for dumping data for diffs and exports.

To download a fresh spell file:
>parser update

To print all spells in text format:
>parser all

To print all spells in delimited format:
>parser all csv

To print a single spell by ID:
>parser id 13

*/

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
            var format = "text";

            try
            {
                int dec = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalDigits;
                if (dec < 2)
                    Console.Error.WriteLine("Your system has been configured to display {0} decimal digits. Some values will be rounded.", dec);

                if (args.Length == 0)
                {
                    Console.Error.WriteLine("You must include a search parameter on the command-line.");
                    Console.Error.WriteLine("e.g. to dump all spells try: parser all");
                    Thread.Sleep(2000);
                    return;
                }

                if (args[0] == "update")
                {
                    var server = args.Length >= 2 ? args[1] : null;
                    LaunchpadPatcher.DownloadSpellFiles(server);
                    return;
                }

                if (args.Any(x => x == "csv"))
                {
                    format = "csv";
                }

                if (File.Exists(args[0]))
                {
                    path = args[0];
                    args[0] = "all";
                }

                if (!File.Exists(path))
                    LaunchpadPatcher.DownloadSpellFiles();

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

                    if (format == "csv")
                    {
                        //DataDump(results, fields => String.Join(",", fields.Select(x => '"' + x.ToString() + '"').ToArray())); // csv
                        DataDump(results, fields => String.Join("^", fields.Select(x => x.ToString()).ToArray())); // eq format
                    }
                    else
                    {
                        TextDump(results);
                    }
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
        /// Print spells in text format.
        /// </summary>
        static void TextDump(IEnumerable<Spell> list)
        {
            foreach (var spell in list)
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
        /// Print spells in delimited data format.
        /// This doesn't include all fields but it easy enough to add whatever else you need in here.
        /// </summary>
        static void DataDump(IEnumerable<Spell> list, Func<List<object>, string> pack)
        {
            // write header
            var fields = new List<object>();
            fields.Add("id");
            fields.Add("group");
            fields.Add("icon");
            fields.Add("name");
            fields.Add("skill");
            fields.Add("mana");
            fields.Add("end");
            fields.Add("end_upkeep");
            fields.Add("cast_time");
            fields.Add("recast_time");
            fields.Add("target");
            fields.Add("resist_type");
            fields.Add("resist_mod");
            fields.Add("range");
            fields.Add("ae_range");
            fields.Add("duration");
            fields.Add("hate_mod");
            fields.Add("hate_given");
            fields.Add("push");
            fields.Add("classes");
            // war, clr, pal, rng, shd, dru, mnk, brd, rog, shm, nec, wiz, mag, enc, bst, ber
            for (var i = 1; i <= 16; i++)
                fields.Add(((SpellClasses)i).ToString().ToLower());
            fields.Add("slots");

            Console.WriteLine(pack(fields));

            foreach (var spell in list)
            {
                fields = new List<object>();
                fields.Add(spell.ID);
                fields.Add(spell.GroupID);
                fields.Add(spell.Icon);
                fields.Add(spell.Name);
                fields.Add(spell.Skill.ToString().Replace('_', ' '));
                fields.Add(spell.Mana);
                fields.Add(spell.Endurance);
                fields.Add(spell.EnduranceUpkeep);
                fields.Add(spell.CastingTime.ToString("F2"));
                fields.Add(spell.RecastTime.ToString("F2"));
                fields.Add(spell.Target);
                fields.Add(spell.ResistType);
                fields.Add(spell.ResistMod);
                fields.Add(spell.Range);
                fields.Add(spell.AERange);
                fields.Add(spell.DurationTicks);
                fields.Add(spell.HateMod);
                fields.Add(spell.HateOverride);
                fields.Add(spell.PushBack);
                fields.Add(spell.ClassesLevels);

                // levels for each class (16 fields)
                for (var i = 0; i < spell.Levels.Length; i++)
                    fields.Add(spell.Levels[i]);

                // encode slots as variable length "|" delimited list
                var slots = new List<string>();
                if (spell.Recourse != null)
                    slots.Add("Recourse: Cast " + spell.Recourse);
                for (int i = 0; i < spell.Slots.Count; i++)
                    if (spell.Slots[i] != null)
                        slots.Add(String.Format("{0}: {1}", i + 1, spell.Slots[i].Desc));
                fields.Add(String.Join("|", slots.ToArray()));

                Console.WriteLine(pack(fields));
            }

        }
    }
}
