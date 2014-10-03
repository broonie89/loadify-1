import sys, os, subprocess, version

HELP_SYNONYMS = ["--help", "-h", "/h", "/help", "/?", "-?", "h", "help", "commands"]

def get_command_details(modulename):
    command = modulename
    try:
        commands_module = __import__('commands.'+command)
        command_module = getattr(commands_module, command)
    except:
        #print sys.exc_info()
        return Command(command, command, "", "[There was an error processing this command]", "", False)
    description = getattr(command_module, 'description', '[No description available]')
    group = getattr(command_module, 'command_group', '')
    synonyms = getattr(command_module, 'command_synonyms', [])
    name = getattr(command_module, 'command_name', command)
    hidden = getattr(command_module, 'command_hidden', False)
    if command not in synonyms:
        synonyms.append(command)
    if name in synonyms:
        synonyms.remove(name)
    return Command(name, command, group, description, synonyms, hidden)

class Command(object):
    def __init__(self, name, modulename, group, description, synonyms, hidden):
        self.name = name
        self.modulename = modulename
        self.group = group
        self.description = description
        self.synonyms = list(synonyms)
        self.hidden = hidden

def getcommandmodules():
    dirpath, scriptname = os.path.split(os.path.abspath(__file__))
    filenames = os.listdir(os.path.join(dirpath, 'commands'))
    commands = [f[:-3] for f in filenames if f.endswith('.py')]
    commands = [c for c in commands if not c.startswith('_')]
    commands = [c for c in commands if not c.startswith('.')]
    return commands

def getcommands():
    commandmodules = getcommandmodules()
    commanddetails = [get_command_details(c) for c in commandmodules]
    return dict(
            (d.name, d)
            for d in commanddetails)

def get_commands_and_synonyms():
    commands = getcommands()
    commands_and_synonyms = dict(commands)
    for cmd in commands.values():
        for synonym in cmd.synonyms:
            commands_and_synonyms[synonym] = cmd
    return commands_and_synonyms

def findcommand(command):
    command_modules = getcommandmodules()
    if command in command_modules:
        return command
    commands = get_commands_and_synonyms()
    if command in commands:
        return commands[command].modulename
    print 'Unrecognized command.'
    print 'Try "go help" for a list of commands.'
    sys.exit(1)

def invoke_module(modulename, args):
    oldpythonpath = os.getenv('PYTHONPATH')
    thisdir = os.path.split(os.path.normcase(os.path.abspath(__file__)))[0]
    pythonpath = (oldpythonpath + os.path.pathsep + thisdir) if oldpythonpath else thisdir
    newenv = dict(os.environ)
    newenv['PYTHONPATH'] = pythonpath
    # This isn't ideal. Importing tkinter, even indirectly, adds unicode
    # strings to the environment. Force them all to str:
    newenv = dict((key,str(value)) for (key, value) in newenv.items())
    exitcode = subprocess.call([sys.executable, '-m', modulename] + args, env=newenv)
    sys.exit(exitcode)


def runcommand(command, args):
    commandname = findcommand(command)
    invoke_module('commands.'+commandname, args)

def showcommandhelp(command):
    commandname = findcommand(command)
    commanddetails = get_command_details(commandname)
    print 'Command: ' + commanddetails.name,
    if commanddetails.synonyms:
        print "(also %s)" % (", ".join(commanddetails.synonyms),)
    else:
        print
    print
    invoke_module('commands.'+commandname, ['--help'])

def showhelp():
    print
    print "Usage:"
    print
    print "  go COMMAND"
    print "  go help COMMAND"
    print
    print "Available commands:"
    commands = sorted(getcommands().items())
    maxlen = max(len(cmd) for (cmd,details) in commands)
    groups = {}
    for cmd, details in commands:
        if details.hidden:
            continue
        groups.setdefault(details.group, []).append(details)
    for group, commandlist in sorted(groups.items()):
        print
        if group!="":
            print "  "+group
        for details in sorted(commandlist, key=lambda c:c.name):
            print "    %s   %s" % (details.name.ljust(maxlen), details.description)

def main():
    version.check_version()
    if len(sys.argv) < 2 or sys.argv[1] in HELP_SYNONYMS:
        if len(sys.argv) >= 3:
            showcommandhelp(sys.argv[2])
        else:
            showhelp()
    else:
        runcommand(sys.argv[1], sys.argv[2:])

if __name__=="__main__":
    main()

