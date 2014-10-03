# Defines the build behaviour for continuous integration builds.

import sys

try:
    from ci import (OpenHomeBuilder, require_version)
except ImportError:
    print "You need to update ohDevTools."
    sys.exit(1)

require_version(22)


class Builder(OpenHomeBuilder):
    test_location = 'build/bin/{configuration}/{assembly}.dll'
    def setup(self):
        self.set_nunit_location('dependencies/nuget/NUnit.Runners.2.6.1/tools/nunit-console-x86.exe')
        if self.system != 'Windows':
            self.shell('mono --version')

    def clean(self):
        self.msbuild('src/ohLibSpotify.sln', target='Clean', platform=self.platform, configuration=self.configuration)

    def build(self):
        self.msbuild('src/ohLibSpotify.sln', target='Build', platform=self.platform, configuration=self.configuration)

    def test(self):
        self.nunit('ToolTests')

    def publish(self):
        self.publish_package(
                'ohLibSpotify-{platform}-{configuration}.tar.gz',
                'ohLibSpotify/ohLibSpotify-{version}-{platform}-{configuration}.tar.gz')
