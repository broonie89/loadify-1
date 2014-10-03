import threading
import sys
import os
import shutil
import subprocess

description = "Publish latest UI files onto sheeva003."
command_group = "Hudson commands"
command_hidden = True


def rssh(username,host,cmd):

    import paramiko

    ssh = paramiko.SSHClient()
    ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())

    ssh.connect(host, username=username, look_for_keys='True')

    stdin, stdout, stderr = ssh.exec_command(cmd)

    def get_thread(pipe):
        for line in pipe.readlines():
            print line

    stdout_thread = threading.Thread(target=get_thread, args=(stdout,))
    stderr_thread = threading.Thread(target=get_thread, args=(stderr,))

    stdout_thread.start()
    stderr_thread.start()
    stdout_thread.join()
    stderr_thread.join()

    channel = stdout.channel

    exit_status = channel.recv_exit_status()

    return exit_status

    ssh.close()

def rsync(username,host,src,dst,excludes):

    cmd = ['rsync', '-avz', ''+src+'', ''+username+'@'+host+':'+dst+'' ]

    for exclude in excludes:
        cmd.append('--exclude='+exclude+'')
    ret = subprocess.call(cmd)
    return ret

def main():
    HELP_SYNONYMS = ["--help", "-h", "/h", "/help", "/?", "-?", "h", "help", "commands"]
    if len(sys.argv)>=2 and sys.argv[1] in HELP_SYNONYMS:
        print description
        return

    excludes = ''

    ret = rsync('hudson-ohwidget','sheeva003.linn.co.uk','UI/SampleWebUI','~/',excludes)

    if ret != 0:
            print ret
            sys.exit(10)

    ret = rssh('hudson-ohwidget','sheeva003.linn.co.uk','find /home/hudson-ohwidget -type f -exec sed -i \'s/resource\///g\' {} \;')

    if ret != 0:
            print ret
            sys.exit(10)

if __name__ == "__main__":
    main()
