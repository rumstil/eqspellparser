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
        private List<Spell> spells;
        private Dictionary<int, Spell> spellsById;
        private ILookup<int, Spell> spellsByGroup;

        public SpellCache(List<Spell> list)
        {
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
        /// Expand a spell list to include referenced spells.
        /// </summary>                
        public void Expand(List<Spell> list)
        {
            // keep track of all spells in the results so that we don't enter into a loop
            HashSet<int> included = new HashSet<int>();
            foreach (Spell spell in list)
                included.Add(spell.ID);

            // search the full spell list to find spells that link to the current results (reverse links)
            // but do not do this for spells that are heavily referenced. e.g. complete heal is referenced by hundreds of focus spells
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

            // search the result to find other spells that they link to (forward links)
            int i = 0;
            while (i < list.Count)
            {
                Spell spell = list[i++];

                // < 0 = spell group
                // > 0 = spell id
                foreach (int id in spell.LinksTo)
                    if (id < 0)
                    {
                        foreach (var linked in spellsByGroup[-id])
                            if (!included.Contains(linked.ID))
                            {
                                included.Add(linked.ID);
                                list.Add(linked);
                            }
                    }
                    else if (!included.Contains(id))
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
