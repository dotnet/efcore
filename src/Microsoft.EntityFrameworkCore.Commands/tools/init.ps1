param ($installPath, $toolsPath, $package, $project)

if (Get-Module | ? Name -eq EntityFramework) {
    Remove-Module EntityFramework
}

Import-Module (Join-Path $PSScriptRoot Microsoft.EntityFrameworkCore.psd1) -DisableNameChecking
