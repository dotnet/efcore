#!/usr/bin/env bash

toolsBase="$2/../../tools"
mkdir -p $toolsBase/netcoreapp1.0
cp ../../src/Tools.Console/bin/$1/netcoreapp1.0/ef.dll $toolsBase/netcoreapp1.0/
