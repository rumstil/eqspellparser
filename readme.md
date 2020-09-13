This project was created to help decode the EQ spell file and provide detailed information on spell, discipline and AA effects beyond what is shown in the game.

e.g. what the game shows for "Guardian of the Forest":

```
This ability transforms you into an exceptionally bloodthirsty wolf that attacks
with lightning speed, for a brief time. Additional ranks increase your offensive
capabilities and cause the transition into the wolf to heal many of your wounds.
```

Compare this with the parser description for the same spell:

```
[16038] Guardian of the Forest
Classes: RNG/254
Skill: Melee
Target: Self
Resist: Beneficial, Blockable: Yes
Casting: 0s
Duration: 60s (10 ticks), Extendable: Yes, Dispelable: Yes
2: Cast: Guardian of the Forest Effect
4: Increase Melee Haste v3 by 25%
7: Increase Current HP by 775 per tick
8: Increase ATK by 228
9: Increase Hit Damage by 42%
Text: The power of the forest surges through your muscles.
```

# Web Version #

The easiest way to use the parser is via the web version here:

https://www.raidloot.com/spells


# Windows Version #

If you want to parse historical, test or beta server spell data you will need to download the parser and run it on your own computer.

https://s3.amazonaws.com/raidloot/parser.zip

The download file contains winparser.exe which can open up spell files to be parsed on your own computer.

On Windows 8 or newer, the first time you run the parser will you get a "Windows protected your PC" alert. This is called SmartScreen and protects your PC from running programs from unrecognized publishers - which is what I am. If you want to run the parser you will need to click the "More Info" link on the popup and then "Run anyway".

You will also need to have Microsoft .NET Framework 3.5 installed to run the parser. This is a windows add-on that you may be missing if you have an older version of windows. If you run the parser and it doesn't work for you, download the required windows update from here:

https://www.microsoft.com/downloads/details.aspx?FamilyId=333325FD-AE52-4E35-B531-508D977D32A6&displaylang=en


# Data Version #

A mostly up to date CSV file can be found here:

https://s3.amazonaws.com/raidloot/spells.csv.gz

To generate this file yourself, you will need to run the command-line version of the parser:

```
parser update
parser all csv > spells.csv
```


# How To Help #

You can help improve this parser even if you're not a programmer. 

This parser is an attempt to gather the collected knowledge of players into a formal set of rules. If you discover spell descriptions that you think are incorrect or incomplete please send me an email at raidloot@gmail.com.

