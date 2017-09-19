#!/usr/bin/env bash

set -euo pipefail
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
source $DIR/sdk/KoreBuild/KoreBuild.sh

__usage() {
    echo "Usage: $0 [-v|--verbose] [-d|--dotnet-home <DIR>] [-s|--tools-source <URL>] [[--] <MSBUILD_ARG>...]"
    echo ""
    echo "Arguments:"
    echo "    <MSBUILD_ARG>...         Arguments passed to MSBuild. Variable number of arguments allowed."
    echo ""
    echo "Options:"
    echo "    -v|--verbose             Show verbose output."
    echo "    -d|--dotnet-home <DIR>   The directory where .NET Core tools will be stored. Defaults to '$DOTNET_HOME'."
    echo "    -s|--tools-source <URL>  The base url where build tools can be downloaded. Defaults to '$tools_source'."
    exit 2
}

#
# main
#

[ -z "${DOTNET_HOME:-}"] && DOTNET_HOME="$HOME/.dotnet"
tools_source='https://aspnetcore.blob.core.windows.net/buildtools'
verbose=false
while [[ $# > 0 ]]; do
    case $1 in
        -\?|-h|--help)
            __usage
            ;;
        -d|--dotnet-home)
            shift
            DOTNET_HOME=${1:-}
            [ -z "$DOTNET_HOME" ] && __usage
            ;;
        -s|--tools-source)
            shift
            tools_source=${1:-}
            [ -z "$tools_source" ] && __usage
            ;;
        -v|--verbose)
            verbose=true
            ;;
        --)
            shift
            break
            ;;
        *)
            break
            ;;
    esac
    shift
done

install_tools $tools_source $DOTNET_HOME
invoke_repository_build $DIR $@
