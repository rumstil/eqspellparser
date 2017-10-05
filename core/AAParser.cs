using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace EQSpellParser
{
    public struct AASlot
    {
        public int SPA;
        public int Base1;
        public int Base2;
        public string Desc;

        public override string ToString()
        {
            return Desc;
        }
    }

    public struct AAReq
    {
        public int GroupID;
        public int Rank;
        //public string Desc;
    }

    public sealed class AA
    {
        public int ID;
        public int PrevID;
        public int GroupID;
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
        public int TimerID;
        public int Recast;
        public int Tab; // 1=general, 2=archetype, 3=class
        public int Category;
        public int Expansion;
        public AASlot[] Slots;
        public AAReq[] Req;
        public DateTime UpdatedOn;


        // these fields are initialized after loading
        public Spell Spell;
        public int[] LinksTo;

        public string TabName
        {
            get
            {
                if (Tab == 1 && Category == 6) return "Tradeskill";
                if (Tab == 1 && Category == 11) return "Clickies";
                if (Tab == 1) return "General";
                if (Tab == 2) return "Archetype";
                if (Tab == 3) return "Class";
                if (Tab == 4) return "Special";
                if (Tab == 5) return "Focus";
                return "Unknown";
            }
        }

        public AA()
        {
            Slots = new AASlot[0];
            Req = new AAReq[0];
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

    /// <summary>
    /// Load AA data from a data file. Unlike the spell data, this file is not distributed with the game - this is just my own adhoc data file format.
    /// </summary>
    public static class AAParser
    {
        static public List<AA> LoadFromFile(string aaPath, string descPath)
        {
            var desc = new Dictionary<string, string>(60000);
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
                    if (fields.Length < 17)
                        continue;

                    var aa = new AA();
                    aa.ID = ParseInt(fields[0]);
                    aa.GroupID = ParseInt(fields[1]);
                    aa.PrevID = ParseInt(fields[2]);
                    desc.TryGetValue("1/" + fields[3], out aa.Name);
                    desc.TryGetValue("4/" + fields[4], out aa.Desc); // save memory for now
                    aa.Rank = ParseInt(fields[5]);
                    aa.MaxRank = ParseInt(fields[6]);
                    aa.ClassesMask = (SpellClassesMask)ParseInt(fields[7]);
                    aa.ReqLevel = ParseInt(fields[8]);
                    aa.Cost = ParseInt(fields[9]);
                    aa.TotalCost = ParseInt(fields[10]);
                    aa.SpellID = ParseInt(fields[11]);
                    aa.Recast = ParseInt(fields[12]);
                    aa.TimerID = ParseInt(fields[13]);
                    aa.Tab = ParseInt(fields[14]);
                    aa.Expansion = ParseInt(fields[15]);
                    aa.Category = ParseInt(fields[16]);

                    // read effect slots (an array of 4-int groups)
                    var slotsArray = fields[17].Split(',');
                    var slots = new List<AASlot>();
                    var spell = new Spell();
                    spell.DurationTicks = 1; // to show regen with "/tick" mode
                    for (int i = 0; i + 3 < slotsArray.Length; i += 4)
                    {
                        int spa = ParseInt(slotsArray[i]);
                        int base1 = ParseInt(slotsArray[i + 1]);
                        int base2 = ParseInt(slotsArray[i + 2]);
                        var slot = new AASlot() { SPA = spa, Base1 = base1, Base2 = base2 };

                        slot.Desc = spell.ParseEffect(spa, base1, base2, 0, 100, aa.ReqLevel);
#if DEBUG
                        //spadesc = String.Format("SPA {0} Base1={1} Base2={2} --- {3}", spa, base1, base2, spadesc);
#endif
                        slots.Add(slot);
                    }
                    aa.Slots = slots.ToArray();

                    /*
                    // read prerequisites (an array of 2-int groups)
                    var reqArray = fields[18].Split(',');
                    var req = new List<AAReq>();
                    for (int i = 0; i + 1 < reqArray.Length; i += 2)
                    {
                        int group = ParseInt(reqArray[i]);
                        int rank = ParseInt(reqArray[i + 1]);
                        // 1685 is the now removed alaran language AA
                        if (group != 0 && group != 1685)
                            req.Add(new AAReq() { GroupID = group, Rank = rank });
                    }
                    aa.Req = req.ToArray();
                    */

                    aa.UpdatedOn = DateTime.Parse(fields[19], CultureInfo.InvariantCulture);

                    list.Add(aa);
                }

            return list;
        }

        static int ParseInt(string s)
        {
            return Int32.Parse(s, CultureInfo.InvariantCulture);
        }

    }
}
