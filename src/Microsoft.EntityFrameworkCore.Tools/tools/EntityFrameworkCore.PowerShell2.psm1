$ErrorActionPreference = 'Stop'

$versionErrorMessage = "EF commands do not support PowerShell version $($PSVersionTable.PSVersion). Please upgrade PowerShell to 3.0 or greater and restart Visual Studio."

function Add-Migration {
    throw $versionErrorMessage 
}

function Enable-Migrations {
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
    throw $versionErrorMessage 
}

function Use-DbContext {
    throw $versionErrorMessage
}