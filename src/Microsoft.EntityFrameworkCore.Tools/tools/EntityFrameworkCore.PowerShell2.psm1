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
        Write-Warning "Both Entity Framework Core and Entity Framework 6.x commands are installed. The Entity Framework Core version is executing. You can fully qualify the command to select which one to execute, 'EntityFramework\$name' for EF6.x and 'EntityFrameworkCore\$name' for EF Core."
    }
}