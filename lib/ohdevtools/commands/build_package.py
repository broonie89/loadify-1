#!/bin/env python

####
## build package for hudson
## used to build nightly packages for ohwidget
## and upload to upstream package repository
## will only work on hudson and our network - not supposed to be called externally
###

## usage: set env arg TARGET_ARCH with an arch and set env arg REPOSITORY as nightly & set a package version
## e.g. export TARGET_ARCH=armel && export REPOSITORY=nightly && export PACKAGE_VERSION=0.11~blah

description = "Build a Debian package for ohWidget."
command_group = "Hudson commands"
command_hidden = True

import os
import subprocess
import sys
import shutil
import tarfile
import threading
import time
import random

class builder():
    def __init__(self):
        self.arch_vars = ''
        self.output = ""
        self.ret = ''
        self.host = "image-builder.linn.co.uk"
        self.oh_rsync_user = "hudson-rsync"
        self.oh_rsync_host = "openhome.org"
        self.username = "repo-incoming"
        self.target = os.environ.get("TARGET_ARCH")
        self.version = os.environ.get("PACKAGE_VERSION")
        self.repo = os.environ.get("REPOSITORY")
        self.rsync = 'rsync -avz ../ --include="*.tar.gz" --include="*.deb" --include="*.changes" --include="*.dsc" --exclude="*" %s@%s:/var/www/openhome/apt-repo/incoming/%s' %(self.username,self.host,self.repo)
    def set_arch_vars(self):
        arm = {
            "setup" : "ls -al && export PATH=$PATH:/usr/local/arm-2010q1/bin && export CROSS_COMPILE=arm-none-linux-gnueabi- && export ARCH=arm",
            "compiler" : "dpkg-buildpackage -rfakeroot -us -uc -aarmel",
            "publish_results" : self.rsync,
            "arch" : "armel",
            }

        linx86 = {
            "setup" : "ls -al",
            "compiler" : "dpkg-buildpackage -rfakeroot -us -uc",
            "publish_results" : self.rsync,
            "arch" : "i386",
            }

        linx64 = {
            "setup" : "ls -al",
            "compiler" : "dpkg-buildpackage -rfakeroot -us -uamd64",
            "publish_results" : self.rsync,
            "arch" : "amd64",
            }

        if self.target == "linux-x86":
            self.arch_vars = linx86
        elif self.target == "linux-x64":
            self.arch_vars = linx64
        elif self.target == "arm":
            self.arch_vars = arm

        print "selected target arch of " + self.target

    def fetch_dependancies(self):
        platform = {'linux-x86':'Linux-x86', 'linux-x64':'Linux-x64', 'arm':'Linux-ARM'}[self.target]
        self.ret = subprocess.call([sys.executable, '-m', 'commands.fetch_dependencies', '-t', platform])

        if self.ret != 0:
            print "fetch dependancies, aborting"
            sys.exit(1)

    def fetch_version(self):
        self.ret = subprocess.call('dch --newversion='+self.version+' < /bin/echo "automated hudson build"', shell=True)

        if self.ret != 0:
            print "version increment failed, aborting"
            sys.exit(1)

    def run_build(self):
        print "running build of package"

        self.ret = subprocess.call(self.arch_vars["setup"] + "&&" + self.arch_vars["compiler"], shell=True)

        if self.ret != 0:
            print "build failed, aborting"
            sys.exit(1)

    def run_remote_build(self,cmd):
        import paramiko
        print "running remote command"

        ssh = paramiko.SSHClient()
        ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())

        ssh.connect(self.host, username=self.username, look_for_keys='True')

        stdin, stdout, stderr = ssh.exec_command(cmd)

        def get_thread(pipe):
            for line in pipe.readlines():
                print line
                sys.stdout.flush()

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



    def publish_build(self):
        print "running package publish"

        self.ret = subprocess.call(self.arch_vars["publish_results"], shell=True)

        if self.ret != 0:
            print "publish failed, aborting"
            sys.exit(1)

        cmd = "sudo /bin/sh -c 'cd /var/www/openhome/apt-repo && reprepro -Vb . include %s incoming/%s/ohwidget_%s_%s.changes'" %(self.repo, self.repo, self.version, self.arch_vars["arch"])
        publish_openhome = "sudo /bin/sh -c 'rsync -avz --del /var/www/openhome/apt-repo/ %s@%s:~/build/nightly/apt-repo'" %(self.oh_rsync_user, self.oh_rsync_host)
        time.sleep(60 * random.random())
        ret = self.run_remote_build(cmd)

	if ret != 0:
	   print "publish failed, aborting"
	   sys.exit(1)

        ret = self.run_remote_build(publish_openhome)

	if ret != 0:
	   print "publish failed, aborting"
	   sys.exit(1)


if __name__ == "__main__":
    HELP_SYNONYMS = ["--help", "-h", "/h", "/help", "/?", "-?", "h", "help", "commands"]
    if len(sys.argv)>=2 and sys.argv[1] in HELP_SYNONYMS:
        print description
        sys.exit(0)
    build = builder()
    build.set_arch_vars()
    build.fetch_dependancies()
    build.fetch_version()
    build.run_build()
    build.publish_build()

