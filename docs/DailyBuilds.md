# Daily Builds

## Quick-start

Create a file called "NuGet.config" with the following contents and put it next to your solution or csproj file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="dotnet7" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet7/nuget/v3/index.json" />
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

For EF7 daily builds, `NuGet.config` should contain:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="dotnet7" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet7/nuget/v3/index.json" />
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    </packageSources>
</configuration>
```

## Package versions to use

### Using wildcards

The easiest way to use daily builds is with wildcards in project references. For example, for EF Core 6.0 daily builds:

```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="7.0.0-*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="7.0.0-*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.0.0-*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite" Version="7.0.0-*" />
  </ItemGroup>
```

Using wildcards will cause NuGet to pull the latest daily build whenever packages are restored.

### Using an explicit version

You can use your IDE to choose the latest version. For example, in Visual Studio:

![image](https://user-images.githubusercontent.com/1430078/92644977-01108780-f299-11ea-897e-bb8e9705ada7.png)

Alternately, your IDE might provide auto-completion directly in the .csproj file:

![image](https://user-images.githubusercontent.com/1430078/92645046-1d142900-f299-11ea-9e40-c2b1fe1f61c1.png)

## What about Visual Studio and the SDK?

EF7 currently targets .NET 6. This means that:

* Your application must target .NET 6 or later; .NET Framework, .NET Core 3.1, and .NET 5 are no longer supported targets.
* The daily builds should work with any IDE that supports .NET 6.
* The daily builds require that the .NET 6 SDK is installed.

## Troubleshooting

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
        <add key="dotnet7" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet7/nuget/v3/index.json" />
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
        <add key="dotnet7" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet7/nuget/v3/index.json" />
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    </packageSources>
</configuration>
```
