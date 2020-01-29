ilmerge ..\winparser\winparser.exe ..\winparser\lzma.dll ..\winparser\EQSpellParser.dll /out:winparser.exe
del parser.zip
zip -add parser readme.txt ..\license.txt winparser.exe winparser.config -path=none