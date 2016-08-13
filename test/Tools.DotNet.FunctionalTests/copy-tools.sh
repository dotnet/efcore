#!/usr/bin/env bash

mkdir -p $2/tools/netcoreapp1.0
cp ../../src/Tools.Console/bin/$1/netcoreapp1.0/ef.dll $2/tools/netcoreapp1.0/
