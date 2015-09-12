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
        public int Recast;
        public int Tab; // 1=general, 2=archetype, 3=class
        public AAExpansion Expansion;
        public int HotKey;
        public string[] Slots; // parsed description for each effect
        public int[] LinksTo;

        public AA()
        {
            Slots = new string[0];
            LinksTo = new int[0];
        }

        public override string ToString()
        {
            return String.Format("[{0}] {1} ({2})", ID, Name, Rank);
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
                    var slots = new List<string>();
                    var spell = new Spell();
                    spell.DurationTicks = 1; // to show regen with "/tick" mode
                    while (effects.Count >= 4)
                    {
                        int spa = ParseInt(effects[0]);
                        int base1 = ParseInt(effects[1]);
                        int base2 = ParseInt(effects[2]);
                        int max = 0;
                        int calc = 100;
                        effects.RemoveRange(0, 4);
                        var spadesc = spell.ParseEffect(spa, base1, base2, max, calc, 105);
#if DEBUG
                        spadesc = String.Format("SPA {0} Base1={1} Base2={2} --- {3}", spa, base1, base2, spadesc);
#endif
                        slots.Add(spadesc);

                    }
                    aa.Slots = slots.ToArray();


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

                foreach (string s in aa.Slots.Where(x => x != null))
                {
                    // match spell refs
                    var matches = Spell.SpellRefExpr.Matches(s);
                    foreach (Match m in matches)
                        if (m.Success)
                            linked.Add(Int32.Parse(m.Groups[1].Value));

                    // match spell group refs
                    Match match = Spell.GroupRefExpr.Match(s);
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
