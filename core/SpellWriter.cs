using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EQSpellParser
{
    static public class SpellWriter
    {
        /// <summary>
        /// Print spells in text format.
        /// </summary>
        static public void ToText(IEnumerable<Spell> list, Action<string> write)
        {
            foreach (var spell in list)
            {
                // spell.ToString() writes ID and Name
                write(spell.ToString()); 
                foreach (var note in spell.Details())
                    write(note);
                if (!String.IsNullOrEmpty(spell.Desc))
                    write(spell.Desc);
                write("");
            }
        }

        /// <summary>
        /// Print spells in CSV format.
        /// This doesn't include all fields but it easy enough to add whatever else you need in here.
        /// </summary>
        static public void ToCsv(IEnumerable<Spell> list, Action<string> write)
        {
            // write header
            var fields = new List<object>();
            fields.Add("id");
            fields.Add("group");
            fields.Add("icon");
            fields.Add("name");
            fields.Add("skill");
            fields.Add("mana");
            fields.Add("end");
            fields.Add("end_upkeep");
            fields.Add("cast_time");
            fields.Add("recast_time");
            fields.Add("target");
            fields.Add("resist_type");
            fields.Add("resist_mod");
            fields.Add("range");
            fields.Add("ae_range");
            fields.Add("duration");
            fields.Add("hate_mod");
            fields.Add("hate_given");
            fields.Add("push");
            fields.Add("classes");
            // war, clr, pal, rng, shd, dru, mnk, brd, rog, shm, nec, wiz, mag, enc, bst, ber
            for (var i = 1; i <= 16; i++)
                fields.Add(((SpellClasses)i).ToString().ToLower());
            fields.Add("slots");

            write(String.Join(",", fields.Select(x => '"' + x.ToString() + '"').ToArray()));

            foreach (var spell in list)
            {
                fields = new List<object>();
                fields.Add(spell.ID);
                fields.Add(spell.GroupID);
                fields.Add(spell.Icon);
                fields.Add(spell.Name);
                fields.Add(spell.Skill.ToString().Replace('_', ' '));
                fields.Add(spell.Mana);
                fields.Add(spell.Endurance);
                fields.Add(spell.EnduranceUpkeep);
                fields.Add(spell.CastingTime.ToString("F2"));
                fields.Add(spell.RecastTime.ToString("F2"));
                fields.Add(spell.Target);
                fields.Add(spell.ResistType);
                fields.Add(spell.ResistMod);
                fields.Add(spell.Range);
                fields.Add(spell.AERange);
                fields.Add(spell.DurationTicks);
                fields.Add(spell.HateMod);
                fields.Add(spell.HateOverride);
                fields.Add(spell.PushBack);
                fields.Add(spell.ClassesLevels);

                // levels for each class (16 fields)
                for (var i = 0; i < spell.Levels.Length; i++)
                    fields.Add(spell.Levels[i]);

                // encode slots as variable length "|" delimited list
                var slots = new List<string>();
                if (spell.Recourse != null)
                    slots.Add("Recourse: Cast " + spell.Recourse);
                for (int i = 0; i < spell.Slots.Count; i++)
                    if (spell.Slots[i] != null)
                        slots.Add(String.Format("{0}: {1}", i + 1, spell.Slots[i].Desc));
                fields.Add(String.Join("|", slots.ToArray()));

                write(String.Join(",", fields.Select(x => '"' + x.ToString() + '"').ToArray()));
            }

        }

        /// <summary>
        /// Print spells in JSON format.
        /// </summary>
        //static public void ToJson(IEnumerable<Spell> list, Action<string> write)
        //{
        //    var serializer = new DataContractJsonSerializer(typeof(Spell));
        //}
    }
}
