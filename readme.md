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

#Web Version#

The easiest way to use the parser is via the web version here:

http://www.raidloot.com/Spells.aspx


#Downloadable Version#

If you want to parse historical, test or beta server spell data you will need to download the parser and run it on your own machine.

The download file contains two applications to help you read the EQ spell file:

- winparser.exe is the user friendly Windows version of the parser.

- parser.exe is the command line version of the parser for people that need to export results to a text file.

In order to run the parser on your computer you will need to have Microsoft .NET Framework 3.5 installed (this is a part of Windows but it doesn't come installed on all versions of windows). If you run the parser and it gives a strange error message, then you probably need to download the framework from Microsoft here:

http://www.microsoft.com/downloads/details.aspx?FamilyId=333325FD-AE52-4E35-B531-508D977D32A6&displaylang=en


#How To Help#

You can help improve this parser even if you're not a programmer. What I need most, is corrections for incorrect or incomplete parsing.

You can contact me at raidloot@gmail.com 

