#!/usr/bin/env sh
set -e

# Colors
GREEN="\033[1;32m"
CYAN="\033[0;36m"
RESET="\033[0m"
RED="\033[0;31m"

# functions
__exec() {
    local cmd=$1
    shift

    local cmdname=$(basename $cmd)
    # put on stderr to avoid it being captured in variables
    echo "${CYAN}> $cmdname $@${RESET}" >&2

    $cmd $@

    local exitCode=$?
    if [ $exitCode -ne 0 ]; then
        echo "${RED}'$cmdname $@' failed with exit code $exitCode${RESET}" 1>&2
        exit $exitCode
    fi
}

# main

while [ $# -ne 0 ]
do
    name=$1
    case $name in
        -p|--sa-password)
            shift
            sa_password=$1
            ;;
        -h|--help)
            script_name="$(basename $0)"
            echo "Setup MSSQL"
            echo "Usage: $script_name [-p|--sa-password <SA_PASSWORD>]"
            echo ""
            echo "Options:"
            echo "  -p,--sa-password <SA_PASSWORD>  The password to set for the 'sa' user on the server"
            echo "  -h,--help                       Shows this help message"
            exit 0
            ;;
        *)
            say_err "Unknown argument \`$name\`"
            exit 1
            ;;
    esac

    shift
done

if [ -z "$sa_password" ]; then
    echo "${RED}Required option -p|--sa-password is not set. Run --help to see usage.${RESET}"
    exit 1
fi

__exec docker pull microsoft/mssql-server-linux
container=$(__exec docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=$sa_password" -p 1433:1433 -d microsoft/mssql-server-linux)
echo "Created container $container"
__exec docker ps -a

# wait long enough for docker to start the container and check if enough memory is available
# mssql requries at least 4 GB of memory
dbserver=localhost
dbport=1433
retries=20
__exec sleep 5s
until nc -z $dbserver $dbport
do
    echo "$(date) - waiting for ${dbserver}:${dbport}..."
    if [ "$retries" -le 0 ]; then
        echo "Done waiting. There might have been a problem starting the server."
        __exec docker logs $container
        exit 1
    fi
    retries=$((retries - 1))
    echo "Waiting before retrying. Retries left: $retries"
    sleep 5s
done

echo "${GREEN}Ready${RESET}"
