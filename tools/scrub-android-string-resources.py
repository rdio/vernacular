#!/usr/bin/env python3

import os
import sys
import re
import argparse
import xml.etree.ElementTree

parser = argparse.ArgumentParser()

parser.add_argument('PROJECT_PATH',
  action = 'store',
  help = 'path to Mono for Android project')

parser.add_argument('STRINGS_XML_PATH',
  action = 'store',
  help = 'path to old strings.xml')

args = parser.parse_args()

string_ids = []

for root, dirnames, filenames in os.walk(args.PROJECT_PATH):
  for filename in filenames:
    try:
      with open(os.path.join(root, filename), encoding = "utf-8") as fp:
        for match in re.finditer(r'@string/([\w]+)', fp.read()):
          if match.group(1) not in string_ids:
            string_ids.append(match.group(1))
    except:
      pass

string_ids.sort()

doc = xml.etree.ElementTree.parse(args.STRINGS_XML_PATH)
resources_element = doc.getroot()
for string_element in resources_element.findall('string'):
  if string_element.get('name') not in string_ids:
    resources_element.remove(string_element)

doc.write(args.STRINGS_XML_PATH, encoding = 'utf-8', xml_declaration = True)
