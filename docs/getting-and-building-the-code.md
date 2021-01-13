# Main branch

Following instructions are for current **main** branch only. For building release/2.x branches go to [Earlier versions](https://github.com/dotnet/efcore/wiki/Getting-and-Building-the-Code#earlier-versions).

## Prerequisites

EF Core does not generally need any prerequisites installed to build the code. However, the SQL Server tests require that SQL Server LocalDb be installed. For this, and for a rich developer experience, install [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) 16.8.0 preview2 or higher version with the "ASP.NET and web development" workload.

## Clone the repository

Using your favorite [git](http://git-scm.com/) client, clone the repository.

```console
git clone https://github.com/dotnet/efcore.git
```

## Build

To build the code just call build[.cmd/.sh]. This will install preview SDK as needed, restore packages, and build all projects. This does not run tests.

```console
build
```

The `build` script has different arguments to perform specific actions. The full list of arguments can be found via `build -h` command. Arguments for common actions are listed in table below. The repository root directory also contains cmd/sh files to invoke some of them directly.

| build argument | action | script file |
| --- | --- | --- |
| -restore | Restore packages. | restore.cmd |
| -build | Build all projects. | build.cmd |
| -test | Build and run all tests. | test.cmd |
| -pack | Build and produce NuGet packages. | None |

## Using Visual Studio

**Important** The command line `build` (see above) must be run before using the solution with Visual Studio.

The build script installs a preview .NET Core SDK. In order to make sure Visual studio (or any other IDE) is using same SDK, certain environment variables need to be set. To configure your local environment and open solution file in Visual Studio, run following command:

```console
startvs.cmd EFCore.slnf
```

You can inspect the script and use similar configuration for other IDEs.

Note that `startvs` actually opens whatever the default application is for `.sln` files. If you have multiple IDEs or multiple versions of Visual Studio installed, then make sure that the appropriate default is set, or edit the script to be explicit.

### Run tests

Our tests are written using [xUnit.net](http://xunit.github.io/), and can be run with most test runners we have tried.
Tests can be run on the command line (after build) by running `test`:

```console
test
```

***

# Earlier versions (2.x or older)

## Prerequisites

The following prerequisites are required to work with the EF Core code:

* [**Visual Studio 2017 15.8**](https://www.visualstudio.com/downloads/)
* **.NET Core 2.2 Preview SDK** (Installed by build script automatically)

## Clone the repository

Using your favorite [git](http://git-scm.com/) client, clone the repository.

```console
git clone https://github.com/dotnet/efcore.git
```

## Build initialization

Run at least the following before using Visual Studio for the first time:

```console
build /t:Compile
```

If you get a ⚠️ warning about KoreBuild using different dotnet than the one which exists in PATH variable, then you need to update your PATH variable such that dotnet installed by KoreBuild in `%USERPROFILE%` appears before any other dotnet installed. Only after that opening solution in Visual Studio will work correctly. See Updating PATH to use preview .NET Core SDK under Build from Visual Studio for more information on steps.

## Build from the command line

To build and run tests:

```console
build
```

To build and create packages without running tests:

```console
build /t:Package
```

## Build from Visual Studio

* Just open the .sln file. (After running the build initialization from the command line and updating your Path if needed.)
* NuGet packages will already have been restored, so you can switch off auto package restore in Visual Studio.

### Updating PATH to use preview .NET Core SDK

* Since KoreBuild installs preview version of .NET Core SDK in `%USERPROFILE%` folder, your path needs to use that dotnet for VS to work correctly. Especially the preview version of dotnet should appear before any other dotnet in your PATH variable. You can either set your environment variables globally and just open solution in VS normally or you can set PATH variable locally in command line and open solution from command line.

Following are steps to update PATH locally & open solution in VS in PowerShell. Similar steps can be used in any other command terminal. (drop `/x64` if you are on 32 bit machine)

```PowerShell
.\build /t:Compile
$env:PATH="$env:USERPROFILE/.dotnet/x64;"+$env:PATH
.\EFCore.sln
```

### Solving common build errors

1. Check that the package source URLs listed in the Nuget.config file in the root of the repository are accessible.
2. Clean the source directory. `git clean -xid` will clean files in the EF source directory. 
3. Clear nuget packages and caches. `nuget.exe locals all -clear` will delete the NuGet caches. (You can get nuget.exe from <https://dist.nuget.org/index.html> or use `dotnet nuget`).
4. Reinstall .NET Core CLI. Our build script automatically installs a version to `%USERPROFILE%\.dotnet`.

### Run tests

Our tests are written using [xUnit.net 2.0](http://xunit.github.io/), and can be run with [TestDriven.Net](http://www.testdriven.net/). You may need to [tweak the TestDriven.NET installation](https://github.com/jcansdale/TestDriven.Net-Issues/issues/76#issuecomment-288583932) to get it working with the latest VS.

Other test runners may also work--people have had luck with the ReSharper test runner and the CodeRush runner.

### ReSharper

Some people on the team use [ReSharper](https://www.jetbrains.com/resharper/download/) to increase productivity. Currently the EAP builds of the latest release seem to be working well. Some people see better performance if ReSharper testing support is disabled.
