from Tkinter import *
import base64
import struct
import sys
import subprocess

description = "GUI for converting between base64 and a sequence of big-endian int32s."
command_group = "Developer tools"
command_synonyms = ["b64c"]


def main():
    if "--help" in sys.argv[1:]:
        print "Usage:"
        print "    go base64converter"
        print
        print "Converts between base64 and a sequence of big-endian int32s."
        return
    if "--nodetach" not in sys.argv[1:]:
        subprocess.Popen([sys.executable, '-m', 'commands.base64converter', '--nodetach'])
        return
    def to_int32():
        s = base64_box.get()
        int32_box.delete(0, END)
        try:
            byte_string = base64.b64decode(s.strip())
            int32_box.insert(0, ", ".join(map(str, struct.unpack("!" +("i"*(len(byte_string)//4)), byte_string))))
        except:
            int32_box.insert(0, "ERROR")
    def to_base64():
        s = int32_box.get()
        try:
            vs = [int(item.strip()) for item in s.split(",")]
            encoded = base64.b64encode(struct.pack("!" + ("i"*len(vs)), *vs))
        except:
            encoded = "ERROR"
        base64_box.delete(0, END)
        base64_box.insert(0, encoded)
    root = Tk()
    root.title("base64 to int32")
    base64_box = Entry(root, width=80)
    base64_box.grid(column=0, row=0)
    int32_box = Entry(root, width=80)
    int32_box.grid(column=0, row=1)
    base64_button = Button(root, text="Convert to int32s", command=to_int32)
    base64_button.grid(column=1, row=0)
    int32_button = Button(root, text="Convert to base64", command=to_base64)
    int32_button.grid(column=1, row=1)
    def quit():
        root.destroy()
        root.quit()
    root.protocol("WM_DELETE_WINDOW", quit)
    root.mainloop()

if __name__ == "__main__":
    main()

