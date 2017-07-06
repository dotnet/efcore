$ErrorActionPreference = 'Stop'

$versionErrorMessage = 'The Entity Framework Core Package Manager Console Tools don''t support PowerShell version ' +
    "$($PSVersionTable.PSVersion). Upgrade to PowerShell version 3.0 or higher, restart Visual Studio, and try again."

function Add-Migration
{
    WarnIfEF6 'Add-Migration'

    throw $versionErrorMessage
}

function Drop-Database
{
    throw $versionErrorMessage
}

function Enable-Migrations
{
    WarnIfEF6 'Enable-Migrations'

    throw $versionErrorMessage
}

function Get-DbContext
{
    throw $versionErrorMessage
}

function Remove-Migration
{
    throw $versionErrorMessage
}

function Scaffold-DbContext
{
    throw $versionErrorMessage
}

function Script-Migration
{
    throw $versionErrorMessage
}

function Update-Database
{
    WarnIfEF6 'Update-Database'

    throw $versionErrorMessage
}

function WarnIfEF6($cmdlet)
{
    if (Get-Module 'EntityFramework')
    {
        Write-Warning "Both Entity Framework Core and Entity Framework 6 are installed. The Entity Framework Core tools are running. Use 'EntityFramework\$cmdlet' for Entity Framework 6."
    }
}