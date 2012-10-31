using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;



/*
 *
 * http://code.google.com/p/projecteqemu/source/browse/trunk/EQEmuServer/zone/spdat.h
 * http://forums.station.sony.com/eq/posts/list.m?start=150&topic_id=162971
 * http://forums.station.sony.com/eq/posts/list.m?start=50&topic_id=165000 - resists
 *
 */

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
        SHM = 512, NEC = 1024, WIZ = 2048, MAG = 4096, ENC = 8192, BST = 16384, BER = 32768
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
        Memory_Blur = 63,
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
        Max_Mana = 97,
        Root = 99,
        Current_HP_Repeating = 100,
        Donals_Heal = 101,
        Translocate = 104,
        All_Resists = 111,
        Aggro_Mult = 114,
        Curse_Counter = 116,
        Spell_Damage_Focus = 124,
        Healing_Focus = 125,
        Haste_Focus = 127,
        Duration_Focus = 128,
        Mana_Cost_Focus = 132,
        Current_HP_Percent = 147,
        Cure_Detrimental = 154,
        Spell_Rune = 161,
        Melee_Rune = 162,
        Absorb_Hits = 163,
        Melee_Mitigation = 168,
        Critical_Hit_Chance = 169,
        Crippling_Blow_Chance = 171,
        Avoid_Melee_Chance = 172, // combat agility AA?
        Riposte_Chance = 173,
        Dodge_Chance = 174,
        Parry_Chance = 175,
        Lifetap_From_Weapon = 178,
        Weapon_Delay = 182,
        Hit_Chance = 184,
        Block_Chance = 188,
        Endurance_Repeating = 189,
        Hate_Repeating = 192,
        Skill_Attack = 193,
        Cancel_All_Aggro = 194,
        Stun_Resist_Chance = 195,
        Taunt = 199,
        Proc_Rate = 200,
        Slay_Undead = 219,
        Weapon_Damage_Bonus = 220,
        Lung_Capacity = 246,
        Frontal_Backstab_Chance = 252,
        Frontal_Backstab_Min_Damage = 253,
        Shroud_Of_Stealth = 256,
        Triple_Backstab_Chance = 258,
        Combat_Stability = 259, // ac soft cap. AA and a few shaman spells
        Song_Range = 270,
        Flurry = 279,
        Spell_Damage_Bonus = 286,
        Dispel_Detrimental = 291,
        Movement_Speed_AA = 271,
        Critical_DoT_Chance = 273,
        Critical_Heal_Chance = 274,
        Double_Special_Attack_Chance = 283, // monk specials
        Movement_Speed_Cap = 290,
        Frontal_Stun_Resist_Chance = 293, // AA
        Critical_Nuke_Chance = 294,
        Archery_Damage = 301, // AA, not sure which
        Damage_Shield_Taken = 305,
        Teleport_To_Bind = 309,
        Invis = 314,
        Targets_Target_Hate = 321,
        Gate_to_Home_City = 322,
        Crit_Hit_Damage = 330,
        Summon_To_Corpse = 332,
        XP_Gain = 337,
        Casting_Trigger = 339,
        Mana_Burn = 350,
        Current_Mana = 358,
        Triple_Attack = 364,
        Corruption_Counter = 369,
        Corruption_Resist = 370,
        Melee_Delay = 371,
        Crit_DoT_Damage = 375,
        Push = 379,
        Cast_On_Spell = 383,
        Healing_Taken = 393,
        Healing_Taken2 = 394,
        Crit_DoT_Chance = 395,
        Pet_Duration = 398,
        Twincast_Chance = 399,
        Heal_From_Mana = 400,
        Song_Effectiveness = 413,
        Teleport_to_Caster_Anchor = 437,
        Teleport_to_Player_Anchor = 438
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
        Piercing = 36,
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
        Make_Poison = 56,
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
        Undead = 3,
        Extraplanar = 6,
        Vampyre = 12,
        Greater_Akheva = 14,
        Draz_Nurakk = 18,
        Zek = 19,
        Luggald = 20,        
        Animal = 21,
        Elemental = 24,
        Plant = 25,
        Dragonkin = 26,
        Summoned = 28,
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
        Undead_AE = 24,
        Summoned_AE = 25,
        Hatelist = 32,
        Hatelist2 = 33,
        Chest = 34,
        Special_Muramites = 35, // bane for Ingenuity group trial in MPG
        Caster_PB_Players = 36,
        Caster_PB_NPC = 37,
        Pet2 = 38,
        No_Pets = 39, // single/group/ae ?
        Nearby_Players = 40, // bard AE hits all players
        Target_Group = 41,
        Directional_AE = 42,
        Frontal_AE = 44,
        Single_In_Group = 43,
        Target_Ring_AE = 45,
        Targets_Target = 46,
        Pet_Owner = 47
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
    }

    public enum SpellZoneRestrict
    {
        None = 0,
        Outdoors = 1,
        Indoors = 2
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
        Old_Aviak = 13,
        Old_Werewolf = 14,
        Old_Brownie = 15,
        Old_Centaur = 16,
        Trakanon = 19,
        Venril_Sathir = 20,
        Froglok = 27,
        Old_Gargoyle = 29,
        Old_Wolf = 42,
        Black_Spirit_Wolf = 42 << 16 + 1,
        White_Spirit_Wolf = 42 << 16 + 2,
        Old_Bear = 43,
        Polar_Bear = 43 << 16 + 2,
        Freeport_Militia = 44,
        Imp = 46,
        Lizard_Man = 51,
        Old_Drachnid = 57,
        Solusek_Ro = 58,
        Tunare = 62,
        Tiger = 63,
        Elemental = 75,
        Earth_Elemental = 75 << 16,
        Fire_Elemental = 75 << 16 + 1,
        Water_Elemental = 75 << 16 + 2,
        Air_Elemental = 75 << 16 + 3,
        Old_Scarecrow = 82,
        Old_Skeleton = 85,
        Old_Drake = 89,
        Alligator = 91,
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
        Mosquito = 134,
        Kunark_Goblin = 137,
        Nearby_Object = 142,
        Tree = 143,
        Old_Iksar_Skeleton = 161,
        Snow_Rabbit = 176,
        Walrus = 177,
        Geonid = 178,
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
        Shissar = 217,
        Fungal_Fiend = 218,
        Stonegrabber = 220,
        Zelniak = 222,
        Lightcrawler = 223,
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
        Gnome_Pirate = 338,
        Ogre_Pirate = 340,
        Froglok_Skeleton = 349,
        Undead_Froglok = 350,
        Scaled_Wolf = 356,
        Vampire = 360,
        Nightrage_Orphan = 360 << 16 + 1,
        Skeleton = 367,
        Drybone_Skeleton = 367 << 16 + 1,
        Frostbone_Skeleton = 367 << 16 + 2,
        Firebone_Skeleton = 367 << 16 + 3,
        Scorched_Skeleton = 367 << 16 + 4,
        Mummy = 368,
        Froglok_Ghost = 371,
        Shade = 373,
        Golem = 374,
        Ice_Golem = 374 << 16 + 1,
        Crystal_Golem = 374 << 16 + 3,
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
        Solusek_Goblin = 433 << 16 + 1,
        Dagnor_Goblin = 433 << 16 + 2,
        Valley_Goblin = 433 << 16 + 3,
        Aqua_Goblin = 433 << 16 + 7,
        Goblin_King = 433 << 16 + 8,
        Rallosian_Goblin = 433 << 16 + 11,
        Frost_Goblin = 433 << 16 + 12,
        Kirin = 434,
        Basilisk = 436,
        Puma = 439,
        Domain_Prowler = 439 << 16 + 9,
        Spider = 440,
        Spider_Queen = 441,
        Animated_Statue = 442,
        Werewolf = 454,
        Kobold = 455,
        Kobold_King = 455 << 16 + 2,
        Sporali = 456,
        Violet_Sporali = 456 << 16 + 2,
        Azure_Sporali = 456 << 16 + 11,
        Gnomework = 457,
        Orc = 458,
        Bloodmoon_Orc = 458 << 16 + 4,
        Drachnid = 461,
        Gargoyle = 464,
        Runed_Gargoyle = 464 << 16 + 1,
        Undead_Shiliskin = 467,
        Evil_Eye = 469,
        Minotaur = 470,
        Zombie = 471,
        Fairy = 473,
        Tree_Fairy = 473 << 16 + 1,
        Witheran = 474,
        Air_Elemental3 = 475,
        Earth_Elemental3 = 476,
        Fire_Elemental3 = 477,
        Water_Elemental3 = 478,
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
        Bixie_Queen = 520 << 16 + 2,
        Centaur = 521,
        Drakkin = 522,
        Gnoll = 524,
        Undead_Gnoll = 524 << 16 + 1,
        Satyr = 529,
        Dragon = 530,
        Hideous_Harpy = 527,
        Goo = 549,
        Aviak = 558,
        Beetle = 559,
        Death_Beetle = 559 << 16 + 1,
        Kedge = 561,
        Kerran = 562,
        Shissar2 = 563,
        Siren = 564,
        Plaguebringer = 566,
        Hooded_Plaguebringer = 566 << 16 + 7,
        Brownie = 568,
        Brownie_Noble = 568 << 16 + 2,
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
        Plague_Fly = 612,
        Burning_Nekhon = 614,
        Crystal_Hydra = 615,
        Crystal_Sphere = 616,
        Vitrik = 620,
        Bellikos = 638,
        Cliknar = 643,
        Crystal_Sessiloid = 647,
        Flame_Telmira = 653,
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
        Gouzah_Rabbit = 668 << 16 + 3,
        Polka_Dot_Rabbit = 668 << 16 + 5,
        Cazic_Thule = 670,
        Selyrah = 686,
        Goral = 687,
        Braxi = 688,
        Kangon = 689,
        Undead_Thelasa = 695,
        Thel_Ereth_Ril = 695 << 16 + 21,
        Swinetor = 696,
        Triumvirate = 697,
        Hadal = 698,
        Hadal_Templar = 698 << 16 + 2,
        Alaran_Ghost = 708
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
        Offensive_Proc_Casts = 11
    }

    public enum SpellTeleport
    {
        Primary_Anchor = 52584,
        Secondary_Anchor = 52585,
        Guild_Anchor = 50874
    }

    #endregion

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
        public bool DurationExtendable;
        public string[] Slots;
        public int[] SlotEffects;
        //public byte Level;
        public byte[] Levels;
        public byte[] ExtLevels; // similar to levels but assigns levels for side effect spells that don't have levels defined (e.g. a proc effect will get the level of it's proc buff)
        public string ClassesLevels;
        public SpellClassesMask ClassesMask;
        public SpellSkill Skill;
        public bool Beneficial;
        public bool BeneficialBlockable;
        public SpellTarget Target;
        public SpellResist ResistType;
        public int ResistMod;
        public int MinResist;
        public int MaxResist;
        public string Extra;
        public int HateOverride;
        public int HateMod;
        public int Range;
        public int AERange;
        public int AEDuration; // rain spells
        public float CastingTime;
        public float QuietTime;
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
        public int ViralTimer;
        public int ViralRange;
        public int ViralTargets;
        public SpellTargetRestrict TargetRestrict;
        public SpellTargetRestrict CasterRestrict;
        public int[] ConsumeItemID;
        public int[] ConsumeItemCount;
        public int[] FocusID;
        public string LandOnSelf;
        //public string LandOnOther;
        public int StartDegree;
        public int EndDegree;
        public bool MGBable;
        public int Rank;
        public bool OutOfCombat;
        public SpellZoneRestrict Zone;
        public bool DurationFrozen; // in guildhall/lobby
        public bool Dispellable;
        public bool PersistAfterDeath;
        public bool ShortDuration; // song window
        public bool CancelOnSit;
        public bool Sneaking;
        public int[] CategoryDescID; // AAs don't have these set
        public string Deity;
        public int SongCap;
        public int[] LinksTo;
        public int RefCount; // number of spells that link to this

#if LargeMemory
        public string Category;
#endif

        public float Unknown;


        /// Effects can reference other spells or items via square bracket notation. e.g.
        /// [Spell 123]    is a reference to spell 123
        /// [Group 123]    is a reference to spell group 123
        /// [Item 123]     is a reference to item 123
        static public readonly Regex SpellRefExpr = new Regex(@"\[Spell\s(\d+)\]");
        static public readonly Regex GroupRefExpr = new Regex(@"\[Group\s(\d+)\]");
        static public readonly Regex ItemRefExpr = new Regex(@"\[Item\s(\d+)\]");

        public Spell()
        {
            Slots = new string[12];
            SlotEffects = new int[12];
            Levels = new byte[16];
            ExtLevels = new byte[16];
            ConsumeItemID = new int[4];
            ConsumeItemCount = new int[4];
            FocusID = new int[4];
            CategoryDescID = new int[3];
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
            List<string> result = new List<string>(20);
            //Action<string> Add = delegate(string s) { result.Add(s); };


            if (!String.IsNullOrEmpty(ClassesLevels))
            {
                result.Add("Classes: " + ClassesLevels);
                if (SongCap > 0)
                    result.Add("Skill: " + FormatEnum(Skill) + ", Cap: " + SongCap);
                else if ((int)Skill != 98) // 98 might be for AA only
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
                    result.Add("Regeant: [Item " + ConsumeItemID[i] + "] x " + ConsumeItemCount[i]);

            for (int i = 0; i < FocusID.Length; i++)
                if (FocusID[i] > 0)
                    result.Add("Focus: [Item " + FocusID[i] + "]");

            if (OutOfCombat)
                result.Add("Restriction: Out of Combat");

            if (Zone != SpellZoneRestrict.None)
                result.Add("Restriction: " + Zone + " Only");

            if (Sneaking)
                result.Add("Restriction: Sneaking");

            if (CancelOnSit)
                result.Add("Restriction: Cancel on Sit");

            if ((int)CasterRestrict > 100)
                result.Add("Restriction: " + FormatEnum(CasterRestrict));

            if (Target == SpellTarget.Directional_AE)
                result.Add("Target: " + FormatEnum(Target) + " (" + StartDegree + " to " + EndDegree + " Degrees)");
            else if (TargetRestrict > 0)
                result.Add("Target: " + FormatEnum(Target) + " (If " + FormatEnum(TargetRestrict) + ")");
            else if ((Target == SpellTarget.Caster_Group || Target == SpellTarget.Target_Group) && (ClassesMask != 0 && ClassesMask != SpellClassesMask.BRD) && DurationTicks > 0)
                result.Add("Target: " + FormatEnum(Target) + ", MGB: " + (MGBable ? "Yes" : "No"));
            else
                result.Add("Target: " + FormatEnum(Target));

            if (AERange > 0 && Range == 0)
                result.Add("AE Range: " + AERange);
            else if (AERange > 0)
                result.Add("Range: " + Range + ", AE Range: " + AERange);
            else if (Range > 0)
                result.Add("Range: " + Range);

            if (ViralRange > 0)
                result.Add("Viral Range: " + ViralRange + ", Recast: " + ViralTimer + "s, Targets: " + ViralTargets);

            if (!Beneficial && ResistMod != 0)
                result.Add("Resist: " + ResistType + " " + ResistMod + (MinResist > 0 ? ", Min: " + MinResist / 2f + "%" : "") + (MaxResist > 0 ? ", Max: " + MaxResist / 2f + "%" : ""));
            else if (!Beneficial)
                result.Add("Resist: " + ResistType);
            else
                result.Add("Beneficial: " + (BeneficialBlockable ? "Blockable" : "Not Blockable"));

            if (TimerID > 0)
                result.Add("Casting: " + CastingTime.ToString() + "s, Recast: " + FormatTime(RecastTime) + ", Timer: " + TimerID);
            else if (RecastTime > 0)
                result.Add("Casting: " + CastingTime.ToString() + "s, Recast: " + FormatTime(RecastTime));
            else
                result.Add("Casting: " + CastingTime.ToString() + "s");

            if (DurationTicks > 0 && Beneficial && ClassesMask != SpellClassesMask.BRD)
                result.Add("Duration: " + FormatTime(DurationTicks * 6) + " (" + DurationTicks + " ticks)" + ", Extend: " + (DurationExtendable ? "Yes" : "No") + (PersistAfterDeath ? ", Keep On Death: Yes" : ""));
            else if (DurationTicks > 0)
                result.Add("Duration: " + FormatTime(DurationTicks * 6) + " (" + DurationTicks + " ticks)" + (PersistAfterDeath ? ", Keep On Death: Yes" : ""));
            else if (AEDuration >= 2500)
                result.Add("AE Waves: " + AEDuration / 2500);

            if (DurationTicks > 0 && !Dispellable)
                result.Add("Dispellable: " + (Dispellable ? "Yes" : "No"));

            if (PushUp != 0)
                result.Add("Push: " + PushBack + ", Up: " + PushUp);
            else if (PushBack != 0)
                result.Add("Push: " + PushBack);

            if (HateMod != 0)
                result.Add("Hate Mod: " + (HateMod > 0 ? "+" : "") + HateMod);

            if (HateOverride != 0)
                result.Add("Hate: " + HateOverride);

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

            for (int i = 0; i < Slots.Length; i++)
                if (Slots[i] != null)
                    result.Add(String.Format("{0}: {1}", i + 1, Slots[i]));

            if (!String.IsNullOrEmpty(LandOnSelf))
                result.Add("Text: " + LandOnSelf);

            return result.ToArray();
        }

        /*
        /// <summary>
        /// Return a list of spells that this spell references.
        /// </summary>
        public IEnumerable<int> LinkedSpells()
        {
            if (RecourseID != 0)
                yield return RecourseID;

            foreach (string s in Slots)
                if (s != null)
                {
                    Match match = Spell.SpellRefExpr.Match(s);
                    if (match.Success)
                        yield return Int32.Parse(match.Groups[1].Value);
                }
        }
        */

        /// <summary>
        /// Search all spell slots for a certain effect
        /// </summary>
        /// <returns>Index of slot with the effect or -1 if not found</returns>
        public int HasEffect(int spa)
        {
            return Array.IndexOf(SlotEffects, spa);
        }

        /// <summary>
        /// Search all spell slots for a certain effect using a string match
        /// </summary>
        // <returns>Index of slot with the effect or -1 if not found</returns>
        public int HasEffect(string text)
        {
            int spa;
            if (Int32.TryParse(text, out spa))
                return HasEffect(spa);

            for (int i = 0; i < Slots.Length; i++)
                if (Slots[i] != null && Slots[i].IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return i;

            return -1;
        }

        /// <summary>
        /// Search all spell slots for a certain effect using a RegEx
        /// </summary>
        /// <returns>Index of slot with the effect or -1 if not found</returns>
        public int HasEffect(Regex re)
        {
            for (int i = 0; i < Slots.Length; i++)
                if (Slots[i] != null && re.IsMatch(Slots[i]))
                    return i;

            return -1;
        }

        /// <summary>
        /// Sum all the spell slots effects that match a regex. The regex must have a capturing group for an integer value.
        /// e.g. Increase Current HP by (\d+)
        /// </summary>
        public int ScoreEffect(Regex re)
        {
            int score = 0;
            for (int i = 0; i < Slots.Length; i++)
                if (Slots[i] != null)
                {
                    Match m = re.Match(Slots[i]);
                    if (m.Success)
                        score += Int32.Parse(m.Groups[1].Value);
                }

            return score;
        }

        /// <summary>
        /// Parse a spell effect. Each spell has 12 effect slots. Devs refer to these as SPAs.
        /// Attributes like ID, Skill, Extra, DurationTicks are referenced and should be set before
        /// calling this function.
        /// </summary>
        public string ParseEffect(int spa, int base1, int base2, int max, int calc, int level)
        {
            // type 254 indicates an unused slot
            if (spa == 254)
                return null;

            // type 10 sometimes indicates an unused slot
            if (spa == 10 && (base1 <= 1 || base1 > 255))
                return null;

            // many SPAs use a scaled value based on either current tick or caster level
            int value = CalcValue(calc, base1, max, 1, level);
            string range = CalcValueRange(calc, base1, max, DurationTicks, level);
            //Func<int> base1_or_value = delegate() { Debug.WriteLineIf(base1 != value, "SPA " + spa + " value uncertain"); return base1; };

            // some hp/mana/end/hate effects repeat for each tick of the duration
            string repeating = (DurationTicks > 0) ? " per tick" : null;

            // some effects are capped at a max level
            string maxlevel = (max > 0) ? String.Format(" up to level {0}", max) : null;

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
                    // 10 is often used as a filler
                    return Spell.FormatCount("CHA", value) + range;
                case 11:
                    // base attack speed is 100. so 85 = 15% slow, 130 = 30% haste
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
                    return String.Format("Stun for {0}s", base1 / 1000f) + maxlevel;
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
                case 40:
                    return "Invulnerability";
                case 41:
                    return "Destroy";
                case 42:
                    // TODO: how does this this work for highsun?
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
                    return String.Format("Absorb Damage: 100% Total: {0}", value);
                case 56:
                    return "True North";
                case 57:
                    return "Levitate";
                case 58:
                    value = base1 << 16 + base2;
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
                    if (value + 40 < 100)
                        return String.Format("Memory Blur ({0}% Chance)", value + 40);
                    return "Memory Blur";
                case 64:
                    return String.Format("Stun and Spin for {0}s", base1 / 1000f) + maxlevel;
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
                    return "Voice Graft";
                case 76:
                    return "Sentinel";
                case 77:
                    return "Locate Corpse";
                case 78:
                    return String.Format("Absorb Spell Damage: 100% Total: {0}", value);
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
                        return String.Format("Add Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                    return String.Format("Add Proc: [Spell {0}]", base1);
                case 86:
                    return String.Format("Decrease Social Radius to {0}", value) + maxlevel;
                case 87:
                    return Spell.FormatPercent("Magnification", value);
                case 88:
                    return String.Format("Evacuate to {0}", Extra);
                case 89:
                    return Spell.FormatPercent("Player Size", base1 - 100);
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
                    return Spell.FormatPercent("Healing Taken", base1); // not range
                case 121:
                    // damages the target whenever it hits something
                    return Spell.FormatCount("Reverse Damage Shield", -value);
                case 123:
                    return "Screech";
                case 124:
                    return Spell.FormatPercent("Spell Damage", base1, base2);
                case 125:
                    return Spell.FormatPercent("Healing", base1, base2);
                case 126:
                    return Spell.FormatPercent("Spell Resist Rate", -value);
                case 127:
                    return Spell.FormatPercent("Spell Haste", value);
                case 128:
                    return Spell.FormatPercent("Spell Duration", value);
                case 129:
                    return Spell.FormatPercent("Spell Range", value);
                case 130:
                    // i think this affects all special attacks. bash/kick/frenzy/etc...
                    return Spell.FormatPercent("Spell and Bash Hate", base1, base2);
                case 131:
                    return Spell.FormatPercent("Chance of Using Reagent", -value);
                case 132:
                    return Spell.FormatPercent("Spell Mana Cost", -base1, -base2);
                case 134:
                    return String.Format("Limit Max Level: {0} (lose {1}% per level)", base1, base2);
                case 135:
                    return String.Format("Limit Resist: {0}", (SpellResist)value);
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
                    // has not been implemented in the game
                    return Spell.FormatCount("Electricity Resist", value);
                case 147:
                    return String.Format("Increase Current HP by {1} Max: {0}% ", value, max);
                case 148:
                    //if (max > 1000) max -= 1000;
                    return String.Format("Stacking: Block new spell if slot {0} is '{1}' and < {2}", calc % 100, Spell.FormatEnum((SpellEffect)base1), max);
                case 149:
                    //if (max > 1000) max -= 1000;
                    return String.Format("Stacking: Overwrite existing spell if slot {0} is '{1}' and < {2}", calc % 100, Spell.FormatEnum((SpellEffect)base1), max);
                case 150:
                    return String.Format("Divine Intervention with {0} Heal", max);
                case 151:
                    return "Suspend Pet";
                case 152:
                    return String.Format("Summon Pet: {0} x {1} for {2}s", Extra, base1, max);
                case 153:
                    return String.Format("Balance Group HP with {0}% Penalty", value);
                case 154:
                    return String.Format("Cure Detrimental ({0})", value);
                case 156:
                    return "Illusion: Target";
                case 157:
                    return Spell.FormatCount("Spell Damage Shield", -value);
                case 158:
                    return Spell.FormatPercent("Chance to Reflect Spell", value);
                case 159:
                    return Spell.FormatCount("Base Stats", value);
                case 160:
                    return String.Format("Intoxicate if Tolerance < {0}", value);
                case 161:
                    return String.Format("Absorb Spell Damage: {0}%", value) + (base2 > 0 ? String.Format(" Per Hit: {0}", base2) : "") + (max > 0 ? String.Format(" Total: {0}", max) : "");
                case 162:
                    return String.Format("Absorb Melee Damage: {0}%", value) + (base2 > 0 ? String.Format(" Per Hit: {0}", base2) : "") + (max > 0 ? String.Format(" Total: {0}", max) : "");
                case 163:
                    return String.Format("Absorb {0} Hits or Spells", value);
                case 164:
                    return String.Format("Appraise Chest ({0})", value);
                case 165:
                    return String.Format("Disarm Chest ({0})", value);
                case 166:
                    return String.Format("Unlock Chest ({0})", value);
                case 167:
                    return String.Format("Pet Power ({0})", value);
                case 168:
                    // how is this different than an endless rune?
                    return Spell.FormatPercent("Melee Mitigation", -value);
                case 169:
                    if ((SpellSkill)base2 != SpellSkill.Hit)
                        return Spell.FormatPercent("Chance to Critical Hit with " + Spell.FormatEnum((SpellSkill)base2), value);
                    return Spell.FormatPercent("Chance to Critical Hit", value);
                case 170:
                    return Spell.FormatPercent("Chance to Critical Cast", value);
                case 171:
                    return Spell.FormatPercent("Chance to Crippling Blow", value);
                case 172:
                    return Spell.FormatPercent("Chance to Avoid Melee", value);
                case 173:
                    return Spell.FormatPercent("Chance to Riposte", value);
                case 174:
                    return Spell.FormatPercent("Chance to Dodge", value);
                case 175:
                    return Spell.FormatPercent("Chance to Parry", value);
                case 176:
                    return Spell.FormatPercent("Chance to Dual Wield", value);
                case 177:
                    return Spell.FormatPercent("Chance to Double Attack", value);
                case 178:
                    return String.Format("Lifetap from Weapon Damage: {0}%", value);
                case 179:
                    return String.Format("Instrument Modifier: {0} {1}", Skill, value);
                case 180:
                    return Spell.FormatPercent("Chance to Resist Spell", value);
                case 181:
                    return Spell.FormatPercent("Chance to Resist Fear Spell", value);
                case 182:
                    // hundred hands effect. how is this different than 371?
                    return Spell.FormatPercent("Weapon Delay", value);
                case 183:
                    return Spell.FormatPercent("Skill Check for " + Spell.FormatEnum((SpellSkill)base2), value);
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
                    return String.Format("{0} Attack for {1} with {2}% Accuracy Mod", Spell.FormatEnum(Skill), base1, base2);
                case 194:
                    if (value < 100)
                        return String.Format("Cancel All Aggro ({0}% Chance)", value);
                    return "Cancel All Aggro";
                case 195:
                    // 100 is full resist. not sure why some spells have more
                    return String.Format("Stun Resist ({0})", value);
                case 196:
                    return String.Format("Srikethrough ({0})", value);
                case 197:
                    return Spell.FormatPercent(Spell.FormatEnum((SpellSkill)base2) + " Damage Taken", value);
                case 198:
                    return Spell.FormatCount("Current Endurance", value);
                case 199:
                    return String.Format("Taunt ({0})", value);
                case 200:
                    // melee/range/defensive?
                    return Spell.FormatPercent("Proc Rate", value);
                case 201:
                    return String.Format("Add Range Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                case 202:
                    return "Casting Mode: Project Illusion";
                case 203:
                    return "Casting Mode: Mass Group Buff";
                case 204:
                    return String.Format("Group Fear Immunity for {0}s", base1 * 10);
                case 205:
                    return String.Format("AE Attack ({0})", value);
                case 206:
                    return String.Format("AE Taunt ({0})", value);
                case 207:
                    return "Flesh to Bone Chips";
                case 209:
                    return String.Format("Dispel Beneficial ({0})", value);
                case 210:
                    return String.Format("Pet Shielding for {0}s", base1 * 12);
                case 211:
                    // use spell duration if it is > 0?
                    return String.Format("AE Attack for {0}s", base1 * 12);
                case 213:
                    return String.Format("Pet Power v2 ({0})", value);
                case 214:
                    if (Math.Abs(value) >= 100)
                        value = (int)(value / 100f);
                    return Spell.FormatPercent("Max HP", value);
                case 216:
                    return Spell.FormatPercent("Accuracy", value);
                case 219:
                    return Spell.FormatPercent("Chance to Slay Undead", value / 100f);
                case 220:
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Damage Bonus", base1);
                case 222:
                    return Spell.FormatPercent("Chance to Block from Back", value);
                case 225:
                    return Spell.FormatCount("Double Attack Skill", base1);
                case 227:
                    return String.Format("Reduce {0} Timer by {1}s", Spell.FormatEnum((SpellSkill)base2), base1);
                case 232:
                    return String.Format("Cast on Death Save: [Spell {0}] ({1}% Chance)", base2, base1);
                case 233:
                    return Spell.FormatPercent("Food Consumption", -value);
                //case 238: // probably buff extension AA
                case 243:
                    return Spell.FormatPercent("Chance of Charm Breaking", -value);
                case 246:
                    return Spell.FormatCount("Lung Capacity", -value);
                case 250:
                    // not sure about this one
                    return Spell.FormatPercent("Defensive Proc Rate", value);
                case 258:
                    return Spell.FormatPercent("Chance to Triple Backstab", value);
                case 259:
                    // i.e. Combat Stability
                    return Spell.FormatCount("AC Soft Cap", value);
                case 262:
                    // affects worn cap
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkillCap)base2) + " Cap", value);
                case 265:
                    // value of zero should negate effects of Mastery of the Past
                    return String.Format("No Fizzle on spells up to level {0}", value);
                case 266:
                    // both double and triple attack? why not just use 177, 364?
                    return Spell.FormatPercent("Chance of Additional Attack", value);
                case 270:
                    return Spell.FormatCount("Beneficial Song Range", base1);
                case 272:
                    return Spell.FormatPercent("Spell Casting Skill", value);
                case 273:
                    return Spell.FormatPercent("Chance to Critical DoT", value);
                case 274:
                    return Spell.FormatPercent("Chance to Critical Heal", value);
                case 279:
                    return Spell.FormatPercent("Chance to Flurry", value);
                case 280:
                    return Spell.FormatPercent("Pet Chance to Flurry", value);
                case 286:
                    // is added after all other multipliers (focus, crit, etc..)
                    // for DoTs it adds base1/ticks to each tick.
                    return Spell.FormatCount("Spell Damage Bonus", base1);
                case 287:
                    return String.Format("Increase Duration by {0}s", base1 * 6);
                case 289:
                    // this only triggers if the spell times out. compare with 373
                    return String.Format("Cast on Duration Fade: [Spell {0}]", base1);
                case 291:
                    return String.Format("Dispel Detrimental ({0})", value);
                case 294:
                    // additive with innate crit multiplier
                    if (base1 > 0 && base2 > 100)
                        return Spell.FormatPercent("Chance to Critical Nuke", base1) + " and " + Spell.FormatPercent("Critical Nuke Damage", base2 - 100) + " of Base Damage";
                    else if (base1 > 0)
                        return Spell.FormatPercent("Chance to Critical Nuke", base1);
                    else
                        return Spell.FormatPercent("Critical Nuke Damage", base2 - 100) + " of Base Damage";
                case 296:
                    return Spell.FormatPercent("Spell Damage Taken", base2, base1);
                case 297:
                    return Spell.FormatCount("Spell Damage Taken", base1);
                case 298:
                    return Spell.FormatPercent("Pet Size", value - 100);
                case 299:
                    return String.Format("Wake the Dead ({0})", max);
                case 300:
                    return "Summon Doppelganger: " + Extra;
                case 302:
                    // see also 124. only used on 2 AA
                    return Spell.FormatPercent("Spell Damage", base1);
                case 303:
                    // used on type 3 augments
                    // is added before crit multipliers, but after SPA 296 and 302 (and maybe 124).
                    // for DoTs it adds base1/ticks to each tick.
                    return Spell.FormatCount("Spell Damage", base1);
                case 305:
                    return Spell.FormatCount("Damage Shield Taken", -Math.Abs(value));
                case 306:
                    return String.Format("Summon Pet: {0} x {1} for {2}s", Extra, base1, max);
                case 308:
                    return "Suspend Minion";
                case 309:
                    return "Teleport to Bind";
                case 310:
                    return String.Format("Reduce Timer by {0}s", base1 / 1000f);
                case 311:
                    // does this affect procs that the caster can also cast as spells?
                    return "Limit Type: Exclude Procs";
                case 312:
                    return "Sanctuary";
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
                    if (base2 != 0)
                        return String.Format("Add Defensive Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                    return String.Format("Add Defensive Proc: [Spell {0}]", base1);
                case 324:
                    // blood magic. uses HP as mana
                    return String.Format("Cast from HP with {0}% Penalty", value);
                case 327:
                    return Spell.FormatCount("Buff Slots", base1);
                case 328:
                    return Spell.FormatCount("Max Negative HP", value);
                case 329:
                    return String.Format("Absorb Damage using Mana: {0}%", value);
                case 330:
                    // additive with innate crit multiplier
                    return Spell.FormatPercent("Critical " + Spell.FormatEnum((SpellSkill)base2) + " Damage", value) + " of Base Damage";
                case 331:
                    return Spell.FormatPercent("Chance to Salvage Components", value);
                case 332:
                    return "Summon to Corpse";
                case 333:
                    // so far this is only used on spells that have a rune
                    return String.Format("Cast on Rune Fade: [Spell {0}]", base1);
                case 334:
                    // only used by a few bard songs. how is this different than 1/100
                    return Spell.FormatCount("Current HP", value) + repeating + range;
                case 335:
                    if (base1 < 100)
                        return String.Format("Block Matching Spell ({0}% Chance)", base1);
                    return "Block Matching Spell";
                case 337:
                    return Spell.FormatPercent("Experience Gain", value);
                case 338:
                    return "Summon and Resurrect All Corpses";
                case 339:
                    // how is this different than 383? (besides chance)
                    return String.Format("Cast on Spell Use: [Spell {0}] ({1}% Chance)", base2, base1);
                case 340:
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
                case 348:
                    return String.Format("Limit Min Mana Cost: {0}", base1);
                case 350:
                    return String.Format("Mana Burn: {0}", value);
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

                    return String.Format("Aura Effect: [Spell {0}] ({1})", aura, Extra);
                case 353:
                    return Spell.FormatCount("Aura Slots", base1);
                case 357:
                    // similar to 96, but i think this prevents casting of spells matching limits
                    return "Inhibit Spell Casting";
                case 358:
                    return Spell.FormatCount("Current Mana", value) + range;
                case 360:
                    return String.Format("Add Killshot Proc: [Spell {0}] ({1}% Chance)", base2, base1);
                case 361:
                    return String.Format("Cast on Death: [Spell {0}] ({1}% Chance)", base2, base1);
                case 364:
                    return Spell.FormatPercent("Chance to Triple Attack", value);
                case 365:
                    return String.Format("Cast on Killshot: [Spell {0}] ({1}% Chance)", base2, base1);
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
                case 373:
                    // this appears to be used when a spell is removed via any method: times out, cured, rune depleted, max hits, mez break
                    return String.Format("Cast on Fade: [Spell {0}]", base1);
                case 374:
                    if (base1 < 100)
                        return String.Format("Cast: [Spell {0}] ({1}% Chance)", base2, base1);
                    return String.Format("Cast: [Spell {0}]", base2);
                case 375:
                    // additive with innate crit multiplier and same effect in other slots
                    return Spell.FormatPercent("Critical DoT Damage", base1) + " of Base Damage";
                case 377:
                    return String.Format("Cast if Not Cured: [Spell {0}]", base1);
                case 378:
                    return Spell.FormatPercent("Chance to Resist " + Spell.FormatEnum((SpellEffect)base2), value);
                case 379:
                    if (base2 > 0)
                        return String.Format("Push for {0} in Direction: {1}", base1, base2);
                    return String.Format("Push forward for {0}", value);
                case 380:
                    return String.Format("Push back for {0} and up for {1}", base2, base1);
                case 381:
                    return String.Format("Summon to {0} in Front", base1);
                case 382:
                    return String.Format("Inhibit Effect: {0}", Spell.FormatEnum((SpellEffect)base2));
                case 383:
                    return String.Format("Cast on Spell Use: [Spell {0}] ({1}% Chance)", base2, base1 / 10);
                case 384:
                    return "Leap";
                case 385:
                    return String.Format("Limit Spells: {1}[Group {0}]", Math.Abs(base1), base1 >= 0 ? "" : "Exclude ");
                case 386:
                    return String.Format("Cast on Curer: [Spell {0}]", base1);
                case 387:
                    return String.Format("Cast if Cured: [Spell {0}]", base1);
                case 388:
                    return "Summon Corpse (From Any Zone)";
                case 389:
                    return "Reset Recast Timers";
                case 390:
                    // what unit? seconds?
                    return String.Format("Set Recast Timers to {0}", value);
                case 392:
                    return Spell.FormatCount("Healing Bonus", base1);
                case 393:
                    return Spell.FormatPercent("Healing Taken", base1, base2); // ranged
                case 394:
                    return Spell.FormatCount("Healing Taken", base1);
                case 396:
                    // used on type 3 augments
                    return Spell.FormatCount("Healing", base1);
                case 398:
                    return String.Format("Increase Pet Duration by {0}s", base1 / 1000);
                case 399:
                    return Spell.FormatPercent("Chance to Twincast", value);
                case 400:
                    // e.g. Channels the power of sunlight, consuming up to #1 mana to heal your group.
                    Mana = base1; // a bit misleading since the spell will cast with 0 mana and scale the heal
                    Target = SpellTarget.Caster_Group; // total hack but makes sense for current spells
                    return String.Format("Increase Current HP by up to {0} ({1} HP per 1 Mana)", Math.Floor(base1 * base2 / 10f), base2 / 10f);
                case 401:
                    return String.Format("Decrease Current HP by up to {0} ({1} HP per 1 Target Mana)", Math.Floor(base1 * base2 / -10f), base2 / -10f);
                case 402:
                    return String.Format("Decrease Current HP by up to {0} ({1} HP per 1 Target End)", Math.Floor(base1 * base2 / -10f), base2 / -10f);
                // 403 = some sort of casting limit. base1=3 might indicate lifetap spells
                // 404 = seems to be a limit based on field 222
                //case 404:
                //    return String.Format("Limit Skill: {1}{0}", Spell.FormatEnum((SpellSkill)Math.Abs(base1)), base1 >= 0 ? "" : "Exclude ");
                case 406:
                    return String.Format("Cast on Max Hits: [Spell {0}]", base1);
                case 407:
                    return String.Format("Cast on Unknown Condition: [Spell {0}]", base1);
                case 408:
                    // unlike 214, this does not show a lower max HP
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
                    return String.Format("Limit Class: {0}", (SpellClassesMask)(value >> 1));
                case 413:
                    return Spell.FormatPercent("Spell Effectiveness", value);
                case 414:
                    return String.Format("Limit Bard Skill: {0}", Spell.FormatEnum((SpellSkill)base1));
                case 416:
                    // how is this differnt than 1?
                    return Spell.FormatCount("AC", (int)(value / (10f / 3f)));
                case 417:
                    // how is this different than 15?
                    return Spell.FormatCount("Current Mana", value) + repeating + range;
                case 418:
                    // how is this different than 220 bonus?
                    return Spell.FormatCount(Spell.FormatEnum((SpellSkill)base2) + " Damage Bonus", base1);
                case 419:
                    // this is used for potions. how is it different than 85? maybe proc rate?
                    if (base2 != 0)
                        return String.Format("Add Proc: [Spell {0}] with {1}% Rate Mod", base1, base2);
                    return String.Format("Add Proc: [Spell {0}]", base1);
                case 424:
                    // base1 is probably velocity. base2 might be range?
                    return String.Format("Gradual {0}: Base1={1} Base2={2}", base1 > 0 ? "Push" : "Pull", Math.Abs(base1), base2);
                //case 425: jump or antigravity?
                case 427:
                    return String.Format("Cast on Skill Use: [Spell {0}] ({1}% Chance)", base1, base2 / 10);
                case 428:
                    return String.Format("Limit Skill: {0}", Spell.FormatEnum((SpellSkill)value));
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
                //case 433: another chance to crit dots?
                case 434:
                    return Spell.FormatPercent("Chance to Critical Heal v2", base1);
                case 435:
                    return Spell.FormatPercent("Chance to Critical HoT v2", base1);
                case 437:
                    return "Teleport to your " + FormatEnum((SpellTeleport)base1);
                case 438:
                    return "Teleport to their " + FormatEnum((SpellTeleport)base1);
                case 441:
                    return String.Format("Cancel if Moved {0}", base1);
                case 442:
                    return String.Format("Cast on {1}: [Spell {0}]", base1, Spell.FormatEnum((SpellTargetRestrict)base2));
                case 444:
                    return "Lock Aggro on Caster and " + Spell.FormatPercent("Other Aggro", base2 - 100) + String.Format(" up to level {0}", base1);
                case 445:
                    return String.Format("Grant {0} Mercenary Slots", base1);

            }

            return String.Format("Unknown Effect: {0} Base1={1} Base2={2} Max={3} Calc={4} Value={5}", spa, base1, base2, max, calc, value);
        }

        /// <summary>
        /// Calculate a duration.
        /// </summary>
        /// <returns>Numbers of ticks (6 second units)</returns>
        static public int CalcDuration(int calc, int max, int level)
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
        static public int CalcValue(int calc, int base1, int max, int tick, int level)
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
        static public string CalcValueRange(int calc, int base1, int max, int duration, int level)
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

        static private string FormatEnum(object o)
        {
            string type = o.ToString().Replace("_", " ").Trim();
            if (Regex.IsMatch(type, @"^-?\d+$"))
                type = "Type " + type; // undefined numeric enum
            else
                type = Regex.Replace(type, @"\d+$", ""); // remove numeric suffix on duplicate enums undead3/summoned3/etc
            return type;
        }

        static private string FormatTime(float seconds)
        {
            if (seconds < 120)
                return seconds.ToString("0.#") + "s";

            if (seconds < 7200)
                return (seconds / 60f).ToString("0.#") + "m";

            return new TimeSpan(0, 0, (int)seconds).ToString();
        }

        static private string FormatCount(string name, int value)
        {
            return String.Format("{0} {1} by {2}", value < 0 ? "Decrease" : "Increase", name, Math.Abs(value));
        }

        static private string FormatPercent(string name, float value)
        {
            return Spell.FormatPercent(name, value, value);
        }

        static private string FormatPercent(string name, float min, float max)
        {
            if (Math.Abs(min) > Math.Abs(max))
            {
                float temp = min;
                min = max;
                max = temp;
            }

            if (min == 0 && max != 0)
                min = 1;

            if (min == max)
                return String.Format("{0} {1} by {2}%", max < 0 ? "Decrease" : "Increase", name, Math.Abs(max));

            return String.Format("{0} {1} by {2}% to {3}%", max < 0 ? "Decrease" : "Increase", name, Math.Abs(min), Math.Abs(max));
        }

        /*
        static private string FormatDesc()
        {
            // Spell descriptions include references to 12 spell attributes. e.g.
            // #7 - base1 for slot 7
            // @7 - calc(base1) for slot 7
            // $7 - base2 for slot 7
            return null;
        }
        */
    }

    public static class SpellParser
    {
        // the spell file is in US culture (dots are used for decimals)
        private static readonly CultureInfo culture = new CultureInfo("en-US", false);

        /// <summary>
        /// Load spell list from the EQ spell definition files.
        /// </summary>
        static public List<Spell> LoadFromFile(string spellPath, string descPath)
        {
            List<Spell> list = new List<Spell>(30000);
            Dictionary<int, Spell> lookup = new Dictionary<int, Spell>(30000);

            // load description text file
            Dictionary<string, string> desc = new Dictionary<string, string>(30000);
            if (File.Exists(descPath))
                using (StreamReader text = File.OpenText(descPath))
                    while (!text.EndOfStream)
                    {
                        string line = text.ReadLine();
                        string[] fields = line.Split('^');
                        if (fields.Length < 3)
                            continue;

                        // 0 = id in type
                        // 1 = type
                        // 2 = description
                        // type 1 = AA names
                        // type 4 = AA desc
                        // type 5 = spell categories
                        // type 6 = spell desc
                        // type 7 = lore groups
                        // type 11 = illusions
                        // type 16 = aug slot desc
                        // type 18 = currency
                        desc[fields[1] + "/" + fields[0]] = fields[2].Trim();
                    }


            // load spell definition file
            if (File.Exists(spellPath))
                using (StreamReader text = File.OpenText(spellPath))
                    while (!text.EndOfStream)
                    {
                        string line = text.ReadLine();
                        string[] fields = line.Split('^');
                        Spell spell = LoadSpell(fields);

#if LargeMemory
                        // all spells are organized into a hierarchical classification system up to 3 levels deep
                        // the 3rd level is too specific so it will be ignored
                        string s;
                        if (desc.TryGetValue("5/" + spell.CategoryDescID[0], out s))
                        {
                            spell.Category = s;
                            if (desc.TryGetValue("5/" + spell.CategoryDescID[1], out s))
                                spell.Category += "/" + s;
                            //if (desc.TryGetValue("5/" + spell.CategoryDescID[2], out s))
                            //    spell.Category += "/" + s;
                        }

#endif

                        if (!desc.TryGetValue("6/" + spell.DescID, out spell.Desc))
                            spell.Desc = null;

                        list.Add(spell);
                        lookup[spell.ID] = spell;
                    }

            // second pass fixes that require the entire spell list to be loaded already
            foreach (Spell spell in list)
            {
                foreach (int id in spell.LinksTo)
                {
                    Spell target = null;
                    if (lookup.TryGetValue(id, out target))
                    {
                        // count references to each spell
                        target.RefCount++;

                        // a lot of side effect spells do not have a level on them. this will copy the level of the referring spell 
                        // onto the side effect spell so that the spell will be searchable.
                        // e.g. Jolting Swings Strike has no level so it won't show up in a ranger search even though Jolting Swings will
                        // note that the levels array are not modified because the devs seem to omit levels on these spells to exclude them from being focused
                        for (int i = 0; i < spell.Levels.Length; i++)
                        {
                            if (target.ExtLevels[i] == 0 && spell.Levels[i] != 0)
                                target.ExtLevels[i] = spell.Levels[i];
                        }
                    }

                }
            }

            return list;
        }

        /// <summary>
        /// Parse a spell from a set of spell fields.
        /// </summary>
        static Spell LoadSpell(string[] fields)
        {
            int MaxLevel = 100;

            Spell spell = new Spell();

            spell.ID = Convert.ToInt32(fields[0]);
            spell.Name = fields[1];

            spell.Extra = fields[3];
            spell.LandOnSelf = fields[6];
            //spell.LandOnOther = fields[7];
            spell.DurationTicks = Spell.CalcDuration(ParseInt(fields[16]), ParseInt(fields[17]), MaxLevel);
            spell.Mana = ParseInt(fields[19]);

            spell.Range = ParseInt(fields[9]);
            spell.AERange = ParseInt(fields[10]);
            spell.PushBack = ParseFloat(fields[11]);
            spell.PushUp = ParseFloat(fields[12]);
            spell.CastingTime = ParseFloat(fields[13]) / 1000f;
            spell.QuietTime = ParseFloat(fields[14]) / 1000f;
            spell.RecastTime = ParseFloat(fields[15]) / 1000f;
            spell.AEDuration = ParseInt(fields[18]);

            // 56 = icon
            // 57 = icon

            for (int i = 0; i < 3; i++)
            {
                spell.ConsumeItemID[i] = ParseInt(fields[58 + i]);
                spell.ConsumeItemCount[i] = ParseInt(fields[62 + i]);
                spell.FocusID[i] = ParseInt(fields[66 + i]);
            }

            spell.Beneficial = ParseBool(fields[83]);
            spell.ResistType = (SpellResist)ParseInt(fields[85]);
            spell.Target = (SpellTarget)ParseInt(fields[98]);
            // 99 =  base difficulty fizzle adjustment?
            spell.Skill = (SpellSkill)ParseInt(fields[100]);
            spell.Zone = (SpellZoneRestrict)ParseInt(fields[101]);
            spell.CancelOnSit = ParseBool(fields[124]);
            spell.Icon = ParseInt(fields[144]);
            spell.ResistMod = ParseInt(fields[147]);
            spell.RecourseID = ParseInt(fields[150]);
            if (spell.RecourseID != 0)
                spell.Recourse = String.Format("[Spell {0}]", spell.RecourseID);
            spell.ShortDuration = ParseBool(fields[154]);
            spell.DescID = ParseInt(fields[155]);
            spell.CategoryDescID[0] = ParseInt(fields[156]);
            spell.CategoryDescID[1] = ParseInt(fields[157]);
            spell.CategoryDescID[2] = ParseInt(fields[158]);
            spell.HateMod = ParseInt(fields[162]);
            spell.Endurance = ParseInt(fields[166]);
            spell.TimerID = ParseInt(fields[167]);
            spell.HateOverride = ParseInt(fields[173]);
            spell.EnduranceUpkeep = ParseInt(fields[174]);
            spell.MaxHitsType = (SpellMaxHits)ParseInt(fields[175]);
            spell.MaxHits = ParseInt(fields[176]);
            spell.MGBable = ParseBool(fields[185]);
            spell.Dispellable = !ParseBool(fields[186]);
            spell.MinResist = ParseInt(fields[189]);
            spell.MaxResist = ParseInt(fields[190]);
            spell.ViralTimer = ParseInt(fields[191]);
            spell.ViralTargets = ParseInt(fields[192]);
            spell.StartDegree = ParseInt(fields[194]);
            spell.EndDegree = ParseInt(fields[195]);
            spell.Sneaking = ParseBool(fields[196]);
            spell.DurationExtendable = !ParseBool(fields[197]);
            spell.DurationFrozen = ParseBool(fields[200]);
            spell.ViralRange = ParseInt(fields[201]);
            spell.SongCap = ParseInt(fields[202]);
            // 203 = melee specials
            // 206/216 seem to be related
            spell.BeneficialBlockable = !ParseBool(fields[205]); // for beneficial spells
            spell.GroupID = ParseInt(fields[207]);
            spell.Rank = ParseInt(fields[208]); // rank 1/5/10. a few auras do not have this set properly
            if (spell.Rank == 5)
                spell.Rank = 2;
            if (spell.Rank == 10)
                spell.Rank = 3;
            spell.TargetRestrict = (SpellTargetRestrict)ParseInt(fields[211]);
            spell.OutOfCombat = !ParseBool(fields[214]);
            spell.MaxTargets = ParseInt(fields[218]);
            spell.CasterRestrict = (SpellTargetRestrict)ParseInt(fields[220]);
            spell.PersistAfterDeath = ParseBool(fields[224]);
            // 225 = song slope?
            // 226 = song offset?

            // debug stuff
            //spell.Unknown = ParseFloat(fields[202]);

            // each spell has a different casting level for all 16 classes
            for (int i = 0; i < spell.Levels.Length; i++)
                spell.Levels[i] = (byte)ParseInt(fields[104 + i]);

            spell.ClassesLevels = String.Empty;
            spell.ClassesMask = 0;
            bool All254 = true;
            for (int i = 0; i < spell.Levels.Length; i++)
            {
                if (spell.Levels[i] == 255)
                    spell.Levels[i] = 0;
                if (spell.Levels[i] != 0)
                {
                    //spell.Level = spell.Levels[i];
                    spell.ClassesMask |= (SpellClassesMask)(1 << i);
                    spell.ClassesLevels += " " + (SpellClasses)(i + 1) + "/" + spell.Levels[i];
                }
                // bard AA i=7 are marked as 255 even though are usable
                if (spell.Levels[i] != 254 && i != 7)
                    All254 = false;
            }
            Array.Copy(spell.Levels, spell.ExtLevels, spell.Levels.Length);
            spell.ClassesLevels = spell.ClassesLevels.TrimStart();
            if (All254)
                spell.ClassesLevels = "ALL/254";

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

                spell.SlotEffects[i] = spa;
                spell.Slots[i] = spell.ParseEffect(spa, base1, base2, max, calc, MaxLevel);

                // debug stuff: detect difference in value/base1 for spells where i'm not sure which one should be used and have chosen one arbitrarily
                //int[] uses_value = new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 15, 21, 24, 35, 36, 46, 47, 48, 49, 50, 55, 58, 59, 69, 79, 92, 97, 100, 111, 116, 158, 159, 164, 165, 166, 169, 184, 189, 190, 192, 262, 334, 417};
                //int[] uses_base1 = new int[] { 32, 64, 109, 148, 149, 193, 254, 323, 360, 374, 414 };
                //int value = Spell.CalcValue(calc, base1, max, 0, 90);
                //if (value != base1 && Array.IndexOf(uses_value, spa) < 0 && Array.IndexOf(uses_base1, spa) < 0)
                //    Console.Error.WriteLine(String.Format("SPA {1} {0} has diff value/base1: {2}/{3} calc: {4}", spell.Name, spa, value, base1, calc));
            }

            // 125..141 deity casting restrictions
            string[] gods = new string[] { "Agnostic", "Bertox", "Brell", "Cazic", "Erollisi", "Bristlebane", "Innoruuk", "Karana", "Mithanial", "Prexus", "Quellious", "Rallos", "Rodcet", "Solusek", "Tribunal", "Tunare", "Veeshan" };
            for (int i = 0; i < gods.Length; i++)
                if (ParseBool(fields[125 + i]))
                    spell.Deity += gods[i] + " ";

            // get list of linked spells
            List<int> linked = new List<int>(10);
            if (spell.RecourseID != 0)
                linked.Add(spell.RecourseID);

            foreach (string s in spell.Slots)
                if (s != null)
                {
                    Match match = Spell.SpellRefExpr.Match(s);
                    if (match.Success)
                        linked.Add(Int32.Parse(match.Groups[1].Value));
                }

            spell.LinksTo = linked.ToArray();

            // debug stuff
            //if (spell.ID == 31123) for (int i = 0; i < fields.Length; i++) Console.WriteLine("{0}: {1}", i, fields[i]);


            Prepare(spell);
            return spell;
        }

        /// <summary>
        /// Correct spell definition bugs.
        /// </summary>
        private static void Prepare(Spell spell)
        {
            if (spell.MaxHitsType == SpellMaxHits.None || spell.DurationTicks == 0)
                spell.MaxHits = 0;

            if (spell.Target == SpellTarget.Self)
            {
                spell.Range = 0;
                spell.AERange = 0;
                spell.MaxTargets = 0;
            }

            if (spell.Target == SpellTarget.Single)
            {
                spell.AERange = 0;
                spell.MaxTargets = 0;
            }

            if (spell.ResistType == SpellResist.Unresistable)
                spell.ResistMod = 0;

            if (spell.Zone != SpellZoneRestrict.Indoors && spell.Zone != SpellZoneRestrict.Outdoors)
                spell.Zone = SpellZoneRestrict.None;
        }

        static float ParseFloat(string s)
        {
            if (String.IsNullOrEmpty(s))
                return 0f;
            return Single.Parse(s, culture);
        }

        static int ParseInt(string s)
        {
            if (String.IsNullOrEmpty(s))
                return 0;
            return (int)Single.Parse(s, culture);
        }

        static bool ParseBool(string s)
        {
            return !String.IsNullOrEmpty(s) && (s != "0");
        }

    }

}
