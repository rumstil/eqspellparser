This project was created to help decode the EQ spell file and provide detailed information on spell, discipline and AA effects beyond what is shown in the game.

e.g. what the game shows for "Guardian of the Forest":

>This ability transforms you into an exceptionally bloodthirsty wolf that attacks
>with lightning speed, for a brief time. Additional ranks increase your offensive
>capabilities and cause the transition into the wolf to heal many of your wounds.

what the parser shows:

>[16038] Guardian of the Forest
>Classes: RNG/254
>Skill: Melee
>Target: Self
>Resist: Beneficial, Blockable: Yes
>Casting: 0s
>Duration: 60s (10 ticks), Extendable: Yes, Dispelable: Yes
>2: Cast: Guardian of the Forest Effect
>4: Increase Melee Haste v3 by 25%
>7: Increase Current HP by 775 per tick
>8: Increase ATK by 228
>9: Increase Hit Damage by 42%
>Text: The power of the forest surges through your muscles.


#Web Version#

The latest version of this parser is always running here: http://www.raidloot.com/Spells.aspx


#Windows Version#

If you want to parse test or beta server spells you will need to download the parser and run it on your own machine.

Download the parser here: Download Updated Oct 9, 2014

The zip file contains two applications to help you read the EQ spell file:

- winparser.exe is the user friendly Windows version of the parser.

- parser.exe is the command line version of the parser for people that need to export results to a text file.

In order to run the parser on your computer you will need to have Microsoft .NET Framework 3.5 installed. Windows 7 comes with the framework installed. If you run the parser without the framework installed it will crash or give an error message. The framework can be installed from here:

http://www.microsoft.com/downloads/details.aspx?FamilyId=333325FD-AE52-4E35-B531-508D977D32A6&displaylang=en


#How To Help#

If you have any corrections or ideas for improvements you can email me at raidloot@gmail.com.