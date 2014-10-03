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

description = "Perform an automated stress test, for use on build agents."
command_group = "Hudson commands"
command_hidden = True

def hudson_build():
    run("build", ["--stresstest-only"])
    # The old script fetched and configured stuff. I don't *think* that's necessary,
    # but I might be wrong. Possibly depends on what artifacts ohwidget publishes.
    #builder = HudsonBuild()
    #builder.prebuild()
    #configure_args = builder.copy_dependencies()
    #builder.configure(configure_args)
    #builder.stresstest()

if __name__ == "__main__":
    hudson_build()
