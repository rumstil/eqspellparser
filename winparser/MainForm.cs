using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DiffMatchPatch;
using Everquest;

namespace winparser
{
    //[ComVisible(true)] // for SearchBrowser.ObjectForScripting
    public partial class MainForm : Form
    {
        private Dictionary<string, string> Effects;

        private SpellCache Spells;

        public string SpellPath;
        public List<Spell> Results;


        public MainForm()
        {
            InitializeComponent();

            Width = Screen.PrimaryScreen.WorkingArea.Width - 100;

            SearchClass.Items.AddRange(Enum.GetNames(typeof(SpellClassesLong)));
            SearchClass.Items.Add("");

            // literal text suggestions (these words appear in parsed text)
            SearchEffect.Items.Add("Charm");
            SearchEffect.Items.Add("Mesmerize");
            SearchEffect.Items.Add("Memory Blur");
            SearchEffect.Items.Add("Root");
            SearchEffect.Items.Add("Stun");
            SearchEffect.Items.Add("Hate");
            SearchEffect.Items.Add("Invisibility");
            SearchEffect.Items.Add("Add Defensive Proc");

            // regex suggestions
            Effects = new Dictionary<string, string>();
            Effects.Add("Cure", @"Decrease \w+ Counter by (\d+)");
            Effects.Add("Heal", @"Increase Current HP by ([1-9]\d+)(?!.*(?:per tick))"); // 1-9 excludes spells with Increase Current HP by 0
            Effects.Add("HoT", @"Increase Current HP by (\d+) per tick");
            Effects.Add("Nuke", @"Decrease Current HP by (\d+)(?!.*(?:per tick))");
            Effects.Add("DoT", @"Decrease Current HP by (\d+) per tick");
            Effects.Add("Haste", @"Increase Melee Haste (?:v3 )?by (\d+)");
            Effects.Add("Slow", @"Decrease Melee Haste by (\d+)");
            Effects.Add("Snare", @"Decrease Movement Speed by (\d+)");
            Effects.Add("Shrink", @"Decrease Player Size");
            Effects.Add("Rune", "@Absorb");
            Effects.Add("Pacify", @"Decrease Social Radius");
            Effects.Add("Damage Shield", @"Increase Damage Shield by (\d+)");
            Effects.Add("Mana Regen", @"Increase Current Mana by (\d+)");
            Effects.Add("Add Proc", @"(?:Add Proc)|(?:Add Skill Proc)");
            Effects.Add("Add Spell Proc", @"Cast on Spell Use");
            SearchEffect.Items.AddRange(Effects.Keys.ToArray());
            SearchEffect.Items.Add("");

            //SearchBrowser.ObjectForScripting = this;
        }

        public new void Load(string spellPath, string descPath)
        {
            SpellPath = spellPath;
            Text = spellPath;

            Cursor.Current = Cursors.WaitCursor;

            Spells = new SpellCache(SpellParser.LoadFromFile(spellPath, descPath));
            SearchClass_TextChanged(this, null);
            AutoSearch.Enabled = false;

            Cursor.Current = Cursors.Default;

            var html = InitHtml();

            html.AppendFormat("<p>Loaded <strong>{0}</strong> spells from {1}.</p></html>", Spells.Count, SpellPath);
            html.Append("<p>Use the search button to perform a search on this spell file based on the filters on the left.");
            html.Append("<p>Use the compare button to compare two different spell files and show the differences. e.g. test server vs live server spells.");
            html.Append("<p>Tip: You can use the up/down arrow keys when the cursor is in the Class/Has Effect/Category fields to quickly try different searches.");
            html.Append("<p>Tip: This parser is an open source application and accepts updates and corrections here: <a href='http://code.google.com/p/eqspellparser/' class='ext' target='_top'>http://code.google.com/p/eqspellparser/</a>");

            ShowHtml(html);
        }

        /// <summary>
        /// Search spell database based on filter settings
        /// </summary>
        public void Search()
        {
            AutoSearch.Enabled = false;

            var text = SearchText.Text.Trim();
            int id = 0;
            Int32.TryParse(text, out id);
            var cls = SpellParser.ParseClass(SearchClass.Text) - 1;
            var effect = SearchEffect.Text;
            int slot = 0;
            Int32.TryParse(SearchEffectSlot.Text, out slot);
            var category = SearchCategory.Text;
            int min;
            int max;
            ParseRange(SearchLevel.Text, out min, out max);

            Results = Search(text, cls, min, max, effect, slot, category).ToList();

            Spells.Expand(Results, Results.Count > 1);


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
            if (cls >= 0)
            {
                Sorting = String.Format("Results sorted by {1} level.", Results.Count, SearchClass.Text);
                Results.Sort((a, b) =>
                {
                    if (a.Levels[cls] > 0 && b.Levels[cls] == 0)
                        return -1;
                    if (b.Levels[cls] > 0 && a.Levels[cls] == 0)
                        return 1;
                    int comp = a.Levels[cls] - b.Levels[cls];
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

            // if searching by id, move the spell to the top of the results because it may be sorted below it's side effect spells
            if (id > 0)
            {
                var i = Results.FindIndex(x => x.ID == id);
                if (i > 0)
                {
                    var move = Results[i];
                    Results.RemoveAt(i);
                    Results.Insert(0, move);
                }
            }
            // move entries that begin with the search text to the front of the results
            else if (!String.IsNullOrEmpty(text))
            {
                var move = Results.FindAll(x => x.Name.StartsWith(text, StringComparison.CurrentCultureIgnoreCase));
                if (move.Count > 0)
                {
                    Results.RemoveAll(x => x.Name.StartsWith(text, StringComparison.CurrentCultureIgnoreCase));
                    Results.InsertRange(0, move);
                }
            }

        }

        private IQueryable<Spell> Search(string text, int cls, int min, int max, string effect, int slot, string category)
        {
            var query = Spells.AsQueryable();

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

            // effect filter  can be a literal string or a regex
            if (!String.IsNullOrEmpty(effect))
            {
                if (Effects.ContainsKey(effect))
                    effect = Effects[effect];

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
                    query = query.Where(x => x.Category != null && x.Category.IndexOf(category, StringComparison.InvariantCultureIgnoreCase) >= 0);
            }

            return query;
        }

        /// <summary>
        /// Display the current search results
        /// </summary>
        private void ShowResults()
        {
            SearchNotes.Text = String.Format("{0} results", Results.Count);

            var html = InitHtml();

            if (Results.Count == 0)
            {
                html.Append("<p><strong>Sorry, no matching spells were found.</strong></p><p>You may have made the filters too restrictive (including levels), accidentally defined conflicting filters, or left one of the filters filled in from a previous search. Try filtering by just one or two filters.</p>");
            }
            else
            {
                if (Results.Count > 2000)
                    html.Append("<p>Only the first 2000 are shown.</p>");

                if (DisplayText.Checked)
                    ShowAsText(Results.Take(2000), html);
                else
                    ShowAsTable(Results.Take(2000), html);
            }

            html.Append("</html>");
            ShowHtml(html);
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
                html.AppendFormat("<p id='spell{0}' class='group{1}'><strong>{2}</strong><br/>", spell.ID, spell.GroupID, spell.ToString());
                foreach (var line in spell.Details())
                    html.Append(InsertSpellRefLinks(line) + "<br/>");
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
                html.AppendFormat("<tr id='spell{0}' class='group{1}'><td>{0}</td>", spell.ID, spell.GroupID);
                //html.AppendFormat("<tr id='spell{0}' class='group{1}'><td>{0}{2}</td>", spell.ID, spell.GroupID, spell.GroupID > 0 ? " / " + spell.GroupID : "");

                html.AppendFormat("<td>{0}</td>", spell.Name);

                html.AppendFormat("<td style='max-width: 12em'>{0}</td>", spell.ClassesLevels);

                if (spell.Endurance == 0 && spell.EnduranceUpkeep > 0)
                    html.AppendFormat("<td class='end'>{0}/tick</td>", spell.EnduranceUpkeep);
                else if (spell.Endurance > 0 && spell.EnduranceUpkeep > 0)
                    html.AppendFormat("<td class='end'>{0} + {1}/tick</td>", spell.Endurance, spell.EnduranceUpkeep);
                else if (spell.Endurance > 0)
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

                if (spell.RestTime > 1.5)
                    html.AppendFormat("Rest: {0}s<br/>", spell.RestTime.ToString());

                if (spell.RecourseID != 0)
                    html.AppendFormat("Recourse: {0}<br/>", InsertSpellRefLinks(String.Format("[Spell {0}]", spell.RecourseID)));

                for (int i = 0; i < spell.Slots.Length; i++)
                    if (!String.IsNullOrEmpty(spell.Slots[i]))
                        html.AppendFormat("{0}: {1}<br/>", i + 1, InsertSpellRefLinks(spell.Slots[i]));

                html.Append("</td>");

                html.Append("</tr>");
                html.AppendLine();
            }


            html.Append("</table>");
        }

        private string InsertSpellRefLinks(string text)
        {
            text = HtmlEncode(text);

            text = Spell.SpellRefExpr.Replace(text, m =>
            {
                int id = Int32.Parse(m.Groups[1].Value);
                string name = Spells.GetSpellName(id) ?? String.Format("[Spell {0}]", id);
                return String.Format("<a href='#spell{0}' onclick='showSpell({0}, this); return false;'>{1}</a>", id, name);
            });

            text = Spell.GroupRefExpr.Replace(text, m =>
            {
                int id = Int32.Parse(m.Groups[1].Value);
                string name = Spells.GetSpellGroupName(id) ?? String.Format("[Group {0}]", id);
                return String.Format("<a href='#group{0}' onclick='showGroup({0}, this); return false;'>{1}</a>", id, name);
            });

            text = Spell.ItemRefExpr.Replace(text, m =>
            {
                //return String.Format("<a href='http://lucy.allakhazam.com/item.html?id={0}' class='ext' target='_top'>Item {0}</a>", m.Groups[1].Value);
                return String.Format("<a href='http://everquest.allakhazam.com/db/item.html?item={0};source=lucy' class='ext' target='_top'>Item {0}</a>", m.Groups[1].Value);
            });

            return text;
        }

        private void ParseRange(string text, out int min, out int max)
        {
            min = 1;
            max = 254;
            if (String.IsNullOrEmpty(text))
                return;

            var parts = text.Replace(" ", "").Split('-');
            if (parts.Length == 2)
            {
                if (!Int32.TryParse(parts[0], out min))
                    min = 1;
                if (min == 0)
                    min = 1; // zero would include spells the class can't cast
                if (!Int32.TryParse(parts[1], out max))
                    max = 254;
            }
            else if (parts.Length == 1 && parts[0].Length > 0)
            {
                if (!Int32.TryParse(parts[0], out min))
                    min = 1;
                max = min;
            }
        }

        private string HtmlEncode(string text)
        {
            // i don't think .net has a html encoder outside of the system.web assembly
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private string TrimNumerals(string text)
        {
            // trim rank
            int i = text.IndexOf("Rk.");
            if (i > 0)
                text = text.Substring(0, i - 1);

            // trim numerals
            text = text.TrimEnd(' ', 'X', 'V', 'I');

            return text;
        }

        private string FormatTime(float seconds)
        {
            if (seconds < 120)
                return seconds.ToString("0.##") + "s";

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
            // start external links in an external window
            // internal links will all be "about:blank"
            // using a target other than target=_top seems to force IE rather than the default browser on one of my computers 
            if (e.Url.Scheme.StartsWith("http") || !String.IsNullOrEmpty(e.TargetFrameName))
            {
                e.Cancel = true;
                System.Diagnostics.Process.Start(e.Url.ToString());
            }
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Search();
            ShowResults();
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

            if (other == null)
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
            other.SearchEffectSlot.Text = SearchEffectSlot.Text;
            other.SearchCategory.Text = SearchCategory.Text;
            other.Search();

            MainForm oldVer = this;
            MainForm newVer = other;
            if (File.GetLastWriteTime(other.SpellPath) < File.GetLastWriteTime(SpellPath))
            {
                oldVer = other;
                newVer = this;
            }

            // these functions generates the comparison text for each spell
            Func<Spell, string> getOldText = x => x.ToString() + "\n" + oldVer.Spells.InsertSpellNames(String.Join("\n", x.Details())) + "\n\n";
            Func<Spell, string> getNewText = x => x.ToString() + "\n" + newVer.Spells.InsertSpellNames(String.Join("\n", x.Details())) + "\n\n";
            var diffs = Compare(oldVer.Results, newVer.Results, getOldText, getNewText);

            var html = InitHtml();

            if (diffs.Count == 0)
                html.AppendFormat("<p>No differences were found between {0} and {1} based on the search filters.</p>", oldVer.SpellPath, newVer.SpellPath);
            else
            {                
                html.AppendFormat("<p>Found {0} differences between <del>{1}</del> and <ins>{2}</ins>.</p>", diffs.Count(x => x.operation != Operation.EQUAL), oldVer.SpellPath, newVer.SpellPath);
                html.Append(diff_match_patch.diff_prettyHtml(diffs));
            }

            html.Append("</html>");

            SearchBrowser.DocumentText = html.ToString();

            //html = InitHtml();
            //html.AppendFormat("<p>See other window for comparison.</p></html>");
            //other.SearchBrowser.DocumentText = html.ToString();

            Cursor.Current = Cursors.Default;
        }

        private static List<Diff> Compare(IEnumerable<Spell> setA, IEnumerable<Spell> setB, Func<Spell, string> getTextA, Func<Spell, string> getTextB)
        {
            var dmp = new DiffMatchPatch.diff_match_patch();
            var diffs = new List<Diff>();

            // compare the same spell ID in each list one by one, then each line by line
            var A = setA.Where(x => x.ID > 0).OrderBy(x => x.ID).ToList();
            var B = setB.Where(x => x.ID > 0).OrderBy(x => x.ID).ToList();
            int a = 0; // current index in list A
            int b = 0; // current index in list B

            int count = 0;
            int id = 0;
            while (true)
            {
                id++;

                if (a >= A.Count && b >= B.Count)
                    break;

                // reached end of list A, treat remainder of list B as inserts
                if (a >= A.Count)
                {
                    while (b < B.Count)
                        diffs.Add(new Diff(Operation.INSERT, getTextB(B[b++])));
                    break;
                }

                // reached end of list B, treat remainder of list A as deletes
                if (b >= B.Count)
                {
                    while (a < A.Count)
                        diffs.Add(new Diff(Operation.DELETE, getTextA(A[a++])));
                    break;
                }

                // id doesn't exist in either list
                if (A[a].ID > id && B[b].ID > id)
                    continue;

                // id exists in both lists
                if (A[a].ID == id && B[b].ID == id)
                {
                    var textA = getTextA(A[a++]);
                    var textB = getTextB(B[b++]);
                    // ignore equal spells
                    if (textA == textB)
                        continue;
                    diffs.AddRange(dmp.diff_lineMode(textA, textB));
                    count++;
                    continue;
                }

                // id exist only in list A
                if (A[a].ID == id)
                {
                    diffs.Add(new Diff(Operation.DELETE, getTextA(A[a++])));
                    count++;
                    continue;
                }

                // id exists only in list B
                if (B[b].ID == id)
                {
                    diffs.Add(new Diff(Operation.INSERT, getTextB(B[b++])));
                    count++;
                    continue;
                }

                throw new NotImplementedException(); // should never get here
            }

            return diffs;
        }

        private void SearchClass_TextChanged(object sender, EventArgs e)
        {
            // whenever the class is changed refresh the list of categories so that it only shows categories that class can cast
            int cls = SpellParser.ParseClass(SearchClass.Text) - 1;
            if (cls >= 0)
            {
                SearchLevel.Enabled = true;

                var cat = Spells.Where(x => x.Levels[cls] > 0).Select(x => x.Category).Where(x => x != null).Distinct().ToList();

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
            SearchCategory.Items.Add("");
            SearchText_TextChanged(sender, e);
        }

        private void SearchText_TextChanged(object sender, EventArgs e)
        {
            // reset timer every time the user presses a key
            AutoSearch.Interval = (sender is TextBox) ? 800 : 400;
            AutoSearch.Enabled = false;
            AutoSearch.Enabled = true;
        }


    }
}
