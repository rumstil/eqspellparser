using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Data;
using System.Net;


namespace parser
{
    public enum SpellClasses
    {
        WAR = 1, CLR, PAL, RNG, SHD, DRU, MNK, BRD, ROG, SHM, NEC, WIZ, MAG, ENC, BST, BER
    }

    public enum SpellEffect
    {
        Current_HP = 0,
        AC = 1,
        ATK = 2,
        Movement_Speed = 3,
        STR = 4,
        DEX = 5,
        AGI = 6,
        STA = 7,
        WIS = 8,
        INT = 9,
        CHA = 10,
        Current_Mana = 15,
        Fear = 23,
        Summon_Item = 32,
        Invulnerability = 40,
        Lycanthropy = 44,
        Rune = 55,
        Damage_Shield = 59,
        Assist_Radius = 86,
        Max_HP = 69,
        Max_Mana = 97,
        Percent_Heal = 147,
        Corruption_Counter = 369,
        Corruption_Resist = 370
    }

    public enum SpellSkill
    {
        Melee = -1,
        _1H_Blunt = 0,
        _1H_Slash = 1,
        _2H_Blunt = 2,
        _2H_Slash = 3,
        Abjuration = 4,
        Alteration = 5,
        Apply_Poison = 6,
        Archery = 7,
        Backstab = 8,
        Bind_Wound = 9,
        Bash = 10,
        Block = 11,
        Brass_Instruments = 12,
        Channeling = 13,
        Conjuration = 14,
        Defense = 15,
        Disarm = 16,
        Disarm_Traps = 17,
        Divination = 18,
        Dodge = 19,
        Double_Attack = 20,
        Dragon_Punch = 21,
        Dual_Wield = 22,
        Eagle_Strike = 23,
        Evocation = 24,
        Feign_Death = 25,
        Flying_Kick = 26,
        Forage = 27,
        Hand_to_Hand = 28,
        Hide = 29,
        Kick = 30,
        Meditate = 31,
        Mend = 32,
        Offense = 33,
        Parry = 34,
        Pick_Lock = 35,
        Piercing = 36,
        Riposte = 37,
        Round_Kick = 38,
        Safe_Fall = 39,
        Sense_Heading = 40,
        Singing = 41,
        Sneak = 42,
        Stringed_Instruments = 49,
        Throwing = 51,
        Tiger_Claw = 52,
        Tracking = 53,
        Wind_Instruments = 54,
        Alcohol_Tolerance = 66,
        Percusion_Instruments = 70,
        Berserking = 72,
        Taunt = 73,
        Frenzy = 74
    }

    public enum SpellSkillCap
    {
        STR = 0,
        STA = 1,
        AGI = 2,
        DEX = 3,
        WIS = 4,
        INT = 5,
        CHA = 6,
        Magic_Resist = 7,
        Fire_Resist = 8,
        Cold_Resist = 9,
        Poison_Resist = 10,
        Disease_Resist = 11
    }

    public enum SpellResist
    {
        Unresistable = 0, // only for detrimental spells
        Magic = 1,
        Fire = 2,
        Cold = 3,
        Poison = 4,
        Disease = 5,
        Lowest = 6, // Chromatic/lowest
        Average = 7, // Prismatic/average
        Physical = 8,
        Corruption = 9
    }

    public enum SpellTarget
    {     
        AEPC = 2,
        Group = 3,
        PBAE = 4,
        Single = 5,
        Self = 6,
        Targeted_AE = 8,
        Velious_Giants = 17,
        Velious_Dragons = 18
    }

    public enum SpellIllusion
    {
        Human = 1,
        Barbarian = 2,
        Erudite = 3,
        Wood_Elf = 4,
        High_Elf = 5,
        Dark_Elf = 6,
        Half_Elf = 7,
        Dwarf = 8,
        Troll = 9,
        Ogre = 10,
        Halfling = 11,
        Gnome = 12,
        Aviak = 13,
        Werewolf = 14,
        Brownie = 15,
        Centaur = 16,
        Golem = 17,
        Froglok = 26,
        Froglok_Ghoul = 27,
        Gargoyle = 29,
        Wolf = 42,
        Bear = 43,
        Imp = 46,        
        Elemental = 75,
        Skeleton = 85,
        Zombie = 471
    }

    public class Spell
    {
        public int ID; 
        public string Name; 
        public string Duration; 
        public string[] Slots;
        public int[] Levels;
        public string Skill;
        public string ResistType;
        public int ResistValue;
        public string Extra;
        public string Focus;
        

        public string DebugEffectList;

        public Spell()
        {
            Slots = new string[12];
            Levels = new int[16];
        }

        /// <summary>
        /// Get a full description of the spell. This is mostly useful as a debug dump.
        /// </summary>        
        public List<string> Details()
        {
            List<string> result = new List<string>();
     
            //if (!String.IsNullOrEmpty(Skill))
            //    result.Add("Skill: " + Skill);

            if (!String.IsNullOrEmpty(Duration))
                result.Add("Duration: " + Duration);

            string classes = null;
            for (int i = 0; i < Levels.Length; i++)
                if (Levels[i] != 0 && Levels[i] != 255)
                    classes += " " + (SpellClasses)(i + 1) + "/" + Levels[i];
            if (!String.IsNullOrEmpty(classes))
                result.Add("Classes: " + classes.Trim());

            for (int i = 0; i < Slots.Length; i++)
                if (!String.IsNullOrEmpty(Slots[i]))
                    result.Add(String.Format("{0}: {1}", i + 1, Slots[i]));

            return result;
        }

        public override string ToString()
        {
            return String.Format("[{0}] {1}", ID, Name);
        }
    }


    static class SpellParser
    {
        const int MaxLevel = 85;

        /// <summary>
        /// Download the official spell_us.txt file from the EQ patch server.
        /// http://patch.station.sony.com:7000/patch/everquest/en/patch1/main/spells_us.txt.gz
        /// </summary>
        /// <param name="path"></param>
        public static void DownloadFile(string path)
        {
            using (WebClient web = new WebClient())
            {                
                byte[] data = web.DownloadData("http://patch.station.sony.com:7000/patch/everquest/en/patch1/main/spells_us.txt.gz");
            
                Stream input = new MemoryStream(data);
                GZipStream gzip = new GZipStream(input, CompressionMode.Decompress);
                using (FileStream output = new FileStream(path, FileMode.Create))
                {
                    byte[] buffer = new byte[32768];
                    while (true)
                    {
                        int read = gzip.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                            return;
                        output.Write(buffer, 0, read);
                    }
                }
            }
        }

        /// <summary>
        /// Load spell list from a stream in spell_us.txt format.
        /// </summary>
        public static List<Spell> LoadFromFile(string path)
        {
            List<Spell> list = new List<Spell>();

            using (StreamReader text = File.OpenText(path))
                while (!text.EndOfStream)
                {
                    string line = text.ReadLine();
                    string[] fields = line.Split('^');
                    list.Add(ParseFields(fields));
                }

            return list;
        }

        /// <summary>
        /// Load spell list from a dataset. Column order must match the official spell file.
        /// </summary>
        public static List<Spell> LoadFromData(IDataReader data)
        {
            List<Spell> list = new List<Spell>();

            while (data.Read())
            {
                string[] fields = new string[data.FieldCount];
                for (int i = 0; i < fields.Length; i++)
                    fields[i] = data.GetValue(i).ToString();                
                list.Add(ParseFields(fields));
            }

            return list;
        }

        /// <summary>
        /// Parse a spell from a set of spell fields. 
        /// </summary>        
        static Spell ParseFields(string[] fields)
        {
            Spell spell = new Spell();
            spell.ID = Convert.ToInt32(fields[0]);
            spell.Name = fields[1];
            spell.Skill = TrimEnum((SpellSkill)ParseInt(fields[100]));
            spell.ResistType = TrimEnum((SpellResist)ParseInt(fields[85]));
            spell.ResistValue = ParseInt(fields[147]);
            spell.Duration = ParseDuration(ParseInt(fields[17]), ParseInt(fields[16]));
            spell.Extra = fields[3];

            // each class can have a different level to cast the spell at
            for (int i = 0; i < spell.Levels.Length; i++)
                spell.Levels[i] = ParseInt(fields[104 + i]);

            // each spell has 12 effect slots:
            // 86..97 - slot 1..12 type
            // 20..31 - slot 1..12 base effect
            // 32..43 - slot 1..12 base_2 effect
            // 44..55 - slot 1..12 max effect
            // 70..81 - slot 1..12 calc forumla data
            for (int i = 0; i < spell.Slots.Length; i++)
            {
                spell.Slots[i] = ParseSlot(spell,
                    ParseInt(fields[86 + i]),
                    ParseInt(fields[20 + i]),
                    ParseInt(fields[32 + i]),
                    ParseInt(fields[44 + i]),
                    ParseInt(fields[70 + i]));

                spell.DebugEffectList += ";" + fields[86 + i].ToString();
            }
            spell.DebugEffectList += ";";

            return spell;
        }

        /// <summary>
        /// Parse a spell duration.
        /// </summary>
        /// <returns>A timespan string if the spell has a duration or a null if the spell is instant</returns>
        static string ParseDuration(int duration, int calc)
        {
            // most of the formulas are used to define a lower bound when scaling by level
            // i'm going to ignore those and only show the upper bound
            if (calc == 50)
                duration = 72000;
            if (calc == 3600 && duration == 0)
                duration = 3600;
          
            if (duration > 0 || calc > 0)
                return new TimeSpan(0, 0, duration * 6).ToString();

            return null;
        }

        /// <summary>
        /// Parses a spell effect slot. Each slot has a series of attributes associated with it.
        /// EQEmu has already parsed almost all of these
        /// http://code.google.com/p/projecteqemu/source/browse/trunk/EQEmuServer/zone/spdat.h
        /// 
        /// Effects can reference other spells or items via square bracket notation. e.g.
        /// [123]          is a reference to spell 123
        /// [Group 123]    is a reference to spell group 123
        /// [Item 123]     is a reference to item 123
        /// </summary>
        /// <returns>A description of the slot effect or a null if the slot has no effect.</returns>
        static string ParseSlot(Spell spell, int type, int value, int value2, int max, int calc)
        {
            // type 10 is sometimes used in empty slots
            if (type == 10 && (value == 0 || value > 255))
                return null;
                      
            string repeating = !String.IsNullOrEmpty(spell.Duration) ? " per tick" : null;

            // for some calculations the base value is set at the min level the spell can be cast at
            int minlevel = 255;
            for (int i = 0; i < spell.Levels.Length; i++)
                if (spell.Levels[i] > 0 && spell.Levels[i] < minlevel)
                    minlevel = spell.Levels[i];
            if (minlevel > MaxLevel)
                minlevel = 0;

            // negate calculations if spell effect is negative
            int level = MaxLevel;  
            if (value < 0)
                level = -level;

            // the default calculation (100) leaves the base value as is.
            // all other calculations alter the base value
            // for spell type 32, 109 (summon item) the calc field is abused as a count value
            if (type != 32 && type != 109) 
            switch (calc)
            {
                case 101:
                    value += level / 2;
                    break;
                case 102:
                    value += level;
                    break;
                case 103:
                    value += level * 2;
                    break;
                case 104:
                    value += level * 3;
                    break;
                case 105:
                    value += level * 4;
                    break;
                case 106:
                    value += level * 5;
                    break;
                case 107:
                    value += level / 2;
                    break;
                case 108:
                    value += level / 3;
                    break;
                case 109:
                    value += level / 4;
                    break;
                case 110:
                    value += level / 5;
                    break;
                case 111:
                    value += (level - minlevel) * 6; 
                    break;
                case 112:
                    value += (level - minlevel) * 8;
                    break;
                case 113:
                    value += (level - minlevel) * 10; 
                    break;
                case 114:
                    value += (level - minlevel) * 15;
                    break;
                case 115:
                    value += (level - minlevel) * 6;
                    break;
                case 116:
                    value += (level - minlevel) * 8;
                    break;
                case 117:
                    value += (level - minlevel) * 12;
                    break;
                case 118:
                    value += (level - minlevel) * 20;
                    break;


                default:
                    if (calc > 0 && calc < 100)
                        value = level * calc;
                    break;
            }

            //if (calc == 115) 
            //Console.WriteLine("------  " + spell + " " + String.Format("Eff={0} Val={1} Val2={2} Max={3} Calc={4}", type, value, value2, max, calc));

            if (calc != 100)
            {
                if (max > 0 && value > max)
                    value = max;
                if (max > 0 && value < -max)
                    value = -max;
            }


            // TODO: Protection of the Paw Rk. III - i don't think the block value2 calc is correct
            // TODO: Reprehend - some of the damage is based on target type
            // TODO: Lycanthropy spells - what is the base value for

            switch (type)
            {                
                case 0:
                    // delta hp for heal/nuke, repeating if with duration
                    return FormatCount("Current HP", value) + repeating;
                    //return String.Format("{0} for {1}", value >= 0 ? "Heal" : "Damage", value) + repeating;
                case 1:
                    return FormatCount("AC", (int)(value / (10f / 3f))); 
                case 2: 
                    return FormatCount("ATK", value);
                case 3:
                    return FormatPercent("Movement Speed", value);
                case 4:
                    return FormatCount("STR", value);
                case 5:
                    return FormatCount("DEX", value);
                case 6:
                    return FormatCount("AGI", value);
                case 7:
                    return FormatCount("STA", value);
                case 8:
                    return FormatCount("INT", value);
                case 9:
                    return FormatCount("WIS", value);
                case 10:
                    return FormatCount("CHA", value); 
                case 11:
                    // haste is given as a factor of 100. so 85 = 15% slow, 130 = 30% haste 
                    if (value < 100)
                        value = 100 - value;
                    else
                        value = value - 100;
                    return FormatPercent("Melee Haste", value);
                case 12:
                    return "Invisible (Unstable)";
                case 13:
                    if (value > 1)
                        return "See Invisible (Enhanced)";
                    return "See Invisible";
                case 14:
                    return "Enduring Breath";
                case 15:
                    return FormatCount("Current Mana", value) + repeating;
                case 18:
                    return "Pacify";
                case 19:
                    return FormatCount("Faction", value);  
                case 20:
                    return "Blind";
                case 21:
                    if (max == 0)
                        max = 55;
                    return String.Format("Stun for {0}s up to level {1}", value / 1000f, max);
                case 22:
                    return String.Format("Charm up to level {0}", max);
                case 23:
                    return String.Format("Fear up to level {0}", max);
                case 24:
                    return FormatCount("Stamina Loss", value);
                case 25:
                    return "Bind";
                case 26:
                    return "Gate to Bind";
                case 27:
                    return String.Format("Dispell ({0})", value);
                case 28:
                    return "Invisible to Undead (Unstable)";
                case 29:
                    return "Invisible to Animals (Unstable)";
                case 30:
                    return String.Format("Decrease Aggro Radius up to level {0}", max);
                case 31:
                    return String.Format("Mesmerize up to level {0}", max);
                case 32:
                    return String.Format("Summon: [Item {0}]", value);
                case 33:
                    return String.Format("Summon Pet: {0}", spell.Extra);
                case 35:
                    return FormatCount("Disease Counter", value);
                case 36:
                    return FormatCount("Poison Counter", value);
                case 40:
                    return "Invulnerability";
                case 41:
                    return "Destroy";
                case 42:
                    return "Shadowstep";                
                case 44:
                    return "Lycanthropy";
                    //return String.Format("Lycanthropy Effect: [{0}]", value);
                case 46:
                    return FormatCount("Fire Resist", value);
                case 47:
                    return FormatCount("Cold Resist", value);
                case 48:
                    return FormatCount("Poison Resist", value);
                case 49:
                    return FormatCount("Disease Resist", value);
                case 50:
                    return FormatCount("Magic Resist", value);
                case 52:
                    return "Sense Undead";
                case 53:
                    return "Sense Summoned";
                case 54:
                    return "Sense Animal";
                case 55:
                    return String.Format("Absorb Damage: 100% Total: {0}", value);
                case 56:
                    return "True North";
                case 57:
                    return "Levitate";
                case 58:
                    return String.Format("Illusion: {0}", TrimEnum((SpellIllusion)value));
                case 59:
                    return FormatCount("Damage Shield", -value);
                case 61:
                    return "Identify Item";
                case 63:
                    return String.Format("Memory Blur ({0})", value);               
                case 64:
                    if (max == 0)
                        max = 55;
                    return String.Format("SpinStun for {0}s up to level {1}", value / 1000f, max);
                case 65:
                    return "Infravision";
                case 66:
                    return "Ultravision";
                case 67:
                    return "Eye of Zomm";
                case 68:
                    return "Reclaim Pet";
                case 69:
                    // max hp
                    return FormatCount("Max HP", value);
                case 73:
                    return "Bind Sight";
                case 74:
                    return "Feign Death";
                case 75:
                    return "Voice Graft";
                case 76:
                    return "Sentinel";
                case 77:
                    return "Locate Corpse";
                case 78:
                    // absorbs raw value rather than %. see also 296
                    return FormatCount("Spell Damage Taken", -value);                    
                case 79:
                    // delta hp for heal/nuke, non repeating
                    return FormatCount("Current HP", value);                    
                case 81:
                    return String.Format("Resurrection: {0}%", value); 
                case 82:
                    return "Call of Hero";
                case 83:
                    return String.Format("Teleport to {0}", spell.Extra);
                case 84:
                    return "Gravity Flux";
                case 85:
                    return String.Format("Add Proc: [{0}] RateMod: {1}%", value, value2);
                case 86:
                    return String.Format("Decrease Assist Radius up to level {0}", max);
                case 87:
                    return FormatPercent("Magnification", value);
                case 88:
                    return String.Format("Evacuate to {0}", spell.Extra);
                case 89:
                    return FormatPercent("Player Size", value - 100);
                case 91:
                    return String.Format("Summon Corpse up to level {0}", value);
                case 92:
                    return FormatCount("Hate", value);
                case 93:
                    return "Stop Rain";
                case 94:
                    return "Fade If Combat Initiated";
                case 95:
                    return "Sacrifice";
                case 96:
                    return "Prevent Casting";
                case 97:
                    return FormatCount("Max Mana", value);
                case 98:
                    // yet another super turbo haste. only on 3 bard songs
                    return FormatPercent("Melee Haste v2", value - 100);
                case 99:
                    return "Root";
                case 100:
                    // heal over time
                   return FormatCount("Current HP", value) + repeating;
                case 101:
                    // only castable via Donal's BP. creates a buf that blocks recasting
                    return "Donal's Heal"; 
                case 104:
                    return String.Format("Translocate to {0}", spell.Extra);
                case 105:
                    return "Prevent Gate";
                case 106:
                    return String.Format("Summon Warder: {0}", spell.Extra);
                case 108:
                    return String.Format("Summon Familiar: {0}", spell.Extra);
                case 109:                    
                    return String.Format("Summon: [Item {0}]", value);
                case 111:
                    return FormatCount("All Resists", value);
                case 112:
                    return FormatCount("Effective Casting Level", value);
                case 113:
                    return String.Format("Summon Mount: {0}", spell.Extra);
                case 114:
                    return FormatPercent("Aggro Multiplier", value);
                case 115:
                    return "Feed Hunger";
                case 116:
                    return FormatCount("Curse Counter", value);
                case 119:
                    return FormatPercent("Melee Haste v3", value);
                case 121:
                    return FormatCount("Reverse Damage Shield", -value);
                case 120:
                    // works like healing focus. no idea why it is a separate effect
                    return FormatPercent("Healing Effectiveness", value);
                case 123:
                    return "Screech";
                case 124:
                    return String.Format("{0} Spell Damage by {1}% to {2}%", value >= 0 ? "Increase" : "Decrease", value, value2);                    
                case 125:                    
                    return String.Format("{0} Healing by {1}% to {2}%", value >= 0 ? "Increase" : "Decrease", value, value2);
                case 126:
                    return FormatPercent("Spell Resist Rate", -value);
                case 127:
                    return FormatPercent("Spell Haste", value);
                case 128:
                    return FormatPercent("Spell Duration", value);
                case 129:
                    return FormatPercent("Spell Range", value);
                case 130:
                    return FormatPercent("Spell and Bash Hate", value);
                case 131:
                    //spell.Focus = "Chance of Using Reagent";
                    return FormatPercent("Chance of Using Reagent", -value);
                case 132:
                    return FormatPercent("Spell Mana Cost", -value2);
                case 134:
                    return String.Format("Limit: Max Level: {0} (lose {1}% per level)", value, value2);
                case 135:
                    return String.Format("Limit: Resist: {0}", (SpellResist)value);
                case 136:
                    return String.Format("Limit: {1} Target: {0}", (SpellTarget)Math.Abs(value), value >= 0 ? "Include" : "Exclude");
                case 137:
                    return String.Format("Limit: {1} Effect: {0}", TrimEnum((SpellEffect)Math.Abs(value)), value >= 0 ? "Include" : "Exclude");
                case 138:
                    return String.Format("Limit: Type: {0}", value == 0 ? "Detrimental" : "Beneficial");
                case 139:
                    return String.Format("Limit: {1} Spell: [{0}]", Math.Abs(value), value >= 0 ? "Include" : "Exclude");
                case 140:
                    return String.Format("Limit: Min Duration: {0}s", value * 6);
                case 141:
                    return "Limit: Instant Spells";
                case 142:
                    return String.Format("Limit: Min Level: {0}", value);
                case 143:
                    return String.Format("Limit: Min Casting Time: {0}s", value / 1000f);
                case 144:
                    return String.Format("Limit: Max Casting Time: {0}s", value / 1000f);
                case 145:
                    return String.Format("Teleport to {0}", spell.Extra);
                case 147:
                    return String.Format("Increase Current HP by {1} Max: {0}% ", value, max);
                case 148:
                    return String.Format("Block new spell if slot {0} is '{1}' and < {2}", calc % 100, (SpellEffect)value, max);
                case 149:                    
                    return String.Format("Overwrite existing spell if slot {0} is '{1}' and < {2}", calc % 100, (SpellEffect)value, max);
                case 150:
                    return "Divine Intervention";
                case 151:
                    return "Suspend Pet";
                case 152:
                    return String.Format("Summon Pet: {0} x {1} for {2}s", spell.Extra, value, max);
                case 153:
                    return String.Format("Balance Group HP ({0}% penalty)", value);                       
                case 154:
                    return String.Format("Remove Detrimental ({0})", value);
                case 157:
                    return FormatCount("Spell Damage Shield", -value);
                case 161:
                    return String.Format("Absorb Spell Damage: {0}% Total: {1}", value, max);
                case 162:
                    return String.Format("Absorb Melee Damage: {0}% Total: {1}", value, max);
                case 163:
                    return "Pet Invulnerability";
                case 164:
                    return "Appraise Chest";
                case 165:
                    return "Disarm Chest";
                case 166:
                    return "Unlock Chest";
                case 167:
                    return String.Format("Pet Power ({0})", value);
                case 168:
                    // how is this different than an endless rune?
                    return FormatPercent("Melee Mitigation", value);
                case 169:
                    return FormatPercent("Chance to Critical Hit with " + TrimEnum((SpellSkill)value2), value);
                case 172:
                    return FormatPercent("Chance to Avoid Melee", value);
                case 173:
                    return FormatPercent("Chance to Riposte", value);
                case 174:
                    return FormatPercent("Chance to Dodge", value);
                case 175:
                    return FormatPercent("Chance to Parry", value);
                case 176:
                    return FormatPercent("Chance to Dual Wield", value);
                case 177:
                    return FormatPercent("Chance to Double Attack", value);
                case 178:
                    return String.Format("Lifetap from Weapon Damage: {0}%", value);
                case 182:
                    return FormatPercent("Weapon Delay", value);
                case 183:
                    return FormatPercent("Skill Check for " + TrimEnum((SpellSkill)value2), value);
                case 184:
                    return FormatPercent("Chance to Hit with " + TrimEnum((SpellSkill)value2), value);
                case 185:
                    return FormatPercent("Damage Modifier for " + TrimEnum((SpellSkill)value2), value);
                case 186:
                    return FormatPercent("Min Damage Modifier for " + TrimEnum((SpellSkill)value2), value);
                case 188:
                    return FormatPercent("Chance to Block", value);
                case 189:
                    return FormatCount("Current Endurance", value);
                case 190:
                    return FormatCount("Max Endurance", value);
                case 191:
                    return "Prevent Combat";
                case 193:
                    return String.Format("{0} Damage Attack for {1}", spell.Skill, value);
                case 197:
                    return FormatPercent(TrimEnum((SpellSkill)value2) + " Damage Taken", value);
                case 200:
                    return FormatPercent("Proc Rate", value);
                case 201:
                    return String.Format("Add Range Proc: [{0}] RateMod: {1}%", value, value2);
                case 206:
                    return "AE Taunt";
                case 209:
                    return String.Format("Dispell Beneficial ({0})", value);
                case 214:
                    return FormatPercent("Max HP", value);
                case 216:
                    return FormatPercent("Accuracy", value);
                case 220:                    
                    return FormatCount(TrimEnum((SpellSkill)value2) + " Damage", value);
                case 227:
                    return String.Format("Reduce {0} Timer by {1}s", TrimEnum((SpellSkill)value2), value);

                case 254:
                    // 254 is used for unused slots
                    return null;                    

                case 258:
                    return String.Format("Triple Backstab ({0})", value);
                case 262:
                    return FormatCount((SpellSkillCap)value2 + " Cap", value);
                case 272:
                    return FormatPercent("Spell Casting Skill", value);
                case 273:
                    return FormatPercent("Chance to Critical DoT", value);
                case 274:
                    return FormatPercent("Chance to Critical Heal", value);
                case 286:
                    // similar to damage focus, but adds a raw amount
                    return FormatCount("Spell Damage", value);
                case 289: 
                    // how is this different than 373?
                    return String.Format("Cast on Fade: [{0}]", value);
                case 291:
                    return String.Format("Purify ({0})", value);
                case 294:
                    if (value > 0 && value2 > 0)
                        return FormatPercent("Chance to Critical Nuke", value) + " and " + FormatPercent("Critical Nuke Damage", value2 - 100);
                    else if (value > 0)
                        return FormatPercent("Chance to Critical Nuke", value);
                    else                    
                        return FormatPercent("Critical Nuke Damage", value2 - 100);
                case 296:
                    return FormatPercent("Spell Damage Taken", value);
                case 300:
                    return "Doppelganger";
                case 310:
                    return String.Format("Reduce Timer by {0}s", value / 1000);
                case 311:
                    return "Limit: Exclude Combat Skills";
                case 314:
                    return "Invisible (Permanent)";
                case 315:
                    return "Invisible to Undead (Permanent)";
                case 319:
                    return FormatPercent("Chance to Critical HoT", value);
                case 322:
                    return "Gate to Origin";
                case 323:
                    return String.Format("Add Defensive Proc: [{0}] RateMod: {1}%", value, value2);
                case 329:
                    return String.Format("Absorb Damage Using Mana: {0}%", value);
                case 330:
                    return FormatPercent("Critical Damage Modifier for " + TrimEnum((SpellSkill)value2), value);
                case 333:
                    return String.Format("Cast on Fade/Cancel: [{0}]", value);
                case 335:
                    return "Prevent Spell Matching Limits from Landing";
                case 337:
                    return FormatPercent("Experience Gain", value);
                case 339:
                    return String.Format("Add Spell Proc: [{0}] Chance: {1}%", value2, value);
                case 340:
                    return String.Format("Cast: [{0}] Chance: {1}%", value2, value); 
                case 348:
                    return String.Format("Limit: Mana Cost > {0}", value); 
                case 351:
                    return String.Format("Aura Effect: [{0}]", spell.ID + value);
                case 360:
                    return String.Format("Add Killshot Proc: [{0}] Chance: {1}%", value2, value); 
                case 369:
                    return FormatCount("Corruption Counter", value);
                case 370:
                    return FormatCount("Corruption Resist", value);
                case 373:
                    return String.Format("Cast on Fade: [{0}]", value);
                case 374:       
                    // i think this is used when several effects need to be placed in a slot. 
                    // i.e. multiple spells are needed but a single cast is required                    
                    return String.Format("Cast: [{0}]", value2); 
                case 385:
                    return String.Format("Limit: Include Spells: [Group {0}]", value); 
                case 392:
                    // similar to heal focus, but adds a raw amount
                    return FormatCount("Healing", value);
                case 406:
                    return String.Format("Cast if Attacked: [{0}]", value); 


            }

            return String.Format("Unknown Effect: {0} Val={1} Val2={2} Max={3} Calc={4}", type, value, value2, max, calc);
        }

        static int ParseInt(string s)
        {
            if (String.IsNullOrEmpty(s))
                return 0;
            return (int)Single.Parse(s);
        }

        static string TrimEnum(object o)
        {
            return o.ToString().Replace("_", " ").Trim();
        }

        static string FormatCount(string name, int count)
        {
            if (count >= 0)
                return String.Format("Increase {0} by {1}", name, count);
            else
                return String.Format("Decrease {0} by {1}", name, -count);
        }

        static string FormatPercent(string name, int count)
        {
            return FormatCount(name, count) + "%";
        }



    }


}
