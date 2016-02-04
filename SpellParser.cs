using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace Everquest
{
    /// <summary>
    /// Loads spell data from the EQ spell definition files.
    /// </summary>
    public static class SpellParser
    {
        public const int MAX_LEVEL = 105;

        static public List<Spell> LoadFromFile(string spellPath, string descPath, string stackPath)
        {
            // the spell file is required. the other files are optional 
            if (!File.Exists(spellPath))
                throw new FileNotFoundException("Could not open spell file.", spellPath);

            List<Spell> list = new List<Spell>(50000);
            Dictionary<int, Spell> listById = new Dictionary<int, Spell>(50000);

            // load description text file
            var desc = new Dictionary<string, string>(50000);
            if (File.Exists(descPath))
                using (var text = PossiblyCompressedFile.OpenText(descPath))
                    while (!text.EndOfStream)
                    {
                        var line = text.ReadLine();
                        var fields = line.Split('^');
                        if (fields.Length < 3 || line.StartsWith("#"))
                            continue;

                        // 0 = id within type
                        // 1 = type
                        // 2 = description
                        // type 1 = AA names
                        // type 4 = AA desc
                        // type 5 = spell categories
                        // type 6 = spell desc
                        // type 7 = lore groups
                        // type 11 = illusions
                        // type 12 = body type
                        // type 16 = aug slot desc
                        // type 18 = currency
                        // type 40 = spell stacking group
                        desc[fields[1] + "/" + fields[0]] = fields[2].Trim();
                    }

            // load spell definition file
            using (var text = PossiblyCompressedFile.OpenText(spellPath))
                while (!text.EndOfStream)
                {
                    var line = text.ReadLine();
                    var fields = line.Split('^');
                    var spell = LoadSpell(fields);

#if !LimitMemoryUse
                    // all spells can be grouped into up to 2 categories (type 5 in db_str)
                    // ignore the "timer" categories because they are frequently wrong
                    List<string> cat = new List<string>();
                    string c1;
                    string c2;
                    if (desc.TryGetValue("5/" + spell.CategoryDescID[0], out c1))
                    {
                        // sub category 1
                        if (desc.TryGetValue("5/" + spell.CategoryDescID[1], out c2) && !c2.StartsWith("Timer"))
                            cat.Add(c1 + "/" + c2);

                        // sub category 2
                        if (desc.TryGetValue("5/" + spell.CategoryDescID[2], out c2) && !c2.StartsWith("Timer"))
                            cat.Add(c1 + "/" + c2);

                        // general category if no subcategories are defined
                        if (cat.Count == 0)
                            cat.Add(c1);
                    }

                    // add a timer category 
                    if (spell.TimerID > 0)
                        cat.Add("Timer " + spell.TimerID.ToString("D2"));
                    spell.Categories = cat.ToArray();
#endif

                    if (!desc.TryGetValue("6/" + spell.DescID, out spell.Desc))
                        spell.Desc = null;

                    list.Add(spell);
                    listById[spell.ID] = spell;
                    //if (spell.GroupID > 0)
                    //    listByGroup[spell.GroupID] = spell;
                }

            // load spell stacking file
            // my guess is that this was made a separate file because a single spell can be part of multiple spell stacking groups
            if (File.Exists(stackPath))
                using (var text = PossiblyCompressedFile.OpenText(stackPath))
                    while (!text.EndOfStream)
                    {
                        var line = text.ReadLine();
                        var fields = line.Split('^');
                        if (fields.Length < 4 || line.StartsWith("#"))
                            continue;

                        var id = ParseInt(fields[0]);
                        Spell spell = null;
                        if (!listById.TryGetValue(id, out spell))
                            continue;

                        var group = fields[1];
                        var rank = fields[2];

                        if (desc.ContainsKey("40/" + group))
                            group = desc["40/" + group];

                        group += " " + rank;

                        // type 6 = non-override stacking group; only one can be active at a time and new procs won't overwrite old ones that are still active
                        var type = fields[3];
                        if (type == "6")
                            group += " (Non-Overide)";

                        if (spell.Stacking.Length == 0)
                        {
                            spell.Stacking = new string[] { group };
                        }
                        else
                        {
                            var stacking = spell.Stacking.ToList();
                            stacking.Add(group);
                            spell.Stacking = stacking.ToArray();
                        }
                    }

            // second pass fixes that require the entire spell list to be loaded already
            List<Spell> groups = list.FindAll(x => x.GroupID > 0);
            foreach (Spell spell in list)
            {
                // get list of linked spells
                var linked = new List<int>(10);
                if (spell.RecourseID != 0)
                    linked.Add(spell.RecourseID);

                foreach (var s in spell.Slots)
                    if (s.Desc != null)
                    {
                        // match spell refs
                        Match match = Spell.SpellRefExpr.Match(s.Desc);
                        if (match.Success)
                            linked.Add(Int32.Parse(match.Groups[1].Value));

                        // match spell group refs
                        match = Spell.GroupRefExpr.Match(s.Desc);
                        if (match.Success)
                        {
                            int id = Int32.Parse(match.Groups[1].Value);
                            // negate id on excluded spells so that we don't set extlevels on them
                            if (s.Desc.Contains("Exclude"))
                                id = -id;
                            groups.FindAll(x => x.GroupID == id).ForEach(delegate(Spell x) { linked.Add(x.ID); });
                        }
                    }

                // process each of the linked spells
                foreach (int id in linked)
                {
                    Spell target = null;
                    if (listById.TryGetValue(id, out target))
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

            return list;
        }

        /// <summary>
        /// Parse a spell from a set of spell fields.
        /// </summary>
        static Spell LoadSpell(string[] fields)
        {
            Spell spell = new Spell();

            spell.ID = Convert.ToInt32(fields[0]);
            spell.Name = fields[1].Trim();

            // replace roman numerals with digits (zero padded for sorting) in AA names 
            //spell.Name = Regex.Replace(spell.Name, @"\s[IVXL]+$", x => " " + ParseRomanNumeral(x.Value.Substring(1)).ToString("D2"));

            // append digit translation of roman numeral spell ranks
            //spell.Name = Regex.Replace(spell.Name, @"\s[IVXL]+$", x => " " + x.Groups[0].Value + " (" + ParseRomanNumeral(x.Value.Substring(1)) +  ")");

            //Target = fields[2];
            spell.Extra = fields[3];
            spell.LandOnSelf = fields[6];
            spell.LandOnOther = fields[7];
            //Wear Off Message = fields[8];
            spell.Range = ParseInt(fields[9]);
            spell.AERange = ParseInt(fields[10]);
            spell.PushBack = ParseFloat(fields[11]);
            spell.PushUp = ParseFloat(fields[12]);
            spell.CastingTime = ParseFloat(fields[13]) / 1000f;
            spell.RestTime = ParseFloat(fields[14]) / 1000f;
            spell.RecastTime = ParseFloat(fields[15]) / 1000f;
            spell.DurationTicks = Spell.CalcDuration(ParseInt(fields[16]), ParseInt(fields[17]), MAX_LEVEL);
            spell.AEDuration = ParseInt(fields[18]);
            spell.Mana = ParseInt(fields[19]);

            // 56 = icon
            // 57 = icon

            for (int i = 0; i < 3; i++)
            {
                spell.ConsumeItemID[i] = ParseInt(fields[58 + i]);
                spell.ConsumeItemCount[i] = ParseInt(fields[62 + i]);
                spell.FocusID[i] = ParseInt(fields[66 + i]);
            }

            //Light_Type = fields[82];
            spell.Beneficial = ParseBool(fields[83]);
            //Activated (AAs?) = fields[84];
            spell.ResistType = (SpellResist)ParseInt(fields[85]);
            spell.Target = (SpellTarget)ParseInt(fields[98]);
            // 99 =  base difficulty fizzle adjustment?
            spell.Skill = (SpellSkill)ParseInt(fields[100]);
            spell.Zone = (SpellZoneRestrict)ParseInt(fields[101]);
            //Environment Type = fields[102];
            //Time of Day = fields[103]; Day, Night, Both

            // each spell has a different casting level for all 16 classes
            for (int i = 0; i < spell.Levels.Length; i++)
                spell.Levels[i] = (byte)ParseInt(fields[104 + i]);

            //Casting Animation = fields[120];
            //Target Animation = fields[121];
            //Travel Type = fields[122];
            //SPA ID = fields[123];
            spell.CancelOnSit = ParseBool(fields[124]);

            // 125..141 deity casting restrictions
            string[] gods = new string[] { "Agnostic", "Bertox", "Brell", "Cazic", "Erollisi", "Bristlebane", "Innoruuk", "Karana", "Mithanial", "Prexus", 
                "Quellious", "Rallos", "Rodcet", "Solusek", "Tribunal", "Tunare", "Veeshan" };
            for (int i = 0; i < gods.Length; i++)
                if (ParseBool(fields[125 + i]))
                    spell.Deity += gods[i] + " ";

            // 142 NPC Do Not Cast 
            // 143 AI PT Bonus
            spell.Icon = ParseInt(fields[144]);
            spell.Interruptable = !ParseBool(fields[146]);
            spell.ResistMod = ParseInt(fields[147]);
            //spell.StackableDoT = !ParseBool(fields[148]);
            // 149 = deletable
            spell.RecourseID = ParseInt(fields[150]);
            // 151 = used to prevent a nuke from being partially resisted.
            // also, it prevents or allows a player to resist a spell fully if they resist "part" of its components.
            spell.PartialResist = ParseBool(fields[151]);
            if (spell.RecourseID != 0)
                spell.Recourse = String.Format("[Spell {0}]", spell.RecourseID);
            // 152 Small Targets Only 
            // 153 Persistent particle effects 
            spell.SongWindow = ParseBool(fields[154]);
            spell.DescID = ParseInt(fields[155]);
            spell.CategoryDescID[0] = ParseInt(fields[156]);
            spell.CategoryDescID[1] = ParseInt(fields[157]);
            spell.CategoryDescID[2] = ParseInt(fields[158]);
            // 159 NPC Does not Require LoS
            spell.Feedbackable = ParseBool(fields[160]); // (Triggers spell damage shield. This is mostly used on procs and non nukes, so it's not that useful to show)
            spell.Reflectable = ParseBool(fields[161]);
            spell.HateMod = ParseInt(fields[162]);
            spell.ResistPerLevel = ParseInt(fields[163]);
            spell.ResistCap = ParseInt(fields[164]);
            // 165 Useable On Objects Boolean
            spell.Endurance = ParseInt(fields[166]);
            spell.TimerID = ParseInt(fields[167]);
            spell.CombatSkill = ParseBool(fields[168]);
            // 169 Attack Open = all 0
            // 170 Defense Open = all 0
            // 171 Skill Open = all 0
            // 172 NPC Error Open= all 0
            spell.HateOverride = ParseInt(fields[173]);
            spell.EnduranceUpkeep = ParseInt(fields[174]);
            spell.MaxHitsType = (SpellMaxHits)ParseInt(fields[175]);
            spell.MaxHits = ParseInt(fields[176]);
            // 177 PVP Resist Mod = 197 values.
            // 178 PVP Resist Level = 20 values. looks similar to calc values
            // 179 PVP Resist Cap = 266 values.
            // 180 Spell Category = 185 values.
            // 181 PVP Duration= 19 values. looks similar to duration calc values
            // 182 PVP Duration = 115 values.
            // 183 No Pet = 3 values. 0, 1, 2
            // 184 Cast While Sitting Boolean
            spell.MGBable = ParseBool(fields[185]);
            spell.Dispelable = !ParseBool(fields[186]);
            // 187 NPC Mem Category = npc stuff
            // 188 NPC Usefulness = 192 values.
            spell.MinResist = ParseInt(fields[189]);
            spell.MaxResist = ParseInt(fields[190]);
            spell.MinViralTime = ParseInt(fields[191]);
            spell.MaxViralTime = ParseInt(fields[192]);
            // 193 Particle Duration Time = 124 values. nimbus type effects
            spell.ConeStartAngle = ParseInt(fields[194]);
            spell.ConeEndAngle = ParseInt(fields[195]);
            spell.Sneaking = ParseBool(fields[196]);
            spell.Focusable = !ParseBool(fields[197]);
            // 198 No Detrimental Spell Aggro Boolean
            // 199 Show Wear Off Message Boolean
            spell.DurationFrozen = ParseBool(fields[200]);
            spell.ViralRange = ParseInt(fields[201]);
            spell.SongCap = ParseInt(fields[202]);
            // 203 Stacks With Self = melee specials
            // 204 Not Shown To Player Boolean
            spell.BeneficialBlockable = !ParseBool(fields[205]); // for beneficial spells
            // 206 Animation Variation 
            spell.GroupID = ParseInt(fields[207]);
            spell.Rank = ParseInt(fields[208]); // rank 1/5/10. a few auras do not have this set properly
            if (spell.Rank == 5 || spell.Name.EndsWith("II") || spell.Name.EndsWith("02"))
                spell.Rank = 2;
            if (spell.Rank == 10 || spell.Name.EndsWith("III") || spell.Name.EndsWith("03"))
                spell.Rank = 3;
            // 209 No Resist Boolean = ignore SPA 180 resist?
            // 210 SpellBook Scribable Boolean
            spell.TargetRestrict = (SpellTargetRestrict)ParseInt(fields[211]);
            spell.AllowFastRegen = ParseBool(fields[212]);
            spell.CastOutOfCombat = !ParseBool(fields[213]); //Cast in Combat
            // 214 Cast Out of Combat Boolean
            // 215 Show DoT Message Boolean
            // 216 Invalid Boolean
            spell.CritOverride = ParseInt(fields[217]);
            spell.MaxTargets = ParseInt(fields[218]);
            // 219 No Effect from Spell Damage / Heal Amount on Items Boolean
            spell.CasterRestrict = (SpellTargetRestrict)ParseInt(fields[220]);
            // 221 = spell class. 13 sequential values.
            // 222 = spell subclass. 57 sequential values.
            // 223 AI Valid Targets = 9 values. looks like a character class mask? 2013-3-13 Hand of Piety can now crit again.
            spell.PersistAfterDeath = ParseBool(fields[224]);
            // 225 = song slope?
            // 226 = song offset?
            // range multiplier seems to be an integer so far
            spell.RangeModCloseDist = ParseInt(fields[227]);
            spell.RangeModCloseMult = ParseInt(fields[228]);
            spell.RangeModFarDist = ParseInt(fields[229]);
            spell.RangeModFarMult = ParseInt(fields[230]);
            spell.MinRange = ParseInt(fields[231]);
            spell.CannotRemove = ParseBool(fields[232]);
            //spell.Recourse Type = fields[233];
            spell.CastInFastRegen = ParseBool(fields[234]);
            spell.BetaOnly = ParseBool(fields[235]);
            //Spell Subgoup = fields[236];

            // debug stuff
            //spell.Unknown = ParseFloat(fields[209]);


            // each spell has 12 effect slots which have 5 attributes each
            // 20..31 - slot 1..12 base1 effect
            // 32..43 - slot 1..12 base2 effect
            // 44..55 - slot 1..12 max effect
            // 70..81 - slot 1..12 calc forumla data
            // 86..97 - slot 1..12 spa/type
            for (int i = 0; i < spell.Slots.Length; i++)
            {
                int spa = ParseInt(fields[86 + i]);
                int calc = ParseInt(fields[70 + i]);
                int max = ParseInt(fields[44 + i]);
                int base1 = ParseInt(fields[20 + i]);
                int base2 = ParseInt(fields[32 + i]);

                spell.Slots[i] = new SpellSlot() { SPA  = spa, Base1 = base1, Base2 = base2, Max = max, Calc = calc };
                spell.Slots[i].Desc = spell.ParseEffect(spa, base1, base2, max, calc, MAX_LEVEL);

#if DEBUG
                if (spell.Slots[i].Desc != null)
                {
                    spell.Slots[i].Desc = String.Format("SPA {0} Base1={1} Base2={2} Max={3} Calc={4} --- ", spa, base1, base2, max, calc) + spell.Slots[i].Desc;
                }
#endif

                // debug stuff: detect difference in value/base1 for spells where i'm not sure which one should be used and have chosen one arbitrarily
                //int[] uses_value = new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 15, 21, 24, 35, 36, 46, 47, 48, 49, 50, 55, 58, 59, 69, 79, 92, 97, 100, 111, 116, 158, 159, 164, 165, 166, 169, 184, 189, 190, 192, 262, 334, 417};
                //int[] uses_base1 = new int[] { 32, 64, 109, 148, 149, 193, 254, 323, 360, 374, 414 };
                //int value = Spell.CalcValue(calc, base1, max, 0, 90);
                //if (value != base1 && Array.IndexOf(uses_value, spa) < 0 && Array.IndexOf(uses_base1, spa) < 0)
                //    Console.Error.WriteLine(String.Format("SPA {1} {0} has diff value/base1: {2}/{3} calc: {4}", spell.Name, spa, value, base1, calc));
            }

            //if (spell.Unknown != 0)
            //    spell.ResistType = SpellResist.Unresistable;


            // debug stuff
            //if (spell.ID == 21683) for (int i = 0; i < fields.Length; i++) Console.WriteLine("{0}: {1}", i, fields[i]);
            //if (fields[198] != "0") Console.WriteLine("\n\n===\n{0} {1}", fields[198], String.Join("\n", spell.Details()));

            spell.Prepare();
            return spell;
        }

        static float ParseFloat(string s)
        {
            if (String.IsNullOrEmpty(s))
                return 0f;
            return Single.Parse(s, CultureInfo.InvariantCulture);
        }

        static int ParseInt(string s)
        {
            if (s == "" || s == "0" || s[0] == '.')
                return 0;
            return (int)Single.Parse(s, CultureInfo.InvariantCulture);
        }

        static bool ParseBool(string s)
        {
            return !String.IsNullOrEmpty(s) && (s != "0");
        }

        public static int ParseClass(string text)
        {
            if (String.IsNullOrEmpty(text))
                return 0;

            if (text.Length == 3 && Enum.IsDefined(typeof(SpellClasses), text.ToUpper()))
                return (int)Enum.Parse(typeof(SpellClasses), text.ToUpper());

            string[] names = Enum.GetNames(typeof(SpellClassesLong));
            for (int i = 0; i < names.Length; i++)
                if (String.Compare(names[i], text, true) == 0)
                    return (int)Enum.Parse(typeof(SpellClassesLong), names[i]);

            return 0;
        }

        /// <summary>
        /// Parse roman numerals up to 50. Returns 0 if the numeral could not be parsed.
        /// </summary>
        private static int ParseRomanNumeral(string num)
        {
            var roman = new string[] { 
                "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" ,
                "XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX", "XX",
                "XXI", "XXII", "XXIII", "XXIV", "XXV", "XXVI", "XXVII", "XXVIII", "XXIX", "XXX",
                "XXXI", "XXXII", "XXXIII", "XXXIV", "XXXV", "XXXVI", "XXXVII", "XXXVIII", "XXXIX", "XL",
                "XLI", "XLII", "XLIII", "XLIV", "XLV", "XLVI", "XLVII", "XLVIII", "XLIX", "L",
            };

            return Array.IndexOf(roman, num.ToUpper()) + 1;
        }

    }

    /// <summary>
    /// Provides transparent gzip decompression on files that may compressed.
    /// The game doesn't ever store spells files in gzip format but I added this in case someone wants to keep lots of versioned spell files around and save space.
    /// </summary>
    public static class PossiblyCompressedFile
    {
        public static StreamReader OpenText(string path)
        {
            if (path.EndsWith(".gz"))
            {
                // todo: should I buffer the uncompressed stream?
                var stream = new GZipStream(File.OpenRead(path), CompressionMode.Decompress);
                return new StreamReader(stream);
            }

            return File.OpenText(path);
        }
    }

}
