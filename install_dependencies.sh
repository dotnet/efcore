#!/usr/bin/env sh
if [ "$TRAVIS_OS_NAME" = "linux" ]; then
	sudo apt-get autoremove sqlite3
	sudo apt-add-repository -y ppa:travis-ci/sqlite3
	sudo apt-get update -qq -y
	sudo apt-cache show sqlite3
	sudo apt-get install -y sqlite3=3.7.15.1-1~travis1
fi

if [ "$TRAVIS_OS_NAME" = "osx" ]; then
	brew update
	brew outdated sqlite || brew upgrade sqlite
	brew link sqlite --force
fi



