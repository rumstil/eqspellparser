
REQUIREMENTS
============
You will need to have Microsoft .NET Framework 3.5 installed to run this application.
If you are running Windows 7 or 8 then you already have it installed.
For older versions of windows you may need to install it from:

http://www.microsoft.com/downloads/details.aspx?FamilyId=333325FD-AE52-4E35-B531-508D977D32A6&displaylang=en

If you run the parser and get the error: "This application failed to initialize properly"
then you will need to install the framework.



HOW TO USE FROM THE COMMAND LINE
================================================
parser.exe is the command line version of the parser.
It should be started from a command prompt with parameters to indicate what type of search you want to run.

You can search using 5 different methods:


>parser all

This shows all spells.


>parser name "healing"

This shows spells with the word "healing" in their name.


>parser id 13

This shows spell #13 (complete heal)


>parser spa 3

This shows spells with SPA type 3 (movement speed)


>parser class rng

This shows ranger spells



UPDATING THE SPELL DATABASE
============================
If you run the parser from a folder other than the EQ folder you will need to manually update spell
files after a patch. This can be done using the "update" command:


>parser update

Downloads the current *live* server spell database


>parser update -test

Downloads the current *test* server spell database






