from xml.etree.cElementTree import parse, Element, ElementTree, dump
from os import walk
from os.path import join
from optparse import OptionParser

description = "Update the master package.config from individual project ones."
command_group = "Developer tools"

# Snippet used from the ElementTree documentation.
# Tidy up the indentation of XML elements.
def indent(elem, level=0):
    i = "\n" + level*"  "
    if len(elem):
        if not elem.text or not elem.text.strip():
            elem.text = i + "  "
        if not elem.tail or not elem.tail.strip():
            elem.tail = i
        for elem in elem:
            indent(elem, level+1)
        if not elem.tail or not elem.tail.strip():
            elem.tail = i
    else:
        if level and (not elem.tail or not elem.tail.strip()):
            elem.tail = i

def parse_args():
    usage = (
        "\n"+
        "    %prog\n"+
        "\n"+
        "Update the master projectdata/packages.config from all the individual project\n"+
        "package.config files throught the src/ directory.")
    parser = OptionParser(usage=usage)
    return parser.parse_args()

def match(value, target_string):
    for target in target_string.split("|"):
        if target=="":
            return True
        if target==value:
            return True
    return False

def main():
    options, args = parse_args()
    rootElement = Element('packages')

    packages = {}

    print "Searching for packages.config files:"

    for dirpath, subdirs, filenames in walk('src'):
        for filename in filenames:
            if filename == 'packages.config':
                filepath = join(dirpath, filename)
                print "    " + filepath
                et = parse(filepath)
                for packageElement in et.findall('package'):
                    pkgId = packageElement.get('id')
                    pkgVersion = packageElement.get('version')
                    packages[pkgId, pkgVersion] = packageElement

    print
    print "Writing projectdata/packages.config:"
    rootElement.extend([value for (key,value) in sorted(packages.items())])
    indent(rootElement)
    tree = ElementTree(rootElement)
    dump(tree)
    tree.write('projectdata/packages.config')

if __name__ == "__main__":
    main()
