param ($installPath, $toolsPath, $package, $project)

if ($PSVersionTable.PSVersion.Major -lt 3) {
    # This section needs to support PS 2.0 syntax
    # Use $toolsPath because PS 2 does not support $PSScriptRoot
    $env:PSModulePath= $env:PSModulePath + ";$toolsPath"
    
    # import a "dummy" module that contains matching functions that throw on PS2
    Import-Module ([System.IO.Path]::Combine($toolsPath, "EntityFrameworkCore.PowerShell2.psd1")) -DisableNameChecking
    
    throw "EF Core commands do not support PowerShell version $($PSVersionTable.PSVersion). Please upgrade PowerShell to 3.0 or greater and restart Visual Studio."
} else {
    
    if (Get-Module | ? Name -eq EntityFrameworkCore) {
        Remove-Module EntityFrameworkCore
    }

    Import-Module (Join-Path $PSScriptRoot EntityFrameworkCore.psd1) -DisableNameChecking
}

