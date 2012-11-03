md pack
ilmerge parser.exe lzma.dll /out:pack\parser.exe
ilmerge winparser\winparser.exe lzma.dll /out:pack\winparser.exe
del parser.zip
zip -add parser readme.txt pack\*.exe winparser\winparser.config -path=none