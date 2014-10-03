#!/usr/bin/python

import signal
import subprocess
import os
import sys

class Program():
        def __init__(self):
                self.pid = ''

        def call(self):

                argv = sys.argv[1]

                process = subprocess.Popen(argv,shell=True)
                self.pid = process.pid
                process.wait()

                sys.exit(process.returncode)

        def kill(self):
                os.kill(self.pid, signal.SIGTERM)

class Watchdog(Exception):
  def __init__(self, time=5):
    self.time = time

  def __enter__(self):
    signal.signal(signal.SIGALRM, self.handler)
    signal.alarm(self.time)

  def __exit__(self, type, value, traceback):
    signal.alarm(0)

  def handler(self, signum, frame):
    raise self

program = Program()

try:
  with Watchdog(420):
    program.call()

except Watchdog:
  print "process took too long to complete - killed!"
  program.kill()
  sys.exit(1)
