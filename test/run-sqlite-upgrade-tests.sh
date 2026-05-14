#!/usr/bin/env bash
# Runs the Sqlite e_sqlite3 -> SQLite3MC upgrade test scenario in two stages.
# Stage 1: creates DB files with SQLitePCLRaw.bundle_e_sqlite3.
# Stage 2: copies those DB files into the reader's output directory and runs the
#          SQLite3MC reader tests.
#
# The two stages MUST run as separate processes because SQLitePCL.raw.SetProvider
# is a one-shot, process-global call.

set -euo pipefail

CONFIGURATION="${CONFIGURATION:-Debug}"

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
REPO_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"
SEED_PROJ="$REPO_ROOT/test/Microsoft.Data.Sqlite.Upgrade.Seed.Tests/Microsoft.Data.Sqlite.Upgrade.Seed.Tests.csproj"
READ_PROJ="$REPO_ROOT/test/Microsoft.Data.Sqlite.Upgrade.Read.sqlite3mc.Tests/Microsoft.Data.Sqlite.Upgrade.Read.sqlite3mc.Tests.csproj"
SEED_BIN="$REPO_ROOT/artifacts/bin/Microsoft.Data.Sqlite.Upgrade.Seed.Tests/$CONFIGURATION"
READ_BIN="$REPO_ROOT/artifacts/bin/Microsoft.Data.Sqlite.Upgrade.Read.sqlite3mc.Tests/$CONFIGURATION"

echo "=== Stage 1: seeding DBs with SQLitePCLRaw.bundle_e_sqlite3 ($CONFIGURATION) ==="
seed_exit=0
dotnet test "$SEED_PROJ" --configuration "$CONFIGURATION" || seed_exit=$?
if [[ "$seed_exit" -ne 0 ]]; then
    echo "Seed stage exited with $seed_exit (probable extension-probe findings); continuing to copy + read."
fi

echo
echo "=== Copying seeded DB files to reader output ==="
if [[ ! -d "$SEED_BIN" ]]; then
    echo "Seed bin not found: $SEED_BIN"
    exit 1
fi

copied=0
while IFS= read -r -d '' src; do
    rel="${src#$SEED_BIN/}"
    dest="$READ_BIN/$rel"
    echo "Copying $src -> $dest"
    rm -rf "$dest"
    mkdir -p "$(dirname "$dest")"
    cp -R "$src" "$dest"
    copied=$((copied + 1))
done < <(find "$SEED_BIN" -type d -name upgrade-dbs -print0)

if [[ "$copied" -eq 0 ]]; then
    echo "No upgrade-dbs folder found under $SEED_BIN."
    exit 1
fi

echo
echo "=== Stage 2: reading DBs with SQLite3MC.PCLRaw.bundle ($CONFIGURATION) ==="
read_exit=0
dotnet test "$READ_PROJ" --configuration "$CONFIGURATION" || read_exit=$?

echo
if [[ "$seed_exit" -eq 0 && "$read_exit" -eq 0 ]]; then
    echo "Upgrade scenario PASSED."
    exit 0
fi
echo "Upgrade scenario completed with findings: seed exit=$seed_exit, read exit=$read_exit."
if [[ "$read_exit" -ne 0 ]]; then
    exit "$read_exit"
fi
exit "$seed_exit"
