@echo off
setlocal DisableDelayedExpansion
set "__BuildArgs=%*"
setlocal EnableDelayedExpansion

set "__EFConfiguration=Debug"
set "__EFCI=false"

:parseArgs
if "%~1"=="" goto runBuild

if /I "%~1"=="-c" (
    set "__EFConfiguration=%~2"
    shift /1
) else if /I "%~1"=="-configuration" (
    set "__EFConfiguration=%~2"
    shift /1
) else if /I "%~1"=="-ci" (
    set "__EFCI=true"
)

shift /1
goto parseArgs

:runBuild
powershell -ExecutionPolicy ByPass -NoProfile -command "& '%~dp0eng\common\build.ps1' -nodeReuse:$false -restore -build %__BuildArgs%"
if errorlevel 1 exit /b %ErrorLevel%

set "__CiArg="
if /I "!__EFCI!"=="true" set "__CiArg=-Ci"

pwsh -File "%~dp0tools\MakeApiBaselines.ps1" -Configuration "!__EFConfiguration!" !__CiArg!
exit /b %ErrorLevel%
