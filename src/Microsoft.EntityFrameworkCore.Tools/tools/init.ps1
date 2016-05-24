param ($installPath, $toolsPath, $package, $project)

if (Get-Module | ? Name -eq EntityFrameworkCore) {
    Remove-Module EntityFrameworkCore
}

Import-Module (Join-Path $PSScriptRoot EntityFrameworkCore.psd1) -DisableNameChecking
