$ErrorActionPreference = 'Stop'

$versionErrorMessage = "EF Core commands do not support PowerShell version $($PSVersionTable.PSVersion). Please upgrade PowerShell to 3.0 or greater and restart Visual Studio."

function Add-Migration {
    Hint-Upgrade $MyInvocation.MyCommand
    throw $versionErrorMessage
}

function Remove-Migration {
    throw $versionErrorMessage
}

function Scaffold-DbContext {
    throw $versionErrorMessage
}

function Script-Migration {
    throw $versionErrorMessage
}

function Update-Database {
    Hint-Upgrade $MyInvocation.MyCommand
    throw $versionErrorMessage
}

function Use-DbContext {
    throw $versionErrorMessage
}

#
# Enable-Migrations (Obsolete)
#

function Enable-Migrations {
    # TODO: Link to some docs on the changes to Migrations
    Hint-Upgrade $MyInvocation.MyCommand
    Write-Warning 'Enable-Migrations is obsolete. Use Add-Migration to start using Migrations.'
}

#
# Private functions
#

function Hint-Upgrade ($name) {
    if (Get-Module | Where { $_.Name -eq 'EntityFramework' }) {
        Write-Warning "Executing the Entity Framework Core version of '$name'. Run 'EntityFramework\$name' to execute for EF 6 and earlier."
    }
}