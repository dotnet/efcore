#!/usr/bin/env bash

source="${BASH_SOURCE[0]}"

# resolve $SOURCE until the file is no longer a symlink
while [[ -h $source ]]; do
  scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"
  source="$(readlink "$source")"

  # if $source was a relative symlink, we need to resolve it relative to the path where the
  # symlink file was located
  [[ $source != /* ]] && source="$scriptroot/$source"
done

scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"
configuration=Debug
ci=false
args=("$@")

while [[ $# -gt 0 ]]; do
  case "$1" in
    -c|--configuration)
      configuration="$2"
      shift 2
      ;;
    --ci)
      ci=true
      shift
      ;;
    *)
      shift
      ;;
  esac
done

"$scriptroot/eng/common/build.sh" --nodeReuse false --build --restore "${args[@]}"
build_exit_code=$?
if [[ $build_exit_code -ne 0 ]]; then
  exit $build_exit_code
fi

baseline_args=(-Configuration "$configuration")
if [[ "$ci" == true ]]; then
  baseline_args+=(-Ci)
fi

pwsh -NoProfile -File "$scriptroot/tools/MakeApiBaselines.ps1" "${baseline_args[@]}"
