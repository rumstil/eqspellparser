ilmerge ..\winparser\winparser.exe ..\core\lzma.dll /out:winparser.exe
del parser.zip
zip -add parser readme.txt ..\license.txt winparser.exe winparser.config -path=none