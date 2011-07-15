
REQUIREMENTS
============
You need to have Microsoft .NET Framework 3.5 installed to run this application. 
If you run the parser and get the error: "This application failed to initialize properly", 
then you don't have the .NET framework installed.

You can download .NET 3.5 from here:

http://www.microsoft.com/downloads/details.aspx?FamilyId=333325FD-AE52-4E35-B531-508D977D32A6&displaylang=en



HOW TO USE
=====================
The parser is a command line program. To search the database you supply the search parameters 
when you start the parser. The parser displays all matching results and quits. 

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






