""" shift spell field indexes after a file update """

import sys, re

def fix(match):
  index = int(match.group(1))
  # 2018-5-9, removed 110 GLOBAL_GROUP
  if index > 110:
    return str(index - 1)
  return str(index)

text = open(sys.argv[1], 'r').read()

text = re.sub(r'(\d{3})', fix, text)

print(text)
