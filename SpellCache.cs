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
            CommonEffects.Add("Cure", @"Decrease \w+ Counter by (\d+)");
            CommonEffects.Add("Heal", @"Increase Current HP by ([1-9]\d+)(?!.*(?:per tick))"); // 1-9 to exclude spells with "Increase Current HP by 0" 
            CommonEffects.Add("HoT", @"Increase Current HP by (\d+) per tick");
            CommonEffects.Add("Nuke", @"Decrease Current HP by (\d+)(?!.*(?:per tick))");
            CommonEffects.Add("DoT", @"Decrease Current HP by (\d+) per tick");
            CommonEffects.Add("Haste", @"Increase Melee Haste (?:v3 )?by (\d+)");
            CommonEffects.Add("Slow", @"(?:Decrease Melee Haste)|(?:Increase Melee Delay) by (\d+)");
            CommonEffects.Add("Snare", @"Decrease Movement Speed by (\d+)");
            CommonEffects.Add("Shrink", @"Decrease Player Size");
            CommonEffects.Add("Rune", "Absorb");
            CommonEffects.Add("Pacify", @"Decrease Social Radius");
            CommonEffects.Add("Damage Shield", @"Increase Damage Shield by (\d+)");
            CommonEffects.Add("Mana Regen", @"Increase Current Mana by (\d+)");
            CommonEffects.Add("Add Proc", @"(?:Add Proc)|(?:Add Skill Proc)");
            CommonEffects.Add("Add Spell Proc", @"Cast.+on Spell Use");

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

        /// <summary>
        /// Apply this search filter to a list of spells and return the matches.
        /// </summary>
        public IEnumerable<Spell> Apply(IEnumerable<Spell> spells)
        {
            if (ClassMaxLevel == 0)
                ClassMinLevel = 1;
            if (ClassMaxLevel == 0)
                ClassMaxLevel = 255;
            
            IEnumerable<Spell> query = spells;

            // if the spell text filter is an integer then just do a quick search by ID and ignore the other filters
            int id;
            if (Int32.TryParse(Text, out id))
                return query.Where(x => x.ID == id || x.GroupID == id);

            //  spell name and description are checked for literal text    
            if (!String.IsNullOrEmpty(Text))
                query = query.Where(x => x.ID.ToString() == Text || x.Name.IndexOf(Text, StringComparison.InvariantCultureIgnoreCase) >= 0 || (x.Desc != null && x.Desc.IndexOf(Text, StringComparison.InvariantCultureIgnoreCase) >= 0));

            // level filter is only used when a class is selected
            int levelArrayIndex = SpellParser.ParseClass(Class) - 1;
            if (Class == "Any PC")
            {
                query = query.Where(x => x.ClassesMask != 0);
            }
            else if (Class == "Non PC")
            {
                //query = query.Where(x => x.ClassesMask == 0);
                query = query.Where(x => x.ExtLevels.All(y => y == 0));
            }
            else if (!String.IsNullOrEmpty(Class) && Category != "AA")
            {
                if (levelArrayIndex >= 0)
                    query = query.Where(x => x.ClassesLevels != "ALL/254" && x.ExtLevels[levelArrayIndex] >= ClassMinLevel && x.ExtLevels[levelArrayIndex] <= ClassMaxLevel);
            }

            if (!String.IsNullOrEmpty(Category))
            {
                if (Category == "AA" && levelArrayIndex >= 0)
                    query = query.Where(x => x.ExtLevels[levelArrayIndex] == 254);
                else if (Category == "AA")
                    query = query.Where(x => x.ExtLevels.Any(y => y == 254));
                else
                    query = query.Where(x => x.Categories.Any(y => y.StartsWith(Category, StringComparison.InvariantCultureIgnoreCase)));
            }

            // effect filter can be a literal string or a regex
            for (int i = 0; i < Effect.Length; i++)
                if (!String.IsNullOrEmpty(Effect[i]))
                {
                    string effect = null;
                    if (CommonEffects.ContainsKey(Effect[i]))
                        effect = CommonEffects[Effect[i]];

                    if (!String.IsNullOrEmpty(effect))
                    {
                        var re = new Regex(effect, RegexOptions.IgnoreCase);
                        int? slot = EffectSlot[i];
                        query = query.Where(x => x.HasEffect(re, slot ?? 0));
                    }
                    else
                    {
                        string text = Effect[i];
                        int? slot = EffectSlot[i];
                        query = query.Where(x => x.HasEffect(text, slot ?? 0));
                    }
                }

            return query;
        }

    }

    /// <summary>
    /// A spell and AA container with some search and cross referencing helpers.
    /// </summary>
    public class SpellCache 
    {
        public List<Spell> SpellList;
        private Dictionary<int, Spell> SpellsById;
        private ILookup<int, Spell> SpellsByGroup;

        public string SpellPath { get; private set; }

        public List<AA> AAList;


        public SpellCache()
        {
            SpellList = new List<Spell>();
            AAList = new List<AA>();
        }

        public void LoadSpells(string spellPath, string descPath, string stackPath)
        {
            SpellPath = spellPath;
            SpellList = SpellParser.LoadFromFile(spellPath, descPath, stackPath);
            SpellsById = SpellList.ToDictionary(x => x.ID);
            SpellsByGroup = SpellList.Where(x => x.GroupID != 0).ToLookup(x => x.GroupID);

            // init LinksTo cross references for each spell
            foreach (var spell in SpellList)
            {
                var linked = new List<int>(10);

                // add recourse link
                if (spell.RecourseID != 0)
                    linked.Add(spell.RecourseID);

                // add spell slot links
                foreach (var s in spell.Slots)
                    if (s.Desc != null)
                    {
                        // match spell refs
                        var match = Spell.SpellRefExpr.Match(s.Desc);
                        if (match.Success)
                            linked.Add(Int32.Parse(match.Groups[1].Value));

                        // match spell group refs
                        match = Spell.GroupRefExpr.Match(s.Desc);
                        if (match.Success)
                        {
                            int id = Int32.Parse(match.Groups[1].Value);
                            // for excluded spells temporarily negate id on excluded spells so that we don't set extlevels on them
                            if (s.Desc.Contains("Exclude"))
                                id = -id;
                            foreach (var ls in SpellsByGroup[id])
                                linked.Add(ls.ID);
                        }
                    }

                // process each of the linked spells
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

                // revert negated IDs on excluded spells/groups
                for (int i = 0; i < linked.Count; i++)
                    if (linked[i] < 0)
                        linked[i] = -linked[i];

                spell.LinksTo = linked.ToArray();
            }        
        }

        public void LoadAA(string aaPath, string descPath)
        {
            if (SpellList.Count == 0)
                throw new Exception("AA must be loaded after spells because they include spell references.");

            AAList = AAParser.LoadFromFile(aaPath, descPath);

            // for each AA build a list of referenced spells - this will be used to include associated spells in search results
            foreach (var aa in AAList)
            {
                var linked = new List<int>(10);
                if (aa.SpellID > 0)
                    linked.Add(aa.SpellID);

                foreach (var s in aa.Slots.Where(x => x.Desc != null))
                {
                    // match spell refs
                    var match = Spell.SpellRefExpr.Match(s.Desc);
                    if (match.Success)
                    {
                        int id = Int32.Parse(match.Groups[1].Value);
                        linked.Add(id);
                    }

                    // match spell group refs
                    match = Spell.GroupRefExpr.Match(s.Desc);
                    if (match.Success)
                    {
                        int id = Int32.Parse(match.Groups[1].Value);
                        linked.AddRange(SpellsByGroup[id].Select(x => x.ID));
                    }
                }

                aa.LinksTo = linked.Distinct().ToArray();
            }
        }

        public IEnumerable<Spell> Search(SpellSearchFilter filter)
        {
            return filter.Apply(SpellList);
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
                        if (SpellsById.TryGetValue(id, out linked))
                            list.Add(linked);
                    }
            }

        }

        /// <summary>
        /// Expand a spell list to include spells that make references to the results.
        /// </summary> 
        public void AddBackRefs(List<Spell> list)
        {
            if (list.Count == 0)
                return;

            var included = new HashSet<int>(list.Select(x => x.ID));

            // do not include not produce back refs for spells that are heavily reference. 
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
