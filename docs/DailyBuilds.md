# Daily Builds

## Quick-start

Create a file called "NuGet.config" with the following contents and put it next to your solution or csproj file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="dotnet9" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json" />
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    </packageSources>
</configuration>
```

Continue reading for full details and troubleshooting.

## Types of builds

Daily builds are created automatically whenever a new commit is merged to the `main` branch. These builds are verified by more than 80,000 tests each running on a range of platforms. These builds are reliable and have significant advantages over using previews:

* Previews typically lag behind daily builds by around three to five weeks. This means that each preview is missing many bug fixes and enhancements, even if you get it on the day it is released. The daily builds always have the latest features and bug fixes.
* Serious bugs are usually fixed and available in a new daily build within one or two days--sometimes less. The same fix will likely not make a new preview/release for weeks.
* You are able to provide feedback immediately on any change we make, which makes it more likely we will be able to take action on this feedback before the change is baked in.

A disadvantage of using daily builds is that there can be significant API churn for new features. However, this should only be an issue if you're trying out new features as they are being developed.

## Package sources

The daily builds are not published to NuGet.org because the .NET build infrastructure is not set up for this. Instead they can be pulled from a custom NuGet package source. To access this custom source, create a `NuGet.config` file in the same directory as your .NET solution or projects.

For EF8 daily builds, `NuGet.config` should contain:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="dotnet9" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json" />
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    </packageSources>
</configuration>
```

### The EF command-line tool

`dotnet ef` is the [the EF command-line tool](https://learn.microsoft.com/ef/core/cli/dotnet), used to perform various design-time tasks such as creating and applying migrations. Stable versions of `dotnet ef` usually work fine with daily build versions of EF; but in some situations you must also update to daily builds of the CLI tool. To use a daily build version of `dotnet ef`, do the following:

```sh
dotnet tool install -g dotnet-ef --version 9.0.0-* --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json
```

### EF reverse engineering templates

EF features code templates for [reverse engineering (or "scaffolding") existing databases](https://learn.microsoft.com/ef/core/managing-schemas/scaffolding/templates); installing daily versions of these templates typically isn't necessary, but you may want to do so to experiment with new features or test bug fixes in the templates:

```sh
dotnet new install Microsoft.EntityFrameworkCore.Templates::9.0.0-* --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json
```

## Package versions to use

### Using wildcards

The easiest way to use daily builds is with wildcards in project references. For example, for EF Core 8.0 daily builds:

```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0-*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0-*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0-*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite" Version="9.0.0-*" />
  </ItemGroup>
```

Using wildcards will cause NuGet to pull the latest daily build whenever packages are restored.

### Using an explicit version

You can use your IDE to choose the latest version. For example, in Visual Studio:

![Pick the daily build to use inside your IDE.](https://github.com/dotnet/efcore/assets/1430078/925aebff-fc88-4812-8cab-a3c4c29e8b94)

Alternately, your IDE might provide auto-completion directly in the .csproj file:

![Use auto-completion in the csproj file, if supported by your IDE.](https://user-images.githubusercontent.com/1430078/92645046-1d142900-f299-11ea-9e40-c2b1fe1f61c1.png)

## What about Visual Studio and the SDK?

EF8 targets .NET 8. This means that:

* Your application must target .NET 8 or later; .NET Framework and .NET 6 and earlier are no longer supported targets.
* The daily builds should work with any IDE that supports .NET 8.
* The daily builds require that the .NET 8 SDK is installed.

## Troubleshooting

### VS isn't showing the new packages

If you can't see the daily build packages after adding the NuGet.config file to your solution, make sure that the "Package Source" is set to "All" in the VS Package Manager UI. You may also need to reload your project or restart Visual Studio for the packages to appear.

### Missing packages

The config files shown above contain the two NuGet package sources needed for EF Core and its dependencies. However, you may need to add additional package sources for daily builds of other projects, or your own internal packages.

In addition, packages may be missing if the standard `nuget.org` package source has been disabled elsewhere; adding the source in this config will not bring it back. To fix this, either don't disable `nuget.org` anywhere, or tell NuGet to ignore previously disabled sources:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <disabledPackageSources>
        <clear />
    </disabledPackageSources>
    <packageSources>
        <add key="dotnet9" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json" />
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    </packageSources>
</configuration>
```

A good way to ensure you're dealing with a completely clean NuGet configuration is to clear both disabled package sources _and_ previously configured package sources. For example:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <disabledPackageSources>
        <clear />
    </disabledPackageSources>
    <packageSources>
        <clear />
        <add key="dotnet9" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json" />
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    </packageSources>
</configuration>
```
