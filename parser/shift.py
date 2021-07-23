""" shift spell field indexes after a file update 

e.g. python shift.py ..\core\SpellParser20210715.cs >..\core\SpellParserCurrent.cs

"""

import sys, re

def fix(match):
  index = int(match.group(1))
  # 2018-04-10 removed 31 ACTIVATED
  # 2018-05-09 removed 110 GLOBAL_GROUP
  # 2019-01-08 removed 77 AI_PT_BONUS
  # 2021-01-13 removed 85 SMALL_TARGETS_ONLY
  # 2021-07-15 removed 15 IMAGENUMBER, 16 MEMIMAGENUMBER
  if index >= 15 and index < 200:
    return str(index - 2)
  return str(index)

text = open(sys.argv[1], 'r').read()

text = re.sub(r'(?<=[\s\[])(\d{2,})', fix, text)

print(text)


