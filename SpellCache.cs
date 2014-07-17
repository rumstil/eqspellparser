using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Everquest
{
    static class SpellSearch
    {
        static public readonly Dictionary<string, string> EffectHelpers;

        static SpellSearch()
        {
            EffectHelpers = new Dictionary<string, string>();

            EffectHelpers.Add("Cure", @"Decrease \w+ Counter by (\d+)");
            EffectHelpers.Add("Heal", @"Increase Current HP by ([1-9]\d+)(?!.*(?:per tick))"); // 1-9 excludes spells with Increase Current HP by 0
            EffectHelpers.Add("HoT", @"Increase Current HP by (\d+) per tick");
            EffectHelpers.Add("Nuke", @"Decrease Current HP by (\d+)(?!.*(?:per tick))");
            EffectHelpers.Add("DoT", @"Decrease Current HP by (\d+) per tick");
            EffectHelpers.Add("Haste", @"Increase Melee Haste (?:v3 )?by (\d+)");
            EffectHelpers.Add("Slow", @"Decrease Melee Haste by (\d+)");
            EffectHelpers.Add("Snare", @"Decrease Movement Speed by (\d+)");
            EffectHelpers.Add("Shrink", @"Decrease Player Size");
            EffectHelpers.Add("Rune", "@Absorb");
            EffectHelpers.Add("Pacify", @"Decrease Social Radius");
            EffectHelpers.Add("Damage Shield", @"Increase Damage Shield by (\d+)");
            EffectHelpers.Add("Mana Regen", @"Increase Current Mana by (\d+)");
            EffectHelpers.Add("Add Proc", @"(?:Add Proc)|(?:Add Skill Proc)");
            EffectHelpers.Add("Add Spell Proc", @"Cast on Spell Use");
        }
    }


    /// <summary>
    /// Encapsulates a simple spell list to provide some search and cross referencing abilities.
    /// </summary>
    public class SpellCache : IEnumerable<Spell>
    {
        private string id;
        private List<Spell> spells;
        private Dictionary<int, Spell> spellsById;
        private ILookup<int, Spell> spellsByGroup;

        public string Id { get { return id; } set { id = value; } }

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

        public IQueryable<Spell> Search(string text, int cls, int min, int max, string effect, int slot, string category)
        {
            var query = spells.AsQueryable();

            //  spell name and description are checked for literal text           
            int id;
            if (Int32.TryParse(text, out id))
                return query.Where(x => x.ID == id || x.GroupID == id);

            if (!String.IsNullOrEmpty(text))
                query = query.Where(x => x.ID.ToString() == text || x.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0 || (x.Desc != null && x.Desc.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0));

            // exclude dragorn breath AA because they spam the results
            query = query.Where(x => !x.Name.StartsWith("Breath of"));

            // level filter is only used when a class is selected
            if (cls >= 0 && category != "AA")
            {
                query = query.Where(x => x.ExtLevels[cls] >= min && x.ExtLevels[cls] <= max);
            }

            // effect filter can be a literal string or a regex
            if (!String.IsNullOrEmpty(effect))
            {
                if (SpellSearch.EffectHelpers.ContainsKey(effect))
                    effect = SpellSearch.EffectHelpers[effect];

                if (Regex.Escape(effect) != effect)
                {
                    var re = new Regex(effect, RegexOptions.IgnoreCase);
                    query = query.Where(x => x.HasEffect(re, slot));
                }
                else
                    query = query.Where(x => x.HasEffect(effect, slot));
            }


            if (!String.IsNullOrEmpty(category))
            {
                if (category == "AA" && cls >= 0)
                    query = query.Where(x => x.ExtLevels[cls] == 254);
                else if (category == "AA")
                    query = query.Where(x => x.ExtLevels.Any(y => y == 254));
                else
                    query = query.Where(x => x.Categories.Any(y => y.StartsWith(category, StringComparison.InvariantCultureIgnoreCase)));
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
