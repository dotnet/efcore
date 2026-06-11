#!/usr/bin/env pwsh
<#
.DESCRIPTION
    Builds and invokes the ApiChief tool to generate API review metadata.
.PARAMETER AssemblyPath
    Path to the assembly to extract the API from.
#>

param (
    [Parameter(Mandatory = $true, HelpMessage="Path to the assembly to extract the API from.", Position = 0, ParameterSetName = "AssemblyPath")]
    [string]$AssemblyPath,
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
. (Join-Path $repoRoot "eng/common/tools.ps1")
$dotnetRoot = InitializeDotNetCli -install $true
$dotnet = Join-Path $dotnetRoot (GetExecutableFileName 'dotnet')
$project = Join-Path $repoRoot "eng/Tools/ApiChief/ApiChief.csproj"

Write-Output "Building ApiChief tool"

& $dotnet build $project --configuration $Configuration --nologo --verbosity q

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$command = (& $dotnet msbuild $project --getProperty:TargetPath -p:Configuration=$Configuration --nologo).Trim()
if ([string]::IsNullOrWhiteSpace($command) -or !(Test-Path $command)) {
    Write-Error "Unable to locate the built ApiChief binary."
    exit 1
}

Write-Output "Creating API review artifacts in the API.* folder"

& $dotnet $command $AssemblyPath emit review
