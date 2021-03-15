uname := $(shell uname -a)
ifneq ($(findstring Msys,$(uname)),)
os := Windows
else
ifneq ($(findstring Darwin,$(uname)),)
os := macOS
else
os := Linux
endif
endif

all: build

build:
	make -C CAPSImg
ifeq (${os},Windows)
	cp CAPSImg/CAPSImg.dll capsimg.dll
else
ifeq (${os},macOS)
	cp CAPSImg/libcapsimage.dylib capsimg.so
	install_name_tool -id capsimg.so capsimg.so
else
	cp CAPSImg/libcapsimage.so.5.1 capsimg.so
endif
endif

clean:
	make -C CAPSImg clean
	rm -Rf capsimg${dll} dist.fs

distclean: clean
	make -C CAPSImg distclean
