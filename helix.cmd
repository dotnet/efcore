@ECHO OFF

:: This command sends helix test job from local machine

SET DOTNET_ROOT=%~dp0.dotnet\
SET BUILD_SOURCEBRANCH="main"
SET BUILD_REPOSITORY_NAME="efcore"
SET SYSTEM_TEAMPROJECT="public"
SET BUILD_REASON="test"

IF NOT EXIST "%DOTNET_ROOT%\dotnet.exe" (
    call dotnet msbuild eng\helix.proj /restore /t:Test %*
) ELSE (
    call %DOTNET_ROOT%\dotnet.exe msbuild eng\helix.proj /restore /t:Test %*
)