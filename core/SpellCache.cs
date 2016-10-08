using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Everquest
{
    public class SpellSearchFilter
    {
        // stores a list of the most popular effect types for populating a search suggestion dropdown
        public static readonly Dictionary<string, string> CommonEffects;

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

        static SpellSearchFilter()
        {
            CommonEffects = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            // some effect types are described differently by the parser than what players commonly call them
            CommonEffects.Add("Cure", @"^Decrease \w+ Counter by (\d+)");
            CommonEffects.Add("Heal", @"^Increase Current HP by ([1-9]\d+)(?!.*(?:per tick))"); // 1-9 to exclude spells with "Increase Current HP by 0" 
            CommonEffects.Add("HoT", @"^Increase Current HP by (\d+) per tick|\sHoT\s");
            CommonEffects.Add("Nuke", @"^Decrease Current HP by (\d+)(?!.*(?:per tick))");
            CommonEffects.Add("DoT", @"^Decrease Current HP by (\d+) per tick|\sDoT\s");
            CommonEffects.Add("Haste", @"^Increase Melee Haste (?:v3 )?by (\d+)");
            CommonEffects.Add("Slow", @"^(?:Decrease Melee Haste)|(?:Increase Melee Delay) by (\d+)");
            CommonEffects.Add("Snare", @"^Decrease Movement Speed by (\d+)");
            CommonEffects.Add("Shrink", @"^Decrease Player Size");
            CommonEffects.Add("Rune", "^Absorb");
            CommonEffects.Add("Pacify", @"^Decrease Social Radius");
            CommonEffects.Add("Fade", @"^Cancel Aggro");
            CommonEffects.Add("Defensive", @"^Increase Melee Mitigation");
            CommonEffects.Add("Damage Shield", @"^Increase Damage Shield by (\d+)");
            CommonEffects.Add("Mana Regen", @"^Increase Current Mana by (\d+)");
            CommonEffects.Add("Add Proc", @"^Add (?:Melee|Weapon|Skill|Range) Proc");
            CommonEffects.Add("Add Spell Proc", @"^Cast.+on Spell Use");
            CommonEffects.Add("Debuff", @"^Decrease (Chance of (?!Charm Breaking)|\w+ Resist|\w{2,3} by|Melee Haste|Damage Shield by)");

            // literal text suggestions (these words appear in parsed text so they don't need to be regexes)
            CommonEffects.Add("Charm", null);
            CommonEffects.Add("Mesmerize", null);
            CommonEffects.Add("Memory Blur", null);
            CommonEffects.Add("Root", null);
            CommonEffects.Add("Stun", null);
            CommonEffects.Add("Hate", null);
            CommonEffects.Add("Invisibility", null);
            CommonEffects.Add("Add Defensive Proc", null);
        }

        public SpellSearchFilter()
        {
            Effect = new string[3];
            EffectSlot = new int?[3];
        }

    }

    /// <summary>
    /// A spell and AA container with some search and cross referencing helpers.
    /// You don't need to use this class if all you want to do is to parse the spell data and dump it to console or a file.
    /// </summary>
    public class SpellCache
    {
        public string SpellPath { get; set; }

        public List<Spell> SpellList;
        private Dictionary<int, Spell> SpellsById;
        private ILookup<int, Spell> SpellsByGroup;

        public List<AA> AAList;


        public SpellCache()
        {
            SpellList = new List<Spell>();
            AAList = new List<AA>();
        }

        public void LoadSpells(string spellPath)
        {
            SpellPath = spellPath;
            SpellList = SpellParser.LoadFromFile(spellPath);
            SpellsById = SpellList.ToDictionary(x => x.ID);
            SpellsByGroup = SpellList.Where(x => x.GroupID != 0).ToLookup(x => x.GroupID);

            // fill LinksTo array for each spells - this will be used to include associated spells in search results
            // excluded spell IDs will be negated 
            foreach (var spell in SpellList)
            {
                var linked = new List<int>(10);

                // add recourse link
                if (spell.RecourseID != 0)
                    linked.Add(spell.RecourseID);

                // add spell slot links
                foreach (var s in spell.Slots)
                    if (s != null)
                    {
                        bool exclude = s.Desc.Contains("Exclude");

                        // match spell refs
                        var match = Spell.SpellRefExpr.Match(s.Desc);
                        if (match.Success)
                        {
                            int id = Int32.Parse(match.Groups[1].Value);
                            linked.Add(exclude ? -id : id);
                        }

                        // match spell group refs
                        match = Spell.GroupRefExpr.Match(s.Desc);
                        if (match.Success)
                        {
                            int id = Int32.Parse(match.Groups[1].Value);
                            linked.AddRange(SpellsByGroup[id].Select(x => exclude ? -x.ID : x.ID));
                        }
                    }

                // for each spell that is referenced, update ExtLevels if the spells isn't already flagged as usable by the class
                foreach (int id in linked)
                {
                    Spell target = null;
                    if (SpellsById.TryGetValue(id, out target))
                    {
                        target.RefCount++;

                        // a lot of side effect spells do not have a level on them. this will copy the level of the referring spell
                        // onto the side effect spell so that the spell will be searchable when filtering by class.
                        // e.g. Jolting Swings Strike has no level so it won't show up in a ranger search even though Jolting Swings will show up
                        // we create this separate array and never display it because modifying the levels array would imply different functionality
                        // e.g. some spells purposely don't have levels assigned so that they are not affected by focus spells
                        for (int i = 0; i < spell.Levels.Length; i++)
                        {
                            if (target.ExtLevels[i] == 0 && spell.Levels[i] != 0)
                                target.ExtLevels[i] = spell.Levels[i];

                            // apply in the reverse direction too. this will probably only be useful for including type3 augs
                            // todo: check if this includes too many focus spells
                            //if (spell.ExtLevels[i] == 0 && target.Levels[i] != 0)
                            //    spell.ExtLevels[i] = target.Levels[i];
                        }
                    }
                }

                spell.LinksTo = linked.ToArray();
            }
        }

        public void LoadAA(string aaPath, string descPath)
        {
            if (SpellList.Count == 0)
                throw new Exception("AA must be loaded after spells because they include spell references.");

            AAList = AAParser.LoadFromFile(aaPath, descPath);

            // fill LinksTo array for each AA - this will be used to include associated spells in search results
            // excluded spell IDs will be negated 
            foreach (var aa in AAList)
            {
                var linked = new List<int>(10);
                if (aa.SpellID > 0)
                    linked.Add(aa.SpellID);

                foreach (var s in aa.Slots.Where(x => x.Desc != null))
                {
                    bool exclude = s.Desc.Contains("Exclude");

                    // match spell refs
                    var match = Spell.SpellRefExpr.Match(s.Desc);
                    if (match.Success)
                    {
                        int id = Int32.Parse(match.Groups[1].Value);
                        linked.Add(exclude ? -id : id);
                    }

                    // match spell group refs
                    match = Spell.GroupRefExpr.Match(s.Desc);
                    if (match.Success)
                    {
                        int id = Int32.Parse(match.Groups[1].Value);
                        linked.AddRange(SpellsByGroup[id].Select(x => exclude ? -x.ID : x.ID));
                    }
                }

                aa.LinksTo = linked.Distinct().ToArray();
            }

            // update ExtLevels to 254 for the class if the spells isn't already flagged as usable
            foreach (var aa in AAList)
                foreach (var id in aa.LinksTo)
                {
                    Spell spell;
                    if (SpellsById.TryGetValue(id, out spell))
                        for (int i = 0; i < spell.Levels.Length; i++)
                        {
                            // todo: exclude spells? e.g. Spell Casting Reinforcement has two excludes
                            int mask = 1 << i;
                            if (spell.Levels[i] == 0 && ((int)aa.ClassesMask & mask) != 0)
                                spell.Levels[i] = 254;
                        }
                }
        }

        public IEnumerable<Spell> Search(SpellSearchFilter filter)
        {
            //return filter.Apply(SpellList);
            if (filter.ClassMaxLevel == 0)
                filter.ClassMinLevel = 1;
            if (filter.ClassMaxLevel == 0)
                filter.ClassMaxLevel = 255;

            IEnumerable<Spell> query = SpellList;

            // if the spell text filter is an integer then just do a quick search by ID and ignore the other filters
            int id;
            if (Int32.TryParse(filter.Text, out id))
                return query.Where(x => x.ID == id || x.GroupID == id);

            //  spell name and description are checked for literal text    
            if (!String.IsNullOrEmpty(filter.Text))
                query = query.Where(x => x.ID.ToString() == filter.Text
                    || x.Name.IndexOf(filter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0
                    || (x.Stacking != null && x.Stacking.Any(y => y.IndexOf(filter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0))
                    || (x.Desc != null && x.Desc.IndexOf(filter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0));

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
                    if (SpellSearchFilter.CommonEffects.ContainsKey(filter.Effect[i]))
                        effect = SpellSearchFilter.CommonEffects[filter.Effect[i]];

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

        public Spell GetSpell(int id)
        {
            Spell s;
            if (SpellsById.TryGetValue(id, out s))
                return s;
            return null;
        }

        public string GetSpellName(int id)
        {
            Spell s;
            if (SpellsById.TryGetValue(id, out s))
                return s.Name;
            return null;
        }

        public string GetSpellGroupName(int id)
        {
            Spell s = SpellsByGroup[id].FirstOrDefault();
            if (s != null)
                return "Group - " + StripRank(s.Name);
            return null;
        }

        /// <summary>
        /// Strip any digit or roman numeral rank from the end of a spell name.
        /// </summary>
        private string StripRank(string name)
        {
            name = Regex.Replace(name, @"\s\(?\d+\)?$", ""); // (3) 
            name = Regex.Replace(name, @"\s(Rk\.\s)?[IVX]+$", ""); // Rk. III
            return name;
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
                        comp = String.Compare(StripRank(a.Name), StripRank(b.Name));
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
                    int comp = String.Compare(StripRank(a.Name), StripRank(b.Name));
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
        /// Expand a spell list to include spells that are referenced by the original list.
        /// </summary>                
        public void AddForwardRefs(List<Spell> list)
        {
            var included = new HashSet<int>(list.Select(x => x.ID));

            // search the results to find other spells that the matches link to (forward links)
            int i = 0;
            while (i < list.Count)
            {
                Spell spell = list[i++];

                foreach (int id in spell.LinksTo.Select(x => x < 0 ? -x : x))
                    if (!included.Contains(id))
                    {
                        included.Add(id);
                        Spell linked;
                        if (SpellsById.TryGetValue(id, out linked))
                            list.Add(linked);
                    }
            }

        }

        /// <summary>
        /// Expand a spell list to include spells that make references to the original list.
        /// </summary> 
        public void AddBackRefs(List<Spell> list)
        {
            if (list.Count == 0)
                return;

            var included = new HashSet<int>(list.Select(x => x.ID));

            // do not include back refs for spells that are heavily referenced. 
            // e.g. complete heal is referenced by hundreds of focus spells (we wouldn't want to add those focus spells to the results)
            var ignore = list.Where(x => x.RefCount > 10).Select(x => x.ID).ToList();
            foreach (var spell in SpellList)
            {
                foreach (int id in spell.LinksTo)
                    if (!ignore.Contains(id) && included.Contains(id) && !included.Contains(spell.ID))
                    {
                        included.Add(spell.ID);
                        list.Add(spell);
                    }
            }
        }


    }


}
