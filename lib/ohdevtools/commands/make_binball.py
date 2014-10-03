import sys
import os
import re
import shutil
from optparse import OptionParser
from antglob import ant_glob
import tarfile

description = "Assemble a tarball of binaries."
command_group = "Build components"
command_name = "make-binball"

def make_parser():
    parser = OptionParser(usage="make_binball [OPTIONS...] <TARBALL_FILENAME.tar.gz> <FILESPEC>...")
    parser.add_option("-b", "--base-dir",  dest="basedir", action="store", default=".",   help="Base directory from which files are referenced.")
    parser.add_option("-p", "--prefix",    dest="prefix",  action="store", default="",    help="Prefix for paths inside archive.")
    return parser

def main():
    parser = make_parser()
    options, args = parser.parse_args()

    if len(args)<2:
        parser.print_usage()
        return

    tf = tarfile.open(args[0], 'w:gz')
    try:
        os.chdir(options.basedir)
        for pattern in args[1:]:
            for fname in ant_glob(pattern):
                if fname.startswith('./'):
                    fname = fname[2:]
                tf.add(fname, options.prefix + fname)
    finally:
        tf.close()

if __name__ == "__main__":
    main()
