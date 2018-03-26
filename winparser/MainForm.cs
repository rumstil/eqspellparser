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
using EQSpellParser;


namespace winparser
{
    //[ComVisible(true)] // for SearchBrowser.ObjectForScripting
    public partial class MainForm : Form
    {
        private const int MAX_RESULTS = 2500;

        private SpellCache Cache;

        public string SpellPath;
        public List<Spell> Results;
        public HashSet<int> VisibleResults;

        public SpellSearchFilter DefaultFilter;
        public List<SpellSearchFilter> SearchHistory = new List<SpellSearchFilter>();


        public MainForm()
        {
            InitializeComponent();

            Width = Math.Min(Screen.PrimaryScreen.WorkingArea.Width - 100, 1800);
            Height = Screen.PrimaryScreen.WorkingArea.Height - 100;

            SearchClass.Items.AddRange(Enum.GetNames(typeof(SpellClassesLong)));
            SearchClass.Sorted = true;
            SearchClass.Sorted = false;
            SearchClass.Items.Add("Non PC");
            SearchClass.Items.Add("Any PC");
            SearchClass.Items.Add("");

            SearchEffect1.Items.AddRange(SpellSearchFilter.CommonEffects.Keys.ToArray());
            SearchEffect1.Items.Add("");
            SearchEffect1.Text = "";
            SearchEffect2.Items.AddRange(SpellSearchFilter.CommonEffects.Keys.ToArray());
            SearchEffect2.Items.Add("");
            SearchEffect3.Items.AddRange(SpellSearchFilter.CommonEffects.Keys.ToArray());
            SearchEffect3.Items.Add("");

            SearchEffectSlot1.Items.Add("");
            SearchEffectSlot1.Items.AddRange(Enumerable.Range(1, 20).Select(x => x.ToString()).ToArray());
            SearchEffectSlot2.Items.Add("");
            SearchEffectSlot2.Items.AddRange(Enumerable.Range(1, 20).Select(x => x.ToString()).ToArray());
            SearchEffectSlot3.Items.Add("");
            SearchEffectSlot3.Items.AddRange(Enumerable.Range(1, 20).Select(x => x.ToString()).ToArray());


            //SearchBrowser.ObjectForScripting = this;
            DefaultFilter = GetFilter();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            // a winforms bug will trigger TextChanged() the first time a combobox with a "" value loses focus (even if no text has changed)
            // this hack will quickly cycle all controls and disable the auto search that would be triggered
            for (int i = 0; i < SearchFilters.Controls.Count; i++)
            {
                var combo = SearchFilters.Controls[i] as ComboBox;
                if (combo != null)
                    combo.Focus();
            }
            SearchText.Focus();
            AutoSearch.Enabled = false;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // quit if no other windows are open (+1 for FileOpenForm which is hidden)
            if (Application.OpenForms.Count <= 2)
                Application.Exit();
        }

        public new void Load(string spellPath)
        {
            SpellPath = spellPath;
            Text = spellPath;

            Cursor.Current = Cursors.WaitCursor;

            Cache = new SpellCache();
            Cache.LoadSpells(spellPath);
            SearchCategory.Items.AddRange(Cache.SpellList.SelectMany(x => x.Categories).Distinct().ToArray());
            SearchClass_TextChanged(this, null);
            AutoSearch.Enabled = false;

            Cursor.Current = Cursors.Default;

            var html = InitHtml();
            html.AppendFormat("<p>Loaded <strong>{0}</strong> spells from {1}.</p></html>", Cache.SpellList.Count(), SpellPath);
            html.Append("<p>Use the search button to perform a search on this spell file based on the filters on the left.");
            html.Append("<p>Use the compare button to compare two different spell files and show the differences. e.g. test server vs live server spells.");
            html.AppendFormat("<p>This parser is an open source application. Visit <a href='{0}' class='ext' target='_top'>{0}</a> to download updates.", "https://github.com/rumstil/eqspellparser");
            //html.Append("<p>Nov 8 2017 - Some spells will now show as 'Mostly Unresistable'. This means they are unresistable by standard resists and can only be resisted by Sanctification/Mystical Shielding AA or SPA 180 spells.");

            ShowHtml(html);
        }

        public SpellSearchFilter GetFilter()
        {

            var filter = new SpellSearchFilter();
            filter.Text = SearchText.Text.Trim();
            filter.Effect[0] = SearchEffect1.Text.Trim();
            filter.Effect[1] = SearchEffect2.Text.Trim();
            filter.Effect[2] = SearchEffect3.Text.Trim();
            int slot;
            if (Int32.TryParse(SearchEffectSlot1.Text.Trim(), out slot))
                filter.EffectSlot[0] = slot;
            if (Int32.TryParse(SearchEffectSlot2.Text.Trim(), out slot))
                filter.EffectSlot[1] = slot;
            if (Int32.TryParse(SearchEffectSlot3.Text.Trim(), out slot))
                filter.EffectSlot[2] = slot;
            filter.Category = SearchCategory.Text.Trim();
            filter.Class = SearchClass.Text.Trim();
            int min;
            int max;
            ParseRange(SearchLevel.Text, out min, out max);
            filter.ClassMinLevel = min;
            filter.ClassMaxLevel = max;
            //filter.AppendForwardRefs = true;
            filter.AddBackRefs = IncludeRelated.Checked;

            return filter;
        }

        public void SetFilter(SpellSearchFilter filter)
        {
            SearchText.Text = filter.Text;
            SearchEffect1.Text = filter.Effect[0];
            SearchEffect2.Text = filter.Effect[1];
            SearchEffect3.Text = filter.Effect[2];
            SearchEffectSlot1.Text = filter.EffectSlot[0].HasValue ? filter.EffectSlot[0].ToString() : "";
            SearchEffectSlot2.Text = filter.EffectSlot[1].HasValue ? filter.EffectSlot[1].ToString() : "";
            SearchEffectSlot3.Text = filter.EffectSlot[2].HasValue ? filter.EffectSlot[2].ToString() : "";
            SearchCategory.Text = filter.Category;
            SearchClass.Text = filter.Class;
            if (filter.ClassMinLevel > 0 && filter.ClassMaxLevel > 0)
                SearchLevel.Text = filter.ClassMinLevel + "-" + filter.ClassMaxLevel;
            else if (filter.ClassMinLevel > 0)
                SearchLevel.Text = filter.ClassMinLevel.ToString();
            else if (filter.ClassMaxLevel > 0)
                SearchLevel.Text = filter.ClassMaxLevel.ToString();
            IncludeRelated.Checked = filter.AddBackRefs;
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Search();
            ShowResults();
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Search spells and save to [Results]
        /// </summary>
        public void Search()
        {
            AutoSearch.Enabled = false;

            var filter = GetFilter();

            Results = Cache.Search(filter).ToList();

            // filter ranks
            //if (filter.Rank != 0)
            //{
            //    // there is no 'rk. 1' suffix so when filtering ranks we need to make sure a spell has a rank 2/3 version
            //    var names = new HashSet<string>(Results.Select(x => x.Name));
            //    if (filter.Rank == 1)
            //        Results.RemoveAll(x => x.Rank == 2 || x.Rank == 3);
            //}


            // optionally add back refs
            if (filter.AddBackRefs)
                Cache.AddBackRefs(Results);


            // The add back refs Adds back some of the ranks of spells that were removed.  Remove them after
            // optionally adding back the references.
            if (!IncludeRk0.Checked)
                Results.RemoveAll(x => x.Rank == 0);

            if (!IncludeRk1.Checked)
                Results.RemoveAll(x => x.Rank == 1);

            if (!IncludeRk2.Checked)
                Results.RemoveAll(x => x.Rank == 2);

            if (!IncludeRk3.Checked)
                Results.RemoveAll(x => x.Rank == 3);



            // hide anything that isn't in the results yet. additional spells will only be shown when a link is clicked
            VisibleResults = new HashSet<int>(Results.Select(x => x.ID));

            // if we are filtering by class then auto hide all spells that aren't castable by that class
            //int i = SpellParser.ParseClass(filter.Class) - 1;
            //VisibleResults = new HashSet<int>(i >= 0 ? Results.Where(x => x.Levels[i] != 0).Select(x => x.ID) : Results.Select(x => x.ID));

            // always add forward refs so that links can be clicked
            Cache.AddForwardRefs(Results);

            Cache.Sort(Results, filter);
        }

        /// <summary>
        /// Display the current search results
        /// </summary>
        private void ShowResults()
        {
            SearchNotes.Text = String.Format("{0} results", VisibleResults.Count);

            var html = InitHtml();

            if (Results.Count == 0)
            {
                html.Append("<p><strong>Sorry, no matching spells were found.</strong></p><p>You may have made the filters too restrictive (including levels), accidentally defined conflicting filters, or left one of the filters filled in from a previous search. Try filtering by just one or two filters.</p>");
            }
            else
            {
                if (Results.Count > MAX_RESULTS)
                    html.Append(String.Format("<p>Too many results -- only the first {0} will be shown.</p>", MAX_RESULTS));

                //Func<Spell, bool> visible = spell => IncludeRelated.Checked || VisibleResults.Contains(spell.ID);
                Func<Spell, bool> visible = spell => VisibleResults.Contains(spell.ID);


                if (DisplayText.Checked)
                    ShowAsText(Results.Take(MAX_RESULTS), visible, html);
                else
                    ShowAsTable(Results.Take(MAX_RESULTS), visible, html);
            }

            html.Append("</html>");
            ShowHtml(html);
        }

        private StringBuilder InitHtml()
        {
            var html = new StringBuilder(1200000);
            html.AppendLine(winparser.Properties.Resources.HtmlTemplate);

            return html;
        }

        private void ShowHtml(StringBuilder html)
        {
            SearchBrowser.DocumentText = html.ToString();

            //SearchBrowser.DocumentText = "";
            //SearchBrowser.Document.Write(html.ToString());
            //SearchBrowser.Refresh();

            //var path = Directory.GetCurrentDirectory() + "\\results.htm";
            //File.WriteAllText(path, html.ToString());
            //SearchBrowser.Navigate("file:///" + path);
        }

        private void ShowAsText(IEnumerable<Spell> list, Func<Spell, bool> visible, StringBuilder html)
        {
            foreach (var spell in list)
            {
                html.AppendFormat("<p id='spell{0}' class='spell group{1} {3}'><strong>{2}</strong><br/>", spell.ID, spell.GroupID, spell.ToString(), visible(spell) ? "" : "hidden");

                foreach (var line in spell.Details())
                {
                    var slot = Regex.Replace(line, @"(\d+): .*", m =>
                    {
                        int i = Int32.Parse(m.Groups[1].Value) - 1;
                        if (i < 0 || i >= spell.Slots.Count || spell.Slots[i] == null)
                            return "Unknown Index " + i;
                        return String.Format("{0}: <span title=\"SPA={2} Base1={3} Base2={4} Max={5} Calc={6}\">{1}</span>", i + 1, spell.Slots[i].Desc, spell.Slots[i].SPA, spell.Slots[i].Base1, spell.Slots[i].Base2, spell.Slots[i].Max, spell.Slots[i].Calc);
                    });

                    html.Append(InsertRefLinks(slot));
                    html.Append("<br/>");
                }

                if (spell.Desc != null)
                    html.Append(spell.Desc);

                html.Append("</p>");
            }
        }

        private void ShowAsTable(IEnumerable<Spell> list, Func<Spell, bool> visible, StringBuilder html)
        {
            html.Append("<table style='table-layout: fixed;'>");
            html.Append("<thead><tr>");
            html.Append("<th style='width: 4em;'>ID</th>");
            html.Append("<th style='width: 18em;'>Name</th>");
            html.Append("<th style='width: 10em;'>Level</th>");
            html.Append("<th style='width: 4em;'>Mana</th>");
            html.Append("<th style='width: 4em;'>Cast</th>");
            html.Append("<th style='width: 4em;'>Recast</th>");
            html.Append("<th style='width: 4em;'>Duration</th>");
            html.Append("<th style='width: 6em;'>Resist</th>");
            html.Append("<th style='width: 5em;'>Target</th>");
            html.Append("<th style='min-width: 30em;'>Effects</th>");
            html.Append("</tr></thead>");

            foreach (var spell in list)
            {
                html.AppendFormat("<tr id='spell{0}' class='spell group{1} {2}'><td>{0}</td>", spell.ID, spell.GroupID, visible(spell) ? "" : "hidden");
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

                html.AppendFormat("<td>{0} {1}</td>", Spell.FormatTime(spell.RecastTime), spell.RecastTime > 0 && spell.TimerID > 0 ? " T" + spell.TimerID : "");

                html.AppendFormat("<td>{0}{1}</td>", Spell.FormatTime(spell.DurationTicks * 6), spell.DurationTicks > 0 && spell.Focusable ? "+" : "");

                if (!spell.Beneficial)
                    html.AppendFormat("<td>{0} {1}</td>", Spell.FormatEnum(spell.ResistType), spell.ResistMod != 0 ? spell.ResistMod.ToString() : "");
                else
                    html.Append("<td class='note'>n/a</td>");

                html.AppendFormat("<td>{0} {1} {2}</td>", FormatEnum(spell.Target), spell.MaxTargets > 0 ? " (" + spell.MaxTargets + ")" : "", spell.ViralRange > 0 ? " + Viral" : "");

                html.Append("<td>");

                if (spell.Stacking != null && spell.Stacking.Count > 0)
                    html.AppendFormat("Stacking: {0}<br/>", String.Join(", ", spell.Stacking.ToArray()));

                if (spell.MaxHits > 0)
                    html.AppendFormat("Max Hits: {0} {1}<br/>", spell.MaxHits, FormatEnum(spell.MaxHitsType));

                for (int i = 0; i < spell.ConsumeItemID.Length; i++)
                    if (spell.ConsumeItemID[i] > 0)
                        html.AppendFormat("Consumes: {0} x {1}<br/>", InsertRefLinks(String.Format("[Item {0}]", spell.ConsumeItemID[i])), spell.ConsumeItemCount[i]);

                if (spell.HateOverride != 0)
                    html.AppendFormat("Hate: {0}<br/>", spell.HateOverride);

                if (spell.HateMod != 0)
                    html.AppendFormat("Hate Mod: {0:+#;-#;0}<br/>", spell.HateMod);

                if (spell.PushBack != 0)
                    html.AppendFormat("Push: {0}<br/>", spell.PushBack);

                if (spell.RestTime > 1.5)
                    html.AppendFormat("Rest: {0}s<br/>", spell.RestTime.ToString());

                if (spell.RecourseID != 0)
                    html.AppendFormat("Recourse: {0}<br/>", InsertRefLinks(String.Format("[Spell {0}]", spell.RecourseID)));

                if (spell.AEDuration >= 2500)
                    html.AppendFormat("AE Waves: {0}<br/>", spell.AEDuration / 2500);

                for (int i = 0; i < spell.Slots.Count; i++)
                    if (spell.Slots[i] != null)
                        html.AppendFormat("{0}: <span title=\"SPA={2} Base1={3} Base2={4} Max={5} Calc={6}\">{1}</span><br/>", i + 1, InsertRefLinks(spell.Slots[i].Desc), spell.Slots[i].SPA, spell.Slots[i].Base1, spell.Slots[i].Base2, spell.Slots[i].Max, spell.Slots[i].Calc);

                html.Append("</td>");

                html.Append("</tr>");
                html.AppendLine();
            }

            html.Append("</table>");
        }

        private string InsertRefLinks(string text)
        {
            text = Spell.SpellRefExpr.Replace(text, m =>
            {
                int id = Int32.Parse(m.Groups[1].Value);
                string name = Cache.GetSpellName(id) ?? String.Format("[Spell {0}]", id);
                return String.Format("<a href='#spell{0}' onclick='showSpell({0}, this); return false;'>{1}</a>", id, name);
            });

            text = Spell.GroupRefExpr.Replace(text, m =>
            {
                int id = Int32.Parse(m.Groups[1].Value);
                string name = Cache.GetSpellGroupName(id) ?? String.Format("[Group {0}]", id);
                return String.Format("<a href='#group{0}' onclick='showGroup({0}, this); return false;'>{1}</a>", id, name);
            });

            text = Spell.ItemRefExpr.Replace(text, m =>
            {
                int id = Int32.Parse(m.Groups[1].Value);
                string name = m.Groups[0].Value;
                if (Enum.IsDefined(typeof(SpellReagent), id))
                    name = ((SpellReagent)id).ToString().Replace('_', ' ');
                //return String.Format("<a href='http://everquest.allakhazam.com/db/item.html?item={0};source=lucy' class='ext' target='_top'>{1}</a>", id, name);
                return String.Format("<a href='http://lucy.allakhazam.com/item.html?id={0}' class='ext' target='_top'>{1}</a>", id, name);
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

        private string FormatEnum(object o)
        {
            string type = o.ToString().Replace("_", " ").Trim();
            if (Regex.IsMatch(type, @"^-?\d+$"))
                type = "Type " + type; // undefined numeric enum
            else
                type = Regex.Replace(type, @"\d+$", ""); // remove numeric suffix on duplicate enums undead3/summoned3/etc
            return type;
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

        private void CompareBtn_Click(object sender, EventArgs e)
        {
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

            Cursor.Current = Cursors.WaitCursor;
            Compare(other);
            Cursor.Current = Cursors.Default;
        }

        public void Compare(MainForm other)
        {
            // perform the same search on both spell files
            var filter = GetFilter();
            Results = Cache.Search(filter).ToList();
            other.Results = other.Cache.Search(filter).ToList();

            MainForm oldVer = this;
            MainForm newVer = other;
            if (File.GetLastWriteTime(other.SpellPath) < File.GetLastWriteTime(SpellPath))
            {
                oldVer = other;
                newVer = this;
            }

            // these functions generates the comparison text for each spell
            Func<Spell, string> getOldText = x => x.ToString() + "\n" + oldVer.Cache.InsertRefNames(String.Join("\n", x.Details())) + "\n\n";
            Func<Spell, string> getNewText = x => x.ToString() + "\n" + newVer.Cache.InsertRefNames(String.Join("\n", x.Details())) + "\n\n";
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
            // cancel if spells haven't been loaded yet
            if (Cache == null)
                return;

            // only show level filter when a class is selected
            int cls = SpellParser.ParseClass(SearchClass.Text) - 1;
            SearchLevel.Enabled = (cls >= 0);

            SearchText_TextChanged(sender, e);
        }

        private void SearchText_TextChanged(object sender, EventArgs e)
        {
            // reset timer every time the user presses a key
            //var combo = sender as ComboBox;
            AutoSearch.Interval = (sender is TextBox) ? 800 : 400;
            AutoSearch.Enabled = false;
            AutoSearch.Enabled = true;
        }

        private void ResetBtn_Click(object sender, EventArgs e)
        {
            SetFilter(DefaultFilter);
            AutoSearch.Enabled = false;
        }



    }
}
