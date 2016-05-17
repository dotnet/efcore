#!/usr/bin/env bash

while [ $1 ]; do
    echo "Deleting $1/TestProjects"
    rm -rf $1/TestProjects
    echo "Deleting $1/artifacts"
    rm -rf $1/artifacts
    shift
done

exit 0