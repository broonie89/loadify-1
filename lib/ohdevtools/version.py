import json
import os
import sys

# The version number of the API. Incremented whenever there
# are new features or bug fixes.
VERSION = 42

# The earliest API version that we're still compatible with.
# Changed only when a change breaks an existing API.
BACKWARD_VERSION = 22

class BadVersionException(Exception):
    def __init__(self, message="Aborted due to error."):
        Exception.__init__(self, message)
        self.usermessage = message

def require_version(required_version):
    '''Fail if the version of ohDevTools is too old.'''
    if VERSION<required_version:
        raise BadVersionException("This project requires a newer version of ohDevTools. You have version {0}, but need version {1}.".format(VERSION, required_version))
    if required_version<BACKWARD_VERSION:
        raise BadVersionException("This project requires an older version of ohDevTools. You have version {0}, but need version {1}.".format(VERSION, required_version))

def check_version():
    if os.path.isfile('projectdata/ohdevtools.json'):
        with open('projectdata/ohdevtools.json', 'r') as configfile:
            config = json.load(configfile)
            required_version = config.get('requires-version',None)
            if required_version is not None:
                try:
                    require_version(required_version)
                except BadVersionException as e:
                    print e.usermessage
                    sys.exit(32)
