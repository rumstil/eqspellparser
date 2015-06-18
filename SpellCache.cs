using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Everquest
{
    public class SpellSearchFilter
    {
        public string Text { get; set; }
        public string[] Effect { get; set; }
        public int?[] EffectSlot { get; set; }
        public string Class { get; set; }
        public int ClassMinLevel { get; set; }
        public int ClassMaxLevel { get; set; }
        public string Category { get; set; }
        public int Rank { get; set; }
        
        // these are for post search processing:
        //public bool AddForwardRefs { get; set; }
        public bool AddBackRefs { get; set; }


        public SpellSearchFilter()
        {
            Effect = new string[3];
            EffectSlot = new int?[3];
        }

    }


    /// <summary>
    /// Encapsulates a simple spell list to provide some search and cross referencing abilities.
    /// </summary>
    public class SpellCache : IEnumerable<Spell>
    {
        public static readonly Dictionary<string, string> EffectSearchHelpers;

        private string path;
        private List<Spell> spells;
        private Dictionary<int, Spell> spellsById;
        //private Dictionary<string, Spell> spellsByName;
        private ILookup<int, Spell> spellsByGroup;

        public string Path { get { return path; } }

        static SpellCache()
        {
            EffectSearchHelpers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            // literal text suggestions (these words appear in parsed text)
            EffectSearchHelpers.Add("Charm", null);
            EffectSearchHelpers.Add("Mesmerize", null);
            EffectSearchHelpers.Add("Memory Blur", null);
            EffectSearchHelpers.Add("Root", null);
            EffectSearchHelpers.Add("Stun", null);
            EffectSearchHelpers.Add("Hate", null);
            EffectSearchHelpers.Add("Invisibility", null);
            EffectSearchHelpers.Add("Add Defensive Proc", null);

            EffectSearchHelpers.Add("Cure", @"Decrease \w+ Counter by (\d+)");
            EffectSearchHelpers.Add("Heal", @"Increase Current HP by ([1-9]\d+)(?!.*(?:per tick))"); // 1-9 to exclude spells with "Increase Current HP by 0" 
            EffectSearchHelpers.Add("HoT", @"Increase Current HP by (\d+) per tick");
            EffectSearchHelpers.Add("Nuke", @"Decrease Current HP by (\d+)(?!.*(?:per tick))");
            EffectSearchHelpers.Add("DoT", @"Decrease Current HP by (\d+) per tick");
            EffectSearchHelpers.Add("Haste", @"Increase Melee Haste (?:v3 )?by (\d+)");
            EffectSearchHelpers.Add("Slow", @"Decrease Melee Haste by (\d+)");
            EffectSearchHelpers.Add("Snare", @"Decrease Movement Speed by (\d+)");
            EffectSearchHelpers.Add("Shrink", @"Decrease Player Size");
            EffectSearchHelpers.Add("Rune", "Absorb");
            EffectSearchHelpers.Add("Pacify", @"Decrease Social Radius");
            EffectSearchHelpers.Add("Damage Shield", @"Increase Damage Shield by (\d+)");
            EffectSearchHelpers.Add("Mana Regen", @"Increase Current Mana by (\d+)");
            EffectSearchHelpers.Add("Add Proc", @"(?:Add Proc)|(?:Add Skill Proc)");
            EffectSearchHelpers.Add("Add Spell Proc", @"Cast.+on Spell Use");
        }

        public SpellCache(string path, List<Spell> list)
        {
            this.path = path;
            spells = list;
            spellsById = list.ToDictionary(x => x.ID);
            //spellsByName = list.ToDictionary(x => x.Name);
            spellsByGroup = list.Where(x => x.GroupID != 0).ToLookup(x => x.GroupID);
        }

        public int Count { get { return spells.Count; } }

        public string GetSpellName(int id)
        {
            Spell s;
            if (spellsById.TryGetValue(id, out s))
                return s.Name;
            return null;
        }

        public string GetSpellGroupName(int id)
        {
            Spell s = spellsByGroup[id].FirstOrDefault();
            if (s != null)
                return "Group - " + s.Name;
            return null;
        }

        /// <summary>
        /// Insert spell names. e.g. [Spell 13] -> Complete Heal [Spell 13]
        /// </summary>
        public string InsertSpellNames(string text)
        {
            text = Spell.SpellRefExpr.Replace(text, m =>
            {
                int id = Int32.Parse(m.Groups[1].Value);
                return String.Format("{1} [Spell {0}]", id, GetSpellName(id));
            });

            text = Spell.GroupRefExpr.Replace(text, m =>
            {
                int id = Int32.Parse(m.Groups[1].Value);
                return String.Format("{1} [Group {0}]", id, GetSpellGroupName(id));
            });

            return text;
        }

        public IEnumerable<Spell> Search(SpellSearchFilter filter) 
        {
            if (filter.ClassMaxLevel == 0)
                filter.ClassMinLevel = 1;
            if (filter.ClassMaxLevel == 0)
                filter.ClassMaxLevel = 255;

            
            IEnumerable<Spell> query = spells;

            // if the spell text filter is an integer then just do a quick search by ID and ignore the other filters
            int id;
            if (Int32.TryParse(filter.Text, out id))
                return query.Where(x => x.ID == id || x.GroupID == id);

            //  spell name and description are checked for literal text    
            if (!String.IsNullOrEmpty(filter.Text))
                query = query.Where(x => x.ID.ToString() == filter.Text || x.Name.IndexOf(filter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0 || (x.Desc != null && x.Desc.IndexOf(filter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0));

            // level filter is only used when a class is selected
            int levelArrayIndex = SpellParser.ParseClass(filter.Class) - 1;
            if (filter.Class == "Any PC")
            {
                query = query.Where(x => x.ClassesMask != 0);
            }
            else if (filter.Class == "Non PC")
            {
                //query = query.Where(x => x.ClassesMask == 0);
                query = query.Where(x => x.ExtLevels.All(y => y == 0));
            }
            else if (!String.IsNullOrEmpty(filter.Class) && filter.Category != "AA")
            {
                if (levelArrayIndex >= 0)
                    query = query.Where(x => x.ClassesLevels != "ALL/254" && x.ExtLevels[levelArrayIndex] >= filter.ClassMinLevel && x.ExtLevels[levelArrayIndex] <= filter.ClassMaxLevel);
            }

            if (!String.IsNullOrEmpty(filter.Category))
            {
                if (filter.Category == "AA" && levelArrayIndex >= 0)
                    query = query.Where(x => x.ExtLevels[levelArrayIndex] == 254);
                else if (filter.Category == "AA")
                    query = query.Where(x => x.ExtLevels.Any(y => y == 254));
                else
                    query = query.Where(x => x.Categories.Any(y => y.StartsWith(filter.Category, StringComparison.InvariantCultureIgnoreCase)));
            }

            // effect filter can be a literal string or a regex
            for (int i = 0; i < filter.Effect.Length; i++)
                if (!String.IsNullOrEmpty(filter.Effect[i]))
                {
                    string effect = null;
                    if (EffectSearchHelpers.ContainsKey(filter.Effect[i]))
                        effect = EffectSearchHelpers[filter.Effect[i]];

                    if (!String.IsNullOrEmpty(effect))
                    {
                        var re = new Regex(effect, RegexOptions.IgnoreCase);
                        int? slot = filter.EffectSlot[i];
                        query = query.Where(x => x.HasEffect(re, slot ?? 0));
                    }
                    else
                    {
                        string text = filter.Effect[i];
                        int? slot = filter.EffectSlot[i];
                        query = query.Where(x => x.HasEffect(text, slot ?? 0));
                    }
                }

            return query;
        }

        /// <summary>
        /// Sort a spell list based on what kind of filters were used.
        /// </summary>
        public void Sort(List<Spell> list, SpellSearchFilter filter)
        {
            int cls = SpellParser.ParseClass(filter.Class) - 1;
            int id;
            Int32.TryParse(filter.Text, out id);

            // 1. if an effect is selected then sort by the effect strength
            // this is problematic since many spells have conditional effects 
            //if (effect.Contains(@"(\d+)"))
            //{
            //    Sorting = "Results have been sorted by descending " + SearchEffect.Text + " strength.";
            //    var re = new Regex(effect, RegexOptions.IgnoreCase);
            //    Results.Sort((a, b) =>
            //    {
            //        int comp = b.ScoreEffect(re) - a.ScoreEffect(re);
            //        if (comp == 0)
            //            comp = a.ID - b.ID;
            //        return comp;
            //    });
            //}
            // 2. if a class is selected then sort by the casting levels for that class first
            // place castable spells before non castable effects (level == 0)
            if (cls >= 0)
            {
                list.Sort((a, b) =>
                {
                    if (a.Levels[cls] > 0 && b.Levels[cls] == 0)
                        return -1;
                    if (b.Levels[cls] > 0 && a.Levels[cls] == 0)
                        return 1;
                    int comp = a.Levels[cls] - b.Levels[cls];
                    if (comp == 0)
                        comp = String.Compare(TrimNumerals(a.Name), TrimNumerals(b.Name));
                    if (comp == 0)
                        comp = a.ID - b.ID;
                    return comp;
                });
            }
            // 3. finally sort by name if no better method is found
            else
            {
                list.Sort((a, b) =>
                {
                    int comp = String.Compare(TrimNumerals(a.Name), TrimNumerals(b.Name));
                    if (comp == 0)
                        comp = a.ID - b.ID;
                    return comp;
                });

            }

            // if searching by id, move the spell to the top of the results because it may be sorted below it's side effect spells
            if (id > 0)
            {
                var i = list.FindIndex(x => x.ID == id);
                if (i > 0)
                {
                    var move = list[i];
                    list.RemoveAt(i);
                    list.Insert(0, move);
                }
            }
            // move entries that begin with the search text to the front of the results
            else if (!String.IsNullOrEmpty(filter.Text))
            {
                var move = list.FindAll(x => x.Name.StartsWith(filter.Text, StringComparison.InvariantCultureIgnoreCase));
                if (move.Count > 0)
                {
                    list.RemoveAll(x => x.Name.StartsWith(filter.Text, StringComparison.InvariantCultureIgnoreCase));
                    list.InsertRange(0, move);
                }
            }

        }

        /// <summary>
        /// Expand a spell list to include spells that the existing list makes references to.
        /// </summary>                
        public void AddForwardRefs(List<Spell> list)
        {
            var included = new HashSet<int>(list.Select(x => x.ID));

            // search the results to find other spells that the matches link to (forward links)
            int i = 0;
            while (i < list.Count)
            {
                Spell spell = list[i++];

                foreach (int id in spell.LinksTo)
                    if (!included.Contains(id))
                    {
                        included.Add(id);
                        Spell linked;
                        if (spellsById.TryGetValue(id, out linked))
                            list.Add(linked);
                    }
            }

        }

        /// <summary>
        /// Expand a spell list to include spells that make reference to the results.
        /// </summary> 
        public void AddBackRefs(List<Spell> list)
        {
            if (list.Count == 0)
                return;

            var included = new HashSet<int>(list.Select(x => x.ID));

            // do not include not produce back refs for spells that are heavily reference. 
            // e.g. complete heal is referenced by hundreds of focus spells (we wouldn't want to add those focus spells to the results)
            var ignore = list.Where(x => x.RefCount > 10).Select(x => x.ID).ToList();
            foreach (var spell in spells)
            {
                foreach (int id in spell.LinksTo)
                    if (!ignore.Contains(id) && included.Contains(id) && !included.Contains(spell.ID))
                    {
                        included.Add(spell.ID);
                        list.Add(spell);
                    }
            }
        }

        private string TrimNumerals(string text)
        {
            // trim rank
            int i = text.IndexOf("Rk.");
            if (i > 0)
                text = text.Substring(0, i - 1);

            // trim numerals
            text = text.TrimEnd(' ', 'X', 'V', 'I');

            return text;
        }


        #region IEnumerable<Spell> Members

        public IEnumerator<Spell> GetEnumerator()
        {
            return spells.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return spells.GetEnumerator();
        }

        #endregion
    }


}
