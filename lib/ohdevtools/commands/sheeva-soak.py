#!/bin/python

# usage:
#
#    sheeva-soak 
#
# Working directory should be ohwidget, 

import sys
import os
import subprocess
import datetime
import time
import shutil
import glob

description = "Run soak tests. ohWidget-specific."
# Hide this from "go help" for now:
command_hidden = True

REQUIRED_DIRS = ["build", "integrationtest", "scripts", "soaktestrepo"]

timestamp = lambda:datetime.datetime.now().isoformat()

def mk_logfunc(logfile):
    def logfunc(msg):
        logfile.write("%s:%s\n" % msg)
        logfile.flush()
    return logfunc

def test_thread_func(logfunc):
    while True:
        try:
            p = subprocess.Popen([sys.executable, "integrationtest/explicit.test.SoakTest.py"])
            logfunc('Started explicit.test.SoakTest.py with PID=%s' % p.pid)
            p.wait()
            logfunc('explicit.test.SoakTest.py (PID=%s) finished with return code 0x%x' % (p.pid, p.returncode))
        except OSError, ose:
            logfunc('Failed to start explicit.test.SoakTest.py with exception:\n%s' % (ose,))
        time.sleep(5.0)

def upload_thread_func(logfunc):
    while True:
        time.sleep(60.0)
        try:
            logfunc('copying csv files to repo...')
            for filename in glob.glob('build/soak_test_output/*.csv'):
                shutil.copy(filename, 'soaktestrepo/')
            logfunc('commiting to repo...')
            subprocess.check_call(['git', 'commit', '-a', '-m', '"automatic commit by sheeva-soak.py"'], cwd='soaktestrepo/')
            logfunc('pushing...')
            subprocess.check_call(['git', 'push'], cwd='soaktestrepo/')
            logfunc('done')
        except OSError, ose:
            logfunc('something went wrong, exception:\n%s' % (ose,))


def main():
    if not all(map(os.path.isdir(REQUIRED_DIRS))):
        sys.stderr.write("Expected to find these directories in current directory:\n")
        sys.stderr.write("    "+"\n    ".join(REQUIRED_DIRS)+"\n")
        sys.exit(1)
    logfile = file('soaktestrepo/sheeva-soak.log','a')
    logfunc = mk_logfunc(logfile)
    logfunc('sheeva-soak.py started')
    test_thread = threading.Thread(target=test_thread_func, args=(logfunc,))


