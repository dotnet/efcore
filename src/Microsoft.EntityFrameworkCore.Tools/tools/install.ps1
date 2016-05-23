param ($installPath, $toolsPath, $package, $project)

if ($PSVersionTable.PSVersion.Major -lt 3) {
    throw "EF commands do not support PowerShell version $($PSVersionTable.PSVersion). Please upgrade PowerShell to 3.0 or greater and restart Visual Studio."
}

Write-Host
Write-Host 'Type ''get-help EntityFrameworkCore'' to see all available Entity Framework Core commands.'
Write-Host
