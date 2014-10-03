import os
from optparse import OptionParser
import tarfile
import subprocess
import platform

description = "Convert a NuGet package into a tarball."
command_group = "Developer tools"
command_name = "convert-nuget"

def make_parser():
    parser = OptionParser(usage="convert-nuget [OPTIONS...] <PACKAGE_NAME> <VERSION>")
    parser.add_option("-u", "--upload", action="store_true", default=False,   help="Upload the package to openhome.org.")
    return parser

def windows_program_exists(program):
    return subprocess.call(["where", "/q", program], shell=False)==0

def other_program_exists(program):
    nul = open(os.devnull, "w")
    return subprocess.call(["/bin/sh", "-c", "command -v "+program], shell=False, stdout=nul, stderr=nul)==0

program_exists = windows_program_exists if platform.platform().startswith("Windows") else other_program_exists

def scp(*args):
    program = None
    for p in ["scp", "pscp"]:
        if program_exists(p):
            program = p
            break
    if program is None:
        raise "Cannot find scp (or pscp) in the path."
    subprocess.check_call([program] + list(args))

def main():
    parser = make_parser()
    options, args = parser.parse_args()

    if len(args)<2:
        parser.print_usage()
        return

    name = args[0]
    version = args[1]

    tf = tarfile.open('{name}.{version}.tar.gz'.format(name=name, version=version), 'w:gz')
    try:
        os.chdir('dependencies/nuget')
        tf.add('{name}.{version}'.format(name=name, version=version))
    finally:
        tf.close()
    os.chdir('../..')

    if options.upload:
        scp('{name}.{version}.tar.gz'.format(name=name, version=version),
            'releases@openhome.org:/home/releases/www/artifacts/nuget/{name}.{version}.tar.gz'.format(name=name, version=version))


if __name__ == "__main__":
    main()
