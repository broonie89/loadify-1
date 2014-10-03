# README

ohLibSpotify is OpenHome's managed wrapper around the libspotify library.
To use ohLibSpotify, an application needs only the managed ohLibSpotify.dll,
and the native libspotify.so|.dll|.dylib. (It is not dependent on any other
OpenHome library.)

ohLibSpotify attempts to expose all libspotify entities - such as sessions,
playlists and tracks - as regular C# classes, and where possible libspotify
functions become instance methods on the appropriate class. It does all the
necessary conversions to and from UTF8, so that users see only managed
strings.

## Quick start

Prerequisites:

 * Git
 * Python 2.6/2.7 (technically you don't *need* Python, but it's much easier.)
 * Visual Studio 2010 or Mono 3.0

Sync up these two repositories:

    git clone https://github.com/openhome/ohdevtools.git
    git clone https://github.com/openhome/ohLibSpotify.git

Build (note that building this way will fetch dependencies from the Internet):

    cd ohLibSpotify
    go build

You need a Spotify API key to use libspotify in an app. You can get one here:

    https://developer.spotify.com/technologies/libspotify/keys/

Copy it into your ohLibSpotify directory.

Now you can run the demo console app:

    build/bin/Release/SpShellSharp.exe

For more details about building ohLibSpotify, see doc/development.md
For more details about using ohLibSpotify, see doc/usage.md

## Other platforms

Currently, only the Windows-x86 platform has been tested. Our focus will be
on Windows, Linux and Mac, but patches for iOS and Android are welcome and we
do try to avoid doing anything that would make porting difficult. (In
particular, we only use static methods when passing callbacks to P/Invoke
methods, so it should not be too hard to run on Mono with the JIT disabled,
as is necessary on iOS.)

Note that the ohLibSpotify.dll assembly is platform-specific. You need to
build a different version for each platform. The managed API should remain
the same between platforms.

## Community

Discuss ohLibSpotify here:

    http://forum.openhome.org/forumdisplay.php?fid=6

Get the source code and submit bug reports on github:

    https://github.com/openhome/ohLibSpotify

You can also ask questions on Stack Overflow tagged with 'libspotify' or
'ohlibspotify'. Stack Overflow is a good place for general questions about
libspotify that are not specific to ohLibSpotify, as you will find a wider
audience.

    http://stackoverflow.com/

## Directory contents

### go / go.bat

Provides the "go" commands that can be used to fetch dependencies,
run builds and maintain files. Before you can invoke these commands,
you must fetch the ohDevTools repository and place it side-by-side
with this one. Run "go" on its own for a list of commands.

### projectdata

Contains configuration information used by automated builds and dependency
fetching tools.

### src

Contains the source code of the project.

### dependencies

This directory will be created by the "go fetch" command. It should contain
external dependencies required during the build process, such as non-framework
third-party assemblies.

### build

This directory will be created during a build. It contains whatever
assemblies, libraries and packages are created by the build.
