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
        public string Effect { get; set; }
        public int EffectSlot { get; set; }
        public string Class { get; set; }
        public int ClassMinLevel { get; set; }
        public int ClassMaxLevel { get; set; }
        public string Category { get; set; }
        //public bool AppendForwardRefs { get; set; }
        //public bool AppendBackRefs { get; set; }
    }


    /// <summary>
    /// Encapsulates a simple spell list to provide some search and cross referencing abilities.
    /// </summary>
    public class SpellCache : IEnumerable<Spell>
    {
        public static readonly Dictionary<string, string> EffectSearchHelpers;

        private string id;
        private List<Spell> spells;
        private Dictionary<int, Spell> spellsById;
        private ILookup<int, Spell> spellsByGroup;

        public string Id { get { return id; } set { id = value; } }

        static SpellCache()
        {
            EffectSearchHelpers = new Dictionary<string, string>();

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
            EffectSearchHelpers.Add("Rune", "@Absorb");
            EffectSearchHelpers.Add("Pacify", @"Decrease Social Radius");
            EffectSearchHelpers.Add("Damage Shield", @"Increase Damage Shield by (\d+)");
            EffectSearchHelpers.Add("Mana Regen", @"Increase Current Mana by (\d+)");
            EffectSearchHelpers.Add("Add Proc", @"(?:Add Proc)|(?:Add Skill Proc)");
            EffectSearchHelpers.Add("Add Spell Proc", @"Cast.+on Spell Use");
        }

        public SpellCache(string id, List<Spell> list)
        {
            Id = id;
            spells = list;
            spellsById = list.ToDictionary(x => x.ID);
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

        public IQueryable<Spell> Search(SpellSearchFilter filter) 
        {
            var query = spells.AsQueryable();

            //  spell name and description are checked for literal text           
            int id;
            if (Int32.TryParse(filter.Text, out id))
                return query.Where(x => x.ID == id || x.GroupID == id);

            if (!String.IsNullOrEmpty(filter.Text))
                query = query.Where(x => x.ID.ToString() == filter.Text || x.Name.IndexOf(filter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0 || (x.Desc != null && x.Desc.IndexOf(filter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0));

            // exclude dragorn breath AA because they spam the results
            query = query.Where(x => !x.Name.StartsWith("Breath of"));

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
                    query = query.Where(x => x.ExtLevels[levelArrayIndex] >= filter.ClassMinLevel && x.ExtLevels[levelArrayIndex] <= filter.ClassMaxLevel);
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
            if (!String.IsNullOrEmpty(filter.Effect))
            {
                var effect = filter.Effect;

                if (EffectSearchHelpers.ContainsKey(filter.Effect))
                    effect = EffectSearchHelpers[filter.Effect] ?? filter.Effect;

                if (Regex.Escape(effect) != effect)
                {
                    var re = new Regex(effect, RegexOptions.IgnoreCase);
                    query = query.Where(x => x.HasEffect(re, filter.EffectSlot));
                }
                else
                    query = query.Where(x => x.HasEffect(effect, filter.EffectSlot));
            }

            return query;
        }

        /// <summary>
        /// Expand a spell list to include referenced spells.
        /// </summary>                
        public void Expand(List<Spell> list, bool backwards)
        {
            // keep track of all spells in the results so that we don't enter into a loop
            HashSet<int> included = new HashSet<int>();
            foreach (Spell spell in list)
                included.Add(spell.ID);

            // search the full spell list to find spells that link to the current results (reverse links)
            // but do not do this for spells that are heavily referenced. e.g. complete heal is referenced by hundreds of focus spells
            if (backwards)
            {
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
