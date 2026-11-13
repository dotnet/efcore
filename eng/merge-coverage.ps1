param(
    [Parameter(Mandatory = $true)]
    [string] $CoverageDirectory,

    [Parameter(Mandatory = $true)]
    [string] $OutputFile
)

$ErrorActionPreference = 'Stop'

$dotnetRoot = if (![string]::IsNullOrEmpty($env:DOTNET_GLOBAL_INSTALL_DIR)) {
    $env:DOTNET_GLOBAL_INSTALL_DIR
} elseif (![string]::IsNullOrEmpty($env:DOTNET_ROOT)) {
    $env:DOTNET_ROOT
} elseif ($env:BUILD_SOURCESDIRECTORY) {
    Join-Path $env:BUILD_SOURCESDIRECTORY '.dotnet'
}

$pathSeparator = [System.IO.Path]::PathSeparator
if ($dotnetRoot -and (Test-Path $dotnetRoot)) {
    $env:DOTNET_ROOT = $dotnetRoot
    $env:PATH = "$dotnetRoot$pathSeparator$env:PATH"
}

if (!(Test-Path $CoverageDirectory)) {
    return
}

$coverageFiles = Get-ChildItem -Path $CoverageDirectory -Filter '*.cobertura.xml' -Recurse |
    Where-Object { $_.FullName -ne [System.IO.Path]::GetFullPath($OutputFile) } |
    Sort-Object FullName

if (!$coverageFiles) {
    return
}

foreach ($coverageFile in $coverageFiles) {
    [xml] $coverage = Get-Content -Raw $coverageFile.FullName

    # Cobertura preserves checkout roots, which prevents coverage for the same file from merging across operating systems.
    foreach ($class in $coverage.SelectNodes('//class[@filename]')) {
        $normalizedPath = $class.filename.Replace('\', '/')
        $sourceIndex = $normalizedPath.IndexOf('/src/', [StringComparison]::OrdinalIgnoreCase)

        if ($sourceIndex -ge 0) {
            $class.filename = $normalizedPath.Substring($sourceIndex + 1)
        }
    }

    $coverage.Save($coverageFile.FullName)
}

$dotnetToolsDirectory = Join-Path ([Environment]::GetFolderPath('UserProfile')) '.dotnet/tools'
$env:PATH = "$env:PATH$pathSeparator$dotnetToolsDirectory"

dotnet tool update --global dotnet-coverage
if ($LASTEXITCODE -ne 0) {
    dotnet tool install --global dotnet-coverage
}

dotnet-coverage merge -o $OutputFile -f cobertura $coverageFiles.FullName
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}