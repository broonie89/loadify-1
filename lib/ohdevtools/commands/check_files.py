import filechecker
import optparse
import sys

description  = "Check where files matching some pattern obey predefined rules"
command_group = "Developer tools"
command_name = "check-files"
command_synonyms = ["check-files"]

def parse_args():
    usage = (
        "\n"+
        "    %prog --include PATTERN [options] RULES...\n"+
        "\n"+
        "Scan files specified by --include PATTERNS to determine if they match the named rules.\n"+
        "\n"+
        "Available rules:\n"
        "    warnings-as-errors: Applies to .csproj files, checks that TreatWarningsAsErrors is enabled.\n"+
        "    import-shared-settings: Applies to .csproj files, checks that SharedSettings.targets is Imported.\n"+
        "    no-tabs: Applies to text files, checks that no <TAB> characters are present.\n"+
        "    disallow: Fails if any file matches the pattern.\n")
    parser = optparse.OptionParser(usage=usage)
    def include(option, opt_str, value, parser):
        if not hasattr(parser.values, "patterns"):
            parser.values.patterns = []
        parser.values.patterns.append(("include", value))
    def exclude(option, opt_str, value, parser):
        if not hasattr(parser.values, "patterns"):
            parser.values.patterns = []
        parser.values.patterns.append(("exclude", value))
    parser.add_option("-i", "--include",   action="callback", callback=include, type="string", nargs=1, help="Include files for testing")
    parser.add_option("-e", "--exclude",   action="callback", callback=exclude, type="string", nargs=1, help="Exclude files from testing")
    return parser.parse_args()

def main():
    options, args = parse_args()
    rule_names = list(args)
    negated_rule_names = ["-" + r for r in rule_names]
    rules = []
    for action, pattern in options.patterns:
        if action == "include":
            rules.append([pattern, rule_names])
        elif action == "exclude":
            rules.append([pattern, negated_rule_names])
    result = filechecker.apply_rules(rules)
    if not result:
        sys.exit(1)

if __name__ == "__main__":
    main()

