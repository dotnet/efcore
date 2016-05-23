param ($installPath, $toolsPath, $package, $project)

if ($PSVersionTable.PSVersion.Major -lt 3) {
    throw "EF commands do not support PowerShell version $($PSVersionTable.PSVersion). Please upgrade PowerShell to 3.0 or greater and restart Visual Studio."
}

if (Get-Module | ? Name -eq EntityFrameworkCore) {
    Remove-Module EntityFrameworkCore
}

Import-Module (Join-Path $PSScriptRoot EntityFrameworkCore.psd1) -DisableNameChecking
