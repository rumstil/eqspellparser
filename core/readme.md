SpellParser.cs - Loads raw spell data from game files. The spells_us.txt file format has changed a few times (with the fields moving around) so this has code to handle the different versions.

SpellData.cs - The meat of the parser. Handles SPA/effect decoding.

SpellEnums.cs - Enumerations for basic SPA descriptions, player classes, weapon types, target types, etc...

SpellCache.cs - Helper class for searching and cross referencing spell data.

Patcher.cs - Helper class for downloading game files directly from the EQ Launchpad service.



## Sample Use ##

```csharp
// Load a spell file from the local directory and dump all spells to the console
var spells = SpellParser.LoadFromFile("spells_us.txt");
foreach (var spell in spells)
{
    Console.WriteLine(spell.Name); 
    foreach (var slot in spell.Slots)
        Console.WriteLine(slot.Desc);
    Console.WriteLine();
}
```


