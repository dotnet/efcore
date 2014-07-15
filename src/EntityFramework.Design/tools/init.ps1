param ($installPath, $toolsPath, $package, $project)

if (Get-Module | ? Name -eq EntityFramework) {
    Remove-Module EntityFramework
}

Import-Module (Join-Path $toolsPath EntityFramework.psd1) -DisableNameChecking
