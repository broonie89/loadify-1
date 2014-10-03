#!/usr/bin/env python

# Build script invoked by our Hudson build server to perform our continuous integration builds.
#
# Fetches dependencies from a network share, then configures, builds and tests ohwidget.
#
# Run with --help to list arguments.
#
# You can run this on a developer machine, but you may need to help the script find the
# network share by using the -a argument.

#from hudson_tools import HudsonBuild
from ci_build import run
import sys

description = "Perform an automated build and test, for use on build agents."
command_group = "Build commands"
command_synonyms = ["ci-build", "build"]
command_name = "ci-build"

def hudson_build():
    buildname = "build"
    if len(sys.argv)>=2:
        if not sys.argv[1].startswith("-"):
            buildname = sys.argv[1]
            sys.argv[1:] = sys.argv[2:]
    run(buildname, sys.argv[1:])

if __name__ == "__main__":
    hudson_build()
