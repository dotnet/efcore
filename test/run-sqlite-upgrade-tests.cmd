@echo off
setlocal

rem Runs the Sqlite e_sqlite3 -> SQLite3MC upgrade test scenario in two stages.
rem Stage 1: creates DB files with SQLitePCLRaw.bundle_e_sqlite3.
rem Stage 2: copies those DB files into the reader's output directory and runs the
rem          SQLite3MC reader tests.
rem
rem The two stages MUST run as separate processes because SQLitePCL.raw.SetProvider
rem is a one-shot, process-global call.

if "%CONFIGURATION%"=="" set CONFIGURATION=Debug

set REPO_ROOT=%~dp0..
set SEED_PROJ=%REPO_ROOT%\test\Microsoft.Data.Sqlite.Upgrade.Seed.Tests\Microsoft.Data.Sqlite.Upgrade.Seed.Tests.csproj
set READ_PROJ=%REPO_ROOT%\test\Microsoft.Data.Sqlite.Upgrade.Read.sqlite3mc.Tests\Microsoft.Data.Sqlite.Upgrade.Read.sqlite3mc.Tests.csproj
set SEED_BIN=%REPO_ROOT%\artifacts\bin\Microsoft.Data.Sqlite.Upgrade.Seed.Tests\%CONFIGURATION%
set READ_BIN=%REPO_ROOT%\artifacts\bin\Microsoft.Data.Sqlite.Upgrade.Read.sqlite3mc.Tests\%CONFIGURATION%

echo === Stage 1: seeding DBs with SQLitePCLRaw.bundle_e_sqlite3 ^(%CONFIGURATION%^) ===
dotnet test "%SEED_PROJ%" --configuration %CONFIGURATION%
set SEED_EXIT=%ERRORLEVEL%
if not "%SEED_EXIT%"=="0" (
    echo Seed stage exited with %SEED_EXIT% ^(probable extension-probe findings^); continuing to copy + read.
)

echo.
echo === Copying seeded DB files to reader output ===
powershell -NoProfile -ExecutionPolicy Bypass -Command "$ErrorActionPreference='Stop'; $seedBin=(Resolve-Path -LiteralPath $env:SEED_BIN).Path; $readBin=[System.IO.Path]::GetFullPath($env:READ_BIN); $dirs = Get-ChildItem -Path $seedBin -Recurse -Directory -Filter upgrade-dbs; if (-not $dirs) { Write-Host ('No upgrade-dbs folder found under ' + $seedBin); exit 1 }; foreach ($d in $dirs) { $rel = $d.FullName.Substring($seedBin.Length).TrimStart('\'); $dest = Join-Path $readBin $rel; Write-Host ('Copying ' + $d.FullName + ' -> ' + $dest); if (Test-Path -LiteralPath $dest) { Remove-Item -LiteralPath $dest -Recurse -Force }; $destParent = Split-Path -Parent $dest; New-Item -ItemType Directory -Path $destParent -Force | Out-Null; Copy-Item -LiteralPath $d.FullName -Destination $dest -Recurse -Force }"
if errorlevel 1 (
    echo Copy step FAILED.
    exit /b 1
)

echo.
echo === Stage 2: reading DBs with SQLite3MC.PCLRaw.bundle ^(%CONFIGURATION%^) ===
dotnet test "%READ_PROJ%" --configuration %CONFIGURATION%
set READ_EXIT=%ERRORLEVEL%

echo.
if "%SEED_EXIT%"=="0" (
    if "%READ_EXIT%"=="0" (
        echo Upgrade scenario PASSED.
        exit /b 0
    )
)
echo Upgrade scenario completed with findings: seed exit=%SEED_EXIT%, read exit=%READ_EXIT%.
if not "%READ_EXIT%"=="0" exit /b %READ_EXIT%
exit /b %SEED_EXIT%
