#!/bin/env python

####
## build node for hudson
## used to build nightly os images
## and upload to upstream package repository
## will only work on hudson and our network - not supposed to be called externally
###

## usage: set env arg NODE_VERSION with a version number and set env arg REPOSITORY as unstable
## e.g. export NODE_VERSION=1.10~blah && export REPOSITORY=unstable

description = "Build a filesystem image for an OpenHome node."
command_group = "Build commands"
command_hidden = True

import subprocess
import sys
import threading
import os

class builder():
    def __init__(self):
        self.host = "image-builder.linn.co.uk"
        self.username = "repo-incoming"
        self.oh_rsync_user = "hudson-rsync"
        self.oh_rsync_host = "openhome.org"
        self.version = os.environ.get("NODE_VERSION")
        self.repository = os.environ.get("REPOSITORY")
        self.rsync = 'rsync -avz image-builder %s@%s:/home/repo-incoming' %(self.username,self.host)
        self.image_types = ['main', 'sdk', 'fallback']

    def push_dependancies(self):
        self.ret = subprocess.call(self.rsync, shell=True)

        if self.ret != 0:
            print "push failed, aborting"
            sys.exit(1)

    def run_build(self,cmd):
        import paramiko
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

    def generate_images(self):
        self.run_build("sudo /bin/sh -c 'rm -rf image-builder/images/*'")

        for image_type in self.image_types:
            cmd = "sudo /bin/sh -c 'cd image-builder && ./build.sh %s %s %s %s'" %(image_type,image_type,"openhome",self.version)
            self.run_build(cmd)

    def publish_images(self):
        for image_type in self.image_types:
            copy_mkdir = "sudo /bin/sh -c 'mkdir -p /var/www/openhome/%s/%s/'" %(self.repository, image_type)
            copy_kernel = "sudo /bin/sh -c 'cp -p oh-linux/uImage /var/www/openhome/%s/%s/%s.uImage'" %(self.repository, image_type, image_type)
            copy_update = "sudo /bin/sh -c 'cp -p image-builder/images/%s/update /var/www/openhome/%s/%s/'" %(image_type, self.repository, image_type)
            copy_ubifs = "sudo /bin/sh -c 'cp -p image-builder/images/%s/%s.ubi.img /var/www/openhome/%s/%s/'" %(image_type, image_type, self.repository, image_type)
            copy_version = "sudo /bin/sh -c 'cp -p image-builder/images/%s/version /var/www/openhome/%s/%s/'" %(image_type, self.repository, image_type)
            publish_openhome = "sudo /bin/sh -c 'rsync -avz --del /var/www/openhome/%s/%s %s@%s:~/build/%s/node/'" %(self.repository, image_type, self.oh_rsync_user, self.oh_rsync_host, self.repository)
            self.run_build(copy_mkdir)
            self.run_build(copy_kernel)
            self.run_build(copy_update)
            self.run_build(copy_ubifs)
            self.run_build(copy_version)
            self.run_build(publish_openhome)


if __name__ == "__main__":
    HELP_SYNONYMS = ["--help", "-h", "/h", "/help", "/?", "-?", "h", "help", "commands"]
    if len(sys.argv)>=2 and sys.argv[1] in HELP_SYNONYMS:
        print description
        sys.exit(0)
    build = builder()
    build.push_dependancies()
    build.generate_images()
    build.publish_images()
