import os
import tarfile
import zipfile
import re
import urllib
import urllib2
import platform
import subprocess
import json
import shutil
import cStringIO
import stat
import tempfile
from glob import glob
from default_platform import default_platform

# Master table of dependency types.

# A dependency definition can specify 'type' to inherit definitions from one of these.
# String values can depend on other string values from the dependency. For example,
# if 'name' is defined as 'Example' then '${name}.exe' will expand to 'Example.exe'.
# It does not matter which order the values are defined.
# String values can also depend on boolean values. For example, the string
# '${test-value?yes-result:no-result}' will get the value of the string named
# 'yes-result' if 'test-value' is a true boolean value, and the string named
# 'no-result' if 'test-value' is a false boolean value.
# Finally, string values can also depend on a lookup table defined as a JSON object.
# For example, given these definitions:
# {
#     "servertable":{
#         "Windows":"windows.openhome.org",
#         "Linux":"linux.openhome.org",
#         "*":"openhome.org"
#     },
#     "server":"${servertable[$system]}"
# }
# If 'system' is defined as 'Windows', then 'server' will be defined as
# 'windows.openhome.org'. The '*' entry is the default: if a lookup fails the default
# will be used instead.

# The principle string values that must be defined are 'archive-path' to point to the
# .tar.gz file with the dependency's binaries, 'dest' to specify where to untar it,
# and 'configure-args' to specify the list of arguments to pass to waf.

# In order for source control fetching to work, the string 'source-git' should point
# to the git repo and 'tag' should identify the git tag that corresponds to the
# fetched binaries.

DEPENDENCY_TYPES = {
    # Label a dependency with the 'ignore' type to prevent it being considered at all.
    # Can be useful to include comments. (Json has no comment syntax.)
    'ignore' : {
        'ignore': True     # This causes the entire dependency entry to be ignored. Useful for comments.
        },

    # Openhome dependencies generally have an associated git repo to allow us to
    # fetch source code. They also have a different directory layout to accomodate
    # the large number of versions created by CI builds.
    #
    # An openhome dependency, at minimum, must define:
    #     name
    #     version
    #
    # Commonly overridden:
    #     archive-suffix
    #     platform-specific
    #     configure-args
    'openhome' : {
        'archive-extension': '.tar.gz',
        'archive-prefix': '',
        'archive-suffix': '',
        'binary-repo': 'http://openhome.org/releases/artifacts',
        'archive-directory': '${binary-repo}/${name}/',
        'archive-filename': '${archive-prefix}${name}-${version}-${archive-platform}${archive-suffix}${archive-extension}',
        'remote-archive-path': '${archive-directory}${archive-filename}',
        'use-local-archive': False,
        'archive-path': '${use-local-archive?local-archive-path:remote-archive-path}',
        'source-path': '${linn-git-user}@core.linn.co.uk:/home/git',
        'repo-name': '${name}',
        'source-git': '${source-path}/${repo-name}.git',
        'tag': '${repo-name}_${version}',
        'any-platform': 'AnyPlatform',
        'platform-specific': True,
        'archive-platform': '${platform-specific?platform:any-platform}',
        'dest': 'dependencies/${archive-platform}/',
        'configure-args': [],
        'strip-archive-dirs': 0
        },

    # Internal dependencies are named and structured in a similar manner
    # to those of type 'openhome', but are considered private, and held
    # on core.linn.co.uk
    #
    # Must define, at minimum:
    #       name
    #       version
    #
    # Commonly overridden:
    #       archive-suffix

    'internal' : {
        'binary-repo': 'http://core.linn.co.uk/~artifacts/artifacts',
        'source-git': None,
        'any-platform': 'AnyPlatform',
        'platform-specific': True,
        'archive-suffix': '',
        'archive-filename': '${name}-${version}-${platform}${archive-suffix}.tar.gz',
        'archive-platform': '${platform-specific?platform:any-platform}',
        'archive-path': '${binary-repo}/${name}/${archive-filename}',
        'dest': 'dependencies/${archive-platform}/',
        'configure-args': [],
        'strip-archive-dirs': 0
        },

    # External dependencies generally don't have a git repo, and even if they do,
    # it won't conform to our conventions.
    #
    # An external dependency, at minimum, must define:
    #     name
    #     archive-filename
    #
    # Commonly overriden:
    #     platform-specific
    #     configure-args
    'external' : {
        'binary-repo': 'http://openhome.org/releases/artifacts',
        'source-git': None,
        'any-platform': 'AnyPlatform',
        'platform-specific': True,
        'archive-platform': '${platform-specific?platform:any-platform}',
        'archive-path': '${binary-repo}/${archive-platform}/${archive-filename}',
        'dest': 'dependencies/${archive-platform}/',
        'configure-args': [],
        'strip-archive-dirs': 0
        },

    # Ex-nuget dependencies don't have a git repo, but they are always
    # AnyPlatform and have a strict convention on location and structure
    # that makes them easy to specify.
    #
    # An exnuget dependency need only specify:
    #     name
    #     version
    'exnuget' : {
        'archive-extension': '.tar.gz',
        'binary-repo': 'http://openhome.org/releases/artifacts',
        'archive-directory': '${binary-repo}/nuget/',
        'archive-filename': '${name}.${version}${archive-extension}',
        'archive-path': '${archive-directory}${archive-filename}',
        'dest': 'dependencies/nuget/',
        'configure-args': [],
        'strip-archive-dirs': 0
        },
    }









def default_log(logfile=None):
    return logfile if logfile is not None else open(os.devnull, "w")

def windows_program_exists(program):
    return subprocess.call(["which", "/q", program], shell=False)==0

def other_program_exists(program):
    return subprocess.call(["/bin/sh", "-c", "command -v "+program], shell=False, stdout=open(os.devnull), stderr=open(os.devnull))==0

program_exists = windows_program_exists if platform.platform().startswith("Windows") else other_program_exists



def scp(source, target):
    program = None
    for p in ["scp", "pscp"]:
        if program_exists(p):
            program = p
            break
    if program is None:
        raise "Cannot find scp (or pscp) in the path."
    subprocess.check_call([program, source, target])


def open_file_url(url):
    smb = False
    if url.startswith("smb://"):
        url = url[6:]
        smb = True
    elif url.startswith("file://"):
        url = url[7:]
    path = urllib.url2pathname(url).replace(os.path.sep, "/")
    if path[0]=='/':
        if path[1]=='/':
            # file:////hostname/path/file.ext
            # Bad remote path.
            remote = True
            legacy = True
            final_path = path.replace("/", os.path.sep)
        else:
            # file:///path/file.ext
            # Good local path.
            remote = False
            legacy = False
            if smb:
                raise Exception("Bad smb:// path")
            final_path = path[1:].replace("/", os.path.sep)
    else:
        if path[0].isalpha() and path[1] == ':':
            # file:///x:/foo/bar/baz
            # Good absolute local path.
            remote = False
            legacy = False
            final_path = path.replace('/', os.path.sep)
        else:
            # file://hostname/path/file.ext
            # Good remote path.
            remote = True
            legacy = False
            final_path = "\\\\" + path.replace("/", os.path.sep)
    if smb and (legacy or not remote):
        raise Exception("Bad smb:// path. Use 'smb://hostname/path/to/file.ext'")
    if (smb or remote) and not platform.platform().startswith("Windows"):
        raise Exception("SMB file access not supported on non-Windows platforms.")
    return final_path


class FileFetcher(object):

    def __init__(self):
        pass

    def fetch(self, path):
        if path.startswith("file:") or path.startswith("smb:"):
            return self.fetch_file_url(path)
        if re.match("[^\W\d]{2,8}:", path):
            return self.fetch_url(path)
        return self.fetch_local(path)

    def fetch_local(self, path):
        return path, 'file'

    def fetch_file_url(self, path):
        return open_file_url(path), 'file'

    def fetch_url(self, path):
        return urlopen(path), 'web'


def urlopen(url):
    handle, temppath = tempfile.mkstemp( suffix='.tmp' )
    try:
        remotefile = urllib2.urlopen(url)
        localfile = os.fdopen( handle, 'wb' )
        chunk = remotefile.read( 100000 )
        while len( chunk ):
            localfile.write( chunk )
            chunk = remotefile.read( 100000 )
        localfile.close()
        remotefile.close()
    except:
        print '\n\n**** FAILED reading from %s ****\n' % url
        os._exit(-1)
    return temppath

def is_trueish(value):
    if hasattr(value, "upper"):
        value = value.upper()
    return value in [1, "1", "YES", "Y", "TRUE", "ON", True]


class EnvironmentExpander(object):
    # template_regex matches 
    template_regex = re.compile(r"""
        (?x)                                # Enable whitespace and comments
        (?P<dollar>\$\$)|                   # Match $$
        (?P<word>\$[a-zA-Z_][a-zA-Z_0-9]*)| # Match $word
        (?P<parens>\$\{[^}]*\})             # Match ${any-thing}
        """)
    # Matches foo[bar]
    index_regex = re.compile(r"""
        (?x)         # Enable whitespace and comments
        ^            # Match only at start of string
        ([^][]*)     # Match table name (no brackets allowed)
        \[           # Match one open bracket: [
        ([^][]*)     # Match key (no brackets allowed)
        \]           # Match one close bracket: ]
        $
        """)
    def __init__(self, env_dict):
        self.env_dict = env_dict
        self.cache = {}
        self.expandset = set()
    def __getitem__(self, key):
        return self.expand(key)
    def getraw(self, key):
        return self.env_dict[key]
    def __contains__(self, key):
        return key in self.env_dict
    def keys(self):
        return self.env_dict.keys()
    def values(self):
        return [self.expand(key) for key in self.keys()]
    def items(self):
        return [(key, self.expand(key)) for key in self.keys()]
    def expand(self, key):
        if key in self.cache:
            return self.cache[key]
        if key in self.expandset:
            raise ValueError("Recursive expansion for key:", key)
        self.expandset.add(key)
        result = self._expand(key)
        self.cache[key] = result
        self.expandset.remove(key)
        return result
    def _expand(self, key):
        if key not in self.env_dict:
            raise KeyError("Key undefined:", key)
        value = self.env_dict[key]
        return self._expandvalue(value)
    def _expandvalue(self, value):
        if isinstance(value, (str, unicode)):
            return self.expandstring(value)
            #return self.template_regex.sub(self.replacematch, value)
        elif isinstance(value, (list, tuple)):
            return [self._expandvalue(x) for x in value]
        elif isinstance(value, dict):
            return dict((k, self._expandvalue(v)) for (k,v) in value.items())
        return value
    def expandstring(self, value):
        firstmatch = self.template_regex.match(value)
        if firstmatch is not None and firstmatch.group(0) == value and value != "$$":
            # Special case: The entire string is a single expansion. In this case,
            # we allow the expansion to be *anything* (bool, int, list...),
            # not just a string.
            return self.replacematch(firstmatch)
        return self.template_regex.sub(self.replacematch, value)

    def replacematch(self, match):
        if match.group('dollar'):
            return '$'
        key = None
        if match.group('word'):
            key = match.group('word')[1:]
        if match.group('parens'):
            key = match.group('parens')[2:-1]
        assert key is not None
        key = key.strip()
        if '[' in key:
            return self.expandlookup(key)
        if '?' in key:
            return self.expandconditional(key)
        return self.expand(key)

    def expandlookup(self, key):
        match = self.index_regex.match(key)
        if match is None:
            raise ValueError('lookup must be of form ${table[key]}')
        tablename = match.group(1).strip()
        keyname = match.group(2).strip()
        table = self.expand(tablename)
        if keyname.startswith('$'):
            key = self.expand(keyname[1:])
        else:
            key = keyname
        if not isinstance(table, dict):
            raise ValueError("lookup table must expand to a JSON object (got {0!r} instead)".format(table))
        if not isinstance(key, (str,unicode)):
            raise ValueError("lookup index must expand to a JSON string (got {0!r} instead)".format(key))
        if not key in table:
            if '*' in table:
                return table['*']
            raise KeyError("Key not in table, and no default '*' entry found: key={0!r}\ntable={1!r}".format(key, table))
        return table[key]

    def expandconditional(self, key):
        if '?' not in key:
            raise ValueError('conditional must be of form ${condition?result:alternative}')
        condition, rest = key.split('?', 1)
        if ':' not in rest:
            raise ValueError('conditional must be of form ${condition?result:alternative}')
        primary, alternative = rest.split(':', 1)
        condition, primary, alternative = [x.strip() for x in [condition, primary, alternative]]
        try:
            conditionvalue = self.expand(condition)
        except KeyError:
            conditionvalue = False
        if is_trueish(conditionvalue):
            return self.expand(primary)
        return self.expand(alternative)


class Archive(object):

    def extract(self, local_path, strip_dirs=0):
        # The general idea is to mutate the in-memory archive, changing the
        # path of files to remove their prefix directories, before invoking
        # extract repeatedly. This can solve the problem of archives that
        # include a top-level directory whose name includes a variable value
        # like version-number, which forces us to change assembly references
        # in every project for every minor change.
        infolist = self.getinfolist()
        goodentries=[]
        for entry in infolist:
            path_fragments = self.getentryname(entry).split('/')
            isgood = len(path_fragments) > strip_dirs
            isdir = self.isdir(entry)
            if isgood:
                path_fragments = path_fragments[strip_dirs:]
                if path_fragments == ['']:
                    continue
                filename = '/'.join(path_fragments)
                self.setentryname(entry, filename)
                goodentries.append(entry)
            if (not isdir) and (not isgood):
                raise ValueError('Attempted to strip more leading directories than contained in archive file:{0}, strip:{1}'.format(self.getentryname(entry), strip_dirs))
        self.extract_many(goodentries, local_path)

    def extract_files(self, entries, local_path):
        for entry in entries:
            if not self.isdir(entry):
                self.extractentry(entry, local_path)

    def extract_directories(self, entries, local_path):
        for entry in entries:
            if self.isdir(entry):
                self.extractentry(entry, local_path)

class ZipArchive(Archive):

    def __init__(self, path):
        self.zf = zipfile.ZipFile(path, "r")

    def getinfolist(self):
        return self.zf.infolist()

    def getentryname(self, entry):
        return entry.filename

    def setentryname(self, entry, name):
        entry.filename = name

    def extract_many(self, entries, localpath):
        # Extract the directories first, as zipfile doesn't create
        # them on demand.
        self.extract_directories(entries, localpath)
        self.extract_files(entries, localpath)

    def extractentry(self, entry, localpath):
        permission_bits = entry.external_attr >> 16
        is_dir = stat.S_ISDIR(permission_bits)
        is_symlink = stat.S_ISLNK(permission_bits)
        if is_dir:
            # Zipfile directory handling is broken in Python 2.6
            # We need to do it ourselves
            dirpath = os.path.join(localpath, entry.filename)
            try:
                os.mkdir(dirpath)
                # We don't currently restore directory permissions
            except OSError:
                # Python makes it unnecessarily hard only to ignore
                # failure to create the directory due to it already
                # existing. Easier to ignore everything. In the
                # rare cases when it would fail for other reasons,
                # it's very likely that another operation will fail
                # shortly anyway.
                pass
        elif is_symlink and platform.system() != 'Windows':
            linktext = self.zf.read(entry)
            # Symlink handling broken in zipfile
            # Create symlinks manually, except on Windows, where
            # symlinks are very poorly supported.
            os.symlink(linktext, os.path.join(localpath, entry.filename))
        else:
            self.zf.extract(entry, localpath)

    def isdir(self, entry):
        return entry.filename.endswith('/')

    def close(self):
        self.zf.close()


class TarArchive(Archive):

    def __init__(self, path):
        self.tf = tarfile.open(path, mode="r:*")

    def getinfolist(self):
        return list(self.tf)

    def getentryname(self, entry):
        return entry.name

    def setentryname(self, entry, name):
        entry.name = name

    def extract_many(self, entries, localpath):
        # Extract files first (directories will be created as needed):
        self.extract_files(entries, localpath)
        # Extract directories to update their attributes:
        self.extract_directories(entries, localpath)

    def extractentry(self, entry, localpath):
        self.tf.extract(entry, localpath)

    def isdir(self, entry):
        return entry.isdir()

    def close(self):
        self.tf.close()


def openarchive( name, temppath ):
    if os.path.splitext(name)[1].upper() in ['.ZIP', '.NUPKG', '.JAR']:
        return ZipArchive( temppath )
    else:
        return TarArchive( temppath )

def extract_archive(archive, local_path, strip_dirs=0):
    archive.extract(local_path, strip_dirs)


class Dependency(object):

    def __init__(self, name, environment, fetcher, logfile=None, has_overrides=False):
        self.expander = EnvironmentExpander(environment)
        self.logfile = default_log(logfile)
        self.has_overrides = has_overrides
        self.fetcher = fetcher

    def fetch(self):
        remote_path = self.expander.expand('archive-path')
        local_path = os.path.abspath(self.expander.expand('dest'))
        fetched_path = None
        strip_dirs = self.expander.expand('strip-archive-dirs')
        self.logfile.write("Fetching '%s'\n  from '%s'" % (self.name, remote_path))
        try:
            fetched_path, method = self.fetcher.fetch(remote_path)
            self.logfile.write(" (" + method + ")\n")
            archive = openarchive(remote_path, fetched_path)
        except IOError:
            self.logfile.write("\n  FAILED\n")
            return False
        try:
            os.makedirs(local_path)
        except OSError:
            # We get an error if the directory exists, which we are happy to
            # ignore. If something worse went wrong, we will find out very
            # soon when we try to extract the files.
            pass
        self.logfile.write("  unpacking to '%s'\n" % (local_path,))
        extract_archive(archive, local_path, strip_dirs)
        archive.close()
        if fetched_path:
            if fetched_path != remote_path:
                os.unlink(fetched_path)
        self.logfile.write("  OK\n")
        return True

    @property
    def name(self):
        return self['name']
    def __getitem__(self, key):
        return self.expander.expand(key)
    def __contains__(self, key):
        return key in self.expander
    def items(self):
        return self.expander.items()
    def checkout(self):
        name = self['name']
        sourcegit = self['source-git']
        if sourcegit is None:
            self.logfile.write('No git repo defined for {0}.\n'.format(name))
            return False
        self.logfile.write("Fetching source for '%s'\n  into '%s'\n" % (name, os.path.abspath('../'+name)))
        tag = self['tag']
        try:
            if not os.path.exists('../'+name):
                self.logfile.write('  git clone {0} {1}\n'.format(sourcegit, name))
                subprocess.check_call(['git', 'clone', sourcegit, name], cwd='..', shell=False)
            elif not os.path.isdir('../'+name):
                self.logfile.write('Cannot checkout {0}, because directory ../{0} already exists\n'.format(name))
                return False
            else:
                self.logfile.write('  git fetch origin\n')
                subprocess.check_call(['git', 'fetch', 'origin'], cwd='../'+name, shell=False)
            self.logfile.write("  git checkout {0}\n".format(tag))
            subprocess.check_call(['git', 'checkout', tag], cwd='../'+name, shell=False)
        except subprocess.CalledProcessError as cpe:
            self.logfile.write(str(cpe)+'\n')
            return False
        return True
    def expand_remote_path(self):
        return self.expander.expand('archive-path')
    def expand_local_path(self):
        return self.expander.expand('dest')
    def expand_configure_args(self):
        return self.expander.expand('configure-args')


class DependencyCollection(object):
    def __init__(self, env, logfile=None):
        fetcher = FileFetcher()
        self.logfile = default_log(logfile)
        self.base_env = env
        self.dependency_types = DEPENDENCY_TYPES
        self.dependencies = {}
        self.fetcher = fetcher
    def create_dependency(self, dependency_definition, overrides={}):
        defn = dependency_definition
        env = {}
        env.update(self.base_env)
        if 'type' in defn:
            dep_type = defn['type']
            env.update(self.dependency_types[dep_type])
        else:
            # default to an 'external' dependency type if none specified
            dep_type = 'external'
            env.update(self.dependency_types[dep_type])
        env.update(defn)
        env.update(overrides)
        if 'name' not in env:
            raise ValueError('Dependency definition contains no name')
        name = env['name']
        new_dependency = Dependency(name, env, self.fetcher, logfile=self.logfile, has_overrides=len(overrides) > 0)
        if 'ignore' in new_dependency and new_dependency['ignore']:
            return
        self.dependencies[name] = new_dependency
    def __contains__(self, key):
        return key in self.dependencies
    def __getitem__(self, key):
        return self.dependencies[key]
    def items(self):
        return self.dependencies.items()
    def _filter(self, subset=None):
        if subset is None:
            return self.dependencies.values()
        missing_dependencies = [name for name in subset if name not in self.dependencies]
        if len(missing_dependencies) > 0:
            raise Exception("No entries in dependency file named: " + ", ".join(missing_dependencies) + ".")
        return [self.dependencies[name] for name in subset]
    def get_args(self, subset=None):
        dependencies = self._filter(subset)
        configure_args=sum((d.expand_configure_args() for d in dependencies), [])
        return configure_args
    def fetch(self, subset=None):
        dependencies = self._filter(subset)
        failed_dependencies = []
        filename = self.fetched_deps_filename( dependencies )
        prefetch_deps = self.load_fetched_deps( filename  )
        postfetch_deps = {}
        for d in dependencies:
            do_fetch = True
            name = ''
            path = ''
            if 'name' in d.expander:
                name = d.expander.expand('name')
            if 'archive-path' in d.expander:
                path = d.expander.expand('archive-path')
            if name in prefetch_deps.keys():
                if prefetch_deps[name] == path:
                    self.logfile.write("Skipping fetch of %s as unchanged (%s)\n" % (name, path))
                    postfetch_deps[name] = path
                    do_fetch = False
            if do_fetch:
                if not d.fetch():
                    failed_dependencies.append(d.name)
                else:
                    if name and path:
                        postfetch_deps[name] = path
        if filename:
            self.save_fetched_deps(filename, postfetch_deps)
        if failed_dependencies:
            self.logfile.write("Failed to fetch some dependencies: " + ' '.join(failed_dependencies) + '\n')
            return False
        return True
    def fetched_deps_filename(self, deps):
        filename = None
        for d in deps:
            if 'dest' in d.expander:
                filename = os.path.join(d.expander.expand('dest').split('/')[0], 'loadedDeps.json')
                break
        return filename
    def load_fetched_deps(self, filename):
        loaded_deps = {}
        if filename and os.path.isfile(filename):
            try:
                f = open(filename, 'rt')
                loaded_deps = json.load(f)
                f.close()
            except:
                self.logfile.write("Error with current fetched dependency file: %s\n" % filename)
        return loaded_deps
    def save_fetched_deps(self, filename, deps):
        f = open(filename, 'wt')
        json.dump(deps, f)
        f.close()
    def checkout(self, subset=None):
        dependencies = self._filter(subset)
        failed_dependencies = []
        for d in dependencies:
            if not d.checkout():
                failed_dependencies.append(d.name)
        if failed_dependencies:
            self.logfile.write("Failed to check out some dependencies: " + ' '.join(failed_dependencies) + '\n')
            return False
        return True

def read_json_dependencies(dependencyfile, overridefile, env, logfile):
    collection = DependencyCollection(env, logfile=logfile)
    dependencies = json.load(dependencyfile)
    overrides = json.load(overridefile)
    overrides_by_name = dict((dep['name'], dep) for dep in overrides)
    for d in dependencies:
        name = d['name']
        override = overrides_by_name.get(name,{})
        collection.create_dependency(d, override)
    return collection

def read_json_dependencies_from_filename(dependencies_filename, overrides_filename, env, logfile):
    try:
        dependencyfile = open(dependencies_filename, "r")
        with open(dependencies_filename) as dependencyfile:
            if overrides_filename is not None and os.path.isfile(overrides_filename):
                with open(overrides_filename) as overridesfile:
                    return read_json_dependencies(dependencyfile, overridesfile, env, logfile)
            else:
                return read_json_dependencies(dependencyfile, cStringIO.StringIO('[]'), env, logfile)
    except (OSError, IOError) as e:
        if e.errno != 2:
            raise
        return DependencyCollection(env, logfile=logfile)

def cli(args):
    if platform.system() != "Windows":
        args = ["mono", "--runtime=v4.0.30319"] + args
    subprocess.check_call(args, shell=False)

def clean_directories(directories):
    """Remove the specified directories, trying very hard not to remove
    anything if a failure occurs."""

    # Some explanation is in order. Windows locks DLLs while they are in
    # use. You can't just unlink them like in Unix and create a new
    # directory entry in their place - the lock isn't just on the file
    # contents, but on the directory entry (and the parent's directory
    # entry, etc.)
    # The scenario we really want to avoid is to start deleting stuff
    # and then fail half-way through with a random selection of files
    # deleted. It's preferable to fail before any file has actually been
    # deleted, so that a user can, for example, decide that they don't
    # really want to run a fetch after all, rather than leaving them in
    # a state where they're forced to close down the app with the locks
    # (probably Visual Studio) and run another fetch.
    # We achieve this by first doing a bunch of top-level directory
    # renames. These will generally fail if any of the subsequent deletes
    # would have failed. If one fails, we just undo the previous renames
    # and report an error. It's not bulletproof, but it should be good
    # enough for the most common scenarios.

    try:
        directories = list(directories)
        moved = []
        try:
            lastdirectory = None
            for directory in directories:
                if not os.path.isdir(directory):
                    continue
                newname = directory + '.deleteme'
                lastdirectory = directory
                os.rename(directory, newname)
                lastdirectory = None
                moved.append((directory, newname))
        except:
            for original, newname in reversed(moved):
                os.rename(newname, original)
            raise
        for original, newname in moved:
            shutil.rmtree(newname)
    except Exception as e:
        if lastdirectory is not None:
            raise Exception("Failed to remove directory '{0}'. Try closing applications that might be using it. (E.g. Visual Studio.)".format(lastdirectory))
        else:
            raise Exception("Failed to remove directory. Try closing applications that might be using it. (E.g. Visual Studio.)\n"+str(e))

def get_data_dir():
    userdata = os.environ.get('LOCALAPPDATA', None)
    if userdata is not None:
        return userdata + '/ohDevTools'
    userdata = os.environ.get('HOME', '.')
    return userdata + '/.ohdevtools'


def fetch_dependencies(dependency_names=None, platform=None, env=None, fetch=True, nuget=True, clean=True, source=False, logfile=None, list_details=False, local_overrides=True, verbose=False):
    '''
    Fetch all the dependencies defined in projectdata/dependencies.json and in
    projectdata/packages.config.
    platform:
        Name of target platform. E.g. 'Windows-x86', 'Linux-x64', 'Mac-x64'...
    env:
        Extra variables referenced by the dependencies file.
    fetch:
        True to fetch the listed dependencies, False to skip.
    nuget:
        True to fetch nuget packages listed in packages.config, False to skip.
    clean:
        True to clean out directories before fetching, False to skip.
    source:
        True to fetch source for the listed dependencies, False to skip.
    logfile:
        File-like object for log messages.
    '''
    if env is None:
        env = {}
    if platform is not None:
        env['platform'] = platform
    if 'platform' not in env:
        platform = env['platform'] = default_platform()
    if '-' in platform:
        env['system'], env['architecture'] = platform.split('-',2)

    if platform is None:
        raise Exception('Platform not specified and unable to guess.')
    if clean and not list_details:
        try:
            os.unlink('dependencies/loadedDeps.json')
        except:
            pass
        clean_dirs = []
        if fetch:
            clean_dirs += [
                'dependencies/AnyPlatform',
                'dependencies/'+platform]
        if nuget:
            clean_dirs += ['dependencies/nuget']
        clean_directories(clean_dirs)


    overrides_filename = '../dependency_overrides.json' if local_overrides else None
    dependencies = read_json_dependencies_from_filename('projectdata/dependencies.json', overrides_filename, env=env, logfile=logfile)
    if list_details:
        for name, dependency in dependencies.items():
            print "Dependency '{0}':".format(name)
            print "    fetches from:     {0!r}".format(dependency['archive-path'])
            print "    unpacks to:       {0!r}".format(dependency['dest'])
            print "    local override:   {0}".format("YES (see '../dependency_overrides.json')" if dependency.has_overrides else 'no')
            if verbose:
                print "    all keys:"
                for key, value in sorted(dependency.items()):
                    print "        {0} = {1!r}".format(key, value)
            print ""
    else:
        if fetch:
            dependencies.fetch(dependency_names)
        if nuget:
            if not os.path.exists('projectdata/packages.config'):
                print "Skipping NuGet invocation because projectdata/packages.config not found."
            else:
                nuget_exes = [os.path.normpath(p) for p in glob('dependencies/AnyPlatform/NuGet.[0-9]*/NuGet.exe')]
                if len(nuget_exes) == 0:
                    raise Exception("'NuGet.exe' not found, cannot fetch NuGet dependencies.")
                nuget_exe = nuget_exes[0]
                if len(nuget_exes) > 1:
                    print "Warning: multiple copies of 'NuGet.exe' found. Using:"
                    print "    " + nuget_exe
                cli([nuget_exe, 'install', 'projectdata/packages.config', '-OutputDirectory', 'dependencies/nuget'])
        if source:
            dependencies.checkout(dependency_names)
    return dependencies


