#!/usr/bin/env pwsh
<#
.DESCRIPTION
    Creates API baseline files representing the current public API surface exposed by this repo.
.PARAMETER ProjectNamePattern
    Optional wildcard used to filter which source projects are processed.
.PARAMETER Configuration
    Build configuration to use when locating binaries and building ApiChief.
.PARAMETER Ci
    Indicates that the script was invoked from a CI build.
#>

param(
    [string]$ProjectNamePattern = "*",
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    [switch]$Ci
)

$apiChiefDeltaNoChangesExitCode = 2

if ($PSVersionTable.PSVersion.Major -lt 6) {
    Write-Host "PowerShell 6.0 or greater is required to run this script. See https://aka.ms/install-powershell."
    Write-Host "Current version:" $PSVersionTable.PSVersion.ToString()
    exit 1
}

Write-Output "Building ApiChief tool"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
. (Join-Path $repoRoot "eng/common/tools.ps1")
$dotnetRoot = InitializeDotNetCli -install $true
$dotnet = Join-Path $dotnetRoot (GetExecutableFileName 'dotnet')
$project = Join-Path $repoRoot "eng/Tools/ApiChief/ApiChief.csproj"
$srcFolder = Join-Path $repoRoot "src"
& $dotnet build $project --configuration $Configuration --nologo --verbosity q

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$command = (& $dotnet msbuild $project --getProperty:TargetPath -p:Configuration=$Configuration --nologo).Trim()
if ([string]::IsNullOrWhiteSpace($command) -or !(Test-Path $command)) {
    Write-Error "Unable to locate the built ApiChief binary."
    exit 1
}

Write-Output "Creating API baseline files in the src folder"

Get-ChildItem -Path $srcFolder -Recurse -Filter *.csproj |
    Sort-Object FullName |
    Where-Object {
        ($_.BaseName -like $ProjectNamePattern) -and (Select-String -Path $_.FullName -Pattern '<GenerateDocumentationFile>true</GenerateDocumentationFile>' -Quiet)
    } |
    ForEach-Object {
        $name = $_.BaseName
        $artifactDir = Join-Path $repoRoot "artifacts/bin/$name/$Configuration"
        $tfm = Get-ChildItem -Path $artifactDir -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match '^net\d+\.\d+$' } |
            Sort-Object { [version]($_.Name -replace '^net', '') } -Descending |
            Select-Object -First 1 -ExpandProperty Name

        if ($null -eq $tfm) {
            Write-Warning "Skipping $name because no built net* target was found under '$artifactDir'. Build the project first."
            return
        }

        $assemblyName = (& $dotnet msbuild $_.FullName --getProperty:AssemblyName -p:Configuration=$Configuration --nologo).Trim()
        if ([string]::IsNullOrWhiteSpace($assemblyName)) {
            $assemblyName = $name
        }

        $assemblyPath = Join-Path $artifactDir "$tfm/$assemblyName.dll"
        if (!(Test-Path $assemblyPath)) {
            Write-Warning "Skipping $name because '$assemblyPath' does not exist. Build the project first."
            return
        }

        $baselinePath = Join-Path $_.Directory.FullName "$name.baseline.json"
        $previousBaselinePath = Join-Path $_.Directory.FullName "$name.previous.baseline.json"
        $deltaPath = Join-Path $_.Directory.FullName "$name.delta.json"

        if (Test-Path $previousBaselinePath) {
            Remove-Item $previousBaselinePath -Force
        }

        if (Test-Path $baselinePath) {
            Rename-Item $baselinePath -NewName (Split-Path $previousBaselinePath -Leaf)
        }

        if (Test-Path $deltaPath) {
            Remove-Item $deltaPath -Force
        }

        Write-Host "  Processing $name ($tfm, $Configuration)"
        & $dotnet $command $assemblyPath emit baseline -o $baselinePath

        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }

        if (Test-Path $previousBaselinePath) {
            if ($Ci) {
                & $dotnet $command $baselinePath emit delta $previousBaselinePath -o $deltaPath
                $deltaExitCode = $LASTEXITCODE

                if ($deltaExitCode -eq 0) {
                    Write-Error "API changes were detected for $name and the baselines in the PR need to be updated by running build locally."
                    exit 1
                }
                elseif ($deltaExitCode -ne $apiChiefDeltaNoChangesExitCode) {
                    exit $deltaExitCode
                }

                if (Test-Path $deltaPath) {
                    Remove-Item $deltaPath -Force
                }
            }

            Remove-Item $previousBaselinePath -Force
        }
    }
