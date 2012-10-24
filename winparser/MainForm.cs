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

            Width = Screen.PrimaryScreen.WorkingArea.Width - 100;

            SearchClass.Items.AddRange(Enum.GetNames(typeof(SpellClassesLong)));

            // literal text suggestions
            SearchEffect.Items.Add("Charm");
            SearchEffect.Items.Add("Mesmerize");
            SearchEffect.Items.Add("Memory Blur");
            SearchEffect.Items.Add("Root");
            SearchEffect.Items.Add("Stun");
            SearchEffect.Items.Add("Hate");
            SearchEffect.Items.Add("Invisibility");

            Effects = new Dictionary<string, string>();
            Effects.Add("Cure", @"Decrease \w+ Counter by (\d+)");
            Effects.Add("Heal", @"Increase Current HP by (\d+)");
            Effects.Add("HoT", @"Increase Current HP by (\d+) per tick");
            Effects.Add("Nuke", @"Decrease Current HP by (\d+)");
            Effects.Add("DoT", @"Decrease Current HP by (\d+) per tick");
            Effects.Add("Haste", @"Increase Melee Haste .*?by (\d+)"); // .* for v3 haste
            Effects.Add("Slow", @"Decrease Melee Haste by (\d+)");
            Effects.Add("Snare", @"Decrease Movement Speed by (\d+)");
            Effects.Add("Shrink", @"Decrease Player Size");
            Effects.Add("Rune", "@Absorb");
            Effects.Add("Pacify", @"Decrease Social Radius");
            Effects.Add("Damage Shield", @"Increase Damage Shield by (\d+)");
            Effects.Add("Mana Regen", @"Increase Current Mana by (\d+)");

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
            SearchClass_TextChanged(this, null);
            AutoSearch.Enabled = false;

            Cursor.Current = Cursors.Default;

            var html = InitHtml();

            html.AppendFormat("<p>Loaded <strong>{0}</strong> spells from {1}.</p></html>", Spells.Count, SpellPath);
            html.Append("<p>Use the search button to perform a search on this spell file based on the criteria on the left.");
            html.Append("<p>Use the compare button to compare two different spell files and show the differences. e.g. test server vs live server spells.");
            html.Append("<p>Tip: You can use the up/down arrow keys when the cursor is in the Class/Has Effect/Category fields to quickly try different searches.");
            html.Append("<p>Tip: This parser is an open source application and accepts updates and corrections here: <a class='ext' href='http://code.google.com/p/eqspellparser/'>http://code.google.com/p/eqspellparser/</a>");


            SearchBrowser.DocumentText = html.ToString();
        }

        public int GetSearchClass()
        {
            return Enum.IsDefined(typeof(SpellClassesLong), SearchClass.Text) ? (int)Enum.Parse(typeof(SpellClassesLong), SearchClass.Text) - 1 : -1;
        }

        /// <summary>
        /// Search spell database based on form filter settings
        /// </summary>
        public void Search()
        {
            AutoSearch.Enabled = false;

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
                if (!Int32.TryParse(levels[0], out min))
                    min = 1;
                if (min == 0)
                    min = 1; // zero would include spells the class can't cast
                if (!Int32.TryParse(levels[1], out max))
                    max = 254;
            }
            else if (levels.Length == 1 && levels[0].Length > 0)
            {
                if (!Int32.TryParse(levels[0], out min))
                    min = 1;
                max = min;
            }

            var _class = GetSearchClass();
            if (_class >= 0)
            {
                query = query.Where(x => x.Levels[_class] >= min && x.Levels[_class] <= max);
            }

            // effect filter  can be a literal string or a regex
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

            string category = SearchCategory.Text;
            if (!String.IsNullOrEmpty(category))
            {
                if (category == "AA")
                    query = query.Where(x => String.IsNullOrEmpty(x.Category));
                else
                    query = query.Where(x => x.Category != null && x.Category.IndexOf(category, StringComparison.InvariantCultureIgnoreCase) >= 0);
            }

            Results = query.ToList();
            Expand(Results);


            string Sorting = null;
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
                Sorting = String.Format("Results sorted by {1} level.", Results.Count, SearchClass.Text);
                Results.Sort((a, b) =>
                {
                    if (a.Levels[_class] > 0 && b.Levels[_class] == 0)
                        return -1;
                    if (b.Levels[_class] > 0 && a.Levels[_class] == 0)
                        return 1;
                    int comp = a.Levels[_class] - b.Levels[_class];
                    if (comp == 0)
                        comp = String.Compare(TrimNumerals(a.Name), TrimNumerals(b.Name));
                    if (comp == 0)
                        comp = a.ID - b.ID;
                    return comp;
                });
            }
            // 3. finally sort by name if no better method is found
            else
            {
                Sorting = String.Format("Results sorted by name.", Results.Count);
                Results.Sort((a, b) =>
                {
                    int comp = String.Compare(TrimNumerals(a.Name), TrimNumerals(b.Name));
                    if (comp == 0)
                        comp = a.ID - b.ID;
                    return comp;
                });
            }


            SearchNotes.Text = String.Format("{0} results", Results.Count);

            var html = InitHtml();

            if (Results.Count == 0)
            {
                html.Append("<p><strong>Sorry, no matching spells were found.</strong></p><p>You may have made the filters too restrictive (including levels), accidentally defined conflicting filters, or left one of the filters filled in from a previous search. Try filtering by just one or two filters.</p>");
            }
            else
            {
                if (Results.Count > 2000)
                    Sorting += " Only the first 2000 are shown.";
                html.Append("<p>" + Sorting + "</p>");

                if (DisplayText.Checked)
                    ShowAsText(Results.Take(2000), html);
                else
                    ShowAsTable(Results.Take(2000), html);
            }

            html.Append("</html>");
            SearchBrowser.DocumentText = html.ToString();
        }

        /// <summary>
        /// Expand the spell list to include associated spells.
        /// </summary>                
        private void Expand(IList<Spell> list)
        {
            // keep track of all spells in the results so that we don't enter into a loop
            HashSet<int> included = new HashSet<int>();
            foreach (Spell spell in list)
                included.Add(spell.ID);

            // search the full spell list to find spells that link to the current results (reverse links)
            // but do not do this for spells that are heavily referenced. e.g. complete heal is referenced by hundreds of focus spells
            var ignore = list.Where(x => x.RefCount > 10).Select(x => x.ID).ToList();
            foreach (var spell in Spells)
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

        private void ShowAsText(IEnumerable<Spell> list, StringBuilder html)
        {
            foreach (var spell in list)
            {
                html.AppendFormat("<p id='spell{0}'><strong>{1}</strong><br/>", spell.ID, spell.ToString());
                foreach (var line in spell.Details())
                    html.Append(GetSpellLink(line) + "<br/>");
                if (spell.Desc != null)
                    html.Append(spell.Desc);
                html.Append("</p>");
            }
        }

        private void ShowAsTable(IEnumerable<Spell> list, StringBuilder html)
        {
            html.Append("<table style='table-layout: fixed; width: 89em;'>");
            html.Append("<thead><tr>");
            html.Append("<th style='width: 4em;'>ID</th>");
            html.Append("<th style='width: 18em;'>Name</th>");
            html.Append("<th style='width: 10em;'>Classes</th>");
            html.Append("<th style='width: 4em;'>Mana</th>");
            html.Append("<th style='width: 4em;'>Cast</th>");
            html.Append("<th style='width: 4em;'>Recast</th>");
            html.Append("<th style='width: 4em;'>Duration</th>");
            html.Append("<th style='width: 6em;'>Resist</th>");
            html.Append("<th style='width: 5em;'>Target</th>");
            html.Append("<th style='width: 30em;'>Effects</th>");
            html.Append("</tr></thead>");

            foreach (var spell in list)
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
                return String.Format("<a class='ext' href='http://lucy.allakhazam.com/item.html?id={0}'>Item {0}</a>", m.Groups[1].Value);
            });

            return text;
        }

        private string HtmlEncode(string text)
        {
            // i don't think .net has a html encoder outside of the system.web assembly
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private string TrimNumerals(string name)
        {
            // trim rank
            int i = name.IndexOf("Rk.");
            if (i > 0)
                name = name.Substring(0, i - 1);

            // trim numerals
            name = name.TrimEnd(' ', 'X', 'V', 'I');

            return name;
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

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // quit if no other windows are open (+1 for FileOpenForm which is hidden)
            if (Application.OpenForms.Count <= 2)
                Application.Exit();
        }

        private void SearchBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            //Text = e.Url.ToString();
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Search();
            Cursor.Current = Cursors.Default;
        }

        private void CompareBtn_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            FileOpenForm open = null;
            MainForm other = null;
            foreach (var f in Application.OpenForms)
            {
                if (f is MainForm && f != this)
                    other = (MainForm)f;
                if (f is FileOpenForm)
                    open = (FileOpenForm)f;
            }

            if (other == null) // || other.Results == null || Results == null)
            {
                open.Show();
                open.WindowState = FormWindowState.Normal;
                open.SetStatus("Please open another spell file to compare with " + SpellPath);
                return;
            }

            // perform the same search on both spell files
            Search();
            other.SearchText.Text = SearchText.Text;
            other.SearchClass.Text = SearchClass.Text;
            other.SearchLevel.Text = SearchLevel.Text;
            other.SearchEffect.Text = SearchEffect.Text;
            other.SearchCategory.Text = SearchCategory.Text;
            other.Search();

            // method 1: show changed and unchanged spells
            //var ver1 = new StringBuilder();
            //foreach (var s in Results)
            //    ver1.AppendLine(s.ToString() + "\n" + String.Join("\n", s.Details()) + "\n");

            //var ver2 = new StringBuilder();
            //foreach (var s in other.Results)
            //    ver2.AppendLine(s.ToString() + "\n" + String.Join("\n", s.Details()) + "\n");

            // method 2: show only changed spells
            var master1 = new List<string>();
            foreach (var s in Results.OrderBy(x => x.ID))
                master1.Add(s.ToString() + "\n" + String.Join("\n", s.Details()) + "\n\n");

            var master2 = new List<string>();
            foreach (var s in other.Results.OrderBy(x => x.ID))
                master2.Add(s.ToString() + "\n" + String.Join("\n", s.Details()) + "\n\n");

            var ver1 = String.Join("", master1.Except(master2).ToArray());
            var ver2 = String.Join("", master2.Except(master1).ToArray());

            var dmp = new DiffMatchPatch.diff_match_patch();
            //var diff = dmp.diff_main(ver1.ToString(), ver2.ToString()); // method 1
            //var diff = dmp.diff_main(ver1, ver2); // method 2
            var diff = dmp.diff_lineMode(ver1, ver2);  // method 2

            var html = InitHtml();

            if (diff.Count == 0)
                html.AppendFormat("<p>No differences were found between {0} and {1} based on the search filters.</p>", SpellPath, other.SpellPath);
            else
            {
                html.AppendFormat("<p>Differences are shown as a series of <ins>additions</ins> and <del>deletions</del> that are needed to convert {0} to {1}.</p>", SpellPath, other.SpellPath);
                html.Append(dmp.diff_prettyHtml(diff));
            }

            html.Append("</html>");

            SearchBrowser.DocumentText = html.ToString();

            //html = InitHtml();
            //html.AppendFormat("<p>See other window for comparison.</p></html>");
            //other.SearchBrowser.DocumentText = html.ToString();

            Cursor.Current = Cursors.Default;
        }

        private void SearchClass_TextChanged(object sender, EventArgs e)
        {
            int _cls = GetSearchClass();
            if (_cls != -1)
            {
                SearchLevel.Enabled = true;

                var cat = Spells.Where(x => x.Levels[_cls] > 0).Select(x => x.Category).Where(x => x != null).Distinct().ToList();

                // add the root categories. e.g. for "Utility Beneficial/Combat Innates/Illusion: Other" add "Utility Beneficial"
                int i = 0;
                while (i < cat.Count)
                {
                    var parts = cat[i].Split('/');
                    if (!cat.Contains(parts[0]))
                        cat.Add(parts[0]);
                    i++;
                }

                SearchCategory.Items.Clear();
                SearchCategory.Items.AddRange(cat.ToArray());
            }
            else
            {
                SearchLevel.Enabled = false;
                SearchCategory.Items.Clear();
                SearchCategory.Items.AddRange(Spells.Select(x => x.Category).Where(x => x != null).Distinct().ToArray());
            }
            SearchCategory.Items.Add("AA");
            SearchText_TextChanged(sender, e);
        }

        private void SearchText_TextChanged(object sender, EventArgs e)
        {
            // reset timer so that we don't query on every single character the user types
            AutoSearch.Interval = (sender is TextBox) ? 800 : 400;
            AutoSearch.Enabled = false;
            AutoSearch.Enabled = true;
        }


    }
}
