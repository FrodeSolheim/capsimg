#!/bin/sh -e
echo "Boostrapping capsimg..."
cd CAPSImg
rm -Rf autom4te.cache
echo "Running autoheader"
autoheader
echo "Running autoconf"
autoconf

cd ..

echo "Bootstrap done, you can now run ./configure.fs"
