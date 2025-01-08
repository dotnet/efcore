#!/usr/bin/env powershell
#requires -version 4

<#
.SYNOPSIS
Executes KoreBuild commands.

.DESCRIPTION
Downloads korebuild if required. Then executes the KoreBuild command. To see available commands, execute with `-Command help`.

.PARAMETER Command
The KoreBuild command to run.

.PARAMETER Path
The folder to build. Defaults to the folder containing this script.

.PARAMETER Channel
The channel of KoreBuild to download. Overrides the value from the config file.

.PARAMETER DotNetHome
The directory where .NET Core tools will be stored.

.PARAMETER ToolsSource
The base url where build tools can be downloaded. Overrides the value from the config file.

.PARAMETER Update
Updates KoreBuild to the latest version even if a lock file is present.

.PARAMETER Reinstall
Re-installs KoreBuild

.PARAMETER ConfigFile
The path to the configuration file that stores values. Defaults to korebuild.json.

.PARAMETER ToolsSourceSuffix
The Suffix to append to the end of the ToolsSource. Useful for query strings in blob stores.

.PARAMETER CI
Sets up CI specific settings and variables.

.PARAMETER Arguments
Arguments to be passed to the command

.NOTES
This function will create a file $PSScriptRoot/korebuild-lock.txt. This lock file can be committed to source, but does not have to be.
When the lockfile is not present, KoreBuild will create one using latest available version from $Channel.

The $ConfigFile is expected to be an JSON file. It is optional, and the configuration values in it are optional as well. Any options set
in the file are overridden by command line parameters.

.EXAMPLE
Example config file:
```json
{
  "$schema": "https://raw.githubusercontent.com/aspnet/BuildTools/master/tools/korebuild.schema.json",
  "channel": "master",
  "toolsSource": "https://dotnetbuilds.blob.core.windows.net/buildtools"
}
```
#>
[CmdletBinding(PositionalBinding = $false)]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Command,
    [string]$Path = $PSScriptRoot,
    [Alias('c')]
    [string]$Channel,
    [Alias('d')]
    [string]$DotNetHome,
    [Alias('s')]
    [string]$ToolsSource,
    [Alias('u')]
    [switch]$Update,
    [switch]$Reinstall,
    [string]$ToolsSourceSuffix,
    [string]$ConfigFile = $null,
    [switch]$CI,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Arguments
)

Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

#
# Functions
#

function Get-KoreBuild {

    $lockFile = Join-Path $Path 'korebuild-lock.txt'

    if (!(Test-Path $lockFile) -or $Update) {
        Get-RemoteFile "$ToolsSource/korebuild/channels/$Channel/latest.txt" $lockFile $ToolsSourceSuffix
    }

    $version = Get-Content $lockFile | Where-Object { $_ -like 'version:*' } | Select-Object -first 1
    if (!$version) {
        Write-Error "Failed to parse version from $lockFile. Expected a line that begins with 'version:'"
    }
    $version = $version.TrimStart('version:').Trim()
    $korebuildPath = Join-Paths $DotNetHome ('buildtools', 'korebuild', $version)

    if ($Reinstall -and (Test-Path $korebuildPath)) {
        Remove-Item -Force -Recurse $korebuildPath
    }

    if (!(Test-Path $korebuildPath)) {
        Write-Host -ForegroundColor Magenta "Downloading KoreBuild $version"
        New-Item -ItemType Directory -Path $korebuildPath | Out-Null
        $remotePath = "$ToolsSource/korebuild/artifacts/$version/korebuild.$version.zip"

        try {
            $tmpfile = Join-Path ([IO.Path]::GetTempPath()) "KoreBuild-$([guid]::NewGuid()).zip"
            Get-RemoteFile $remotePath $tmpfile $ToolsSourceSuffix
            if (Get-Command -Name 'Microsoft.PowerShell.Archive\Expand-Archive' -ErrorAction Ignore) {
                # Use built-in commands where possible as they are cross-plat compatible
                Microsoft.PowerShell.Archive\Expand-Archive -Path $tmpfile -DestinationPath $korebuildPath
            }
            else {
                # Fallback to old approach for old installations of PowerShell
                Add-Type -AssemblyName System.IO.Compression.FileSystem
                [System.IO.Compression.ZipFile]::ExtractToDirectory($tmpfile, $korebuildPath)
            }
        }
        catch {
            Remove-Item -Recurse -Force $korebuildPath -ErrorAction Ignore
            throw
        }
        finally {
            Remove-Item $tmpfile -ErrorAction Ignore
        }
    }

    return $korebuildPath
}

function Join-Paths([string]$path, [string[]]$childPaths) {
    $childPaths | ForEach-Object { $path = Join-Path $path $_ }
    return $path
}

function Get-RemoteFile([string]$RemotePath, [string]$LocalPath, [string]$RemoteSuffix) {
    if ($RemotePath -notlike 'http*') {
        Copy-Item $RemotePath $LocalPath
        return
    }

    $retries = 10
    while ($retries -gt 0) {
        $retries -= 1
        try {
            Invoke-WebRequest -UseBasicParsing -Uri $($RemotePath + $RemoteSuffix) -OutFile $LocalPath
            return
        }
        catch {
            Write-Verbose "Request failed. $retries retries remaining"
        }
    }

    Write-Error "Download failed: '$RemotePath'."
}

#
# Main
#

# Load configuration or set defaults

$Path = Resolve-Path $Path
if (!$ConfigFile) { $ConfigFile = Join-Path $Path 'korebuild.json' }

if (Test-Path $ConfigFile) {
    try {
        $config = Get-Content -Raw -Encoding UTF8 -Path $ConfigFile | ConvertFrom-Json
        if ($config) {
            if (!($Channel) -and (Get-Member -Name 'channel' -InputObject $config)) { [string] $Channel = $config.channel }
            if (!($ToolsSource) -and (Get-Member -Name 'toolsSource' -InputObject $config)) { [string] $ToolsSource = $config.toolsSource}
        }
    }
    catch {
        Write-Host -ForegroundColor Red $Error[0]
        Write-Error "$ConfigFile contains invalid JSON."
        exit 1
    }
}

if (!$DotNetHome) {
    $DotNetHome = if ($env:DOTNET_HOME) { $env:DOTNET_HOME } `
        elseif ($CI) { Join-Path $PSScriptRoot '.dotnet' } `
        elseif ($env:USERPROFILE) { Join-Path $env:USERPROFILE '.dotnet'} `
        elseif ($env:HOME) {Join-Path $env:HOME '.dotnet'}`
        else { Join-Path $PSScriptRoot '.dotnet'}
}

if (!$Channel) { $Channel = 'master' }
if (!$ToolsSource) { $ToolsSource = 'https://dotnetbuilds.blob.core.windows.net/buildtools' }

# Execute

$korebuildPath = Get-KoreBuild
Import-Module -Force -Scope Local (Join-Path $korebuildPath 'KoreBuild.psd1')

try {
    Set-KoreBuildSettings -ToolsSource $ToolsSource -DotNetHome $DotNetHome -RepoPath $Path -ConfigFile $ConfigFile -CI:$CI
    Invoke-KoreBuildCommand $Command @Arguments
}
finally {
    Remove-Module 'KoreBuild' -ErrorAction Ignore
}
