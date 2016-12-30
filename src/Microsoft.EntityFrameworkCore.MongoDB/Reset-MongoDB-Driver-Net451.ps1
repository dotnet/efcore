<#
 The MongoDB team has stopped strong-naming their assemblies:
    http://mongodb.github.io/mongo-csharp-driver/2.0/upgrading/#packaging

 The .MongoDB.Tests project strong-names the MongoDB driver libraries for .NET 4.5.1
 for the sole purpose of allowing the net451 unit tests to pass. This script deletes
 the strong-named package assemblies to ensure that the EF Core MongoDB provider is
 re-built with the correct references so that it can be correctly packaged.
#>

$sn = "${env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\sn.exe"

Function Check-Package([string]$package) {
    $assembly = "${env:UserProfile}\.nuget\packages\${package}\2.4.0\lib\net45\${package}.dll"

    &"$sn" -q -Tp "$assembly"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Skipping ${package}: not signed..."
    } else {
        Write-Host "Deleting NuGet package: ${package}..."
        Remove-Item -Recurse "${env:UserProfile}\.nuget\packages\${package}\2.4.0"
    }
}

#the .MongoDB.Tests project signs the MongoDB driver libraries in order for the net451 tests to pass
#so we need to delete the files and restore the NuGet dependencies in order to bring back the
Check-Package "MongoDB.Bson"
Check-Package "MongoDB.Driver"
Check-Package "MongoDB.Driver.Core"

if (Test-Path "..\..\src\Microsoft.EntityFrameworkCore.MongoDB\bin\Debug\net451\Microsoft.EntityFrameworkCore.MongoDB.dll") {
    Remove-Item "..\..\src\Microsoft.EntityFrameworkCore.MongoDB\bin\Debug\net451\Microsoft.EntityFrameworkCore.MongoDB.dll"
}

&dotnet.exe restore