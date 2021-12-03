using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;


namespace EQSpellParser
{
    /// <summary>
    /// SpellParser reads spells_us.txt and related files to generate Spell data instances for every spell.
    /// Most of the interesting SPA and calc parsing is handled by the Spell class.
    /// </summary>
    public static partial class SpellParser
    {
        /// <summary>
        /// Load all spells from spells_us.txt.
        /// This method only takes the path to the main spells_us.txt. It will attempt to load supporting files 
        /// as long as they are in the same folder and use the expected naming convention.
        /// </summary>
        public static List<Spell> LoadFromFile(string spellPath)
        {
            // the spell file is required. the other files are optional 
            if (!File.Exists(spellPath))
                throw new FileNotFoundException("Could not open spell file.", spellPath);

            var version = GetFileVersion(spellPath);

            var list = new List<Spell>(60000);
            var listById = new Dictionary<int, Spell>(list.Capacity);

            // load description strings file
            var desc = new Dictionary<string, string>(80000);
            var descPath = spellPath.Replace("spells_us", "dbstr_us");
            foreach (var fields in LoadAllLines(descPath))
            {
                // currently 4 fields - older files are 3 fields
                if (fields.Length >= 3)
                    desc[fields[1] + "/" + fields[0]] = fields[2].Trim();
                // 0 = id within type
                // 1 = type
                // 2 = description
                // type 1 = AA names
                // type 4 = AA desc
                // type 5 = spell categories
                // type 6 = spell desc
                // type 7 = lore groups
                // type 11 = illusions
                // type 12 = body type
                // type 16 = aug slot desc
                // type 18 = currency
                // type 40 = spell stacking group
                // type 45 = faction 
            }

            // load spell definition file
            foreach (var fields in LoadAllLines(spellPath))
            {
                // parser the spell
                var spell = ParseSpell(fields, version);

                // debug
                //if (spell.ID == 3)
                //{
                //    for (int i = 0; i < fields.Length; i++)
                //        Console.Error.WriteLine("{0}: {1}", i, fields[i]);
                //}


                list.Add(spell);
                listById[spell.ID] = spell;

                // store group ID of first spell in a group by using negative IDs to avoid conflicts with spell IDs
                if (spell.GroupID > 0 && !listById.ContainsKey(-spell.GroupID))
                    listById[-spell.GroupID] = spell;

                // update faction references with actual names
                for (int i = 0; i < spell.Slots.Count; i++)
                    if (spell.Slots[i] != null)
                    {
                        var slot = spell.Slots[i];

                        slot.Desc = Spell.FactionRefExpr.Replace(slot.Desc, m =>
                        {
                            string fname = null;
                            if (desc.TryGetValue("45/" + m.Groups[1].Value, out fname))
                                return fname;
                            return m.Groups[0].Value;
                        });
                    }

                // update description
                if (!desc.TryGetValue("6/" + spell.DescID, out spell.Desc))
                    spell.Desc = null;

                // all spells can be grouped into up to 2 categories (type 5 in db_str)
                // ignore the "timer" categories because they are frequently wrong
                var cat = new List<string>(3);
                string c1;
                string c2;
                if (desc.TryGetValue("5/" + spell.CategoryDescID[0], out c1))
                {
                    // sub category 1
                    if (desc.TryGetValue("5/" + spell.CategoryDescID[1], out c2) && !c2.StartsWith("Timer"))
                        cat.Add(c1 + "/" + c2);

                    // sub category 2
                    if (desc.TryGetValue("5/" + spell.CategoryDescID[2], out c2) && !c2.StartsWith("Timer"))
                        cat.Add(c1 + "/" + c2);

                    // general category if no subcategories are defined
                    if (cat.Count == 0)
                        cat.Add(c1);
                }

                // add a timer category 
                if (spell.TimerID > 0)
                    cat.Add("Timer " + spell.TimerID.ToString("D2"));
                spell.Categories = cat.ToArray();

            }

            // load spell string file (starting 2018-2-14)
            // *SPELLINDEX^CASTERMETXT^CASTEROTHERTXT^CASTEDMETXT^CASTEDOTHERTXT^SPELLGONE^
            var strPath = spellPath.Replace("spells_us", "spells_us_str");
            foreach (var fields in LoadAllLines(strPath))
            {
                var id = ParseInt(fields[0]);
                var landself = fields[3];
                var landother = fields[4];

                Spell spell = null;
                if (!listById.TryGetValue(id, out spell))
                    continue;

                spell.LandOnSelf = landself;
                //spell.LandOnOther = landother; // this doesn't really get used so saving memory by excluding it
            }

            // load spell stacking file
            // my guess is that this was made a separate file because a single spell can be part of multiple spell stacking groups
            // #*SPELL_ID^SPELL_STACKING_GROUP^STACKING_RANK^SPELL_STACKING_TYPE^
            var stackPath = spellPath.Replace("spells_us", "SpellStackingGroups");
            var stackGroups = new Dictionary<int, string>();
            foreach (var fields in LoadAllLines(stackPath))
            {
                var id = ParseInt(fields[0]);
                var group = ParseInt(fields[1]);
                var rank = fields[2];
                var type = fields[3];
                string stacking = group.ToString();

                Spell spell = null;
                if (!listById.TryGetValue(id, out spell))
                    continue;

                if (desc.ContainsKey("40/" + group))
                {
                    // most stacking groups reference a description in dbstr
                    stacking = desc["40/" + group];
                }
                else
                {
                    // however some stacking groups are missing from dbstr
                    // in these cases we should use the name of the first spell that references the stacking group
                    stackGroups.TryGetValue(group, out stacking);
                    if (String.IsNullOrEmpty(stacking))
                        stackGroups[group] = stacking = spell.Name;
                }

                stacking += " " + rank;

                // type 6 = non-override stacking group; only one can be active at a time and new procs won't overwrite old ones that are still active
                if (type == "6")
                    stacking += " (Non-Overide)";

                spell.Stacking.Add(stacking);
            }

            // replace tokens in spell descriptions 
            foreach (var spell in list)
            {
                if (spell.Desc == null)
                    continue;

                spell.PrepareDesc(listById);
            }

            return list;
        }

        /// <summary>
        /// Load a single spell from the spells file.
        /// This will automatically pick the best parser for the file format (which has changed over time)
        /// </summary>
        static Spell ParseSpell(string[] fields, int version)
        {
            Func<string[], int, Spell> parser = ParseSpellCurrent;

            // spells_us.txt field layout has changed over time - pick the best parser
            if (fields.Length > 220)
                parser = ParseSpell20160210;
            if (fields.Length == 179 && version < 20160413)
                parser = ParseSpell20160413;
            if (fields.Length == 174 && version < 20180204)
                parser = ParseSpell20180214;
            if (fields.Length == 169 && version < 20180410)
                parser = ParseSpell20180410;
            if (fields.Length == 168 && version < 20180509)
                parser = ParseSpell20180509;
            if (fields.Length == 167 && version < 20191008)
                parser = ParseSpell20190108;
            if (fields.Length == 166 && version < 20210113)
                parser = ParseSpell20210113;
            if (fields.Length == 168 && version < 20210113)
                parser = ParseSpell20210113;
            if (fields.Length == 167 && version < 20210715)
                parser = ParseSpell20210715;

            return parser(fields, version);
        }

        static float ParseFloat(string s)
        {
            if (String.IsNullOrEmpty(s))
                return 0f;
            return Single.Parse(s, CultureInfo.InvariantCulture);
        }

        static int ParseInt(string s)
        {
            if (s == "" || s == "0" || s[0] == '.')
                return 0;
            // strip decimals. i.e. floor()
            s = Regex.Replace(s, @"\..+", "");
            return Int32.Parse(s, CultureInfo.InvariantCulture);
        }

        static bool ParseBool(string s)
        {
            return !String.IsNullOrEmpty(s) && (s != "0");
        }

        public static int ParseClass(string text)
        {
            if (String.IsNullOrEmpty(text))
                return 0;

            if (text.Length == 3 && Enum.IsDefined(typeof(SpellClasses), text.ToUpper()))
                return (int)Enum.Parse(typeof(SpellClasses), text.ToUpper());

            string[] names = Enum.GetNames(typeof(SpellClassesLong));
            for (int i = 0; i < names.Length; i++)
                if (String.Compare(names[i], text, true) == 0)
                    return (int)Enum.Parse(typeof(SpellClassesLong), names[i]);

            return 0;
        }

        /// <summary>
        /// Parse roman numerals up to 50. Returns 0 if the numeral could not be parsed.
        /// </summary>
        static int ParseRomanNumeral(string num)
        {
            var roman = new string[] {
                "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" ,
                "XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX", "XX",
                "XXI", "XXII", "XXIII", "XXIV", "XXV", "XXVI", "XXVII", "XXVIII", "XXIX", "XXX",
                "XXXI", "XXXII", "XXXIII", "XXXIV", "XXXV", "XXXVI", "XXXVII", "XXXVIII", "XXXIX", "XL",
                "XLI", "XLII", "XLIII", "XLIV", "XLV", "XLVI", "XLVII", "XLVIII", "XLIX", "L",
            };

            return Array.IndexOf(roman, num.ToUpper()) + 1;
        }

        /// <summary>
        /// Open a spell file and perform gzip decompression if needed.
        /// The game doesn't ever store spells files in gzip format but I added this in case someone wants to keep lots of versioned spell files around and save space.
        /// </summary>
        public static StreamReader OpenFileReader(string path)
        {
            var f = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (path.EndsWith(".gz"))
            {
                // todo: should I buffer the uncompressed stream?
                var gzip = new GZipStream(f, CompressionMode.Decompress);
                return new StreamReader(gzip);
            }

            return new StreamReader(f);
        }

        /// <summary>
        /// Load all lines from a delimited spell data file and split each up into individual fields.
        /// </summary>
        public static IEnumerable<string[]> LoadAllLines(string path)
        {
            if (File.Exists(path))
                using (var text = OpenFileReader(path))
                    while (!text.EndOfStream)
                    {
                        var line = text.ReadLine();
                        if (line.StartsWith("#"))
                            continue;
                        var fields = line.Split('^');
                        if (fields.Length < 2 || line.StartsWith("#"))
                            continue;
                        yield return fields;
                    }
        }

        public static int CountFields(string path)
        {
            if (path.EndsWith(".gz"))
                return 0;

            using (var f = File.OpenText(path))
            {
                return f.ReadLine().Split('^').Length;
            }
        }

        /// <summary>
        /// Return file date as an integer. e.g. Feb 1, 2016 -> 20160201
        /// </summary>
        public static int GetFileVersion(string path)
        {
            // use the file timestamp as the version string 
            // (this is not perfect because file timestamps can be accidentally changed)
            var version = File.GetCreationTime(path).ToString("yyyyMMdd");

            // also check the filename for a date version string. e.g. spells_us-2016-01-01.txt
            var versionInName = Regex.Match(Path.GetFileName(path), @"\d\d\d\d-\d\d-\d\d");
            if (versionInName.Success)
                version = versionInName.Groups[0].Value.Replace("-", "");

            return Int32.Parse(version);
        }

    }

}
