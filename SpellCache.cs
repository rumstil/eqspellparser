using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Everquest
{
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

            // search the result to find other spells that they link to (forward links)
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
