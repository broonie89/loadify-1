import re
import os

def fragment_to_regex(f):
    if "*" in f:
        return onestar_regex.join(fragment_to_regex(ff) for ff in f.split("*"))
    if "?" in f:
        return qmark_regex.join(fragment_to_regex(ff) for ff in f.split("?"))
    return re.escape(f)

twostars_regex = "(?:.*/)*"
onestar_regex = "[^/]*"
qmark_regex = "[^/]"

def fragments_to_regex(fragments):
    regex_str_pieces = []
    for fragment in fragments:
        if fragment == "":
            regex_str_pieces.append("/")
        elif fragment == "**":
            regex_str_pieces.append(twostars_regex)
        else:
            regex_str_pieces.append(fragment_to_regex(fragment))
            regex_str_pieces.append("/")
    if len(regex_str_pieces)>0 and regex_str_pieces[-1]=="/":
        regex_str_pieces.pop()
    regex_str_pieces.append("$")
    return "".join(regex_str_pieces)

def ant_glob(pattern):
    pattern = pattern.replace("\\", "/")
    fragments = pattern.split("/")
    leftfragments=[]
    for i in xrange(len(fragments)):
        if "*" in fragments[i] or "?" in fragments[i]:
            leftfragments=fragments[:i]
            #rightfragments=fragments[i:]
            break
    else:
        leftfragments=fragments[:-1]
        #rightfragments=fragments[-1:]
    if len(leftfragments)==0:
        leftfragments = ["."] + leftfragments
        fragments = ["."] + fragments
    if leftfragments==[""]:
        basedir = "/"
    else:
        basedir = "/".join(leftfragments)
    regex_str = fragments_to_regex(fragments)
    regex = re.compile(regex_str)
    for (directory, subdirs, filenames) in os.walk(basedir):
        directory = directory.replace("\\", "/")
        for filename in filenames:
            if regex.match(directory+"/"+filename) is not None:
                yield directory+"/"+filename
