#!/usr/bin/env bash

while [ $1 ]; do
    echo "Deleting $1/TestAssets"
    rm -rf $1/TestAssets
    echo "Deleting $1/artifacts"
    rm -rf $1/artifacts
    shift
done

exit 0