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

    [ComVisible(true)]
    public partial class MainForm : Form
    {
        //static string SpellFilename = "spells_us.txt";
        //static string DescFilename = "dbstr_us.txt";

        static Dictionary<string, string> Effects;

        static List<Spell> Spells;
        static Dictionary<int, Spell> SpellsById;


        public MainForm()
        {
            InitializeComponent();            

            SearchClass.Items.AddRange(Enum.GetNames(typeof(SpellClassesLong)));

            Effects = new Dictionary<string, string>();
            Effects.Add("Heal", @"Increase Current HP");
            Effects.Add("Cure", @"Decrease \w+ Counter");
            Effects.Add("Nuke", @"Decrease Current HP");
            Effects.Add("DoT", @"Decrease Current HP by \d+ per tick");
            Effects.Add("Haste", @"Increase Melee Haste");
            Effects.Add("Slow", @"Decrease Melee Haste");
            Effects.Add("Snare", @"Decrease Movement Speed");
            Effects.Add("Shrink", @"Decrease Player Size");
            Effects.Add("Rune", "@Absorb");
            Effects.Add("Pacify", @"Decrease Social Radius");
            // literal text suggestions
            SearchEffect.Items.Add("Mesmerize");
            SearchEffect.Items.Add("Memory Blur");            
            SearchEffect.Items.AddRange(Effects.Keys.ToArray());

            //SearchBrowser.ObjectForScripting = this;

        }

        public new void Load(string spellPath, string descPath)
        {
            Text += " - " + spellPath;

            Cursor.Current = Cursors.WaitCursor;

            Spells = SpellParser.LoadFromFile(spellPath, descPath).ToList();
            SpellsById = Spells.ToDictionary(x => x.ID, x => x);

            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Search spell database based on form filter settings
        /// </summary>
        private void Search()
        {
            var query = Spells.AsQueryable();

            // exclude spammy breath of AA
            query = query.Where(x => !x.Name.StartsWith("Breath of"));

            var text = SearchText.Text;
            int id;
            if (Int32.TryParse(text, out id))
                query = query.Where(x => x.ID == id);
            else if (!String.IsNullOrEmpty(text))
                query = query.Where(x => x.ID.ToString() == text || x.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0 || x.Desc != null && x.Desc.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0);

            var cls = SearchClass.Text;
            if (!String.IsNullOrEmpty(cls))
            {
                int i = (int)Enum.Parse(typeof(SpellClassesLong), cls) - 1;
                if (i >= 0)
                    query = query.Where(x => x.Levels[i] > 0 && x.Levels[i] < 255);
            }

            var effect = SearchEffect.Text;
            if (!String.IsNullOrEmpty(effect))
            {
                if (Effects.ContainsKey(effect) && Effects[effect] != effect)
                    query = query.Where(x => x.HasEffectRegex(Effects[effect]) >= 0);
                else                    
                    query = query.Where(x => x.HasEffect(effect) >= 0);
            }


            // limit results to 2000 spells to keep the webbrowser responsive
            var list = query.Take(2000).ToList();
            Expand(list);
            SearchNotes.Text = String.Format("{0} results", list.Count);

            if (DisplayTable.Checked)
                ShowAsTable(list);
            else
                ShowAsText(list);
        }

        /// <summary>
        /// Recursively expand the spell list to include referenced spells.
        /// </summary>                
        static void Expand(IList<Spell> list)
        {
            // keep a hash based index of existing results to avoid doing a linear search on results
            // when checking if a spell is already included
            HashSet<int> included = new HashSet<int>();
            foreach (Spell spell in list)
                included.Add(spell.ID);

            Func<string, string> expand = text => Spell.SpellRefExpr.Replace(text, delegate(Match m)
            {
                Spell spellref;
                if (SpellsById.TryGetValue(Int32.Parse(m.Groups[1].Value), out spellref))
                {
                    if (!included.Contains(spellref.ID))
                    {
                        included.Add(spellref.ID);
                        list.Add(spellref);
                    }
                    //return spellref.Name;
                    return String.Format("{1} [Spell {0}]", spellref.ID, spellref.Name);
                }
                return m.Groups[0].Value;
            });


            // scan each spell in the queue for spell references. if a new reference is found
            // then add it to the queue so that it can also be checked
            int i = 0;
            while (i < list.Count)
            {
                Spell spell = list[i++];

                if (spell.Recourse != null)
                    spell.Recourse = expand(spell.Recourse);

                // check effects slots for the [Spell 1234] references
                for (int j = 0; j < spell.Slots.Length; j++)
                    if (spell.Slots[j] != null)
                        spell.Slots[j] = expand(spell.Slots[j]);
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

        private void ShowAsText(IList<Spell> list)
        {
            var html = InitHtml();

            foreach (var spell in list)
            {
                html.AppendFormat("<p id='spell{0}'><strong>{1}</strong><br/>", spell.ID, spell.ToString());
                foreach (var line in spell.Details())
                    html.Append(GetSpellLink(line) + "<br/>");
                html.Append("</p>");
            }

            html.Append("</html>");
            ShowHtml(html);
        }

        private void ShowAsTable(IList<Spell> list)
        {
            var html = InitHtml();

            html.Append("<table>");
            html.Append("<tr><th>ID</th><th>Name</th><th>Classes</th><th>Mana</th><th>Cast</th><th>Recast</th><th>Duration</th><th>Resist</th><th>Target</th><th>Effects</th></tr>");

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

                html.AppendFormat("<td>{0}</td>", spell.DurationTicks);

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
            html.Append("</html>");
            ShowHtml(html);
        }

        public string GetSpellInfo(int id)
        {
            Spell spell;
            if (SpellsById.TryGetValue(id, out spell))
                return String.Join("<br/>", spell.Details());
            return "Not Found";
        }

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
            // no &amp; encoding. i don't think .net has a html encoder outside of the system.web assembly
            return text.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private string FormatTime(float seconds)
        {
            if (seconds < 120)
                return seconds.ToString("0.#") + "s";

            if (seconds < 7200)
                return (seconds / 60f).ToString("0.#") + "m";

            return new TimeSpan(0, 0, (int)seconds).ToString();
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
            Application.Exit();
        }

        private void SearchBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            //Text = e.Url.ToString();
        }



    }
}
