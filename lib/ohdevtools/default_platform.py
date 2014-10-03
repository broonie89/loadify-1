import platform

def default_platform():
    if platform.system() == 'Windows':
        return 'Windows-x86'
    if platform.system() == 'Linux' and platform.architecture()[0] == '32bit' and platform.machine()[0:3] == 'ppc':
        return 'Linux-ppc32'
    if platform.system() == 'Linux' and platform.architecture()[0] == '32bit':
        return 'Linux-x86'
    if platform.system() == 'Linux' and platform.architecture()[0] == '64bit':
        return 'Linux-x64'
    if platform.system() == 'Darwin':
        # Mac behaves similarly to Windows - a 64-bit machine can support
        # both 32-bit and 64-bit processes. We prefer 32-bit because it's
        # generally more compatible and in particular Mono is 32-bit-only.
        return 'Mac-x86'
    return None
