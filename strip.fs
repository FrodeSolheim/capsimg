#!/usr/bin/env python3
import os
import sys

magics = [b"MZ\x90\x00", b"\x7fELF", b"\xcf\xfa\xed\xfe"]


def run(directory):
    for dir_path, dir_names, file_names in os.walk(directory):
        for file_name in file_names:
            path = os.path.join(dir_path, file_name)
            with open(path, "rb") as f:
                magic = f.read(4)
            if magic not in magics:
                continue
            print("Stripping", path)
            assert os.system("strip {}".format(path)) == 0


if __name__ == "__main__":
    run(sys.argv[1])
