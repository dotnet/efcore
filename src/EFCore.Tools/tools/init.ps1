param($installPath, $toolsPath, $package, $project)

# NB: Not set for scripts in PowerShell 2.0
if (!$PSScriptRoot)
{
    $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}

if ($PSVersionTable.PSVersion -lt '3.0')
{
    # Import a "dummy" module that contains matching functions that throw on PS2
    Import-Module (Join-Path $PSScriptRoot 'EntityFrameworkCore.PS2.psd1') -DisableNameChecking

    return
}

$importedModule = Get-Module 'EntityFrameworkCore'
$moduleToImport = Test-ModuleManifest (Join-Path $PSScriptRoot 'EntityFrameworkCore.psd1')
$import = $true
if ($importedModule)
{
    if ($importedModule.Version -le $moduleToImport.Version)
    {
        Remove-Module 'EntityFrameworkCore'
    }
    else
    {
        $import = $false
    }
}

if ($import)
{
    Import-Module $moduleToImport -DisableNameChecking
}
