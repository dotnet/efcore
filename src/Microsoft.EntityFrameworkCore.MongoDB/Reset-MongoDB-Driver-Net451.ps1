<#
 The MongoDB team has stopped strong-naming their assemblies:
    http://mongodb.github.io/mongo-csharp-driver/2.0/upgrading/#packaging

 The .MongoDB.Tests project strong-names the MongoDB driver libraries for .NET 4.5.1
 for the sole purpose of allowing the net451 unit tests to pass. This script deletes
 the strong-named package assemblies to ensure that the EF Core MongoDB provider is
 re-built with the correct references so that it can be correctly packaged.
#>

Param(
    [Parameter(Mandatory=$true, Position=1)][string]$Platform
)

if ($Platform -ne "net451") {
    exit
}

$sn = "${env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\sn.exe"

Function Restore-Dll([string]$package, [string]$assembly) {
    if (Test-Path("$assembly.original")) {
        Write-Host "Restoring library: ${package}..."
        Copy-Item -Force "$assembly.original" $assembly
    } else {
        Write-Host "Skipping ${package}: not strong-named..."
    }
}

#the .MongoDB.Tests project signs the MongoDB driver libraries in order for the net451 tests to pass
#so we need to delete the files and restore the NuGet dependencies in order to bring back the
"MongoDB.Bson", "MongoDB.Driver", "MongoDB.Driver.Core" | % { Restore-Dll $_ "${env:UserProfile}\.nuget\packages\$_\2.4.0\lib\net45\$_.dll" }

del "..\..\src\Microsoft.EntityFrameworkCore.MongoDB\bin\Debug\net451\Microsoft.EntityFrameworkCore.MongoDB.dll*"
