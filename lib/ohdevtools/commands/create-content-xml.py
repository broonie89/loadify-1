from antglob import ant_glob
import json
import sys
import os
import ntpath
from xml.sax.saxutils import escape, quoteattr


description = "Generates MSBuild fragments for embedding content."
# Hide from "go help" for now: not relevant to most projects.
command_hidden = True

usage_text = """

Creates XML to insert in a csproj file to embed content.

Usage:
    create-content-xml config.json

Configuration file should be pure JSON, and contain one object
with these fields:

    basedir - Directory that contains the csproj file.
    files - List of file specification objects.

File specification objects have these fields:

    source   - Relative path from the project directory to the
               tree of files to add.
    target   - Location to output the tree of files during a build.
    patterns - List of glob patterns to select content files.

For example:

{   "basedir":"src/ChatFormsTouch",
    "files":[
        {   "source":"../../build/main/http/ohj",
            "target":"http/ohj",
            "patterns":["**/*"]
        },
        {   "source":"../../build/apps/OpenHome.ChatForms/http",
            "target":"app/http",
            "patterns":["**/*"]
        },
        {   "source":"../../build/main/modules",
            "target":"modules",
            "patterns":["**/*"]
        }
    ]
}

""".strip()

def main():
    if len(sys.argv) <= 1 or sys.argv[1] == '--help':
        print usage_text
        return
    with open(sys.argv[1]) as f:
        config = json.load(f)
    basedir = os.path.abspath(config['basedir'])
    os.chdir(basedir)
    output_directories = set()
    output_files = []
    for fileset in config['files']:
        os.chdir(basedir)
        os.chdir(fileset['source'])
        for pattern in fileset['patterns']:
            files = ant_glob(pattern)
            for filename in files:
                frompath = ntpath.normpath(ntpath.join(fileset['source'], filename))
                topath = ntpath.normpath(ntpath.join(fileset['target'], filename))
                output_directories.update(topath[:index+1] for (index, ch) in enumerate(topath) if ch=='\\')
                output_files.append((frompath, topath))
    print "  <ItemGroup>"
    for dirname in sorted(output_directories):
        print "    <Folder Include={0} />".format(quoteattr(dirname))
    print "  </ItemGroup>"
    print "  <ItemGroup>"
    for (frompath, topath) in output_files:
        print "    <Content Include=\"{0}\">\n      <Link>{1}</Link>\n    </Content>".format(escape(frompath), escape(topath))
    print "  </ItemGroup>"

    
if __name__ == "__main__":
    main()
