from optparse import OptionParser
import os

description = "Write a simple AssemblyInfo.cs file."
command_group = "Build components"
command_name = "make-assemblyinfo"

TEMPLATE = '''\
// Automatically generated file

using System;
using System.Reflection;

[assembly: AssemblyVersion({assembly_version})]
[assembly: AssemblyFileVersion({assembly_file_version})]
'''


def encode_csharp_string(s):
    result = ['"']
    for ch in s:
        if ch == '\0': result.append('\\0')
        elif ch == '\a': result.append('\\a')
        elif ch == '\b': result.append('\\b')
        elif ch == '\f': result.append('\\f')
        elif ch == '\n': result.append('\\n')
        elif ch == '\r': result.append('\\r')
        elif ch == '\t': result.append('\\t')
        elif ch == '\v': result.append('\\v')
        elif ch == '\"': result.append('\\"')
        elif ch == '\\': result.append('\\\\')
        else: result.append(ch)
    result.append('"')
    return ''.join(result)


def make_parser():
    parser = OptionParser(usage="make_binball [OPTIONS...] <OUTPUT_FILENAME.cs>")
    parser.add_option("-v", "--assembly-version",      dest="assembly_version",     action="store", default="0.0.0.1", help=".NET assembly version to output.")
    parser.add_option("-f", "--assembly-file-version", dest="assembly_file_version",action="store", default="0.0.0.1", help="DLL file version to output.")
    return parser

def main():
    parser = make_parser()
    options, args = parser.parse_args()

    if len(args)<1:
        parser.print_usage()
        return

    new_content = TEMPLATE.format(
            assembly_version=encode_csharp_string(options.assembly_version),
            assembly_file_version=encode_csharp_string(options.assembly_file_version))

    try:
        with open(args[0], 'r') as infile:
            old_content = infile.read()
            if old_content == new_content:
                return
    except IOError:
        # E.g. file not found. Continue and try to write the file.
        pass

    dirpath, _ = os.path.split(os.path.abspath(args[0]))
    if not os.path.isdir(dirpath):
        os.makedirs(dirpath)
    with open(args[0], 'w') as outfile:
        outfile.write(new_content)

if __name__ == "__main__":
    main()
