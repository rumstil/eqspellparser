using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Everquest;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

namespace winparser
{
    //[ComVisible(true)] // for SearchBrowser.ObjectForScripting
    public partial class MainForm : Form
    {
        private Dictionary<string, string> Effects;

        private List<Spell> Spells;
        private Dictionary<int, Spell> SpellsById;

        public string SpellPath;
        public List<Spell> Results;


        public MainForm()
        {
            InitializeComponent();

            SearchClass.Items.AddRange(Enum.GetNames(typeof(SpellClassesLong)));

            // literal text suggestions
            SearchEffect.Items.Add("Charm");
            SearchEffect.Items.Add("Mesmerize");
            SearchEffect.Items.Add("Memory Blur");
            SearchEffect.Items.Add("Root");

            Effects = new Dictionary<string, string>();
            Effects.Add("Cure", @"Decrease \w+ Counter by (\d+)");
            Effects.Add("Heal", @"Increase Current HP by (\d+)");
            Effects.Add("HoT", @"Increase Current HP by (\d+) per tick");
            Effects.Add("Nuke", @"Decrease Current HP by (\d+)");
            Effects.Add("DoT", @"Decrease Current HP by (\d+) per tick");
            Effects.Add("Haste", @"Increase Melee Haste .*?by (\d+)"); // .* for v3 haste
            Effects.Add("Slow", @"Decrease Melee Haste by (\d+)");
            Effects.Add("Snare", @"Decrease Movement Speed");
            Effects.Add("Shrink", @"Decrease Player Size");
            Effects.Add("Rune", "@Absorb");
            Effects.Add("Pacify", @"Decrease Social Radius");
            SearchEffect.Items.AddRange(Effects.Keys.ToArray());

            //SearchBrowser.ObjectForScripting = this;
        }

        public new void Load(string spellPath, string descPath)
        {
            SpellPath = spellPath;
            Text += " - " + spellPath;

            Cursor.Current = Cursors.WaitCursor;

            Spells = SpellParser.LoadFromFile(spellPath, descPath).ToList();
            SpellsById = Spells.ToDictionary(x => x.ID, x => x);

            Cursor.Current = Cursors.Default;

            var html = InitHtml();
            html.AppendFormat("<p>Loaded <strong>{0}</strong> spells from {1}.</p></html>", Spells.Count, SpellPath);
            html.Append("<p>Use the search button to perform a search on this spell file. Only the first 2000 search results are shown.");
            html.Append("<p>Use the compare button to perform a search on both spell files and show the differences (this only works if you have opened two spell files).");
            SearchBrowser.DocumentText = html.ToString();
        }

        /// <summary>
        /// Search spell database based on form filter settings
        /// </summary>
        public void Search()
        {
            var query = Spells.AsQueryable();

            // exclude spammy breath of AA
            query = query.Where(x => !x.Name.StartsWith("Breath of"));

            //  spell name and description are checked for literal text
            var text = SearchText.Text;
            int id;
            if (Int32.TryParse(text, out id))
                query = query.Where(x => x.ID == id);
            else if (!String.IsNullOrEmpty(text))
                query = query.Where(x => x.ID.ToString() == text || x.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0 || (x.Desc != null && x.Desc.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0));
            //query = query.Where(x => x.ID.ToString() == text || x.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0);

            // levels can be either a single "level" or a range "min-max"
            // they are only used when a class is selected            
            int min = 1;
            int max = 254;
            var levels = SearchLevel.Text.Replace(" ", "").Split('-');
            if (levels.Length == 2)
            {
                min = Int32.Parse(levels[0]);
                if (min == 0)
                    min = 1; // zero would include spells the class can't cast
                max = Int32.Parse(levels[1]);
            }
            else if (levels.Length == 1 && levels[0].Length > 0)
            {
                min = max = Int32.Parse(levels[0]);
            }

            var _class = Enum.IsDefined(typeof(SpellClassesLong), SearchClass.Text) ? (int)Enum.Parse(typeof(SpellClassesLong), SearchClass.Text) - 1 : -1;
            if (_class >= 0)
            {
                query = query.Where(x => x.Levels[_class] >= min && x.Levels[_class] <= max);
            }

            string effect = SearchEffect.Text;
            if (!String.IsNullOrEmpty(effect))
            {
                if (Effects.ContainsKey(effect))
                    effect = Effects[effect];                
                if (Regex.Escape(effect) != effect)
                {
                    var re = new Regex(effect, RegexOptions.IgnoreCase);
                    query = query.Where(x => x.HasEffect(re) >= 0);
                }
                else
                    query = query.Where(x => x.HasEffect(effect) >= 0);
            }

            Results = query.ToList();
            //int visible = Results.Count;
            string Sorting = null;
            Expand(Results);

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
            if (_class >= 0)
            {
                Sorting = String.Format("{0} results - sorted by {1} level.", Results.Count, SearchClass.Text);
                Results.Sort((a, b) =>
                {
                    if (a.Levels[_class] > 0 && b.Levels[_class] == 0)
                        return -1;
                    if (b.Levels[_class] > 0 && a.Levels[_class] == 0)
                        return 1;
                    int comp = a.Levels[_class] - b.Levels[_class];
                    if (comp == 0)
                        comp = String.Compare(a.Name, b.Name);
                    if (comp == 0)
                        comp = a.ID - b.ID;
                    return comp;
                });
            }
            // 3. finally sort by name if no better method is found
            else
            {
                Sorting = String.Format("{0} results - sorted by name.", Results.Count);
                Results.Sort((a, b) =>
                {
                    int comp = String.Compare(a.Name, b.Name);
                    if (comp == 0)
                        comp = a.ID - b.ID;
                    return comp;
                });
            }


            SearchNotes.Text = String.Format("{0} results", Results.Count);

            var html = InitHtml();
            html.Append("<p>" + Sorting + "</p>");

            if (DisplayTable.Checked)
                ShowAsTable(Results, html);
            else
                ShowAsText(Results, html);

            html.Append("</html>");
            SearchBrowser.DocumentText = html.ToString();
        }

        /// <summary>
        /// Recursively expand the spell list to include referenced spells.
        /// </summary>                
        private void Expand(IList<Spell> list)
        {
            // keep a hash based index of existing results to avoid doing a linear search on results
            // when checking if a spell is already included
            HashSet<int> included = new HashSet<int>();
            foreach (Spell spell in list)
                included.Add(spell.ID);

            int i = 0;
            while (i < list.Count)
            {
                Spell spell = list[i++];

                if (spell.RecourseID != 0)
                {
                    if (!included.Contains(spell.RecourseID))
                    {
                        included.Add(spell.RecourseID);
                        Spell spellref;
                        if (SpellsById.TryGetValue(spell.RecourseID, out spellref))
                            list.Add(spellref);
                    }
                }

                // check effects slots for the [Spell 1234] references
                foreach (string s in spell.Slots)
                    if (s != null)
                    {
                        Match match = Spell.SpellRefExpr.Match(s);
                        if (match.Success)
                        {
                            int id = Int32.Parse(match.Groups[1].Value);
                            if (!included.Contains(id))
                            {
                                included.Add(id);
                                Spell spellref;
                                if (SpellsById.TryGetValue(id, out spellref))
                                    list.Add(spellref);
                            }
                        }
                    }
            }
        }

        private StringBuilder InitHtml()
        {
            var html = new StringBuilder();
            html.AppendLine(winparser.Properties.Resources.HtmlTemplate);

            return html;
        }

        private void ShowHtml(StringBuilder html)
        {
            SearchBrowser.DocumentText = html.ToString();

            //var path = Directory.GetCurrentDirectory() + "\\results.htm";
            //File.WriteAllText(path, html.ToString());
            //SearchBrowser.Navigate("file:///" + path);
        }

        private void ShowAsText(IList<Spell> list, StringBuilder html)
        {
            foreach (var spell in list.Take(2000))
            {
                html.AppendFormat("<p id='spell{0}'><strong>{1}</strong><br/>", spell.ID, spell.ToString());
                foreach (var line in spell.Details())
                    html.Append(GetSpellLink(line) + "<br/>");
                if (spell.Desc != null)
                    html.Append(spell.Desc);
                html.Append("</p>");
            }
        }

        private void ShowAsTable(IList<Spell> list, StringBuilder html)
        {
            html.Append("<table>");
            html.Append("<thead><tr><th>ID</th><th>Name</th><th>Classes</th><th>Mana</th><th>Cast</th><th>Recast</th><th>Duration</th><th>Resist</th><th>Target</th><th>Effects</th></tr></thead>");

            foreach (var spell in list.Take(2000))
            {
                html.AppendFormat("<tr id='spell{0}'><td>{0}</td><td>{1}</td>", spell.ID, spell.Name);

                html.AppendFormat("<td style='max-width: 12em'>{0}</td>", spell.ClassesLevels);

                if (spell.Endurance > 0)
                    html.AppendFormat("<td class='end'>{0}</td>", spell.Endurance);
                else
                    html.AppendFormat("<td class='mana'>{0}</td>", spell.Mana);

                html.AppendFormat("<td>{0}s</td>", spell.CastingTime);

                html.AppendFormat("<td>{0} {1}</td>", FormatTime(spell.RecastTime), spell.RecastTime > 0 && spell.TimerID > 0 ? " T" + spell.TimerID : "");

                html.AppendFormat("<td>{0}</td>", FormatTime(spell.DurationTicks * 6));

                if (!spell.Beneficial)
                    html.AppendFormat("<td>{0} {1}</td>", spell.ResistType, spell.ResistMod != 0 ? spell.ResistMod.ToString() : "");
                else
                    html.Append("<td class='note'>n/a</td>");

                html.AppendFormat("<td>{0} {1} {2}</td>", FormatEnum(spell.Target), spell.MaxTargets > 0 ? " (" + spell.MaxTargets + ")" : "", spell.ViralRange > 0 ? " + Viral" : "");

                html.Append("<td>");

                if (spell.MaxHits > 0)
                    html.AppendFormat("Max Hits: {0} {1}<br/>", spell.MaxHits, FormatEnum(spell.MaxHitsType));

                if (spell.HateOverride != 0)
                    html.AppendFormat("Hate: {0}<br/>", spell.HateOverride);

                if (spell.PushBack != 0)
                    html.AppendFormat("Push: {0}<br/>", spell.PushBack);

                if (spell.RecourseID != 0)
                    html.AppendFormat("Recourse: {0}<br/>", GetSpellLink(String.Format("[Spell {0}]", spell.RecourseID)));

                for (int i = 0; i < spell.Slots.Length; i++)
                    if (!String.IsNullOrEmpty(spell.Slots[i]))
                        html.AppendFormat("{0}: {1}<br/>", i + 1, GetSpellLink(spell.Slots[i]));

                html.Append("</td>");

                html.Append("</tr>");
                html.AppendLine();
            }


            html.Append("</table>");
        }

        //public string GetSpellInfo(int id)
        //{
        //    Spell spell;
        //    if (SpellsById.TryGetValue(id, out spell))
        //        return String.Join("<br/>", spell.Details());
        //    return "Not Found";
        //}

        private string GetSpellLink(string text)
        {
            text = HtmlEncode(text);

            text = Spell.SpellRefExpr.Replace(text, delegate(Match m)
            {
                int id = Int32.Parse(m.Groups[1].Value);
                string name = String.Format("[Spell {0}]", id);
                if (SpellsById.ContainsKey(id))
                    name = SpellsById[id].Name;
                return String.Format("<a href='#spell{0}' onclick='showSpell({0}, this); return false;'>{1}</a>", id, name);
                //return String.Format("<a class='spell' href='#spell{0}'>{1}</a>", id, name);
                //return String.Format("<a class='spell' href='#' onclick='alert(window.external.GetSpellInfo({0})); return false;'>{1}</a>", id, name);
            });

            //text = Spell.GroupRefExpr.Replace(text, delegate(Match m)
            //{
            //    int id = Int32.Parse(m.Groups[1].Value);
            //    string name = SpellCache.GetGroupName(id) ?? String.Format("[Group {0}]", id);

            //    return String.Format("<a href='#' onclick=\"$('.group{0}').insertAfter($(this).closest('tr')).show().addClass('linked'); return false; \">{1}</a>", id, name);
            //});

            text = Spell.ItemRefExpr.Replace(text, delegate(Match m)
            {
                return String.Format("<a href='http://lucy.allakhazam.com/item.html?id={0}'>Item {0}</a>", m.Groups[1].Value);
            });

            return text;
        }

        private string HtmlEncode(string text)
        {
            // i don't think .net has a html encoder outside of the system.web assembly
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private string FormatTime(float seconds)
        {
            if (seconds < 120)
                return seconds.ToString("0.#") + "s";

            if (seconds <= 7200)
                return (seconds / 60f).ToString("0.#") + "m";

            var ts = new TimeSpan(0, 0, (int)seconds);
            return ts.Hours + "h" + ts.Minutes + "m";
            //return new TimeSpan(0, 0, (int)seconds).ToString().TrimStart('0');
        }

        private string FormatEnum(object o)
        {
            string type = o.ToString().Replace("_", " ").Trim();
            if (Regex.IsMatch(type, @"^-?\d+$"))
                type = "Type " + type; // undefined numeric enum
            else
                type = Regex.Replace(type, @"\d+$", ""); // remove numeric suffix on duplicate enums undead3/summoned3/etc
            return type;
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // <= 1 because the FileOpenForm is never closed
            if (Application.OpenForms.Count <= 1)
                Application.Exit();
        }

        private void SearchBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            //Text = e.Url.ToString();
        }

        private void CompareBtn_Click(object sender, EventArgs e)
        {
            MainForm other = null;
            foreach (var f in Application.OpenForms)
                if (f is MainForm && f != this)
                    other = (MainForm)f;

            if (other == null) // || other.Results == null || Results == null)
            {
                MessageBox.Show("You must have one other spell file open to compare with. This is done by selecting two spell files when the program starts.");
                return;
            }

            Search();
            other.SearchText.Text = SearchText.Text;
            other.SearchClass.Text = SearchClass.Text;
            other.SearchLevel.Text = SearchLevel.Text;
            other.SearchEffect.Text = SearchEffect.Text;
            other.Search();

            // method 1: show unchanged spells
            //var ver1 = new StringBuilder();
            //foreach (var s in Results)
            //    ver1.AppendLine(s.ToString() + "\n" + String.Join("\n", s.Details()) + "\n");

            //var ver2 = new StringBuilder();
            //foreach (var s in other.Results)
            //    ver2.AppendLine(s.ToString() + "\n" + String.Join("\n", s.Details()) + "\n");

            // method 2: show only changed spells
            var master1 = new List<string>();
            foreach (var s in Results)
                master1.Add(s.ToString() + "\n" + String.Join("\n", s.Details()) + "\n\n");

            var master2 = new List<string>();
            foreach (var s in other.Results)
                master2.Add(s.ToString() + "\n" + String.Join("\n", s.Details()) + "\n\n");

            var ver1 = String.Join("", master1.Except(master2).ToArray());
            var ver2 = String.Join("", master2.Except(master1).ToArray());

            var dmp = new DiffMatchPatch.diff_match_patch();
            //var diff = dmp.diff_main(ver1.ToString(), ver2.ToString());
            var diff = dmp.diff_main(ver1, ver2);
            //CompareNotes.Text = String.Format("{0} differences", diff.Count); 
            //dmp.diff_cleanupEfficiency(diff);

            var html = InitHtml();
            html.AppendFormat("<p>Differences are shown as a series of <ins>additions</ins> and <del>deletions</del> to convert {0} to {1}.</p>", SpellPath, other.SpellPath);

            html.Append(dmp.diff_prettyHtml(diff));
            html.Append("</html>");

            SearchBrowser.DocumentText = html.ToString();

            html = InitHtml();
            html.AppendFormat("<p>See other window for comparison.</p></html>");
            other.SearchBrowser.DocumentText = html.ToString();
        }



    }
}
