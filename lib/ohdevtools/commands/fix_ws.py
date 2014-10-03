import sys
import os
import re
import shutil
from optparse import OptionParser
from antglob import ant_glob

description = "Re-write files to change line-endings and/or indentation."
command_group = "Developer tools"
command_name = "fix-ws"

# State machine for analysing text files
class LineEndingMachine(object):
    def checklineend(self, ch):
        if ch=="\r":
            return True, "", self.cr
        if ch=="\n":
            self.has_lf=True
            self.indent = 0
            return True, self.lineending, self.linestart
        return False, None, None
    def linestart(self, ch):
        lineend, output, nextstate = self.checklineend(ch)
        if lineend:
            return output, nextstate
        return ch, self.linestart
    def cr(self, ch):
        if ch=="\n":
            return self.lineending, self.linestart
        return self.lineending+ch, self.linestart
    def __init__(self, lineending):
        self.state = self.linestart
        self.lineending = lineending
    def nextchar(self, ch):
        outputch, self.state = self.state(ch)
        return outputch

BUFFSIZE=8192

def fix_file(filename, lineending="\n"):
    m = LineEndingMachine(lineending)
    with open(filename, 'rb') as f:
        with open(filename+".fixedendings", 'wb') as f2:
            while 1:
                buff = f.read(BUFFSIZE)
                obuff = []
                if buff == "":
                    break
                for ch in buff:
                    obuff.append(m.nextchar(ch))
                f2.write(''.join(obuff))
    shutil.copystat(filename, filename+".fixedendings")
    if os.path.exists(filename+".oldendings"):
        os.remove(filename+".oldendings")
    os.rename(filename, filename+".oldendings")
    os.rename(filename+".fixedendings", filename)
    os.remove(filename+".oldendings")

def parse_args():
    parser = OptionParser()
    parser.add_option("-u", "--unix",      dest="endings", action="store_const", const="\n",   help="Convert to Unix line-endings (LF).")
    parser.add_option("-d", "--dos",       dest="endings", action="store_const", const="\r\n", help="Convert to DOS line-endings (CRLF).")
    parser.add_option("-m", "--mac",       dest="endings", action="store_const", const="\r",   help="Convert to Max line-endings (CR).")
    parser.set_defaults(indent="", endings="\n")
    return parser.parse_args()

def main():
    options, args = parse_args()
    for pattern in args:
        for fname in ant_glob(pattern):
            fix_file(fname, options.endings)
            print fname

if __name__ == "__main__":
    main()
