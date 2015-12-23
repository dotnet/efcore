#!/usr/bin/env bash

RED='\033[0;31m'
GREEN='\033[0;32m'
CYAN='\033[1;36m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Setup DNVM

DNVM_LOCATION=packages/dnvm.sh

if [ ! -e $DNVM_LOCATION ]; then
    mkdir -p `dirname $DNVM_LOCATION`
    curl -sSL -o $DNVM_LOCATION https://raw.githubusercontent.com/aspnet/Home/dev/dnvm.sh
fi

source $DNVM_LOCATION

# Install dnx coreclr

if [ -z $SKIP_DNX_INSTALL ]; then
    DNX_UNSTABLE_FEED=https://www.myget.org/F/aspnetcidev/ dnvm install latest -u -r coreclr -alias default
fi

dnvm use default -r coreclr

# Restore

dnu restore --quiet
if [[ $? != 0 ]]; then
    printf "${RED}Package restore failed${NC}\n"
    exit 1
fi

# Execute tests

ERRORS=()
SKIPPED=()

for t in `grep -l "xunit.runner" test/*/project.json`; do
    TEST_DIR=$(dirname $t)

    _=$(grep "dnxcore50" $t)
    rc=$?
    if [[ $rc != 0 ]]; then
        printf "${YELLOW}Skipping tests on project ${TEST_DIR}. Project does not support CoreCLR${NC}\n"
        SKIPPED+=("${TEST_DIR} skipped")
        continue
    fi
    
    printf "${CYAN}Running tests on ${TEST_DIR}${NC}\n"

    (cd $TEST_DIR && dnx test $@)
    rc=$?
    if [[ $rc != 0 ]]; then
        printf "${RED}Test ${TEST_DIR} failed error code ${rc}${NC}\n"
        ERRORS+=("${TEST_DIR} failed")
    fi
done

# Summary and exit

echo "============= TEST SUMMARY =============="

printf "${YELLOW}%s${NC}\n" "${SKIPPED[@]}"

if [ "${#ERRORS}" -ne "0" ]; then
    printf "${RED}%s${NC}\n" "${ERRORS[@]}"
    rc=1
else
    printf "${GREEN}All tests passed${NC}\n"
    rc=0
fi
rm $DNVM_LOCATION
exit $rc