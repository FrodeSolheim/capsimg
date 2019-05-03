version = $(shell cat VERSION.fs)
uname := $(shell uname -a)
ifneq ($(findstring Msys,$(uname)),)
os = Windows
arch = x86_64
exe = .exe
dll = .dll
else ifneq ($(findstring Darwin,$(uname)),)
os = macOS
arch = x86_64
exe =
dll = .so
else
os = Linux
arch = x86_64
exe =
dll = .so
endif

all: build
	@echo Built ${version} for ${os}-${arch}

install: plugin-clean plugin-build
	rm -Rf ../OpenRetro/System/Plugins/${name}
	mkdir -p ../OpenRetro/System/Plugins
	mv dist/${name} ../OpenRetro/System/Plugins/

plugin: plugin-clean plugin-build plugin-strip plugin-pack

plugin-clean:
	rm -Rf dist/${name}

plugin-pack:
	cd dist && tar cfJ ${name}-${version}-${os}-${arch}.tar.xz ${name}
	@echo Packaged ${version} for ${os}-${arch}

plugin-strip:
	./strip.fs dist/${name}

name = CAPSImg

build:
	make -C CAPSImg
ifeq ($(os), Windows)
	cp CAPSImg/CAPSImg.dll capsimg.dll
else ifeq ($(os), macOS)
	cp CAPSImg/libcapsimage.dylib capsimg.so
else
	cp CAPSImg/libcapsimage.so.5.1 capsimg.so
endif

clean:
	make -C CAPSImg clean
	rm -Rf capsimg${dll} dist

distclean: clean
	make -C CAPSImg distclean

plugin-build:
	mkdir -p dist/${name}/${os}/${arch}
	cp README.fs dist/${name}/ReadMe.txt
	echo ${version} > dist/${name}/Version.txt
	echo ${version} > dist/${name}/${os}/${arch}/Version.txt
	cp capsimg${dll} dist/${name}/${os}/${arch}/
	mkdir -p dist/${name}/Licences
	cp LICENCE.txt dist/${name}/Licences/CAPSImg.txt
