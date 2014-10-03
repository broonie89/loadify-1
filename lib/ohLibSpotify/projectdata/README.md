# projectdata

This directory contains configuration information used by automated builds
and the dependency fetching systems.

## packages.config

This is a NuGet packages file. It contains nuget packages that will be fetched
either by the "go fetch --nuget" command or by the "fetch" step during
"go ci-build".

This file should not be edited directly. It is normally reconstructed by running
the "go update-nuget" command, which searchs for all packages.config files in the
/src hierarchy, and consolidates them into this one file.

## dependencies.json

This is a JSON-formatted file that describes non-NuGet dependencies which will
be fetched by the "go fetch" command. Generally these are fetched from
openhome.org. See the ohDevTools documentation, or look at dependencies.py from
ohDevTools for more information.

## build_behaviour.py

This file specifies the behaviour of the "go ci-build" command, which is used to
automate builds on the continuous integration servers. It should define a Python
class called "Builder" that sub-classes "OpenHomeBuilder" from ci_build.py in
ohDevTools. Possible build steps that can be defined are:

setup   - Performs necessary configuration before the build
fetch   - Fetch dependencies defined in dependencies.json and packages.config
clean   - Removed output of previous builds
build   - Builds all projects and packages
test    - Runs all automated tests
publish - Publishes packages to a remote server
