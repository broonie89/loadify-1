from xml.etree.ElementTree import parse
from antglob import ant_glob
from collections import defaultdict


# Define a bunch of useful rules that can be applied to files in
# the source tree.

MSBUILD = "{http://schemas.microsoft.com/developer/msbuild/2003}"

def check_warnings_as_errors(filename):
    # We don't want to parse the entire msbuild file. There are basically
    # two ways we expect this to be set up:
    #
    # 1. The default way used by Visual Studio is to define awkward conditions
    #    on property groups to specify different behaviour for debug and release.
    #    In that case, we want to make sure that both debug and release turn on
    #    warnings-as-errors. We use the simple heuristic of looking for the
    #    text "debug" or "release" somewhere in the condition attribute of the
    #    PropertyGroup to detect this.
    # 2. In some cases we have a cleaner setup where warnings-as-errors appears
    #    in an unconditional block. In that case, it doesn't matter if there are
    #    no entries in the configuration-specific blocks.
    #
    # If a csproj file does something much weirder with conditional properties,
    # we may have false negatives or false positives. It doesn't seem worth
    # trying to handle those cases right now.
    xmlroot = parse(filename)
    propertygroup_elements = xmlroot.findall('.//'+MSBUILD+'PropertyGroup')

    okay_in_debug = False
    okay_in_release = False

    for parent_group in propertygroup_elements:
        warning_elements = parent_group.findall(MSBUILD+'TreatWarningsAsErrors')
        condition = parent_group.attrib.get('Condition', '')
        for element in warning_elements:
            if element.text != 'true':
                return False
            if 'debug' in condition.lower():
                okay_in_debug = True
            elif 'release' in condition.lower():
                okay_in_release = True
            else:
                okay_in_debug = True
                okay_in_release = True

    return okay_in_debug and okay_in_release

def check_imports_prime(predicate, filename):
    xmlroot = parse(filename)
    import_elements = xmlroot.findall('.//'+MSBUILD+'Import')
    return any(predicate(x.attrib.get('Project', '')) for x in import_elements)

def check_imports(required_import):
    def check_imports_partial(filename):
        return check_imports_prime(required_import, filename)
    return check_imports_partial

def check_notabs(filename):
    with open(filename,"r") as f:
        return not any('\t' in line for line in f)

def disallow(filename):
    return False

DEFAULT_DEFINITIONS = {
    'warnings-as-errors' : check_warnings_as_errors,
    'import-shared-settings' : check_imports(lambda x:x.endswith('SharedSettings.targets')),
    'no-tabs' : check_notabs,
    'disallow' : disallow
    }

def _print(message):
    print message

def apply_rules(rules, definitions = DEFAULT_DEFINITIONS, message_func=None):
    if message_func is None:
        message_func = _print
    files_to_check = defaultdict(set)
    for patterns, categories in rules:
        if not isinstance(patterns, list):
            patterns = [patterns]
        if not isinstance(categories, list):
            categories = [categories]
        for p in patterns:
            for filename in ant_glob(p):
                file_categories = files_to_check[filename]
                for category in categories:
                    if category.startswith('-'):
                        file_categories.discard(category[1:])
                    else:
                        file_categories.add(category)
    failures = defaultdict(set)
    for filename in sorted(files_to_check.keys()):
        for category in files_to_check[filename]:
            if not definitions[category](filename):
                failures[category].add(filename)
    message_func("{0} errors in {1} scanned files:".format(len(failures), len(files_to_check)))
    for category, filenames in sorted(failures.items()):
        if len(filenames) == 0:
            continue
        message_func("{0}, {1} files failed:".format(category, len(filenames)))
        for fname in sorted(filenames):
            print "    {0}".format(fname)
    return len(failures) == 0


