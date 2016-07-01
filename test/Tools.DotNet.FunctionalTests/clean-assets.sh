#!/usr/bin/env bash

while [ $1 ]; do
    echo "Deleting $1/TestProjects"
    rm -rf $1/TestProjects
    echo "Deleting $1/tools"
    rm -rf $1/tools
    shift
done

exit 0