The Entity Framework Core tools help with design-time development tasks. They're primarily used to manage Migrations and to scaffold a `DbContext` and entity types by reverse engineering the schema of a database.

This package, `Microsoft.EntityFrameworkCore.Tools` is for PowerShell tooling that works in the Visual Studio Package Manager Console (PMC).

## Usage

Install the tools package by running the following in the Visual Studio PMC:

```powershell
Install-Package Microsoft.EntityFrameworkCore.Tools
```

The available commands are listed in the following table.

| PMC Command                                                                                            | Usage                                                                     |
|--------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------|
| [Add-Migration](https://learn.microsoft.com/ef/core/cli/powershell#add-migration)                      | Adds a new migration.                                                     |
| [Bundle-Migration](https://learn.microsoft.com/ef/core/cli/powershell#bundle-migration)                | Creates an executable to update the database.                             |
| [Drop-Database](https://learn.microsoft.com/ef/core/cli/powershell#drop-database)                      | Drops the database.                                                       |
| [Get-DbContext](https://learn.microsoft.com/ef/core/cli/powershell#get-dbcontext)                      | Gets information about a `DbContext` type.                                |
| [Get-Help EntityFramework](https://learn.microsoft.com/en-us/ef/core/cli/powershell#common-parameters) | Displays information about Entity Framework commands.                     |
| [Get-Migration](https://learn.microsoft.com/ef/core/cli/powershell#get-migration)                      | Lists available migrations.                                               |
| [Optimize-DbContext](https://learn.microsoft.com/ef/core/cli/powershell#optimize-dbcontext)            | Generates a compiled version of the model used by the `DbContext`.        |
| [Remove-Migration](https://learn.microsoft.com/ef/core/cli/powershell#remove-migration)                | Removes the last migration.                                               |
| [Scaffold-DbContext](https://learn.microsoft.com/ef/core/cli/powershell#scaffold-dbcontext)            | Generates a `DbContext` and entity type classes for a specified database. |
| [Script-DbContext](https://learn.microsoft.com/ef/core/cli/powershell#script-dbcontext)                | Generates a SQL script from the `DbContext`. Bypasses any migrations.     |
| [Script-Migration](https://learn.microsoft.com/ef/core/cli/powershell#script-migration)                | Generates a SQL script from the migrations.                               |
| [Update-Database](https://learn.microsoft.com/ef/core/cli/powershell#update-database)                  | Updates the database to the last migration or to a specified migration.   |

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).


