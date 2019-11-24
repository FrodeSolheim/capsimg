plugin = CAPSImg

# Makefile.fs begin common ---------------------------------------------------

version = $(shell cat VERSION.fs)
uname := $(shell uname -a)
ifneq ($(findstring Msys,$(uname)),)
os := Windows
arch := x86-64
exe := .exe
dll := .dll
opengl_libs := -lopengl32 -lglew32
else
ifneq ($(findstring Darwin,$(uname)),)
os := macOS
arch := x86-64
exe :=
dll := .so
opengl_libs := -framework OpenGL -lGLEW
else
os := Linux
arch := x86-64
exe :=
dll := .so
opengl_libs := -lGL -lGLEW
endif
endif

plugin_dir = dist.fs/${plugin}
data_dir = dist.fs/${plugin}/Data
licenses_dir = dist.fs/${plugin}/Licenses
locale_dir = dist.fs/${plugin}/Locale
os_arch_dir = dist.fs/${plugin}/${os}/${arch}

glib_cflags = $(shell pkg-config --cflags glib-2.0)
glib_libs = $(shell pkg-config --libs glib-2.0)

cppflags = -DFSGS -DFSEMU -DFSE ${glib_cflags}
libs = ${glib_libs} ${opengl_libs}
libs += -lpng -lSDL2_ttf -lsamplerate

fsemu-all: fsemu-build
	@echo Built ${version} for ${os}_${arch}

.PHONY: assemble bootstrap build clean configure distclean install strip \
	package plugin plugin-noclean rebuild

assemble: fsemu-assemble
bootstrap: fsemu-bootstrap
build: fsemu-build
clean: fsemu-clean
configure: fsemu-configure
distclean: fsemu-distclean
install: fsemu-install
strip: fsemu-strip
package: fsemu-package
plugin: fsemu-plugin
plugin-noclean: fsemu-plugin-noclean
rebuild: fsemu-rebuild

fsemu-rebuild: fsemu-bootstrap fsemu-configure fsemu-clean fsemu-build

fsemu-install: fsemu-build
	rm -Rf ../OpenRetro/System/${plugin}
	mkdir -p ../OpenRetro/System
	mv dist.fs/${plugin} ../OpenRetro/System

fsemu-assemble-pre:
	rm -Rf ${plugin_dir}
	mkdir -p ${plugin_dir}
	echo "[plugin]" > ${plugin_dir}/Plugin.ini
	echo "name = ${plugin}" >> ${plugin_dir}/Plugin.ini
	echo "version = ${version}" >> ${plugin_dir}/Plugin.ini

fsemu-assemble-wrap: fsemu-assemble-pre fsemu-assemble

fsemu-plugin-noclean: fsemu-plugin-prep fsemu-build fsemu-assemble-wrap \
		fsemu-strip fsemu-package
ifeq (${os}, Linux)
	if [ -d ${os_arch_dir} ]; then cp ${os_arch_dir}/*.so* .; fi
endif

fsemu-plugin-prep:
	# Remove locally installed shared libraries (used for development)
	# before building plugin, to avoid getting stale libraries included
	# in the dist.
	rm -f *.so*

fsemu-plugin: fsemu-bootstrap fsemu-configure fsemu-clean \
		fsemu-plugin-noclean

# fsemu-plugin-clean:
# 	rm -Rf dist.fs/${plugin}

fsemu-package:
	cd dist.fs && tar cfJ ${plugin}_${version}_${os}_${arch}.tar.xz ${plugin}
	@echo Packaged ${version} for ${os}-${arch}

fsemu-strip:
	./standalone.fs ${os_arch_dir}
ifeq (${os}, macOS)
	./wrap-macos.fs ${os_arch_dir} ${executable}
endif

# Makefile.fs end common -----------------------------------------------------

fsemu-bootstrap:
	./bootstrap.fs

fsemu-configure:
	./configure.fs

fsemu-build:
	make -C CAPSImg
ifeq ($(os), Windows)
	cp CAPSImg/CAPSImg.dll capsimg.dll
else ifeq ($(os), macOS)
	cp CAPSImg/libcapsimage.dylib capsimg.so
else
	cp CAPSImg/libcapsimage.so.5.1 capsimg.so
endif

fsemu-clean:
	make -C CAPSImg clean
	rm -Rf capsimg${dll} dist.fs

fsemu-distclean: clean
	make -C CAPSImg distclean

fsemu-assemble:
	mkdir -p ${plugin_dir}
	echo ${version} > ${plugin_dir}/Version.txt
	cp README.fs ${plugin_dir}/ReadMe.txt

	mkdir -p ${os_arch_dir}
	echo ${version} > ${os_arch_dir}/Version.txt
	cp capsimg${dll} ${os_arch_dir}

	mkdir -p ${licenses_dir}
	cp LICENCE.txt ${licenses_dir}/CAPSImg.txt

# Override the default strip target
fsemu-strip:
	./strip.fs dist.fs/${plugin}
