using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace Everquest
{
    #region Enums

    public enum SpellClasses
    {
        WAR = 1, CLR, PAL, RNG, SHD, DRU, MNK, BRD, ROG, SHM, NEC, WIZ, MAG, ENC, BST, BER
    }

    public enum SpellClassesLong
    {
        Warrior = 1, Cleric, Paladin, Ranger, ShadowKnight, Druid, Monk, Bard, Rogue, Shaman,
        Necro, Wizard, Mage, Enchanter, Beastlord, Berserker
    }

    [Flags]
    public enum SpellClassesMask
    {
        WAR = 1, CLR = 2, PAL = 4, RNG = 8, SHD = 16, DRU = 32, MNK = 64, BRD = 128, ROG = 256,
        SHM = 512, NEC = 1024, WIZ = 2048, MAG = 4096, ENC = 8192, BST = 16384, BER = 32768,
        ALL = 65535
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
        Melee_Haste = 11,
        Unstable_Invis = 12,
        See_Invis = 13,
        Enduring_Breath = 14,
        Current_Mana_Repeating = 15,
        Pacify = 18,
        Blind = 20,
        Stun = 21,
        Charm = 22,
        Fear = 23,
        Bind = 25,
        Gate = 26,
        Dispel = 27,
        Mesmerize = 31,
        Summon_Item = 32,
        Summon_Pet = 33,
        Disease_Counter = 35,
        Poison_Counter = 36,
        Twincast_Blocker = 39,
        Invulnerability = 40,
        Shadowstep = 42,
        Delayed_Heal_Marker = 44,
        Fire_Resist = 46,
        Cold_Resist = 47,
        Poison_Resist = 48,
        Disease_Resist = 49,
        Magic_Resist = 50,
        Rune = 55,
        Levitate = 57,
        Illusion = 58,
        Damage_Shield = 59,
        Identify = 61,
        Memory_Blur = 63,
        Stun_Spin = 64,
        Summon_Skeleton_Pet = 71,
        Feign_Death = 74,
        Current_HP_Non_Repeating = 79,
        Resurrect = 81,
        Summon_Player = 82,
        Teleport = 83,
        Melee_Proc = 85,
        Assist_Radius = 86,
        Evacuate = 88,
        Max_HP = 69,
        Summon_Corpse = 91,
        Hate = 92,
        Silence = 96,
        Max_Mana = 97,
        Melee_Haste_v2 = 98,
        Root = 99,
        Current_HP_Repeating = 100,
        Donals_Heal = 101,
        Translocate = 104,
        All_Resists = 111,
        Summon_Mount = 113,
        Hate_Mod = 114,
        Curse_Counter = 116,
        Melee_Haste_v3 = 119,
        Spell_Damage_Focus = 124,
        Healing_Focus = 125,
        Haste_Focus = 127,
        Duration_Focus = 128,
        Hate_Mod_Spells = 130,
        Mana_Cost_Focus = 132,
        Current_HP_Percent = 147,
        Stacking_Blocker = 148,
        Divine_Intervention_With_Heal = 150,
        Suspend_Pet = 151,
        Summon_Swarm_Pet = 152,
        Dispel_Detrimental = 154,
        Reflect_Spell = 158,
        Spell_Rune = 161,
        Melee_Rune = 162,
        Absorb_Hits = 163,
        Melee_Mitigation = 168,
        Critical_Hit_Chance = 169,
        Critical_Nuke_Damage = 170,
        Crippling_Blow_Chance = 171,
        Avoid_Melee_Chance = 172,
        Riposte_Chance = 173,
        Dodge_Chance = 174,
        Parry_Chance = 175,
        Lifetap_From_Weapon = 178,
        Spell_Resist_Chance = 180,
        Weapon_Delay = 182,
        Hit_Chance = 184,
        Hit_Damage = 185,
        Min_Hit_Damage = 186,
        Block_Chance = 188,
        Endurance_Repeating = 189,
        Hate_Repeating = 192,
        Skill_Attack = 193,
        Cancel_All_Aggro = 194,
        Stun_Resist_Chance = 195,
        Taunt = 199,
        Proc_Rate = 200,
        Rampage = 205,
        AE_Taunt = 206,
        AE_Attack = 211,
        Frenzied_Devastation = 212,
        Slay_Undead = 219,
        Weapon_Damage_Bonus = 220,
        Back_Block_Chance = 222,
        Double_Riposte_Skill = 223,
        Additional_Riposte_Skill = 224,
        Double_Attack_Skill = 225,
        Persistent_Casting_AA = 229, // cast through stun
        Divine_Intervention = 232,
        Lung_Capacity = 246,
        Frontal_Backstab_Chance = 252,
        Frontal_Backstab_Min_Damage = 253,
        Shroud_Of_Stealth = 256,
        Triple_Backstab_Chance = 258,
        Combat_Stability = 259, // ac soft cap. AA and a few shaman spells
        No_Fizzle = 265,
        Song_Range = 270,
        Innate_Movement_Speed = 271, // AA
        Flurry = 279,
        Critical_DoT_Chance = 273,
        Critical_Heal_Chance = 274,
        Double_Special_Attack_Chance = 283, // monk specials
        Spell_Damage_Bonus = 286,
        Dragon_Punch_Knockback = 288,
        Movement_Speed_Cap = 290,
        Purify = 291,
        Frontal_Stun_Resist_Chance = 293, // AA
        Critical_Nuke_Chance = 294,
        Spell_Damage_Taken = 296,
        Ranged_Damage = 301,
        Base_Spell_Damage = 303,
        Avoid_Riposte_Chance = 304,
        Damage_Shield_Taken = 305,
        Teleport_To_Bind = 309,
        Invis = 314,
        Shield_Block = 320,
        Targets_Target_Hate = 321,
        Gate_to_Home_City = 322,
        Defensive_Proc = 323,
        Blood_Magic = 324,
        Crit_Hit_Damage = 330,
        Summon_To_Corpse = 332,
        Block_Matching_Spell = 335,
        XP_Gain_Mod = 337,
        Spell_Proc = 339,
        Interrupt_Casting = 343,
        Shield_Equip_Hate_Mod = 349, // AA
        Mana_Burn = 350,
        Summon_Aura = 351,
        Aura_Slots = 353,
        Silence_With_Limits = 357,
        Current_Mana = 358,
        Cast_On_Death = 361,
        Triple_Attack = 364,
        Cast_On_Killshot = 365,
        Group_Shielding = 366,
        Corruption_Counter = 369,
        Corruption_Resist = 370,
        Melee_Delay = 371,
        Foraging_Skill = 372,
        Cast_Always_On_Fade = 373,
        Cast_With_Chance = 374,
        Crit_DoT_Damage = 375,
        Fling = 376,
        Cast_If_Not_Cured = 377,
        Resist_Other_Effect = 378,
        Directional_Shadowstep = 379,
        PushBackUp = 380,
        Fling_To_Self = 381,
        Inhibit_Effect = 382,
        Cast_On_Spell = 383,
        Fling_To_Target = 384,
        Limit_Group = 385,
        Cast_On_Curer = 386,
        Cast_On_Cured = 387,
        Summon_All_Corpses = 388,
        Reset_Recast_Timer = 389,
        Lockout_Recast_Timer = 390,
        Limit_Max_Mana = 391,
        Healing_Bonus = 392,
        Healing_Taken_Pct = 393,
        Healing_Taken = 394,
        Crit_Incoming_Heal_Chance = 395,
        Base_Healing = 396,
        Pet_Melee_Mitigation = 397,
        Pet_Duration = 398,
        Twincast_Chance = 399,
        Heal_From_Mana = 400,
        Ignite_Mana = 401,
        Ignite_Endurance = 402,
        Limit_Spell_Class = 403,
        Limit_Spell_Subclass = 404,
        Staff_Block_Chance = 405,
        Cast_On_Max_Hits = 406,
        Cap_HP = 408,
        Cap_Mana = 409,
        Cap_End = 410,
        Limit_Player_Class = 411,
        Limit_Race = 412,
        Song_Effectiveness = 413,
        Limit_Casting_Skill = 414,
        Limit_Item_Class = 415,
        AC_v2 = 416,
        Current_Mana_Repeating_v2 = 417,
        Weapon_Damage_Bonus_v2 = 418,
        Max_Hits_Counter = 421,
        Limit_Max_Hits_Min = 422,
        Limit_Max_Hits_Type = 423,
        Gravitate = 424,
        Fly = 425,
        Teleport_to_Caster_Anchor = 437,
        Teleport_to_Player_Anchor = 438,
        Lock_Aggro = 444,
        Buff_Blocker_A = 446,
        Buff_Blocker_B = 447,
        Buff_Blocker_C = 448,
        Buff_Blocker_D = 449,
        Absorb_DoT_Damage = 450,
        Absorb_Melee_Damage = 451,
        Absorb_Spell_Damage = 452,
        Resource_Tap = 457,
        Faction_Hit = 458,
        Hit_Damage_v2 = 459,
        Repeat_Melee_Round_Chance = 471,
        Pet_Crit_Hit_Damage = 474
    }

    public enum SpellSkill
    {
        Hit = -1, // weapons/archery/backstab/frenzy/kick/etc..
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
        Brass = 12,
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
        _1H_Pierce = 36,
        Riposte = 37,
        Round_Kick = 38,
        Safe_Fall = 39,
        Sense_Heading = 40,
        Singing = 41,
        Sneak = 42,
        Specialize_Abjure = 43,
        Specialize_Alteration = 44,
        Specialize_Conjuration = 45,
        Specialize_Divination = 46,
        Specialize_Evocation = 47,
        Pick_Pockets = 48,
        Stringed = 49,
        Swimming = 50,
        Throwing = 51,
        Tiger_Claw = 52,
        Tracking = 53,
        Wind = 54,
        Fishing = 55,
        Poison_Making = 56,
        Tinkering = 57,
        Research = 58,
        Alchemy = 59,
        Baking = 60,
        Tailoring = 61,
        Sense_Traps = 62,
        Blacksmithing = 63,
        Fletching = 64,
        Brewing = 65,
        Alcohol_Tolerance = 66,
        Begging = 67,
        Jewelry_Making = 68,
        Pottery = 69,
        Percusion = 70,
        Intimidation = 71,
        Berserking = 72,
        Taunt = 73,
        Frenzy = 74,
        Remove_Trap = 75,
        Triple_Attack = 76,
        _2H_Pierce = 77,
        Melee = 98, // generic melee hit that doesn't get scaled up like weapon skills 
        Harm_Touch = 105,
        Lay_Hands = 107,
        Slam = 111,
        Inspect_Chest = 114,
        Open_Chest = 115,
        Reveal_Trap_Chest = 116
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
        Cold_Resist = 8,
        Fire_Resist = 9,
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

    public enum SpellBodyType
    {
        Humanoid = 1,
        Werewolf = 2,
        Undead = 3,
        Giant = 4,
        Golem = 5,
        Extraplanar = 6,
        UndeadPet = 8,
        Vampyre = 12,
        Atenha_Ra = 13,
        Greater_Akheva = 14,
        Khati_Sha = 15,
        Seru = 16,
        Draz_Nurakk = 18,
        Zek = 19,
        Luggald = 20,
        Animal = 21,
        Insect = 22,
        Elemental = 24,
        Plant = 25,
        Dragonkin = 26,
        Summoned = 28,
        Dragon = 29,
        Familiar = 31,
        Muramite = 34
    }

    public enum SpellTarget
    {
        Line_of_Sight = 1,
        Caster_AE = 2,
        Caster_Group = 3,
        Caster_PB = 4,
        Single = 5,
        Self = 6,
        Target_AE = 8,
        Animal = 9,
        Undead = 10,
        Summoned = 11,
        Lifetap = 13,
        Pet = 14,
        Corpse = 15,
        Plant = 16,
        Old_Giants = 17,
        Old_Dragons = 18,
        Target_AE_Lifetap = 20,
        Target_AE_Undead = 24,
        Target_AE_Summoned = 25,
        Hatelist = 32,
        Hatelist2 = 33,
        Chest = 34,
        Special_Muramites = 35, // bane for Ingenuity group trial in MPG
        Caster_PB_Players = 36,
        Caster_PB_NPC = 37,
        Pet2 = 38,
        No_Pets = 39, // single/group/ae ?
        Caster_AE_Players = 40, // bard AE hits all players
        Target_Group = 41,
        Directional_AE = 42,
        Frontal_AE = 44,
        Single_In_Group = 43,
        Target_Ring_AE = 45,
        Targets_Target = 46,
        Pet_Owner = 47,
        Target_AE_No_Players_Pets = 50 // blanket of forgetfullness. beneficial, AE mem blur, with max targets
    }

    public enum SpellTargetRestrict
    {
        Caster = 3, // (any NPC with mana) guess
        Not_On_Horse = 5, // guess
        Animal_or_Humanoid = 100,
        Dragon = 101,
        Animal_or_Insect = 102,
        Animal = 104,
        Plant = 105,
        Giant = 106,
        Not_Animal_or_Humanoid = 108,
        Bixie = 109,
        Harpy = 110,
        Gnoll = 111,
        Sporali = 112,
        Kobald = 113,
        Shade = 114,
        Drakkin = 115,
        Animal_or_Plant = 117,
        Summoned = 118,
        Fire_Pet = 119,
        Undead = 120,
        Living = 121,
        Fairy = 122,
        Humanoid = 123,
        Undead_HP_Less_Than_10_Percent = 124,
        Clockwork_HP_Less_Than_45_Percent = 125,
        Wisp_HP_Less_Than_10_Percent = 126,
        Not_Raid_Boss = 190,
        Raid_Boss = 191,
        HP_Above_75_Percent = 201,
        HP_Less_Than_20_Percent = 203, // dupe of 504
        HP_Less_Than_50_Percent = 204,
        //HP_Less_Than_50_Percent = 205,
        Not_In_Combat = 216,
        At_Least_1_Pet_On_Hatelist = 221,
        At_Least_2_Pets_On_Hatelist = 222,
        At_Least_3_Pets_On_Hatelist = 223,
        At_Least_4_Pets_On_Hatelist = 224,
        At_Least_5_Pets_On_Hatelist = 225,
        At_Least_6_Pets_On_Hatelist = 226,
        At_Least_7_Pets_On_Hatelist = 227,
        At_Least_8_Pets_On_Hatelist = 228,
        At_Least_9_Pets_On_Hatelist = 229,
        At_Least_10_Pets_On_Hatelist = 230,
        At_Least_11_Pets_On_Hatelist = 231,
        At_Least_12_Pets_On_Hatelist = 232,
        At_Least_13_Pets_On_Hatelist = 233,
        At_Least_14_Pets_On_Hatelist = 234,
        At_Least_15_Pets_On_Hatelist = 235,
        At_Least_16_Pets_On_Hatelist = 236,
        At_Least_17_Pets_On_Hatelist = 237,
        At_Least_18_Pets_On_Hatelist = 238,
        At_Least_19_Pets_On_Hatelist = 239,
        At_Least_20_Pets_On_Hatelist = 240,
        HP_Less_Than_35_Percent = 250, // duple of 507
        Chain_Plate_Classes = 304,
        HP_Between_15_and_25_Percent = 399,
        HP_Between_1_and_25_Percent = 400,
        HP_Between_25_and_35_Percent = 401,
        HP_Between_35_and_45_Percent = 402,
        HP_Between_45_and_55_Percent = 403,
        HP_Between_55_and_65_Percent = 404,
        HP_Above_99_Percent = 412,
        Mana_Above_10_Percent = 429,
        //Has_Mana = 412, // guess based on Suppressive Strike
        HP_Below_5_Percent = 501,
        HP_Below_10_Percent = 502,
        HP_Below_15_Percent = 503,
        HP_Below_20_Percent = 504,
        HP_Below_25_Percent = 505,
        HP_Below_30_Percent = 506,
        HP_Below_35_Percent = 507,
        HP_Below_40_Percent = 508,
        HP_Below_45_Percent = 509,
        HP_Below_50_Percent = 510,
        HP_Below_55_Percent = 511,
        HP_Below_60_Percent = 512,
        HP_Below_65_Percent = 513,
        HP_Below_70_Percent = 514,
        HP_Below_75_Percent = 515,
        HP_Below_80_Percent = 516,
        HP_Below_85_Percent = 517,
        HP_Below_90_Percent = 518,
        HP_Below_95_Percent = 519,
        Mana_Below_X_Percent = 521, // 5?
        End_Below_40_Percent = 522,
        Mana_Below_40_Percent = 523,

        Undead2 = 603, // vampiric too? Celestial Contravention Strike
        Undead3 = 608,
        Summoned2 = 624,
        Not_Pet = 701,
        Undead4 = 818,
        Not_Undead4 = 819,
        End_Below_21_Percent = 825,
        End_Below_25_Percent = 826,
        End_Below_29_Percent = 827,
        Regular_Server = 836,
        Progression_Server = 837,

        Humanoid_Level_84_Max = 842,
        Humanoid_Level_86_Max = 843,
        Humanoid_Level_88_Max = 844,

        Level_90_Max = 860,
        Level_92_Max = 861,
        Level_94_Max = 862,
        Level_95_Max = 863,
        Level_97_Max = 864,
        Level_99_Max = 865,
        Level_100_Max = 869,
        Level_102_Max = 870,
        Level_104_Max = 871,

        Between_Level_1_and_75 = 1000,
        Between_Level_76_and_85 = 1001,
        Between_Level_86_and_95 = 1002,
        Between_Level_96_and_105 = 1003,

        HP_Less_Than_80_Percent = 1004,

        // [38311] Mana Reserve is tagged with both, not sure which is which
        Mana_Below_10_Percent = 38311,
        Mana_Below_20_Percent = 38312,

        Caster_or_Priest = 49529
    }

    public enum SpellZoneRestrict
    {
        None = 0,
        Outdoors = 1,
        Indoors = 2
    }

    // these are found as type 11 in the dbstr file
    public enum SpellIllusion
    {
        Gender_Change = -1,
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
        Old_Aviak = 13,
        Old_Werewolf = 14,
        Old_Brownie = 15,
        Old_Centaur = 16,
        Trakanon = 19,
        Venril_Sathir = 20,
        Old_Evil_Eye = 21,
        Froglok = 27,
        Old_Gargoyle = 29,
        Gelatinous_Cube = 31,
        Old_Gnoll = 39,
        Old_Wolf = 42,
        Black_Spirit_Wolf = (42 << 16) + 1,
        White_Spirit_Wolf = (42 << 16) + 2,
        Old_Bear = 43,
        Polar_Bear = (43 << 16) + 2,
        Freeport_Militia = 44,
        Imp = 46,
        Lizard_Man = 51,
        Old_Drachnid = 57,
        Solusek_Ro = 58,
        Tunare = 62,
        Tiger = 63,
        Mayong = 65,
        Ralos_Zek = 66,
        Elemental = 75,
        Earth_Elemental = 75 << 16,
        Fire_Elemental = (75 << 16) + 1,
        Water_Elemental = (75 << 16) + 2,
        Air_Elemental = (75 << 16) + 3,
        Old_Scarecrow = 82,
        Old_Skeleton = 85,
        Old_Drake = 89,
        Old_Alligator = 91,
        Old_Cazic_Thule = 95,
        Cockatrice = 96,
        Old_Vampire = 98,
        Old_Amygdalan = 99,
        Old_Dervish = 100,
        Tadpole = 102,
        Old_Kedge = 103,
        Mammoth = 107,
        Wasp = 109,
        Mermaid = 110,
        Seahorse = 116,
        Ghost = 118,
        Sabertooth = 119,
        Spirit_Wolf = 120,
        Gorgon = 121,
        Old_Dragon = 122,
        Innoruuk = 123,
        Unicorn = 124,
        Pegasus = 125,
        Djinn = 126,
        Invisible_Man = 127,
        Iksar = 128,
        Vah_Shir = 130,
        Old_Sarnak = 131,
        Old_Drolvarg = 133,
        Mosquito = 134,
        Rhinoceros = 135,
        Xalgoz = 136,
        Kunark_Goblin = 137,
        Yeti = 138,
        Kunark_Giant = 140,
        Nearby_Object = 142,
        Erollisi_Marr = 150,
        Tribunal = 151,
        Bristlebane = 153,
        Tree = 143,
        Old_Iksar_Skeleton = 161,
        Snow_Rabbit = 176,
        Walrus = 177,
        Geonid = 178,
        Coldain = 183,
        Hag = 185,
        Othmir = 190,
        Ulthork = 191,
        Sea_Turtle = 194,
        Shik_Nar = 199,
        Rockhopper = 200,
        Underbulk = 201,
        Grimling = 202,
        Worm = 203,
        Shadel = 205,
        Owlbear = 206,
        Rhino_Beetle = 207,
        Earth_Elemental2 = 209,
        Air_Elemental2 = 210,
        Water_Elemental2 = 211,
        Fire_Elemental2 = 212,
        Thought_Horror = 214,
        Shissar = 217,
        Fungal_Fiend = 218,
        Stonegrabber = 220,
        Zelniak = 222,
        Lightcrawler = 223,
        Shadow = 224,
        Sunflower = 225,
        Sun_Revenant = 226,
        Shrieker = 227,
        Galorian = 228,
        Netherbian = 229,
        Akheva = 230,
        Wretch = 235,
        Guard = 239,
        Arachnid = 326,
        Guktan = 330,
        Troll_Pirate = 331,
        Gnome_Pirate = 338,
        Dark_Elf_Pirate = 339,
        Ogre_Pirate = 340,
        Human_Pirate = 341,
        Erudite_Pirate = 342,
        Froglok_Skeleton = 349,
        Undead_Froglok = 350,
        Scaled_Wolf = 356,
        Vampire = 360,
        Nightrage_Orphan = (360 << 16) + 1,
        Skeleton = 367,
        Drybone_Skeleton = (367 << 16) + 1,
        Frostbone_Skeleton = (367 << 16) + 2,
        Firebone_Skeleton = (367 << 16) + 3,
        Scorched_Skeleton = (367 << 16) + 4,
        Mummy = 368,
        Froglok_Ghost = 371,
        Shade = 373,
        Golem = 374,
        Ice_Golem = (374 << 16) + 1,
        Crystal_Golem = (374 << 16) + 3,
        Jokester = 384,
        Nihil = 385,
        Trusik = 386,
        Hynid = 388,
        Turepta = 389,
        Cragbeast = 390,
        Stonemite = 391,
        Ukun = 392,
        Ikaav = 394,
        Aneuk = 395,
        Kyv = 396,
        Noc = 397,
        Ra_tuk = 398,
        Taneth = 399,
        Huvul = 400,
        Mutna = 401,
        Mastruq = 402,
        Taelosian = 403,
        Mata_Muram = 406,
        Lightning_Warrior = 407,
        Feran = 410,
        Pyrilen = 411,
        Chimera = 412,
        Dragorn = 413,
        Murkglider = 414,
        Rat = 415,
        Bat = 416,
        Gelidran = 417,
        Girplan = 419,
        Crystal_Shard = 425,
        Dervish = 431,
        Drake = 432,
        Goblin = 433,
        Solusek_Goblin = (433 << 16) + 1,
        Dagnor_Goblin = (433 << 16) + 2,
        Valley_Goblin = (433 << 16) + 3,
        Aqua_Goblin = (433 << 16) + 7,
        Goblin_King = (433 << 16) + 8,
        Rallosian_Goblin = (433 << 16) + 11,
        Frost_Goblin = (433 << 16) + 12,
        Kirin = 434,
        Basilisk = 436,
        Puma = 439,
        Domain_Prowler = (439 << 16) + 9,
        Spider = 440,
        Spider_Queen = 441,
        Animated_Statue = 442,
        Lava_Spider = 450,
        Lava_Spider_Queen = 451,
        Dragon_Egg = 445,
        Werewolf = 454,
        White_Werewolf = (454 << 16) + 2,
        Kobold = 455,
        Kobold_King = (455 << 16) + 2,
        Sporali = 456,
        Violet_Sporali = (456 << 16) + 2,
        Azure_Sporali = (456 << 16) + 11,
        Gnomework = 457,
        Orc = 458,
        Bloodmoon_Orc = (458 << 16) + 4,
        Drachnid = 461,
        Drachnid_Cocoon = 462,
        Fungus_Patch = 463,
        Gargoyle = 464,
        Runed_Gargoyle = (464 << 16) + 1,
        Undead_Shiliskin = 467,
        Armored_Shiliskin = (467 << 16) + 5,
        Snake = 468,
        Evil_Eye = 469,
        Minotaur = 470,
        Zombie = 471,
        Clockwork_Boar = 472,
        Fairy = 473,
        Tree_Fairy = (473 << 16) + 1,
        Witheran = 474,
        Air_Elemental3 = 475,
        Earth_Elemental3 = 476,
        Fire_Elemental3 = 477,
        Water_Elemental3 = 478,
        Alligator = 479,
        Bear = 480,
        Wolf = 482,
        Spectre = 485,
        Banshee = 487,
        Banshee2 = 488,
        Bone_Golem = 491,
        Scrykin = 495,
        Treant = 496, // or izon
        Regal_Vampire = 497,
        Floating_Skull = 512,
        Totem = 514,
        Bixie_Drone = 520,
        Bixie_Queen = (520 << 16) + 2,
        Centaur = 521,
        Centaur_Warrior = (521 << 16) + 3,
        Drakkin = 522,
        Gnoll = 524,
        Undead_Gnoll = (524 << 16) + 1,
        Mucktail_Gnoll = (524 << 16) + 2,
        Gnoll_Reaver = (524 << 16) + 3,
        Blackburrow_Gnoll = (524 << 16) + 4,
        Satyr = 529,
        Dragon = 530,
        Hideous_Harpy = 527,
        Goo = 549,
        Aviak = 558,
        Beetle = 559,
        Death_Beetle = (559 << 16) + 1,
        Kedge = 561,
        Kerran = 562,
        Shissar2 = 563,
        Siren = 564,
        Siren_Sorceress = (564 << 16) + 1,
        Plaguebringer = 566,
        Hooded_Plaguebringer = (566 << 16) + 7,
        Brownie = 568,
        Brownie_Noble = (568 << 16) + 2,
        Steam_Suit = 570,
        Embattled_Minotaur = 574,
        Scarecrow = 575,
        Shade2 = 576,
        Steamwork = 577,
        Tyranont = 578,
        Worg = 580,
        Wyvern = 581,
        Elven_Ghost = 587,
        Burynai = 602,
        Dracolich = 604,
        Iksar_Ghost = 605,
        Iksar_Skeleton = 606,
        Mephit = 607,
        Muddite = 608,
        Raptor = 609,
        Sarnak = 610,
        Scorpion = 611,
        Plague_Fly = 612,
        Burning_Nekhon = 614,
        Shadow_Nekhon = (614 << 16) + 1,
        Crystal_Hydra = 615,
        Crystal_Sphere = 616,
        Vitrik = 620,
        Bellikos = 638,
        Cliknar = 643,
        Ant = 644,
        Crystal_Sessiloid = 647,
        Telmira = 653,
        Flood_Telmira = (653 << 16) + 2,
        Morell_Thule = 658,
        Marionette = 659,
        Book_Dervish = 660,
        Topiary_Lion = 661,
        Rotdog = 662,
        Amygdalan = 663,
        Sandman = 664,
        Grandfather_Clock = 665,
        Gingerbread_Man = 666,
        Royal_Guardian = 667,
        Rabbit = 668,
        Gouzah_Rabbit = (668 << 16) + 3,
        Polka_Dot_Rabbit = (668 << 16) + 5,
        Cazic_Thule = 670,
        Selyrah = 686,
        Goral = 687,
        Braxi = 688,
        Kangon = 689,
        Undead_Thelasa = 695,
        Thel_Ereth_Ril = (695 << 16) + 21,
        Swinetor = 696,
        Swinetor_Necro = (696 << 16) + 1,
        Triumvirate = 697,
        Hadal = 698,
        Hadal_Templar = (698 << 16) + 2,
        Alaran_Ghost = 708,
        Holgresh = 715,
        Ratman = 718,
        Fallen_Knight = 719,
        Akhevan = 722,
        Tirun = 734,
        Bixie = 741,
        Bixie_Soldier = (741 << 16) + 2,
        Butterfly = 742,
        Arc_Worker = 766,
        Cursed_Siren = 769,
        Tyrannosaurus = 771,
        Ankylosaurus = 774,
        Thaell_Ew = 785,
        Drolvarg = 843,
    }

    public enum SpellFaction
    {
        SHIP_Workshop = 1178,
        Kithicor_Good = 1204, // army of light
        Kithicor_Evil = 1205, // army of obliteration
        Ancient_Iksar = 1229
    }

    public enum SpellMaxHits
    {
        None = 0,
        Incoming_Hit_Attempts = 1, // incoming melee attempts (prior to success checks)
        Outgoing_Hit_Attempts = 2, // outgoing melee attempts of Skill type (prior to success checks)
        Incoming_Spells = 3,
        Outgoing_Spells = 4,
        Outgoing_Hit_Successes = 5,
        Incoming_Hit_Successes = 6,
        Matching_Spells = 7, // mostly outgoing, sometimes incoming (puratus) matching limits
        Incoming_Hits_Or_Spells = 8,
        Reflected_Spells = 9,
        Defensive_Proc_Casts = 10,
        Offensive_Proc_Casts = 11,
        // 2015-7-22 patch:
        // Damage shield damage is now considered magical non-melee damage; this means that melee guard and melee threshold guard spell effects will no longer 
        // negate damage shield damage. Rune, spell guard, spell threshold guard, and spells that allow you to absorb damage as mana will continue to block damage shield damage.
        Incoming_Hits_Or_Spells_Or_DS = 13
    }

    public enum SpellTeleport
    {
        Primary_Anchor = 52584,
        Secondary_Anchor = 52585,
        Guild_Anchor = 50874
    }

    // it's hard to stick many spells into a single category, but i think this is only used by SPA 403
    public enum SpellCategory
    {
        Cures = 2,
        Offensive_Damage = 3, // nukes, DoT, AA discs, and spells that cast nukes as a side effect
        Heals = 5,
        Lifetap = 6,
        Transport = 8
    }


    #endregion

    public sealed class SpellSlot
    {
        public int SPA;
        public int Base1;
        public int Base2;
        public int Max;
        public int Calc;
        public string Desc;

        public override string ToString()
        {
            return Desc;
        }
    }

    public sealed class Spell
    {
        public int ID;
        public int GroupID;
        public string Name;
        public int Icon;
        public int Mana;
        public int Endurance;
        public int EnduranceUpkeep;
        public int DurationTicks;
        public bool Focusable;
        public List<SpellSlot> Slots;
        //public byte Level;
        public byte[] Levels; // casting level for each of the 16 classes
        public byte[] ExtLevels; // similar to levels but assigns levels for side effect spells that don't have levels defined (e.g. a proc effect will get the level of it's proc buff)
        public string ClassesLevels;
        public SpellClassesMask ClassesMask;
        public SpellSkill Skill;
        public bool Beneficial;
        public bool BeneficialBlockable;
        public SpellTarget Target;
        public SpellResist ResistType;
        public int ResistMod;
        public bool PartialResist;
        public int MinResist;
        public int MaxResist;
        public string Extra;
        public int HateOverride;
        public int HateMod;
        public int Range;
        public int AERange;
        public int AEDuration; // rain spells
        public float CastingTime;
        public float RestTime; // refresh time
        public float RecastTime;
        public float PushBack;
        public float PushUp;
        public int DescID;
        public string Desc;
        public int MaxHits;
        public SpellMaxHits MaxHitsType;
        public int MaxTargets;
        public int RecourseID;
        public string Recourse;
        public int TimerID;
        public int ViralRange;
        public int MinViralTime;
        public int MaxViralTime;
        public SpellTargetRestrict TargetRestrict;
        public SpellTargetRestrict CasterRestrict;
        public int[] ConsumeItemID;
        public int[] ConsumeItemCount;
        public int[] FocusID;
        public string LandOnSelf;
        public string LandOnOther;
        public int ConeStartAngle;
        public int ConeEndAngle;
        public bool MGBable;
        public int Rank;
        public bool CastOutOfCombat;
        public SpellZoneRestrict Zone;
        public bool DurationFrozen; // in guildhall/lobby
        public bool Dispelable;
        public bool PersistAfterDeath;
        public bool SongWindow;
        public bool CancelOnSit;
        public bool Sneaking;
        public int[] CategoryDescID; // most AAs don't have these set
        public string Deity;
        public int SongCap;
        public int MinRange;
        public int RangeModCloseDist;
        public int RangeModCloseMult;
        public int RangeModFarDist;
        public int RangeModFarMult;
        public bool Interruptable;
        public bool Feedbackable; // triger spell DS
        public bool Reflectable;
        public int SpellClass;
        public int SpellSubclass;
        public bool CastInFastRegen;
        public bool AllowFastRegen;
        public bool BetaOnly;
        public bool CannotRemove;
        public int CritOverride; // when set the spell has this max % crit chance and mod 
        public bool CombatSkill;
        public int ResistPerLevel;
        public int ResistCap;
        public List<string> Stacking;
        //public string Version;

        public int[] LinksTo;
        public int RefCount; // number of spells that link to this

#if !LimitMemoryUse
        public string[] Categories;
        //public string[] RawData;
#endif

        public float Unknown;


        /// Effects can reference other spells or items via square bracket notation. e.g.
        /// [Spell 123]    is a reference to spell 123
        /// [Group 123]    is a reference to spell group 123
        /// [Item 123]     is a reference to item 123
        /// [AA 123]     is a reference to AA group 123
        public static readonly Regex SpellRefExpr = new Regex(@"\[Spell\s(\d+)\]");
        public static readonly Regex GroupRefExpr = new Regex(@"\[Group\s(\d+)\]");
        public static readonly Regex ItemRefExpr = new Regex(@"\[Item\s(\d+)\]");
        public static readonly Regex AARefExpr = new Regex(@"\[AA\s(\d+)\]");

        public static readonly string DateVersionFormat = "yyyy-MM-dd";

        public Spell()
        {
            Slots = new List<SpellSlot>(6); // first grow will make it a list of 12
            Levels = new byte[16];
            ExtLevels = new byte[16];
            ConsumeItemID = new int[4];
            ConsumeItemCount = new int[4];
            FocusID = new int[4];
            CategoryDescID = new int[3];
            Stacking = new List<string>();
        }

        /// <summary>
        /// Each spell can have a number of slots for variable spell effects. The game developers call these "SPAs".
        /// TODO: this should be a static function but it makes references to spell attributes like ID, Skill, Extra, DurationTicks and in a few cases even modifies the Mana attribute.
        /// </summary>
        public string ParseEffect(int spa, int base1, int base2, int max, int calc, int level, string version)
        {
            // type 254 indicates an unused slot and end of slots (i.e. all the rest will also be 254)
            if (spa == 254)
                return null;

            // many SPAs use a scaled value based on either current tick or caster level
            int value = CalcValue(calc, base1, max, 1, level);
            string range = CalcValueRange(calc, base1, max, DurationTicks, level);
            //Func<int> base1_or_value = delegate() { Debug.WriteLineIf(base1 != value, "SPA " + spa + " value uncertain"); return base1; };

            // some hp/mana/end/hate effects repeat for each tick of the duration
            string repeating = (DurationTicks > 0) ? " per tick" : null;

            // some effects are capped at a max level
            string maxlevel = (max > 0) ? String.Format(" up to level {0}", max) : null;

            Func<string, bool> versionLessThan = date => String.Compare(version, date) < 0;

            //Func<string, string> FormatCount = delegate(string name) { return ((value > 0) ? "Increase " : "Decrease ") + name + " by " + Math.Abs(value); };

            switch (spa)
            {
                case 0:
                    if (base2 > 0)
                        return Spell.FormatCount("Current HP", value) + repeating + range + " (If " + Spell.FormatEnum((SpellTargetRestrict)base2) + ")";
                    return Spell.FormatCount("Current HP", value) + repeating + range;
                case 1:
                    return Spell.FormatCount("AC", (int)(value / (10f / 3f)));
                case 2:
                    return Spell.FormatCount("ATK", value) + range;
                case 3:
                    return Spell.FormatPercent("Movement Speed", value);
                case 4:
                    return Spell.FormatCount("STR", value) + range;
                case 5:
                    return Spell.FormatCount("DEX", value) + range;
                case 6:
                    return Spell.FormatCount("AGI", value) + range;
                case 7:
                    return Spell.FormatCount("STA", value) + range;
                case 8:
                    return Spell.FormatCount("INT", value) + range;
                case 9:
                    return Spell.FormatCount("WIS", value) + range;
                case 10:
                    // type 10 sometimes indicates an unused slot or some special script trigger
                    if (base1 <= 1 || base1 > 255)
                        return null;
                    return Spell.FormatCount("CHA", value) + range;
                case 11:
                    // base attack speed is 100. so 85 = 15% slow, 130 = 30% haste
                    // reverse check on max value for slow spells
                    if (value < 100 && max > 0 && value < max)
                        value = max;
                    return Spell.FormatPercent("Melee Haste", value - 100);
                case 12:
                    if (base1 > 1)
                        return String.Format("Invisibility (Enhanced {0})", base1);
                    return "Invisibility (Unstable)";
                case 13:
                    if (base1 > 1)
                        return String.Format("See Invisible (Enhanced {0})", base1);
                    return "See Invisible";
                case 14:
                    return "Enduring Breath";
                case 15:
                    return Spell.FormatCount("Current Mana", value) + repeating + range;
                case 18:
                    return "Pacify";
                case 19:
                    return Spell.FormatCount("Faction", value);
                case 20:
                    return "Blind";
                case 21:
                    //if (base2 != base1 && base2 != 0)
                    //    return String.Format("Stun for {0:0.##}s ({1:0.##}s in PvP)", base1 / 1000f, base2 / 1000f) + maxlevel;
                    return String.Format("Stun for {0:0.##}s", base1 / 1000f) + maxlevel;
                case 22:
                    return "Charm" + maxlevel;
                case 23:
                    return "Fear" + maxlevel;
                case 24:
                    return Spell.FormatCount("Stamina Loss", value);
                case 25:
                    return "Bind";
                case 26:
                    if (base2 > 1)
                        return "Gate to Secondary Bind";
                    return "Gate";
                case 27:
                    return String.Format("Dispel ({0})", value);
                case 28:
                    return "Invisibility to Undead (Unstable)";
                case 29:
                    return "Invisibility to Animals (Unstable)";
                case 30:
                    // how close can you get before the mob aggroes you
                    return String.Format("Decrease Aggro Radius to {0}", value) + maxlevel;
                case 31:
                    return "Mesmerize" + maxlevel;
                case 32:
                    // calc 100 = summon a stack? (based on item stack size) Pouch of Quellious, Quiver of Marr
                    //return String.Format("Summon: [Item {0}] x {1} {2} {3}", base1, calc, max, base2);
                    return String.Format("Summon: [Item {0}]", base1);
                case 33:
                    return String.Format("Summon Pet: {0}", Extra);
                case 35:
                    return Spell.FormatCount("Disease Counter", value);
                case 36:
                    return Spell.FormatCount("Poison Counter", value);
                case 39:
                    // this doesn't actually block twincast by itself. 
                    // twincast excludes spells that have this marker
                    return "Stacking: Twincast Blocker";
                case 40:
                    return "Invulnerability";
                case 41:
                    return "Destroy";
                case 42:
                    // TODO: does shadowstep always gate an NPC? e.g. highsun
                    return "Shadowstep";
                case 44:
                    return String.Format("Stacking: Delayed Heal Marker ({0})", value);
                case 46:
                    return Spell.FormatCount("Fire Resist", value);
                case 47:
                    return Spell.FormatCount("Cold Resist", value);
                case 48:
                    return Spell.FormatCount("Poison Resist", value);
                case 49:
                    return Spell.FormatCount("Disease Resist", value);
                case 50:
                    return Spell.FormatCount("Magic Resist", value);
                case 52:
                    return "Sense Undead";
                case 53:
                    return "Sense Summoned";
                case 54:
                    return "Sense Animal";
                case 55:
                    return String.Format("Absorb Damage: 100%, Total: {0}", value);
                case 56:
                    return "True North";
                case 57:
                    return "Levitate";
                case 58:
                    value = (base1 << 16) + base2;
                    if (Enum.IsDefined(typeof(SpellIllusion), value))
                        return String.Format("Illusion: {0}", Spell.FormatEnum((SpellIllusion)value));
                    if (base2 > 0)
                        return String.Format("Illusion: {0} ({1})", Spell.FormatEnum((SpellIllusion)base1), base2);
                    return String.Format("Illusion: {0}", Spell.FormatEnum((SpellIllusion)base1));
                case 59:
                    return Spell.FormatCount("Damage Shield", -value);
                case 61:
                    return "Identify Item";
                case 63:
                    // +25 if over level 53, +(cha - 150)/10 max:15. so base is 40 + whatever the value is
                    //return String.Format("Memory Blur ({0})", value);
                    return String.Format("Memory Blur ({0}% Chance)", Math.Min(value + 40, 100));
                case 64:
                    if (base2 != base1 && base2 != 0)
                        return String.Format("Stun and Spin NPC for {0:0.##}s (PC for {1:0.##}s)", base1 / 1000f, base2 / 1000f) + maxlevel;
                    return String.Format("Stun and Spin for {0:0.##}s", base1 / 1000f) + maxlevel;
                case 65:
                    return "Infravision";
                case 66:
                    return "Ultravision";
                case 67:
                    return "Eye of Zomm";
                case 68:
                    return "Reclaim Pet";
                case 69:
                    return Spell.FormatCount("Max HP", value) + range;
                case 71:
                    return String.Format("Summon Pet: {0}", Extra);
                case 73:
                    return "Bind Sight";
                case 74:
                    if (value < 100)
                        return String.Format("Feign Death ({0}% Chance)", value);
                    return "Feign Death";
                case 75:
                    return "Project Voice";
                case 76:
                    return "Sentinel";
                case 77:
                    return "Locate Corpse";
                case 78:
                    return String.Format("Absorb Spell Damage: 100%, Total: {0}", value);
                case 79:
                    // delta hp for heal/nuke, non repeating
                    if (base2 > 0)
                        return Spell.FormatCount("Current HP", value) + range + " (If " + Spell.FormatEnum((SpellTargetRestrict)base2) + ")";
                    return Spell.FormatCount("Current HP", value) + range;
                case 81:
                    return String.Format("Resurrect with {0}% XP", value);
                case 82:
                    // call of the hero
                    return "Summon Player";
                case 83:
                    return String.Format("Teleport to {0}", Extra);
                case 84:
                    return "Gravity Flux";
                case 85:
                    if (base2 != 0)
                        return String.Format("Add Melee Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                    return String.Format("Add Melee Proc: [Spell {0}]", base1);
                case 86:
                    return String.Format("Decrease Social Radius to {0}", value) + maxlevel;
                case 87:
                    return Spell.FormatPercent("Magnification", value);
                case 88:
                    return String.Format("Evacuate to {0}", Extra);
                case 89:
                    return Spell.FormatPercent("Player Size", base1 - 100);
                case 90:
                    // aka pet invis
                    return "Ignore Pet";
                case 91:
                    return String.Format("Summon Corpse up to level {0}", base1);
                case 92:
                    // calming strike spells are all capped at 100. so base1 would be more appropriate for those
                    // but most other hate spells seem to imply scaled value is used
                    return Spell.FormatCount("Hate", value);
                case 93:
                    return "Stop Rain";
                case 94:
                    return "Cancel if Combat Initiated";
                case 95:
                    return "Sacrifice";
                case 96:
                    // aka silence, but named this way to match melee version
                    return "Inhibit Spell Casting";
                case 97:
                    return Spell.FormatCount("Max Mana", value);
                case 98:
                    // yet another super turbo haste. only on 3 bard songs
                    return Spell.FormatPercent("Melee Haste v2", value - 100);
                case 99:
                    return "Root";
                case 100:
                    // heal over time
                    if (base2 > 0)
                        return Spell.FormatCount("Current HP", value) + repeating + range + " (If " + Spell.FormatEnum((SpellTargetRestrict)base2) + ")";
                    return Spell.FormatCount("Current HP", value) + repeating + range;
                case 101:
                    // only castable via Donal's BP. creates a buf that blocks recasting
                    return "Increase Current HP by 7500";
                case 102:
                    return "Fear Immunity";
                case 103:
                    return "Summon Pet";
                case 104:
                    return String.Format("Translocate to {0}", Extra);
                case 105:
                    return "Inhibit Gate";
                case 106:
                    return String.Format("Summon Warder: {0}", Extra);
                case 108:
                    return String.Format("Summon Familiar: {0}", Extra);
                case 109:
                    return String.Format("Summon: [Item {0}]", base1);
                case 111:
                    return Spell.FormatCount("All Resists", value);
                case 112:
                    return Spell.FormatCount("Effective Casting Level", value);
                case 113:
                    return String.Format("Summon Mount: {0}", Extra);
                case 114:
                    return Spell.FormatPercent("Hate Generated", value);
                case 115:
                    return "Reset Hunger Counter";
                case 116:
                    return Spell.FormatCount("Curse Counter", value);
                case 117:
                    // fear me now wil o wisps
                    return "Make Weapon Magical";
                case 118:
                    return Spell.FormatCount("Singing Skill", value);
                case 119:
                    return Spell.FormatPercent("Melee Haste v3", value);
                case 120:
                    return Spell.FormatPercent("Healing Taken", base1); // no min/max range
                case 121:
                    // damages the target whenever it hits something
                    return Spell.FormatCount("Reverse Damage Shield", -value);
                case 122:
                    return String.Format("Decrease {0} Skill ({1})", Spell.FormatEnum((SpellSkill)base1), calc);
                case 123:
                    return "Screech";
                case 124:
                    return Spell.FormatPercentRange("Spell Damage", base1, base2);
                case 125:
                    return Spell.FormatPercentRange("Healing", base1, base2);
                case 126:
                    return Spell.FormatPercentRange("Spell Resist Rate", base1, base2, true);
                case 127:
                    return Spell.FormatPercent("Spell Haste", base1);
                case 128:
                    return Spell.FormatPercent("Spell Duration", base1);
                case 129:
                    return Spell.FormatPercent("Spell Range", base1);
                case 130:
                    // i think this affects all special attacks (bash/kick/frenzy/etc...) and unlike 114 it checks focus limit rules
                    return Spell.FormatPercentRange("Spell and Bash Hate", base1, base2);
                case 131:
                    return Spell.FormatPercentRange("Chance of Using Reagent", base1, base2, true);
                case 132:
                    return Spell.FormatPercentRange("Spell Mana Cost", base1, base2, true);
                case 134:
                    if (base2 == 0)
                        base2 = 100; // just to make it obvious that 0 means the focus stops functioning
                    return String.Format("Limit Max Level: {0} (lose {1}% per level)", base1, base2);
                case 135:
                    return String.Format("Limit Resist: {1}{0}", (SpellResist)Math.Abs(base1), base1 >= 0 ? "" : "Exclude ");
                case 136:
                    return String.Format("Limit Target: {1}{0}", Spell.FormatEnum((SpellTarget)Math.Abs(base1)), base1 >= 0 ? "" : "Exclude ");
                case 137:
                    return String.Format("Limit Effect: {1}{0}", Spell.FormatEnum((SpellEffect)Math.Abs(base1)), base1 >= 0 ? "" : "Exclude ");
                case 138:
                    return String.Format("Limit Type: {0}", base1 == 0 ? "Detrimental" : "Beneficial");
                case 139:
                    return String.Format("Limit Spell: {1}[Spell {0}]", Math.Abs(base1), base1 >= 0 ? "" : "Exclude ");
                case 140:
                    return String.Format("Limit Min Duration: {0}s", base1 * 6);
                case 141:
                    return String.Format("Limit Max Duration: {0}s", 0);
                case 142:
                    return String.Format("Limit Min Level: {0}", base1);
                case 143:
                    return String.Format("Limit Min Casting Time: {0}s", base1 / 1000f);
                case 144:
                    return String.Format("Limit Max Casting Time: {0}s", base1 / 1000f);
                case 145:
                    return String.Format("Teleport to {0}", Extra);
                case 146:
                    // x, y, z, heading coordinates on slots 1, 2, 3, 4
                    //return String.Format("Set position to {0}", base1);
                    return null;
                case 147:
                    //return String.Format("Increase Current HP by {1} Max: {0}% ", value, max);
                    return FormatPercent("Current HP", value) + String.Format(" up to {0}", max);
                case 148:
                    //if (max > 1000) max -= 1000;
                    return String.Format("Stacking: Block new spell if slot {0} is '{1}' and < {2}", calc % 100, Spell.FormatEnum((SpellEffect)base1), max);
                case 149:
                    //if (max > 1000) max -= 1000;
                    return String.Format("Stacking: Overwrite existing spell if slot {0} is '{1}' and < {2}", calc % 100, Spell.FormatEnum((SpellEffect)base1), max);
                case 150:
                    return String.Format("Divine Intervention with {0} Heal", max);
                case 151:
                    if (base1 == 1)
                        return "Suspend Pet with Buffs";
                    return "Suspend Pet";
                case 152:
                    return String.Format("Summon Pet: {0} x {1} for {2}s", Extra, base1, max);
                case 153:
                    return String.Format("Balance Group HP with {0}% Penalty", value);
                case 154:
                    // +0.5% per level difference
                    if (base2 != 0)
                        return String.Format("Decrease Detrimental Duration by 50% ({0}% Chance)", base1 / 10) + maxlevel;
                    return String.Format("Dispel Detrimental ({0}% Chance)", base1 / 10) + maxlevel;
                case 156:
                    return "Illusion: Target";
                case 157:
                    return Spell.FormatCount("Spell Damage Shield", -value);
                case 158:
                    if (max != 0)
                        return Spell.FormatPercent("Chance to Reflect Spell", base1) + String.Format(" with up to {0}% Damage", max);
                    return Spell.FormatPercent("Chance to Reflect Spell", base1);
                case 159:
                    return Spell.FormatCount("Base Stats", value);
                case 160:
                    return String.Format("Intoxicate if Tolerance under {0}", base1);
                case 161:
                    return String.Format("Absorb Spell Damage: {0}%", base1) + (base2 > 0 ? String.Format(", Max Per Hit: {0}", base2) : "") + (max > 0 ? String.Format(", Total: {0}", max) : "");
                case 162:
                    return String.Format("Absorb Melee Damage: {0}%", base1) + (base2 > 0 ? String.Format(", Max Per Hit: {0}", base2) : "") + (max > 0 ? String.Format(", Total: {0}", max) : "");
                case 163:
                    return String.Format("Absorb {0} Hits or Spells", base1) + (max > 0 ? String.Format(", Max Per Hit: {0}", max) : "");
                case 164:
                    return String.Format("Appraise Chest ({0})", value);
                case 165:
                    return String.Format("Disarm Chest ({0})", value);
                case 166:
                    return String.Format("Unlock Chest ({0})", value);
                case 167:
                    return String.Format("Increase Pet Power ({0})", value);
                case 168:
                    // defensive disc/how is this different than an endless rune?
                    return Spell.FormatPercent("Melee Mitigation", -value);
                case 169:
                    if ((SpellSkill)base2 != SpellSkill.Hit)
                        return Spell.FormatPercent("Chance to Critical Hit with " + Spell.FormatEnum((SpellSkill)base2), value);
                    return Spell.FormatPercent("Chance to Critical Hit", value);
                case 170:
                    // stacks with itself in other slots
                    return Spell.FormatPercent("Critical Nuke Damage", base1) + " of Base Damage";
                case 171:
                    return Spell.FormatPercent("Chance to Crippling Blow", value);
                case 172:
                    // combat agility AA
                    return Spell.FormatPercent("Chance to Avoid Melee", base1);
                case 173:
                    return Spell.FormatPercent("Chance to Riposte", value);
                case 174:
                    return Spell.FormatPercent("Chance to Dodge", value);
                case 175:
                    return Spell.FormatPercent("Chance to Parry", value);
                case 176:
                    return Spell.FormatPercent("Chance to Dual Wield", value);
                case 177:
                    // this is multiplicative
                    return Spell.FormatPercent("Chance to Double Attack", base1);
                case 178:
                    if (versionLessThan("2016-08-16"))
                        return String.Format("Lifetap from Weapon Damage: {0}%", base1);
                    return String.Format("Lifetap from Weapon Damage: {0}%", base1 / 10f);
                case 179:
                    return String.Format("Instrument Modifier: {0} {1}", Skill, value);
                case 180:
                    // devs call this Sanctification
                    // AA is called mystical shielding is 5%, fervor of the dark reign / sanctity of the keepers is 2%.
                    return Spell.FormatPercent("Chance to Resist Spell", value);
                case 181:
                    return Spell.FormatPercent("Chance to Resist Fear Spell", value);
                case 182:
                    // hundred hands effect. how is this different than 371?
                    return Spell.FormatPercent("Weapon Delay", base1 / 10f);
                case 183:
                    // according to prathun this effect does nothing
                    //return Spell.FormatPercent("Skill Check for " + Spell.FormatEnum((SpellSkill)base2), value);
                    return null;
                case 184:
                    if ((SpellSkill)base2 != SpellSkill.Hit)
                        return Spell.FormatPercent("Chance to Hit with " + Spell.FormatEnum((SpellSkill)base2), value);
                    return Spell.FormatPercent("Chance to Hit", value);
                case 185:
                    return Spell.FormatPercent(Spell.FormatEnum((SpellSkill)base2) + " Damage", value);
                case 186:
                    return Spell.FormatPercent("Min " + Spell.FormatEnum((SpellSkill)base2) + " Damage", value); // only DI1?
                case 187:
                    return String.Format("Balance Group Mana with {0}% Penalty", value);
                case 188:
                    return Spell.FormatPercent("Chance to Block", value);
                case 189:
                    return Spell.FormatCount("Current Endurance", value) + repeating + range;
                case 190:
                    return Spell.FormatCount("Max Endurance", value);
                case 191:
                    // melee and special skills
                    return "Inhibit Combat";
                case 192:
                    return Spell.FormatCount("Hate", value) + repeating + range;
                case 193:
                    //if (max != 0)
                    //    return String.Format("{0} Attack for {1} ({2} in PvP) with {3}% Accuracy Mod", Spell.FormatEnum(Skill), base1, max, base2);
                    return String.Format("{0} Attack for {1} with {2}% Accuracy Mod", Spell.FormatEnum(Skill), base1, base2);
                case 194:
                    // aka Fade
                    // if successful and target is outside the hardcoded 30' distance restriction the linked spell will be cast
                    // base=75 was invalid data before base2 was used as a spell id link (ignored in case we are parsing an old spell file)
                    if (base2 > 0 && base2 != 75)
                        return String.Format("Cancel Aggro {2} ({0}% Chance) and Cast: [Spell {1}] on Success", base1, base2, maxlevel);
                    return String.Format("Cancel Aggro {1} ({0}% Chance)", base1, maxlevel);
                case 195:
                    // melee + spell
                    // 100 is full resist. not sure why some spells have more
                    return String.Format("Stun Resist ({0})", value);
                case 196:
                    // no longer used
                    return String.Format("Srikethrough ({0})", value);
                case 197:
                    return Spell.FormatPercent(Spell.FormatEnum((SpellSkill)base2) + " Damage Taken", value);
                case 198:
                    return Spell.FormatCount("Current Endurance", value);
                case 199:
                    return String.Format("Taunt ({0})", value);
                case 200:
                    // affects worn melee/range weapon procs
                    // doesn't affect 85, 429. pretty sure 201 is also unaffected
                    return Spell.FormatPercent("Worn Proc Rate", base1);
                case 201:
                    return String.Format("Add Range Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                case 202:
                    return "Project Illusion on Next Spell";
                case 203:
                    return "Mass Group Buff on Next Spell";
                case 204:
                    return String.Format("Group Fear Immunity for {0}s", base1 * 10);
                case 205:
                    return String.Format("Rampage ({0})", base1);
                case 206:
                    // places you [base1] points of hate higher than all of the taunted targets around you - Dzarn
                    return String.Format("AE Taunt ({0})", base1);
                case 207:
                    return "Flesh to Bone Chips";
                case 209:
                    // +0.5% per level difference
                    if (base2 != 0)
                        return String.Format("Decrease Beneficial Duration by 50% ({0}% Chance)", base1 / 10) + maxlevel;
                    return String.Format("Dispel Beneficial ({0}% Chance)", base1 / 10) + maxlevel;
                case 210:
                    return String.Format("Pet Shielding for {0}s", base1 * 12);
                case 211:
                    // % chance applies individually to each mob in radius
                    //if (base2 > 0)
                    //    return String.Format("AE Attack ({0}% Chance) for {0}% Damage", base1, base2);
                    return Spell.FormatPercent("Chance to AE Attack", base1) + (base2 != 100 ? String.Format(" with {0}% Damage", base2) : "");
                case 212:
                    return Spell.FormatPercent("Chance to Critical Nuke", base1) + " and " + Spell.FormatPercent("Spell Mana Cost v2", base2);
                case 213:
                    return Spell.FormatPercent("Pet Max HP", base1);
                case 214:
                    return Spell.FormatPercent("Max HP", base1 / 100f);
                case 215:
                    return Spell.FormatPercent("Pet Chance to Avoid Melee", base1);
                case 216:
                    if ((SpellSkill)base2 != SpellSkill.Hit)
                        return Spell.FormatPercent(Spell.FormatEnum((SpellSkill)base2) + " Accuracy", value);
                    return Spell.FormatPercent("Accuracy", value);
                case 217:
                    return String.Format("Add Headshot Proc with up to {0} Damage", base2);
                case 218:
                    return Spell.FormatPercent("Pet Chance to Critical Hit", value);
                case 219:
                    return Spell.FormatPercent("Chance to Slay Undead", base1 / 100f) + String.Format(" with {0} Damage Mod", base2);
                //return Spell.FormatPercent("Chance to Slay Undead", base1 / 100f) + " and " + Spell.FormatPercent("Slay Damage", base2) + " of Base Damage";
                case 220:
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Damage Bonus", base1);
                case 221:
                    return Spell.FormatPercent("Weight", -base1);
                case 222:
                    return Spell.FormatPercent("Chance to Block from Back", base1);
                case 224:
                    if (base2 > 0)
                        return Spell.FormatPercent("Chance of Additional Riposte with " + Spell.FormatEnum((SpellSkill)base2), base1);
                    return Spell.FormatPercent("Chance of Additional Riposte", base1);
                case 225:
                    // this is additive. 100 = 100% chance
                    return Spell.FormatPercent("Chance to Double Attack", base1) + " (Additive)";
                case 226:
                    // allows bash while weilding a 2h weapon
                    return "Add Two-Handed Bash Ability";
                case 227:
                    return String.Format("Decrease {0} Timer by {1}", Spell.FormatEnum((SpellSkill)base2), FormatTime(base1));
                case 228:
                    return Spell.FormatPercent("Falling Damage", -base1);
                case 229:
                    return Spell.FormatPercent("Chance to Cast Through Stun", base1);
                case 230:
                    return Spell.FormatPercent("Shielding Distance", base1);
                case 231:
                    return Spell.FormatPercent("Chance to Stun Bash", base1);
                case 232:
                    return Spell.FormatPercent("Chance to Trigger Divine Intervention", base1);
                case 233:
                    return Spell.FormatPercent("Food Consumption", -base1);
                case 234:
                    return String.Format("Decrease Poison Application Time by {0}s", 10f - base1 / 1000f);
                case 238:
                    return "Permanent Illusion";
                case 237:
                    return "Passive Pet Ability: Spell Affinity";
                case 239:
                    return Spell.FormatPercent("Chance to Feign Death Through Spell Hit", base1);
                case 242:
                    return Spell.FormatPercent("Chance to Memory Blur", base1);
                case 243:
                    return Spell.FormatPercent("Chance of Charm Breaking", -base1);
                case 244:
                    return Spell.FormatPercent("Chance of Root Breaking", -100 + base1);
                case 245:
                    return Spell.FormatPercent("Chance of Trap Circumvention", base1);
                case 246:
                    return Spell.FormatCount("Lung Capacity", base1);
                case 247:
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Skill Cap", base1);
                case 248:
                    // ability to train spell skills over 200 is limited to 1
                    return Spell.FormatCount("Magic Specialization Ability", base1);
                case 249:
                    // 1-Handed weapon - Primary Hand: (damage * delay * level * 80) / 400000
                    // 1-Handed weapon - Secondary Hand: [Primary Hand Formula * Base1 of Sinister Strike SPA]
                    return Spell.FormatPercent("Offhand Weapon Damage Bonus", base1);
                case 250:
                    // increase chance of spa 85 
                    // compare with 200 which increase worn proc rate
                    return Spell.FormatPercent("Melee Proc Rate", base1);
                case 251:
                    // endless quiver AA
                    return Spell.FormatPercent("Chance of Using Ammo", -base1);
                case 252:
                    return Spell.FormatPercent("Chance to Backstab From Front", base1);
                case 253:
                    return String.Format("Chaotic Stab ({0})", base1);
                case 255:
                    return String.Format("Increase Shielding Duration by {0}", FormatTime(base1));
                case 256:
                    return "Shroud of Stealth";
                case 257:
                    // no longer used
                    return "Enable Pet Ability: Hold";
                case 258:
                    return Spell.FormatPercent("Chance to Triple Backstab", value);
                case 259:
                    // combat stability AA
                    return Spell.FormatPercent("AC Soft Cap", value);
                case 260:
                    return String.Format("Instrument Modifier: {0} {1}", Spell.FormatEnum((SpellSkill)base2), value);
                case 262:
                    // affects worn cap
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkillCap)base2) + " Cap", value);
                case 263:
                    // ability to train tradeskills over 200 is limited to 1 by default
                    return Spell.FormatCount("Ability to Specialize Tradeskills", base1);
                case 264:
                    return String.Format("Reduce [AA {0}] Timer by {1}", base2, FormatTime(base1));
                case 265:
                    // value of zero should negate effects of Mastery of the Past
                    return String.Format("No Fizzle up to level {0}", base1);
                case 266:
                    return Spell.FormatPercent("Chance of Additional 2H Attack", base1);
                case 267:
                    return String.Format("Enable Pet Ability: {0}", base2);
                case 268:
                    return Spell.FormatPercent("Chance to Fail " + Spell.FormatEnum((SpellSkill)base2) + " Combine", -base1);
                case 269:
                    return Spell.FormatPercent("Bandage HP Cap", base1);
                case 270:
                    return Spell.FormatCount("Beneficial Song Range", base1);
                case 271:
                    // each 0.7 points seems to equal 10% normal run speed
                    return Spell.FormatPercent("Innate Movement Speed", base1 / 0.7f);
                case 272:
                    return Spell.FormatPercent("Song Casting Skill", value);
                case 273:
                    return Spell.FormatPercent("Chance to Critical DoT", base1) + maxlevel;
                case 274:
                    return Spell.FormatPercent("Chance to Critical Heal", base1);
                case 275:
                    return Spell.FormatPercent("Chance to Critical Mend", base1);
                case 276:
                    return String.Format("Dual Wield Amount ({0})", base1);
                case 277:
                    return Spell.FormatPercent("Chance to Trigger Divine Intervention", base1);
                case 278:
                    // not sure what base1 is. maybe minimum hit damage needed to proc?
                    return String.Format("Add Finishing Blow Proc with up to {0} Damage", base2);
                case 279:
                    return Spell.FormatPercent("Chance to Flurry", value);
                case 280:
                    return Spell.FormatPercent("Pet Chance to Flurry", value);
                case 281:
                    return Spell.FormatPercent("Pet Chance to Feign Death", base1);
                case 282:
                    return Spell.FormatPercent("Bandage Amount", base1);
                case 283:
                    // only special monks attack skills?
                    return Spell.FormatPercent("Chance to Double Special Attack", base1);
                case 285:
                    return Spell.FormatPercent("Chance to Evade", base1);
                case 286:
                    // is added after all other multipliers (focus, crit, etc..)
                    // for DoTs it adds base1/ticks to each tick.
                    return Spell.FormatCount("Spell Damage Bonus", base1);
                case 287:
                    return String.Format("Increase Duration by {0}s", base1 * 6);
                case 288:
                    // this procs the spell associated with the AA
                    return String.Format("Add " + Spell.FormatEnum((SpellSkill)base2) + " Proc with {1}% Rate Mod", base2, base1);
                case 289:
                    // this only triggers if the spell times out. compare with 373
                    return String.Format("Cast: [Spell {0}] on Duration Fade", base1);
                case 290:
                    return Spell.FormatCount("Movement Speed Cap", value);
                case 291:
                    return String.Format("Purify ({0})", value);
                case 292:
                    return String.Format("Strikethrough v2 ({0})", base1);
                case 293:
                    // melee only
                    // clenched jaw aa. 75 seems to be full resist
                    return String.Format("Stun Resist v2 ({0})", base1);
                case 294:
                    // the base2 nuke damage increase only appears on 4 spells after the 2015-7-22 patch 
                    if (base2 > 0)
                        return Spell.FormatPercent("Chance to Critical Nuke", base1) + " and " + Spell.FormatPercent("Critical Nuke Damage v2", base2) + " of Base Damage";
                    else
                        return Spell.FormatPercent("Chance to Critical Nuke", base1);
                case 296:
                    // applied pre-crit
                    return Spell.FormatPercentRange("Base Spell Damage Taken", base1, base2);
                case 297:
                    // applied pre-crit
                    return Spell.FormatCount("Base Spell Damage Taken", base1);
                case 298:
                    return Spell.FormatPercent("Pet Size", value - 100);
                case 299:
                    return String.Format("Wake the Dead ({0})", max);
                case 300:
                    return "Summon Doppelganger: " + Extra;
                case 301:
                    return Spell.FormatPercent("Archery Damage", base1);
                case 302:
                    // see also 124. only used on a few AA
                    return Spell.FormatPercent("Base Spell Damage", base1);
                case 303:
                    // used on type 3 augments
                    // is added before crit multipliers, but after SPA 296 and 302 (and maybe 124)?
                    // for DoTs it adds base1/ticks to each tick.
                    return Spell.FormatCount("Base Spell Damage", base1);
                case 304:
                    // it's worded as 'avoid getting riposted' for Open Wound Effect - however, this may just be chance to avoid offhand riposte
                    return Spell.FormatPercent("Chance to Avoid Riposte", -base1);
                case 305:
                    return Spell.FormatPercent("Offhand Damage Shield Taken", base1);
                case 306:
                    return String.Format("Wake the Dead: {0} x {1} for {2}s", Extra, base1, max);
                case 307:
                    return "Appraisal";
                case 308:
                    return "Suspend Minion";
                case 309:
                    return "Teleport to Caster's Bind";
                case 310:
                    return String.Format("Reduce Timer by {0}", FormatTime(base1 / 1000f));
                case 311:
                    // filter based on field 168 
                    return String.Format("Limit Type: {0} Combat Skills", base1 == 1 ? "Include" : "Exclude");
                case 312:
                    return "Sanctuary";
                case 313:
                    return Spell.FormatPercent("Chance to Double Forage", base1);
                case 314:
                    return "Invisibility";
                case 315:
                    return "Invisibility to Undead";
                case 316:
                    return "Invisibility to Animals";
                case 317:
                    return Spell.FormatCount("HP Regen Cap", value);
                case 318:
                    return Spell.FormatCount("Mana Regen Cap", value);
                case 319:
                    return Spell.FormatPercent("Chance to Critical HoT", value);
                case 320:
                    return String.Format("Shield Block ({0})", value);
                case 321:
                    return Spell.FormatCount("Target's Target Hate", -value);
                case 322:
                    return "Gate to Home City";
                case 323:
                    // max may be some sort of level limit for reducing the proc rate
                    if (base2 != 0)
                        return String.Format("Add Defensive Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                    return String.Format("Add Defensive Proc: [Spell {0}]", base1);
                case 324:
                    // blood magic. uses HP as mana
                    return String.Format("Cast from HP with {0}% Penalty", value);
                case 325:
                    return Spell.FormatPercent("Chance to Remain Hidden When Hit By AE", base1);
                case 326:
                    return Spell.FormatCount("Spell Memorization Gems", base1);
                case 327:
                    return Spell.FormatCount("Buff Slots", base1);
                case 328:
                    return Spell.FormatCount("Max Negative HP", value);
                case 329:
                    return String.Format("Absorb Damage using Mana: {0}%", value);
                case 330:
                    // additive with innate crit multiplier
                    return Spell.FormatPercent("Critical " + Spell.FormatEnum((SpellSkill)base2) + " Damage", base1) + " of Base Damage";
                case 331:
                    return Spell.FormatPercent("Chance to Salvage Components", value);
                case 332:
                    return "Summon to Corpse";
                case 333:
                    // so far this is only used on spells that have a rune
                    return String.Format("Cast: [Spell {0}] on Rune Fade", base1);
                case 334:
                    return Spell.FormatCount("Current HP", value) + repeating + range + " (If Target Not Moving)";
                case 335:
                    if (base1 < 100)
                        return String.Format("Block Next Matching Spell ({0}% Chance)", base1);
                    return "Block Next Matching Spell";
                case 337:
                    return Spell.FormatPercent("Experience Gain", value);
                case 338:
                    return "Summon and Resurrect All Corpses";
                case 339:
                    // compare with 383 which is modified by casting time of triggering spell
                    return String.Format("Cast: [Spell {0}] on Spell Use ({1}% Chance)", base2, base1);
                case 340:
                    // how is this different than 374?
                    if (base1 < 100)
                        return String.Format("Cast: [Spell {0}] ({1}% Chance)", base2, base1);
                    return String.Format("Cast: [Spell {0}]", base2);
                case 341:
                    return Spell.FormatCount("ATK Cap", base1);
                case 342:
                    return "Inhibit Low Health Fleeing";
                case 343:
                    if (base1 < 100)
                        return String.Format("Interrupt Casting ({0}% Chance)", base1);
                    return "Interrupt Casting";
                case 344:
                    return Spell.FormatPercent("Chance to Channel Item Procs", base1);
                case 345:
                    return String.Format("Limit Assassinate Level: {0}", base1);
                case 346:
                    return String.Format("Limit Headshot Level: {0}", base1);
                case 347:
                    return Spell.FormatPercent("Chance of Additional Archery Attack", base1);
                case 348:
                    return String.Format("Limit Min Mana Cost: {0}", base1);
                case 349:
                    // increases weapon damage when a shield is equiped
                    return Spell.FormatPercent("Damage When Shield Equiped", base1);
                case 350:
                    if (base1 > Mana)
                        Mana = base1; // hack
                    return String.Format("Mana Burn up to {0} damage", base1 * -base2 / 10);
                case 351:
                    // the actual aura spell effect reference doesn't seem to be stored in the spell file so we have to handle this SPA
                    // with guesses and some hardcoding. most of the time the effect is placed right after the aura in the spell file
                    int aura = (Rank >= 1) || Extra.Contains("Rk") ? ID + 3 : ID + 1;
                    // hardcoded fixes for failed guesses
                    if (ID == 8629) aura = 8628;
                    if (ID == 8921) aura = 8935;
                    if (ID == 8922) aura = 8936;
                    if (ID == 8923) aura = 8937;
                    if (ID == 8924) aura = 8959;
                    if (ID == 8925) aura = 8938;
                    if (ID == 8926) aura = 8939;
                    if (ID == 8654) aura = 8649;
                    if (ID == 8928) aura = 8940;
                    if (ID == 8929) aura = 8943;
                    if (ID == 8930) aura = 8945;
                    if (ID == 8931) aura = 8946;
                    if (ID == 8932) aura = 8947;
                    if (ID == 8933) aura = 8948;
                    if (ID == 8934) aura = 8949;
                    if (ID == 9000) aura = 9001;
                    if (ID == 9002) aura = 9003;
                    if (ID == 9004) aura = 9005;
                    if (ID == 9006) aura = 9007;
                    if (ID == 9008) aura = 9009;
                    if (ID == 9010) aura = 9011;
                    if (ID == 9012) aura = 9013;
                    if (ID == 9014) aura = 9015;
                    if (ID == 11519) aura = 11539;
                    if (ID == 11520) aura = 11538;
                    if (ID == 11521) aura = 11551;
                    if (ID == 11523) aura = 11540;
                    if (ID == 21827) aura = 21848;
                    if (ID == 32007) aura = 31993;
                    if (ID == 32271) aura = 32257;
                    if (ID == 22510) aura = 22574;
                    if (ID == 22511) aura = 22575;

                    if (Extra.StartsWith("IOQuicksandTrap85")) aura = 22655;
                    if (Extra.StartsWith("IOAuraCantataRk")) aura = 19713 + Rank;
                    if (Extra.StartsWith("IOEncEchoProc95Rk")) aura = 30179 + Rank;
                    if (Extra.StartsWith("IORogTrapAggro92Rk")) aura = 26111 + Rank;
                    if (Extra.StartsWith("IOEncEchoProc100Rk")) aura = 36227 + Rank;
                    if (Extra.StartsWith("IOEncEchoProc105Rk")) aura = 45018 + Rank;

                    return String.Format("Aura Effect: [Spell {0}] ({1})", aura, Extra);
                case 353:
                    return Spell.FormatCount("Aura Slots", base1);
                case 357:
                    // similar to 96, but i think this prevents casting of spells matching limits
                    return "Inhibit Spell Casting";
                case 358:
                    return Spell.FormatCount("Current Mana", value) + range;
                case 359:
                    return Spell.FormatPercent("Chance to Sense Trap", base1);
                case 360:
                    return String.Format("Add Killshot Proc: [Spell {0}] ({1}% Chance)", base2, base1);
                case 361:
                    return String.Format("Cast: [Spell {0}] on Death ({1}% Chance)", base2, base1);
                case 362:
                    return Spell.FormatCount("Potion Belt Slots", base1);
                case 363:
                    return Spell.FormatCount("Bandolier Slots", base1);
                case 364:
                    return Spell.FormatPercent("Chance to Triple Attack", value);
                case 365:
                    // included on nukes and triggered when the nuke kills the mob
                    return String.Format("Cast: [Spell {0}] on Killshot ({1}% Chance)", base2, base1);
                case 367:
                    return String.Format("Transform Body Type to {0}", FormatEnum((SpellBodyType)base1));
                case 368:
                    return Spell.FormatCount("Faction with " + FormatEnum((SpellFaction)base1), base2);
                case 369:
                    return Spell.FormatCount("Corruption Counter", value);
                case 370:
                    return Spell.FormatCount("Corruption Resist", value);
                case 371:
                    return Spell.FormatPercent("Melee Delay", Math.Abs(value));
                case 372:
                    return String.Format("Foraging Skill ({0})", base1);
                case 373:
                    // this appears to be used when a spell is removed via any method: times out, cured, rune depleted, max hits, mez break, etc...
                    // devs call this a "doom" effect
                    return String.Format("Cast: [Spell {0}] on Fade", base1);
                case 374:
                    // very few spells have base1 < 100
                    if (base1 < 100)
                        return String.Format("Cast: [Spell {0}] ({1}% Chance)", base2, base1);
                    return String.Format("Cast: [Spell {0}]", base2);
                case 375:
                    // additive with innate crit multiplier and same effect in other slots
                    return Spell.FormatPercent("Critical DoT Damage", base1) + " of Base Damage";
                case 376:
                    return "Fling";
                case 377:
                    return String.Format("Cast: [Spell {0}] if Not Cured", base1);
                case 378:
                    return Spell.FormatPercent("Chance to Resist " + Spell.FormatEnum((SpellEffect)base2) + " Effects", base1);
                case 379:
                    if (base2 == 0)
                        return String.Format("Shadowstep Forward {0}'", base1);
                    if (base2 == 90)
                        return String.Format("Shadowstep Right {0}'", base1);
                    if (base2 == 180)
                        return String.Format("Shadowstep Back {0}'", base1);
                    if (base2 == 270)
                        return String.Format("Shadowstep Left {0}'", base1);
                    return String.Format("Shadowstep {0}' and {1} Degrees", base1, base2);
                case 380:
                    return String.Format("Push Back {0}' and Up {1}'", base2, base1);
                case 381:
                    return String.Format("Fling to Self ({0}' away)", base1) + maxlevel;
                case 382:
                    return String.Format("Inhibit Effect: {0}", Spell.FormatEnum((SpellEffect)base2), base2);
                case 383:
                    // chance % modified by the cast time of the spell cast that triggers the proc, whereas 339 is not
                    // according to Beimeith:
                    // Cast Time < 2.5 then multiplier = 0.25
                    // Cast Time > 2.5 and < 7 then multiplier = 0.167 * (Cast Time - 1)
                    // Cast Time > 7 then multiplier = 1 * Cast Time / 7
                    string sample383 = String.Format(" e.g. Cast Time 2s={0}% 3s={1:F1}% 4s={2:F1}% 5s={3:F1}%", 0.25 * (base1 / 10), 0.334 * (base1 / 10), 0.5 * (base1 / 10), 0.668 * (base1 / 10));
                    return String.Format("Cast: [Spell {0}] on Spell Use (Base1={1})", base2, base1 / 10) + sample383;
                case 384:
                    return "Fling to Target";
                case 385:
                    return String.Format("Limit Spells: {1}[Group {0}]", Math.Abs(base1), base1 >= 0 ? "" : "Exclude ");
                case 386:
                    return String.Format("Cast: [Spell {0}] on Curer", base1);
                case 387:
                    return String.Format("Cast: [Spell {0}] if Cured", base1);
                case 388:
                    return "Summon All Corpses (From Any Zone)";
                case 389:
                    return "Reset Recast Timers";
                case 390:
                    return "Lockout Recast Timers";
                case 391:
                    return String.Format("Limit Max Mana Cost: {0}", base1);
                case 392:
                    return Spell.FormatCount("Healing Bonus", base1);
                case 393:
                    return Spell.FormatPercentRange("Healing Taken", base1, base2);
                case 394:
                    return Spell.FormatCount("Healing Taken", base1);
                case 395:
                    return Spell.FormatPercent("Chance to Crit Incoming Heal", value);
                case 396:
                    // used on type 3 augments
                    // is added before crit multipliers
                    return Spell.FormatCount("Base Healing", base1);
                case 397:
                    // sturdy companion AA
                    // does this work like defensive disc (168) for warriors?
                    return Spell.FormatPercent("Pet Melee Mitigation", base1 / 100f);
                case 398:
                    return String.Format("Increase Pet Duration by {0}s", base1 / 1000);
                case 399:
                    return Spell.FormatPercent("Chance to Twincast", value);
                case 400:
                    // e.g. Channels the power of sunlight, consuming up to #1 mana to heal your group.
                    // this effect doesn't cause hate - it was added to divine arb as a non-aggro heal
                    Mana = base1; // a bit misleading since the spell will cast with 0 mana and scale the heal
                    Target = SpellTarget.Caster_Group; // total hack but makes sense for current spells
                    return String.Format("Increase Current HP by up to {0} ({1} HP per 1 Mana)", Math.Floor(base1 * base2 / 10f), base2 / 10f);
                case 401:
                    return String.Format("Decrease Current HP by up to {0} ({1} HP per 1 Target Mana)", Math.Floor(base1 * base2 / -10f), base2 / -10f);
                case 402:
                    return String.Format("Decrease Current HP by up to {0} ({1} HP per 1 Target End)", Math.Floor(base1 * base2 / -10f), base2 / -10f);
                case 403:
                    return String.Format("Limit Spell Class: {0}{1}", base1 >= 0 ? "" : "Exclude ", Spell.FormatEnum((SpellCategory)Math.Abs(base1)));
                case 404:
                    return String.Format("Limit Spell Subclass: {0}{1}", base1 >= 0 ? "" : "Exclude ", Math.Abs(base1));
                case 405:
                    return Spell.FormatPercent("Staff Block Chance", base1);
                case 406:
                    return String.Format("Cast: [Spell {0}] on Max Hits", base1);
                case 407:
                    // this is a guess. haven't tested this
                    return String.Format("Cast: [Spell {0}] on Hit By Spell", base1);
                case 408:
                    // target will still have normal max HP but cannot be regen/heal past the cap
                    if (base2 > 0)
                        return String.Format("Cap HP at lowest of {0}% or {1}", base1, base2);
                    return String.Format("Cap HP at {0}%", base1);
                case 409:
                    if (base2 > 0)
                        return String.Format("Cap Mana at lowest of {0}% or {1}", base1, base2);
                    return String.Format("Cap Mana at {0}%", base1);
                case 410:
                    if (base2 > 0)
                        return String.Format("Cap Endurance at lowest of {0}% or {1}", base1, base2);
                    return String.Format("Cap Endurance at {0}%", base1);
                case 411:
                    return String.Format("Limit Class: {0}", (SpellClassesMask)(base1 >> 1));
                case 412:
                    return String.Format("Limit Race: {0}", base1);
                case 413:
                    return Spell.FormatPercent("Spell Effectiveness", value);
                case 414:
                    return String.Format("Limit Casting Skill: {0}", Spell.FormatEnum((SpellSkill)base1));
                case 416:
                    // SPA 416 functions exactly like SPA 1, it was added so that we could avoid stacking conflicts with only 12 spell slots. - Dzarn
                    return Spell.FormatCount("AC v2", (int)(value / (10f / 3f)));
                case 417:
                    // same as 15 and used for stacking
                    return Spell.FormatCount("Current Mana v2", value) + repeating + range;
                case 418:
                    // same as 220 and used for stacking
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Damage Bonus v2", base1);
                case 419:
                    // this is used for potions. how is it different than 85? maybe proc rate?
                    if (base2 != 0)
                        return String.Format("Add Melee Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                    return String.Format("Add Melee Proc: [Spell {0}]", base1);
                case 421:
                    return Spell.FormatCount("Max Hits Counter", base1);
                case 422:
                    return String.Format("Limit Max Hits Min: {0}", base1);
                case 423:
                    return String.Format("Limit Max Hits Type: {0}", Spell.FormatEnum((SpellMaxHits)base1));
                case 424:
                    return String.Format("Gradual {0} to {2}' away (Force={1})", base1 > 0 ? "Push" : "Pull", Math.Abs(base1), base2) + maxlevel;
                case 425:
                    return "Fly";
                case 426:
                    return Spell.FormatCount("Extended Target Window Slots", base1);
                case 427:
                    // not sure how this works. base2 / 10 doesn't seem to be the correct chance.
                    // raising base2 increases the frequency of the cast.
                    // it may only have an opportunity to fire once a round or maybe once per some timespan?
                    return String.Format("Cast: [Spell {0}] on Skill Use ({1})", base1, base2);
                case 428:
                    return String.Format("Limit Skill: {0}", Spell.FormatEnum((SpellSkill)base1));
                case 429:
                    if (base2 != 0)
                        return String.Format("Add Skill Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                    return String.Format("Add Skill Proc: [Spell {0}]", base1);
                case 430:
                    return String.Format("Alter Vision: Base1={0} Base2={1} Max={2}", base1, base2, max);
                case 431:
                    // base1 tints
                    // base2 blurs?
                    if (base1 < 0)
                        return String.Format("Tint Vision: Red={0} Green={1} Blue={2}", base1 >> 16 & 0xff, base1 >> 8 & 0xff, base1 & 0xff);
                    return String.Format("Alter Vision: Base1={0} Base2={1} Max={2}", base1, base2, max);
                case 432:
                    return Spell.FormatCount("Trophy Slots", base1);
                case 433:
                    // similar to 220 except the values get lowered with faster weapons 
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Damage Bonus v2", base1);
                case 434:
                    // similar to 220 except the values get lowered with faster weapons 
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Damage Bonus v3", base1);
                case 435:
                    return String.Format("Fragile Defense ({0})", base1);
                case 436:
                    return "Beneficial Countdown Hold";
                case 437:
                    return "Teleport to your " + FormatEnum((SpellTeleport)base1);
                case 438:
                    return "Teleport to their " + FormatEnum((SpellTeleport)base1);
                case 439:
                    return String.Format("Add Assasinate Proc with up to {0} Damage", base2);
                case 440:
                    return String.Format("Limit Finishing Blow Level: {0}", base1);
                case 441:
                    return String.Format("Cancel if Moved {0}", base1);
                case 442:
                    return String.Format("Cast: [Spell {0}] once if {1}", base1, Spell.FormatEnum((SpellTargetRestrict)base2));
                case 443:
                    return String.Format("Cast: [Spell {0}] once if Caster {1}", base1, Spell.FormatEnum((SpellTargetRestrict)base2));
                case 444:
                    return "Lock Aggro on Caster and " + Spell.FormatPercent("Other Aggro", base2 - 100) + String.Format(" up to level {0}", base1);
                case 445:
                    return String.Format("Grant {0} Mercenary Slots", base1);
                case 446:
                    // no idea how these 4 buff blockers work
                    return String.Format("Buff Blocker A ({0})", base1);
                case 447:
                    return String.Format("Buff Blocker B ({0})", base1);
                case 448:
                    return String.Format("Buff Blocker C ({0})", base1);
                case 449:
                    return String.Format("Buff Blocker D ({0})", base1);
                case 450:
                    return String.Format("Absorb DoT Damage: {0}% over {1}", base1, base2) + (max > 0 ? String.Format(", Total: {0}", max) : "");
                case 451:
                    return String.Format("Absorb Melee Damage: {0}% over {1}", base1, base2) + (max > 0 ? String.Format(", Total: {0}", max) : "");
                case 452:
                    return String.Format("Absorb Spell Damage: {0}% over {1}", base1, base2) + (max > 0 ? String.Format(", Total: {0}", max) : "");
                case 453:
                    return String.Format("Cast: [Spell {0}] if {1} Melee Damage Taken in Single Hit", base1, base2);
                case 454:
                    return String.Format("Cast: [Spell {0}] if {1} Spell Damage Taken in Single Hit", base1, base2);
                case 455:
                    // adds a % of your own hate using base1. Example: 1000 hate base1 = 50. Means you will be 1500 hate.
                    return Spell.FormatPercent("Current Hate", base1);
                case 456:
                    // adds a % of your own hate using base1, per tick, scalable. Example: 1000 hate base1 = 50. Means you will be 1500 hate @ 1 tick, 2250 @ 2 ticks.
                    return Spell.FormatPercent("Current Hate", base1) + " per tick";
                case 457:
                    // offical name is "Resource Tap." Formula is base1 / 1000 * damage value. Example: 88001 damage, base1 = 100. 100 / 1000 = .1 * 88001.
                    // simply dividing by 10 gives the same result.
                    return string.Format("Return {0}% of Damage as {1}", base1 / 10, new string[] { "HP", "Mana", "Endurance" }[base2 % 3]) + (max > 0 ? String.Format(", Max Per Hit: {0}", max) : "");
                case 458:
                    // -100 = no faction hit, 100 = double faction
                    return Spell.FormatPercent("Faction Hit", base1);
                case 459:
                    // same as 185, created to stack
                    return Spell.FormatPercent(Spell.FormatEnum((SpellSkill)base2) + " Damage v2", base1);
                case 460:
                    // some spells are tagged as non focusable (field 197) this overrides that
                    return "Limit Type: Include Non-Focusable";
                case 463:
                    // same as /shield command?
                    return Spell.FormatPercent("Melee Shielding: {0}%", base1);
                case 464:
                    return Spell.FormatPercent("Pet Chance to Rampage", base1);
                case 465:
                    return Spell.FormatPercent("Pet Chance to AE Rampage", base1);
                case 466:
                    // this chance is additive with the owner's passive pet flurry chance AA abilities.
                    // how does this differ from 280?
                    return Spell.FormatPercent("Pet Chance to Flurry", base1);
                case 467:
                    return Spell.FormatCount("Damage Shield Taken", base1);
                case 468:
                    return Spell.FormatPercent("Damage Shield Taken", base1);
                case 469:
                    // 469/470 seem to be similar to spa 340/374 except the cast a spell by group ID rather than spell ID
                    // is the chance on this shared with other chance SPAs (i.e. only 1 can be cast)?
                    return String.Format("Cast: [Group {0}] ({1}% Chance)", base2, base1);
                case 470:
                    // is the chance on this independant of other chance SPAs (i.e. each one has it's own chance to cast)?
                    if (base1 < 100)
                        return String.Format("Cast: [Group {0}] ({1}% Chance)", base2, base1);
                    return String.Format("Cast: [Group {0}]", base2);
                case 471:
                    // add an extra melee round. i.e. main attack, double attack, triple
                    // this is sort of like 211 AE attack except it was added to nerf DPS by only affecting the current target
                    return Spell.FormatPercent("Chance to Repeat Melee Round", base1) + (base2 != 100 ? String.Format(" with {0}% Damage", base2) : "");
                case 472:
                    return String.Format("Buy AA Rank ({0})", base1);
                case 473:
                    return Spell.FormatPercent("Chance to Double Backstab From Front", base1);
                case 474:
                    // similar to 330
                    return Spell.FormatPercent("Pet Critical Hit Damage", base1) + " of Base Damage";
                case 475:
                    // only activates if the spell is being cast from memory rather than an item
                    return String.Format("Cast: [Spell {0}] if not click", base2);
                case 476:
                    return String.Format("Weapon Stance ({0})", base1);
                case 477:
                    return String.Format("Move to top of Hatelist ({0}% Chance)", base1);
                case 478:
                    return String.Format("Move to bottom of Hatelist ({0}% Chance)", base1);
                //479 Ff_Value_Min
                //480 Ff_Value_Max
                //481 Fc_Cast_Spell_On_Land
                //482 Skill Base Damage Mod
                case 483:
                    return Spell.FormatPercentRange("Spell Damage Taken", base1, base2);
                case 484:
                    // Modifies incoming spell damage by Base1 points. Applies post-crit for both instant damage and DoTs.
                    // Differs from 297 which applies pre-crit to both instant damage and DoTs. 
                    return Spell.FormatCount("Spell Damage Taken", base1);
                //485 Ff_CasterClass
                //486 Ff_Same_Caster
            }

            return String.Format("Unknown SPA={0} Base1={1} Base2={2} Max={3} Calc={4} Value={5}", spa, base1, base2, max, calc, value);
        }

        /// <summary>
        /// Calculate a duration.
        /// </summary>
        /// <returns>Numbers of ticks (6 second units)</returns>
        public static int CalcDuration(int calc, int max, int level)
        {
            int value = 0;

            switch (calc)
            {
                case 0:
                    value = 0;
                    break;
                case 1:
                    value = level / 2;
                    if (value < 1)
                        value = 1;
                    break;
                case 2:
                    value = (level / 2) + 5;
                    if (value < 6)
                        value = 6;
                    break;
                case 3:
                    value = level * 30;
                    break;
                case 4:
                    value = 50;
                    break;
                case 5:
                    value = 2;
                    break;
                case 6:
                    value = level / 2;
                    break;
                case 7:
                    value = level;
                    break;
                case 8:
                    value = level + 10;
                    break;
                case 9:
                    value = level * 2 + 10;
                    break;
                case 10:
                    value = level * 30 + 10;
                    break;
                case 11:
                    value = (level + 3) * 30;
                    break;
                case 12:
                    value = level / 2;
                    if (value < 1)
                        value = 1;
                    break;
                case 13:
                    value = level * 3 + 10;
                    break;
                case 14:
                    value = (level + 2) * 5;
                    break;
                case 15:
                    value = (level + 10) * 10;
                    break;
                case 50:
                    value = 72000;
                    break;
                case 3600:
                    value = 3600;
                    break;
                default:
                    value = max;
                    break;
            }

            if (max > 0 && value > max)
                value = max;

            return value;
        }

        /// <summary>
        /// Calculate a level/tick scaled value.
        /// </summary>
        public static int CalcValue(int calc, int base1, int max, int tick, int level)
        {
            if (calc == 0)
                return base1;

            if (calc == 100)
            {
                if (max > 0 && base1 > max)
                    return max;
                return base1;
            }

            int change = 0;

            switch (calc)
            {
                case 100:
                    break;
                case 101:
                    change = level / 2;
                    break;
                case 102:
                    change = level;
                    break;
                case 103:
                    change = level * 2;
                    break;
                case 104:
                    change = level * 3;
                    break;
                case 105:
                    change = level * 4;
                    break;
                case 107:
                    change = -1 * tick;
                    break;
                case 108:
                    change = -2 * tick;
                    break;
                case 109:
                    change = level / 4;
                    break;
                case 110:
                    change = level / 6;
                    break;
                case 111:
                    if (level > 16) change = (level - 16) * 6;
                    break;
                case 112:
                    if (level > 24) change = (level - 24) * 8;
                    break;
                case 113:
                    if (level > 34) change = (level - 34) * 10;
                    break;
                case 114:
                    if (level > 44) change = (level - 44) * 15;
                    break;
                case 115:
                    if (level > 15) change = (level - 15) * 7;
                    break;
                case 116:
                    if (level > 24) change = (level - 24) * 10;
                    break;
                case 117:
                    if (level > 34) change = (level - 34) * 13;
                    break;
                case 118:
                    if (level > 44) change = (level - 44) * 20;
                    break;
                case 119:
                    change = level / 8;
                    break;
                case 120:
                    change = -5 * tick;
                    break;
                case 121:
                    change = level / 3;
                    break;
                case 122:
                    change = -12 * tick;
                    break;
                case 123:
                    // random in range
                    change = (Math.Abs(max) - Math.Abs(base1)) / 2;
                    break;
                case 124:
                    if (level > 50) change = (level - 50);
                    break;
                case 125:
                    if (level > 50) change = (level - 50) * 2;
                    break;
                case 126:
                    if (level > 50) change = (level - 50) * 3;
                    break;
                case 127:
                    if (level > 50) change = (level - 50) * 4;
                    break;
                case 128:
                    if (level > 50) change = (level - 50) * 5;
                    break;
                case 129:
                    if (level > 50) change = (level - 50) * 10;
                    break;
                case 130:
                    if (level > 50) change = (level - 50) * 15;
                    break;
                case 131:
                    if (level > 50) change = (level - 50) * 20;
                    break;
                case 132:
                    if (level > 50) change = (level - 50) * 25;
                    break;
                case 139:
                    if (level > 30) change = (level - 30) / 2;
                    break;
                case 140:
                    if (level > 30) change = (level - 30);
                    break;
                case 141:
                    if (level > 30) change = 3 * (level - 30) / 2;
                    break;
                case 142:
                    if (level > 30) change = 2 * (level - 60);
                    break;
                case 143:
                    change = 3 * level / 4;
                    break;

                case 3000:
                    // todo: this appears to be scaled by the targets level
                    // base1 value how it affects a level 100 target
                    return base1;

                default:
                    if (calc > 0 && calc < 1000)
                        change = level * calc;

                    // 1000..1999 variable by tick
                    // e.g. splort (growing): Effect=0 Base1=1 Base2=0 Max=0 Calc=1035
                    //      34 - 69 - 104 - 139 - 174 - 209 - 244 - 279 - 314 - 349 - 384 - 419 - 454 - 489 - 524 - 559 - 594 - 629 - 664 - 699 - 699
                    // e.g. venonscale (decaying): Effect=0 Base1=-822 Base2=0 Max=822 Calc=1018
                    //
                    // e.g. Deathcloth Spore: Base1=-1000 Base2=0 Max=0 Calc=1999
                    // e.g. Bleeding Bite: Base1=-1000 Base2=0 Max=0 Calc=1100 (The damage done will decrease in severity over time.)
                    // e.g. Blood Rites: Base1=-1500 Base2=0 Max=0 Calc=1999
                    if (calc >= 1000 && calc < 2000)
                        change = tick * (calc - 1000) * -1;

                    // 2000..2999 variable by level
                    if (calc >= 2000)
                        change = level * (calc - 2000);
                    break;
            }

            int value = Math.Abs(base1) + change;

            if (max != 0 && value > Math.Abs(max))
                value = Math.Abs(max);

            if (base1 < 0)
                value = -value;

            return value;
        }

        /// <summary>
        /// Calculate the min/max values for a scaled value.
        /// </summary>
        public static string CalcValueRange(int calc, int base1, int max, int duration, int level)
        {
            int start = CalcValue(calc, base1, max, 1, level);
            int finish = Math.Abs(CalcValue(calc, base1, max, duration, level));

            string type = Math.Abs(start) < Math.Abs(finish) ? "Growing" : "Decaying";

            if (calc == 123)
                return String.Format(" (Random: {0} to {1})", base1, max * ((base1 >= 0) ? 1 : -1));

            if (calc == 107)
                return String.Format(" ({0} to {1} @ 1/tick)", type, finish);

            if (calc == 108)
                return String.Format(" ({0} to {1} @ 2/tick)", type, finish);

            if (calc == 120)
                return String.Format(" ({0} to {1} @ 5/tick)", type, finish);

            if (calc == 122)
                return String.Format(" ({0} to {1} @ 12/tick)", type, finish);

            if (calc > 1000 && calc < 2000)
                return String.Format(" ({0} to {1} @ {2}/tick)", type, finish, calc - 1000);

            return null;
        }

        public override string ToString()
        {
            if (GroupID <= 0)
                return String.Format("[{0}] {1}", ID, Name);
            return String.Format("[{0}/{2}] {1}", ID, Name, GroupID);
        }

        /// <summary>
        /// Get a full description of the spell. This is mostly useful as a debug dump.
        /// </summary>
        public string[] Details()
        {
            var result = new List<string>(20);
            //Action<string> Add = delegate(string s) { result.Add(s); };


            if (!String.IsNullOrEmpty(ClassesLevels))
            {
                result.Add("Classes: " + ClassesLevels);
                // the skill field is full of random values for spells that aren't PC castable so it's best to hide it 
                if (SongCap > 0)
                    result.Add("Skill: " + FormatEnum(Skill) + ", Cap: " + SongCap);
                else if (CombatSkill)
                    result.Add("Skill: " + FormatEnum(Skill) + " (Combat Skill)");
                else
                    result.Add("Skill: " + FormatEnum(Skill));
            }

            if (!String.IsNullOrEmpty(Deity))
                result.Add("Deity: " + Deity);

            if (Mana > 0)
                result.Add("Mana: " + Mana);

            if (EnduranceUpkeep > 0)
                result.Add("Endurance: " + Endurance + ", Upkeep: " + EnduranceUpkeep + " per tick");
            else if (Endurance > 0)
                result.Add("Endurance: " + Endurance);

            for (int i = 0; i < ConsumeItemID.Length; i++)
                if (ConsumeItemID[i] > 0)
                    result.Add("Consumes: [Item " + ConsumeItemID[i] + "] x " + ConsumeItemCount[i]);

            for (int i = 0; i < FocusID.Length; i++)
                if (FocusID[i] > 0)
                    result.Add("Focus: [Item " + FocusID[i] + "]");

            if (BetaOnly)
                result.Add("Restriction: Beta Only");

            if (CannotRemove)
                result.Add("Restriction: Cannot Remove");

            if (CastOutOfCombat)
                result.Add("Restriction: Out of Combat"); // i.e. no aggro

            if (CastInFastRegen)
                result.Add("Restriction: In Fast Regen");

            if (Zone != SpellZoneRestrict.None)
                result.Add("Restriction: " + Zone + " Only");

            if (Sneaking)
                result.Add("Restriction: Sneaking");

            if (CancelOnSit)
                result.Add("Restriction: Cancel on Sit");

            if ((int)CasterRestrict > 100)
                result.Add("Restriction: " + FormatEnum(CasterRestrict));

            if (Target == SpellTarget.Directional_AE)
                result.Add("Target: " + FormatEnum(Target) + " (" + ConeStartAngle + " to " + ConeEndAngle + " Degrees)");
            else if (TargetRestrict > 0)
                result.Add("Target: " + FormatEnum(Target) + " (If " + FormatEnum(TargetRestrict) + ")");
            else if ((Target == SpellTarget.Caster_Group || Target == SpellTarget.Target_Group) && (ClassesMask != 0 && ClassesMask != SpellClassesMask.BRD) && DurationTicks > 0)
                result.Add("Target: " + FormatEnum(Target) + ", MGB: " + (MGBable ? "Yes" : "No"));
            else
                result.Add("Target: " + FormatEnum(Target));

            if (AERange > 0 && Range == 0)
                result.Add("AE Range: " + (MinRange > 0 ? MinRange + "' to " : "") + AERange + "'");
            else if (AERange > 0)
                result.Add("Range: " + Range + "', AE Range: " + (MinRange > 0 ? MinRange + "' to " : "") + AERange + "'"); // unsure where the min range should be applied
            else if (Range > 0)
                result.Add("Range: " + (MinRange > 0 ? MinRange + "' to " : "") + Range + "'");

            if (RangeModFarDist != 0)
                result.Add("Range Based Mod: " + (RangeModCloseMult * 100) + "% at " + RangeModCloseDist + "' to " + (RangeModFarMult * 100) + "% at " + RangeModFarDist + "'");

            if (ViralRange > 0)
                result.Add("Viral Range: " + ViralRange + "', Recast: " + MinViralTime + "s to " + MaxViralTime + "s");

            if (!Beneficial)
                result.Add("Resist: " + ResistType + (ResistMod != 0 ? " " + ResistMod : "") + (MinResist > 0 ? ", Min Resist Chance: " + MinResist / 2f + "%" : "") + (MaxResist > 0 ? ", Max Resist Chance: " + MaxResist / 2f + "%" : "")); // + (!PartialResist ? ", No Partials" : ""));
            else
                result.Add("Resist: Beneficial, Blockable: " + (BeneficialBlockable ? "Yes" : "No"));

            if (Stacking.Count > 0)
                result.Add("Stacking: " + String.Join(", ", Stacking.ToArray()));


            //if (ResistPerLevel != 0)
            //    result.Add("Resist Per Level: " + ResistPerLevel + ", Cap: " + ResistCap);

            // this includes both spell and AA focuses
            result.Add("Focusable: " + (Focusable ? "Yes" : "No"));

            // only nukes and DoT can trigger spell damage shields
            // no points showing it for NPC spells since NPCs will never take significant damage from nuking players
            if (!Beneficial && ClassesMask != 0 && HasEffect("Decrease Current HP", 0))
                result.Add("Trigger Spell DS: " + (Feedbackable ? "Yes" : "No"));

            if (!Beneficial)
                result.Add("Reflectable: " + (Reflectable ? "Yes" : "No"));

            //if (!Beneficial && DurationTicks > 0 && HasEffect("Decrease Current HP", 0))
            //    result.Add("Stackable: " + (Stackable ? "Yes" : "No"));

            string rest = ClassesMask == 0 || ClassesMask == SpellClassesMask.BRD || RestTime == 0 ? "" : ", Rest: " + RestTime.ToString() + "s";
            if (TimerID > 0)
                result.Add("Casting: " + CastingTime.ToString() + "s, Recast: " + FormatTime(RecastTime) + ", Timer: " + TimerID + rest);
            else if (RecastTime > 0)
                result.Add("Casting: " + CastingTime.ToString() + "s, Recast: " + FormatTime(RecastTime) + rest);
            else
                result.Add("Casting: " + CastingTime.ToString() + "s" + rest);

            if (DurationTicks > 0)
                result.Add("Duration: " + FormatTime(DurationTicks * 6) + " (" + DurationTicks + " ticks)"
                    + (SongWindow ? " Song" : "")
                    + (Beneficial && ClassesMask != SpellClassesMask.BRD ? ", Extendable: " + (Focusable ? "Yes" : "No") : "")
                    + ", Dispelable: " + (Dispelable ? "Yes" : "No")
                    + (!Beneficial && DurationTicks > 10 ? ", Allow Fast Regen: " + (AllowFastRegen ? "Yes" : "No") : "")  // it applies on <10 ticks, but there really is no need to show it for short term debuffs 
                    + (PersistAfterDeath ? ", Persist After Death" : "")); // pretty rare, so only shown when it's used
            else if (AEDuration >= 2500)
                result.Add("AE Waves: " + AEDuration / 2500);

            if (PushUp != 0)
                result.Add("Push: " + PushBack + "' Up: " + PushUp + "'");
            else if (PushBack != 0)
                result.Add("Push: " + PushBack + "'");

            if (HateMod != 0)
                result.Add("Hate Mod: " + HateMod.ToString("+#;-#;0"));

            if (HateOverride != 0)
                result.Add("Hate: " + HateOverride);

            if (CritOverride > 0)
                result.Add("Max Crit Chance: " + CritOverride + "%");

            if (MaxHits > 0)
                result.Add("Max Hits: " + MaxHits + " " + FormatEnum((SpellMaxHits)MaxHitsType));

            if (MaxTargets > 0)
                result.Add("Max Targets: " + MaxTargets);

            if (Recourse != null)
                result.Add("Recourse: " + Recourse);

            if (Unknown != 0)
                result.Add("Unknown: " + Unknown);

            //if (!String.IsNullOrEmpty(Category))
            //    result.Add("Category: " + Category);

            for (int i = 0; i < Slots.Count; i++)
                if (Slots[i] != null)
                    result.Add(String.Format("{0}: {1}", i + 1, Slots[i].Desc));

            if (!String.IsNullOrEmpty(LandOnSelf))
                result.Add("Text: " + LandOnSelf);

            return result.ToArray();
        }

        /// <summary>
        /// Finalize spell data after all the attributes have been loaded.
        /// </summary>
        public void Prepare()
        {
            ClassesLevels = String.Empty;
            ClassesMask = 0;
            bool All254 = true;
            for (int i = 0; i < Levels.Length; i++)
            {
                // level 255 means the class can't use this spell (except for bards)
                if (Levels[i] == 255)
                    Levels[i] = 0;
                if (Levels[i] != 254 && (i + 1) != (int)SpellClasses.BRD)
                    All254 = false;
                if (Levels[i] != 0)
                {
                    ClassesMask |= (SpellClassesMask)(1 << i);
                    ClassesLevels += " " + (SpellClasses)(i + 1) + "/" + Levels[i];
                }
            }
            Array.Copy(Levels, ExtLevels, Levels.Length);
            ClassesLevels = ClassesLevels.TrimStart();
            if (All254)
                ClassesLevels = "ALL/254";

            if (MaxHitsType == SpellMaxHits.None || (DurationTicks == 0 && !Name.Contains("Aura")))
                MaxHits = 0;

            if (Target == SpellTarget.Caster_PB)
            {
                Range = 0;
            }

            if (Target == SpellTarget.Self)
            {
                Range = 0;
                AERange = 0;
                MaxTargets = 0;
            }

            if (Target == SpellTarget.Single)
            {
                AERange = 0;
                MaxTargets = 0;
            }

            if (ResistType == SpellResist.Unresistable)
                ResistMod = 0;

            if (Zone != SpellZoneRestrict.Indoors && Zone != SpellZoneRestrict.Outdoors)
                Zone = SpellZoneRestrict.None;

            if (RangeModCloseDist == RangeModFarDist)
            {
                RangeModCloseDist = RangeModFarDist = 0;
                RangeModCloseMult = RangeModFarMult = 0;
            }
        }

        /// <summary>
        /// Search all effect slots using a SPA match.
        /// </summary>
        /// <param name="slot">0 to check all slots, or a non zero value to check a specific slot.</param>
        public bool HasEffect(int spa, int slot)
        {
            if (slot > 0)
                return slot <= Slots.Count && Slots[slot - 1] != null && Slots[slot - 1].SPA == spa;

            for (int i = 0; i < Slots.Count; i++)
                if (Slots[i] != null && Slots[i].SPA == spa)
                    return true;

            return false;
        }

        /// <summary>
        /// Search all effect slots using a text match.
        /// </summary>
        /// <param name="desc">Effect to search for. Can be text or a integer representing an SPA.</param>
        /// <param name="slot">0 to check all slots, or a non zero value to check a specific slot.</param>
        public bool HasEffect(string text, int slot)
        {
            int spa;
            if (Int32.TryParse(text, out spa))
                return HasEffect(spa, slot);

            if (slot > 0)
                return slot <= Slots.Count && Slots[slot - 1] != null && Slots[slot - 1].Desc.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0;

            for (int i = 0; i < Slots.Count; i++)
                if (Slots[i] != null && Slots[i].Desc.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return true;

            return false;
        }

        /// <summary>
        /// Search all effect slots using a regular expression.
        /// </summary>
        /// <param name="slot">0 to check all slots, or a non zero value to check a specific slot.</param>
        public bool HasEffect(Regex re, int slot)
        {
            if (slot > 0)
                return slot <= Slots.Count && Slots[slot - 1] != null && re.IsMatch(Slots[slot - 1].Desc);

            for (int i = 0; i < Slots.Count; i++)
                if (Slots[i] != null && re.IsMatch(Slots[i].Desc))
                    return true;

            return false;
        }

        /// <summary>
        /// Sum all the spell slots effects that match a regex. The regex must have a capturing group for an integer value.
        /// e.g. Increase Current HP by (\d+)
        /// </summary>
        public int ScoreEffect(Regex re)
        {
            int score = 0;
            for (int i = 0; i < Slots.Count; i++)
                if (Slots[i] != null)
                {
                    Match m = re.Match(Slots[i].Desc);
                    if (m.Success)
                        score += Int32.Parse(m.Groups[1].Value);
                }

            return score;
        }

        static private string FormatEnum(Enum e)
        {
            string type = e.ToString().Replace("_", " ").Trim();
            if (Regex.IsMatch(type, @"^-?\d+$"))
                type = "Type " + type; // undefined numeric enum
            //else
            //    type = Regex.Replace(type, @"(^[\dv])\d$", "$1"); // remove numeric suffix on duplicate enums undead3/summoned3/etc
            return type;
        }

        public static string FormatTime(float seconds)
        {
            if (seconds < 120)
                return seconds.ToString("0.##") + "s";

            if (seconds < 7200)
                return (seconds / 60f).ToString("0.#") + "m";

            return (seconds / 3600f).ToString("0.#") + "h";

            //return new TimeSpan(0, 0, (int)seconds).ToString();
        }

        static private string FormatCount(string name, int value)
        {
            return String.Format("{0} {1} by {2}", value < 0 ? "Decrease" : "Increase", name, Math.Abs(value));
        }

        static private string FormatPercent(string name, float value)
        {
            return String.Format("{0} {1} by {2:0.#}%", value < 0 ? "Decrease" : "Increase", name, Math.Abs(value));
        }

        static private string FormatPercentRange(string name, int min, int max)
        {
            return FormatPercentRange(name, min, max, false);
        }

        static private string FormatPercentRange(string name, int min, int max, bool negate)
        {
            if (min < 0)
            {
                // for negative min values, min < max is valid but should be swapped
                // e.g. cleric vow spells: min=-50 max=0, decrease healing by 0% to 50% (not sure why they didn't use min=0 max=-50)
                if (min < max)
                {
                    //return String.Format("{0} {1}", min, max);
                    int temp = min;
                    min = max;
                    max = temp;
                }
            }
            else
            {
                // for positive min values, min < max is bad data and max should be ignored
                if (min > max)
                    max = min;
            }

            // some effects like 'increase mana conservation' use negated wording 'decrease mana cost' 
            if (negate)
            {
                min = -min;
                max = -max;
            }

            if (min == max)
                return String.Format("{0} {1} by {2:0.#}%", max < 0 ? "Decrease" : "Increase", name, Math.Abs(min));

            return String.Format("{0} {1} by {2:0.#}% to {3:0.#}%", max < 0 ? "Decrease" : "Increase", name, Math.Abs(min), Math.Abs(max));
        }


        /*
        static private string FormatDesc()
        {
            // Spell descriptions include references to spell slots. e.g.
            // #7 = base1 for slot 7
            // @7 = calc(base1) for slot 7
            // $7 = base2 for slot 7
            return null;
        }
        */
    }

}