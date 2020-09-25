# Daily Builds

Daily builds are created automatically whenever a new commit is merged to the `main` branch. These builds are verified by more than 70,000 tests each running on a range of platforms. These builds are reliable and have significant advantages over using previews:

* Previews typically lag behind daily builds by around three to five weeks. This means that each preview is missing many bug fixes and enhancements, even if you get it on the day it is released. The daily builds always have the latest features and bug fixes.
* Serious bugs are usually fixed and available in a new daily build within one or two days--sometimes less. The same fix will likely not make a new preview/release for weeks.
* You are able to provide feedback immediately on any change we make, which makes it more likely we will be able to take action on this feedback before the change is baked in.

A disadvantage of using daily builds is that there can be significant API churn for new features. However, this should only be an issue if you're trying out new features as they are being developed.

## Using daily builds

The daily builds are not published to NuGet.org because the .NET build infrastructure is not set up for this. Instead they can be pulled from a custom NuGet feed. To access this feed, create a `NuGet.config` file in the same directory as your .NET solution or projects. The file should contain the following content:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <clear />
        <add key="dotnet6" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet6/nuget/v3/index.json" />
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    </packageSources>
</configuration>
```

## Which version to use

Daily builds are currently branded as EF Core 6.0. For example, `6.0.0-alpha.1.20457.2`. This is an artifact of the build system; **these builds still contain the bits what we plan to ship as EF Core 5.0**.

### Using wildcards

The easiest way to use daily builds is with wildcards in project references. For example:

```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.0-*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="6.0.0-*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.0-*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite" Version="6.0.0-*" />
  </ItemGroup>
```

This will cause NuGet to pull the latest daily build whenever packages are restored.

### Using an explicit version

You can use your IDE to choose the latest version. For example, in Visual Studio:

![image](https://user-images.githubusercontent.com/1430078/92644977-01108780-f299-11ea-897e-bb8e9705ada7.png)

Alternately, your IDE might provide auto-completion directly in the .csproj file:

![image](https://user-images.githubusercontent.com/1430078/92645046-1d142900-f299-11ea-9e40-c2b1fe1f61c1.png)

## What about Visual Studio and the SDK?

EF Core 5.0 targets .NET Standard 2.1. This means that:

* Your application does not need to target .NET 5; .NET Core 3.1 is fine.
* The daily builds should work with any IDE that supports .NET Core 3.1.
  * They do not require a Visual Studio preview release, although previews will also work.
* The daily builds should work with either the .NET Core 3.1 SDK or the .NET 5 SDK installed.
