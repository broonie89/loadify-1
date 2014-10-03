# Development

## Prerequisites and dependencies

Before you can build ohLibSpotify, you'll need a few pieces of software.
You'll need to have:

 * Git, or another means to acquire the source code
 * Python 2.6/2.7 (not strictly necessary, see below)
 * A C# compiler (VS2010 or later should do, Mono 3.0 is good)
 * MSBuild/xbuild
 * The ohDevTools scripts (https://github.com/openhome/ohdevtools)

You'll also need some 3rd-party libraries and tools. These can be fetched
automatically with ohDevTools and stored in the the 'dependencies' subfolder:

 * Nuget
 * NUnit
 * MSBuildTasks
 * Newtonsoft.Json
 * libspotify

The specific versions required can be found by looking at
projectdata/dependencies.json and projectdata/packages.config. These will be
fetched for you when you run "go fetch" or "go build".

Apart from libspotify, the other libraries and tools are only required at
build-time. None of the other assemblies are referenced by ohLibSpotify.


## The "go build" command

The "go build" command is a Python script to automate a number of steps:

 * fetch: It fetches 3rd-party dependencies as described above.
 * clean: It invokes msbuild/xbuild with the Clean target.
 * build: It invokes msbuild/xbuild with the Build target.
 * test: It invokes NUnit to run unit tests.

This is entirely optional. If you prefer to do these things manually you don't
need Python or ohDevTools.

You can perform a subset of these actions like so:

    go build --steps="build,test"


## Building on different platforms

When invoking "go build" or "go fetch" the script will try to figure out the
current platform. If it gets it wrong or you want to build for a different
platform, you can override it with the "--platform" option. The possible
values are:

 * Windows-x86
 * Linux-x86
 * Linux-x64
 * Linux-armv5
 * Linux-armv6
 * Linux-armv7
 * Mac-x86
 * Mac-x64
 * iOs-x86
 * iOs-armv6
 * iOs-armv7

So, for example:

    go build --platform=Linux-86

These correspond to all the platforms for which libspotify is available.
Currently only Windows-x86 and Linux-x86 have been tested. Patches are welcome
to fix problems building and running on other platforms.

When in Visual Studio, these platforms can be found in the list of solution
platforms. You should select the platform that matches your system.

