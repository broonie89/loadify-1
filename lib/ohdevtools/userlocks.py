import os
import platform
import ctypes
import time
import datetime

class BaseUserLock(object):
    def __init__(self, filename):
        self.filename = filename
        self.locktime = None
    def __enter__(self):
        dirname = os.path.split(os.path.abspath(self.filename))[0]
        if not os.path.exists(dirname):
            os.makedirs(dirname)
        while True:
            if self.tryacquire(self.filename):
                break
            print "Lockfile "+self.filename+" not available."
            print "Wait 30s..."
            time.sleep(30.0)
        self.locktime = datetime.datetime.now()
        print "Lock acquired at "+str(self.locktime)
    def __exit__(self, etype, einstance, etraceback):
        self.release()
        unlocktime = datetime.datetime.now()
        print "Lock released at "+str(unlocktime)
        print "Lock was held for "+str(unlocktime - self.locktime)

class WindowsUserLock(BaseUserLock):
    def __init__(self, name):
        BaseUserLock.__init__(self, os.environ["APPDATA"]+"\\openhome-build\\"+name+".lock")
    def tryacquire(self, filename):
        self.handle = ctypes.windll.kernel32.CreateFileA(filename,7,0,0,2,0x04000100,0)
        return self.handle != -1
    def release(self):
        ctypes.windll.kernel32.CloseHandle(self.handle)

class PosixUserLock(BaseUserLock):
    def __init__(self, name):
        BaseUserLock.__init__(self, os.environ["HOME"]+"/.openhome-build/"+name+".lock")
    def tryacquire(self, filename):
        import fcntl
        self.f = file(filename, "w")
        try:
            fcntl.lockf(self.f, fcntl.LOCK_EX)
            return True
        except IOError:
            self.f.close()
            return False
    def release(self):
        self.f.close()

def userlock(name):
    '''
    Acquire a lock scoped to the local user. Only one build at a time can run
    with the given name per user per machine. While waiting for the lock, prints
    a notice to stdout every 30s.
    '''
    if platform.system() == 'Windows':
        return WindowsUserLock(name)
    return PosixUserLock(name)
