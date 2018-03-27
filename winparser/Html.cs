using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EQSpellParser;


namespace winparser
{
    public class HtmlBuilder
    {
        private readonly SpellCache Cache;
        public readonly StringBuilder Html;

        public static StringBuilder InitTemplate()
        {
            // 1.5MB is about enough to fit the largest search results
            var html = new StringBuilder(1500000);
            html.AppendLine(winparser.Properties.Resources.HtmlTemplate);
            return html;
        }

        public HtmlBuilder(SpellCache cache)
        {
            Cache = cache;
            Html = InitTemplate();
        }

        public new string ToString()
        {
            Html.Append("</html>");
            return Html.ToString();
        }

        public void ShowAsText(IEnumerable<Spell> list, Func<Spell, bool> visible)
        {
            foreach (var spell in list)
            {
                Html.AppendFormat("<p id='spell{0}' class='spell group{1} {3}'><strong>{2}</strong><br/>", spell.ID, spell.GroupID, spell.ToString(), visible(spell) ? "" : "hidden");
                
                foreach (var line in spell.Details())
                {
                    var slot = Regex.Replace(line, @"(\d+): .*", m =>
                    {
                        int i = Int32.Parse(m.Groups[1].Value) - 1;
                        if (i < 0 || i >= spell.Slots.Count || spell.Slots[i] == null)
                            return "Unknown Index " + i;
                        return String.Format("{0}: <span title=\"SPA={2} Base1={3} Base2={4} Max={5} Calc={6}\">{1}</span>", i + 1, spell.Slots[i].Desc, spell.Slots[i].SPA, spell.Slots[i].Base1, spell.Slots[i].Base2, spell.Slots[i].Max, spell.Slots[i].Calc);
                    });

                    Html.Append(InsertRefLinks(slot));
                    Html.Append("<br/>");
                }

                if (spell.Desc != null)
                    Html.Append(spell.Desc);

                Html.Append("</p>");
            }
        }

        public void ShowAsTable(IEnumerable<Spell> list, Func<Spell, bool> visible)
        {
            Html.Append("<table style='table-layout: fixed;'>");
            Html.Append("<thead><tr>");
            Html.Append("<th style='width: 4em;'>ID</th>");
            Html.Append("<th style='width: 18em;'>Name</th>");
            Html.Append("<th style='width: 10em;'>Level</th>");
            Html.Append("<th style='width: 4em;'>Mana</th>");
            Html.Append("<th style='width: 4em;'>Cast</th>");
            Html.Append("<th style='width: 4em;'>Recast</th>");
            Html.Append("<th style='width: 4em;'>Duration</th>");
            Html.Append("<th style='width: 6em;'>Resist</th>");
            Html.Append("<th style='width: 5em;'>Target</th>");
            Html.Append("<th style='min-width: 30em;'>Effects</th>");
            Html.Append("</tr></thead>");

            foreach (var spell in list)
            {
                Html.AppendFormat("<tr id='spell{0}' class='spell group{1} {2}'><td>{0}</td>", spell.ID, spell.GroupID, visible(spell) ? "" : "hidden");
                //Html.AppendFormat("<tr id='spell{0}' class='group{1}'><td>{0}{2}</td>", spell.ID, spell.GroupID, spell.GroupID > 0 ? " / " + spell.GroupID : "");

                Html.AppendFormat("<td>{0}</td>", spell.Name);

                Html.AppendFormat("<td style='max-width: 12em'>{0}</td>", spell.ClassesLevels);

                if (spell.Endurance == 0 && spell.EnduranceUpkeep > 0)
                    Html.AppendFormat("<td class='end'>{0}/tick</td>", spell.EnduranceUpkeep);
                else if (spell.Endurance > 0 && spell.EnduranceUpkeep > 0)
                    Html.AppendFormat("<td class='end'>{0} + {1}/tick</td>", spell.Endurance, spell.EnduranceUpkeep);
                else if (spell.Endurance > 0)
                    Html.AppendFormat("<td class='end'>{0}</td>", spell.Endurance);
                else
                    Html.AppendFormat("<td class='mana'>{0}</td>", spell.Mana);

                Html.AppendFormat("<td>{0}s</td>", spell.CastingTime);

                Html.AppendFormat("<td>{0} {1}</td>", Spell.FormatTime(spell.RecastTime), spell.RecastTime > 0 && spell.TimerID > 0 ? " T" + spell.TimerID : "");

                Html.AppendFormat("<td>{0}{1}</td>", Spell.FormatTime(spell.DurationTicks * 6), spell.DurationTicks > 0 && spell.Focusable ? "+" : "");

                if (!spell.Beneficial)
                    Html.AppendFormat("<td>{0} {1}</td>", Spell.FormatEnum(spell.ResistType), spell.ResistMod != 0 ? spell.ResistMod.ToString() : "");
                else
                    Html.Append("<td class='note'>n/a</td>");

                Html.AppendFormat("<td>{0} {1} {2}</td>", FormatEnum(spell.Target), spell.MaxTargets > 0 ? " (" + spell.MaxTargets + ")" : "", spell.ViralRange > 0 ? " + Viral" : "");

                Html.Append("<td>");

                if (spell.Stacking != null && spell.Stacking.Count > 0)
                    Html.AppendFormat("Stacking: {0}<br/>", String.Join(", ", spell.Stacking.ToArray()));

                if (spell.MaxHits > 0)
                    Html.AppendFormat("Max Hits: {0} {1}<br/>", spell.MaxHits, FormatEnum(spell.MaxHitsType));

                for (int i = 0; i < spell.ConsumeItemID.Length; i++)
                    if (spell.ConsumeItemID[i] > 0)
                        Html.AppendFormat("Consumes: {0} x {1}<br/>", InsertRefLinks(String.Format("[Item {0}]", spell.ConsumeItemID[i])), spell.ConsumeItemCount[i]);
                
                if (spell.HateOverride != 0)
                    Html.AppendFormat("Hate: {0}<br/>", spell.HateOverride);

                if (spell.HateMod != 0)
                    Html.AppendFormat("Hate Mod: {0:+#;-#;0}<br/>", spell.HateMod);

                if (spell.PushBack != 0)
                    Html.AppendFormat("Push: {0}<br/>", spell.PushBack);

                if (spell.RestTime > 1.5)
                    Html.AppendFormat("Rest: {0}s<br/>", spell.RestTime.ToString());

                if (spell.RecourseID != 0)
                    Html.AppendFormat("Recourse: {0}<br/>", InsertRefLinks(String.Format("[Spell {0}]", spell.RecourseID)));

                if (spell.AEDuration >= 2500)
                    Html.AppendFormat("AE Waves: {0}<br/>", spell.AEDuration / 2500);

                for (int i = 0; i < spell.Slots.Count; i++)
                    if (spell.Slots[i] != null)
                        Html.AppendFormat("{0}: <span title=\"SPA={2} Base1={3} Base2={4} Max={5} Calc={6}\">{1}</span><br/>", i + 1, InsertRefLinks(spell.Slots[i].Desc), spell.Slots[i].SPA, spell.Slots[i].Base1, spell.Slots[i].Base2, spell.Slots[i].Max, spell.Slots[i].Calc);

                Html.Append("</td>");

                Html.Append("</tr>");
                Html.AppendLine();
            }

            Html.Append("</table>");
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
                //return String.Format("<a href='http://everquest.allakhazam.com/db/item.Html?item={0};source=lucy' class='ext' target='_top'>{1}</a>", id, name);
                return String.Format("<a href='http://lucy.allakhazam.com/item.Html?id={0}' class='ext' target='_top'>{1}</a>", id, name);
            });

            return text;
        }

        private string HtmlEncode(string text)
        {
            // i don't think .net has a html encoder outside of the system.web assembly
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private string FormatEnum(object o)
        {
            var type = o.ToString().Replace("_", " ").Trim();

            if (Regex.IsMatch(type, @"^-?\d+$"))
                // undefined numeric enum
                type = "Type " + type; 
            else
                // remove numeric suffix on duplicate enums undead3/summoned3/etc
                type = Regex.Replace(type, @"\d+$", "");

            return type;
        }

    }
}
