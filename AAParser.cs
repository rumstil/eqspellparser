using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Everquest
{

    public enum AAExpansion
    {
        Original = 0, 
        Ruins_of_Kunark,
        Scars_of_Velious,
        Shadows_of_Luclin,
        Planes_of_Power,
        Legacy_of_Ykesha,
        Lost_Dungeons_of_Norrath,
        Gates_of_Discord,
        Omens_of_War,
        Dragons_of_Norrath,
        Depths_of_Darkhollow,
        Prochecy_of_Ro,
        The_Serpents_Spine,
        The_Buried_Sea,
        Secrets_of_Faydwer,
        Seeds_of_Destruction,
        Underfoot,
        House_of_Thule,
        Veil_of_Alaris,
        Rain_of_Fear,
        Call_of_the_Forsaken,
        The_Darkened_Sea      
    }

    public struct AASlot
    {
        public int SPA;
        public int Base1;
        public int Base2;
        public string Desc;
    }

    public sealed class AA
    {
        public int ID;
        public int PrevID;
        public int GroupID;
        //public int NameID;
        public string Name;
        public string Desc;
        public int Mana;
        public int Rank;
        public int MaxRank;
        public int ReqLevel;
        public int Cost;
        public int TotalCost;
        public SpellClassesMask ClassesMask;
        public int SpellID;
        public Spell Spell;
        public int Recast;
        public int Tab; // 1=general, 2=archetype, 3=class
        public AAExpansion Expansion;
        public int HotKey;
        public AASlot[] Slots;
        public int[] LinksTo;

        public AA()
        {
            Slots = new AASlot[0];
            //SlotDesc = new string[0];
            LinksTo = new int[0];
        }

        public override string ToString()
        {
            return String.Format("[{0}] {1} ({2})", ID, Name, Rank);
        }

        /// <summary>
        /// Search all effect slots using a SPA match.
        /// </summary>
        public bool HasEffect(int spa)
        {
            for (int i = 0; i < Slots.Length; i++)
                if (Slots[i].SPA == spa)
                    return true;

            if (Spell != null)
                return Spell.HasEffect(spa, 0);

            return false;
        }

        /// <summary>
        /// Search all effect slots using a text match.
        /// </summary>
        /// <param name="desc">Effect to search for. Can be text or a integer representing an SPA.</param>
        public bool HasEffect(string text)
        {
            int spa;
            if (Int32.TryParse(text, out spa))
                return HasEffect(spa);

            for (int i = 0; i < Slots.Length; i++)
                if (Slots[i].Desc != null && Slots[i].Desc.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return true;

            if (Spell != null)
                return Spell.HasEffect(text, 0);

            return false;
        }

        /// <summary>
        /// Search all effect slots using a regular expression.
        /// </summary>
        public bool HasEffect(Regex re)
        {
            for (int i = 0; i < Slots.Length; i++)
                if (Slots[i].Desc != null && re.IsMatch(Slots[i].Desc))
                    return true;

            if (Spell != null)
                return Spell.HasEffect(re, 0);

            return false;
        }
    }


    public static class AAParser
    {
        static public List<AA> LoadFromFile(string aaPath, string descPath)
        {
            var desc = new Dictionary<string, string>(50000);
            if (File.Exists(descPath))
                using (var text = File.OpenText(descPath))
                    while (!text.EndOfStream)
                    {
                        string line = text.ReadLine();
                        string[] fields = line.Split('^');
                        if (fields.Length < 3)
                            continue;

                        // 0 = id within type
                        // 1 = type
                        // 2 = description
                        // type 1 = AA names
                        // type 4 = AA desc
                        desc[fields[1] + "/" + fields[0]] = fields[2].Trim();
                    }
            
            var list = new List<AA>();

            using (var text = File.OpenText(aaPath))
                while (!text.EndOfStream)
                {
                    string line = text.ReadLine();
                    string[] fields = line.Split('^');
                    if (fields.Length < 13) 
                        continue;

                    var aa = new AA();
                    aa.ID = ParseInt(fields[0]);
                    aa.GroupID = ParseInt(fields[1]);
                    aa.PrevID = ParseInt(fields[2]);
                    //aa.NameID = ParseInt(fields[3]);
                    desc.TryGetValue("1/" + fields[3], out aa.Name);
                    desc.TryGetValue("4/" + fields[4], out aa.Desc);
                    aa.Rank = ParseInt(fields[5]);
                    aa.MaxRank = ParseInt(fields[6]);
                    aa.ClassesMask = (SpellClassesMask)ParseInt(fields[7]);
                    aa.ReqLevel = ParseInt(fields[8]);
                    aa.Cost = ParseInt(fields[9]);
                    aa.TotalCost = ParseInt(fields[10]);
                    aa.SpellID = ParseInt(fields[11]);
                    aa.Recast = ParseInt(fields[12]);
                    // spell type
                    aa.Tab = ParseInt(fields[14]);
                    aa.Expansion = (AAExpansion)ParseInt(fields[15]);

                    var effects = fields[16].Split(',').ToList();
                    var slots = new List<AASlot>();
                    var spell = new Spell();
                    spell.DurationTicks = 1; // to show regen with "/tick" mode
                    while (effects.Count >= 4)
                    {
                        int spa = ParseInt(effects[0]);
                        int base1 = ParseInt(effects[1]);
                        int base2 = ParseInt(effects[2]);
                        effects.RemoveRange(0, 4);
                        var slot = new AASlot() { SPA = spa, Base1 = base1, Base2 = base2 };
                        slot.Desc = spell.ParseEffect(spa, base1, base2, 0, 100, aa.ReqLevel);
#if DEBUG
                        //spadesc = String.Format("SPA {0} Base1={1} Base2={2} --- {3}", spa, base1, base2, spadesc);
#endif
                        slots.Add(slot);

                    }
                    aa.Slots = slots.ToArray();
                    //aa.SlotDesc = slots.Select(x => spell.ParseEffect(x.SPA, x.Base1, x.Base2, 0, 100, aa.ReqLevel)).ToArray();


                    list.Add(aa);
                }

            BuildSpellLinks(list);

            return list;
        }

        /// <summary>
        /// Build a list of each spell that is referenced by the AAs. This will later be used to include associated spells in search results.
        /// </summary>
        static private void BuildSpellLinks(List<AA> list)
        {
            foreach (AA aa in list)
            {
                List<int> linked = new List<int>(10);
                if (aa.SpellID > 0)
                    linked.Add(aa.SpellID);

                foreach (var s in aa.Slots.Where(x => x.Desc != null))
                {
                    // match spell refs
                    var matches = Spell.SpellRefExpr.Matches(s.Desc);
                    foreach (Match m in matches)
                        if (m.Success)
                            linked.Add(Int32.Parse(m.Groups[1].Value));

                    // match spell group refs
                    Match match = Spell.GroupRefExpr.Match(s.Desc);
                    if (match.Success)
                    {
                        int id = Int32.Parse(match.Groups[1].Value);
                        // negate id on excluded spells so that we don't set extlevels on them
                        //if (s.Contains("Exclude"))
                        //    id = -id;
                        //groups.FindAll(delegate(Spell x) { return x.GroupID == id; }).ForEach(delegate(Spell x) { linked.Add(x.ID); });
                    }
                }

                aa.LinksTo = linked.ToArray();
            }

        }

        static int ParseInt(string s)
        {
            return Int32.Parse(s, CultureInfo.InvariantCulture);
        }

    }
}
