> ## Microsoft.EntityFrameworkCore.Tools 10.0.6 — Design package dependency change
>
> ### Summary
>
> Starting with version **10.0.6**, the `Microsoft.EntityFrameworkCore.Tools` NuGet package no longer pulls in a
> matching version of `Microsoft.EntityFrameworkCore.Design` as a transitive dependency. Instead, it declares
> a minimum dependency on `Microsoft.EntityFrameworkCore.Design` version **8.0.0**. This means NuGet may resolve
> an older, incompatible version of the Design package, causing EF Core tooling commands
> (`Add-Migration`, `Update-Database`, `dotnet ef migrations add`, etc.) to fail at runtime.
>
> ### Background and motivation
>
> In EF Core 10.0.5 and earlier, `Microsoft.EntityFrameworkCore.Tools` declared a dependency on
> `Microsoft.EntityFrameworkCore.Design` with a version matching the Tools package itself (e.g. `>= 10.0.5`).
> This meant installing the Tools package would automatically bring in the correct version of the Design package.
>
> However, the Tools package did not specify a target framework, while the Design package only targets `net10.0`.
> Some tooling (e.g. Dependabot, package analyzers) interpreted the lack of a target framework to mean
> the Tools package was compatible with all frameworks — including `net8.0` — and would suggest upgrading to
> `Microsoft.EntityFrameworkCore.Tools` 10.0.x for projects still targeting `net8.0`. Since the Design 10.0.x
> package only targets `net10.0`, this would cause restore failures for those projects.
>
> To fix this ([#37515](https://github.com/dotnet/efcore/issues/37515)), the minimum version of the Design
> dependency was lowered to **8.0.0** in the 10.0.6 release, and a `lib/net8.0/_._` marker was added to make the
> supported target frameworks explicit.
>
> ### Errors you may encounter
>
> If your project references `Microsoft.EntityFrameworkCore.Tools` 10.0.6 (or later) **without** an explicit,
> version-matched reference to `Microsoft.EntityFrameworkCore.Design`, NuGet may resolve an old version of the
> Design package. This results in runtime errors when executing EF Core commands:
>
> #### `System.MissingMethodException`
>
> ```
> System.MissingMethodException: Method not found: 'System.String
> Microsoft.EntityFrameworkCore.Diagnostics.AbstractionsStrings.ArgumentIsEmpty(System.Object)'.
>    at Microsoft.EntityFrameworkCore.Utilities.Check.NotEmpty(String value, String parameterName)
>    at Microsoft.EntityFrameworkCore.Design.OperationExecutor.AddMigrationImpl(String name, String outputDir,
>         String contextType, String namespace)
>    at Microsoft.EntityFrameworkCore.Design.OperationExecutor.AddMigration.<>c__DisplayClass0_0.<.ctor>b__0()
>    at Microsoft.EntityFrameworkCore.Design.OperationExecutor.OperationBase.<>c__DisplayClass3_0`1.b__0()
>    at Microsoft.EntityFrameworkCore.Design.OperationExecutor.OperationBase.Execute(Action action)
> Method not found: 'System.String Microsoft.EntityFrameworkCore.Diagnostics.AbstractionsStrings.ArgumentIsEmpty(System.Object)'.
> ```
>
> #### `System.TypeLoadException`
>
> ```
> System.TypeLoadException: Method 'Identifier' in type
> 'Microsoft.EntityFrameworkCore.Design.Internal.CSharpHelper' from assembly
> 'Microsoft.EntityFrameworkCore.Design, Version=8.0.0.0, Culture=neutral,
> PublicKeyToken=adb9793829ddae60' does not have an implementation.
> ```
>
> These errors occur because the old Design assembly (e.g. 8.0.0) is missing methods and interface
> implementations that the 10.0.x runtime code expects.
>
> ### How to fix
>
> Add an **explicit** `PackageReference` for `Microsoft.EntityFrameworkCore.Design` to the project that
> contains your `DbContext` and migrations. The version must match the version of the other
> `Microsoft.EntityFrameworkCore.*` packages in your project.
>
> #### Package Manager Console (Visual Studio)
>
> ```powershell
> Install-Package Microsoft.EntityFrameworkCore.Design -Version 10.0.6
> ```
>
> #### .NET CLI
>
> ```bash
> dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.0.6
> ```
>
> #### Manual `.csproj` edit
>
> ```xml
> <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.6">
>   <PrivateAssets>all</PrivateAssets>
>   <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
> </PackageReference>
> ```
>
> > **Note:** Replace `10.0.6` with whatever version of EF Core you are using. The important thing is that
> > the Design package version matches your other `Microsoft.EntityFrameworkCore.*` package versions.
>
> #### Central Package Management (`Directory.Packages.props`)
>
> If your solution uses [Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management),
> add or update the entry in your `Directory.Packages.props`:
>
> ```xml
> <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.6" />
> ```
>
> And ensure your project file has the `PackageReference` (without a version):
>
> ```xml
> <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
>   <PrivateAssets>all</PrivateAssets>
>   <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
> </PackageReference>
> ```
>
> ### Affected versions
>
> - `Microsoft.EntityFrameworkCore.Tools` **>= 10.0.6**
>
> ### Related issues
>
> - [#38107](https://github.com/dotnet/efcore/issues/38107) — `Add-Migration` throws `MissingMethodException` after upgrade to 10.0.6
> - [#38108](https://github.com/dotnet/efcore/issues/38108) — `CSharpHelper.Identifier` missing implementation (loads Design 8.0.0.0)
> - [#38123](https://github.com/dotnet/efcore/issues/38123) — Duplicate report of `MissingMethodException` after 10.0.6 upgrade
> - [#37515](https://github.com/dotnet/efcore/issues/37515) — Original issue: Tools package missing target framework
