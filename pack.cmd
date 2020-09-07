del /Q artifacts\packages\is24\*.*

set nugetserver=http://nuget.immoscout24.ch/api/v2/package
set nugetKey=e5bf3fb41a3347908b3df92b87742128

set version=3.1.8
set versionSuffix=is24-fix-1

dotnet pack src\EFCore\EFCore.csproj -o artifacts\packages\is24 --version-suffix %versionSuffix%
dotnet pack src\EFCore.Abstractions\EFCore.Abstractions.csproj -o artifacts\packages\is24 --version-suffix %versionSuffix%
dotnet pack src\EFCore.Analyzers\EFCore.Analyzers.csproj -o artifacts\packages\is24 --version-suffix %versionSuffix%
dotnet pack src\EFCore.InMemory\EFCore.InMemory.csproj -o artifacts\packages\is24 --version-suffix %versionSuffix%
dotnet pack src\EFCore.Sqlite\EFCore.Sqlite.csproj -o artifacts\packages\is24 --version-suffix %versionSuffix%
dotnet pack src\EFCore.SqlServer\EFCore.SqlServer.csproj -o artifacts\packages\is24 --version-suffix %versionSuffix%

dotnet nuget push artifacts\packages\is24\Microsoft.EntityFrameworkCore.%version%-%versionSuffix%.nupkg -k %nugetKey% -s %nugetserver%
dotnet nuget push artifacts\packages\is24\Microsoft.EntityFrameworkCore.Abstractions.%version%-%versionSuffix%.nupkg -k %nugetKey% -s %nugetserver%
dotnet nuget push artifacts\packages\is24\Microsoft.EntityFrameworkCore.Analyzers.%version%-%versionSuffix%.nupkg -k %nugetKey% -s %nugetserver%
dotnet nuget push artifacts\packages\is24\Microsoft.EntityFrameworkCore.InMemory.%version%-%versionSuffix%.nupkg -k %nugetKey% -s %nugetserver%
dotnet nuget push artifacts\packages\is24\Microsoft.EntityFrameworkCore.Sqlite.%version%-%versionSuffix%.nupkg -k %nugetKey% -s %nugetserver%
dotnet nuget push artifacts\packages\is24\Microsoft.EntityFrameworkCore.SqlServer.%version%-%versionSuffix%.nupkg -k %nugetKey% -s %nugetserver%
