The Entity Framework Core tools help with design-time development tasks. They're primarily used to manage Migrations and to scaffold a `DbContext` and entity types by reverse engineering the schema of a database.

This package, `dotnet-ef` is for cross-platform command line tooling that can be used anywhere.

## Usage

Install the tool package using:

```dotnetcli
dotnet tool install --global dotnet-ef
```

The available commands are listed in the following table.

| Command                                                                                                                                         | Usage                                                                       |
|-------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------|
| [dotnet ef --help](https://learn.microsoft.com/ef/core/cli/dotnet#common-options)                                                               | Displays information about Entity Framework commands.                       |
| [dotnet ef database drop](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-database-drop)                                               | Drops the database.                                                         |
| [dotnet ef database update](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-database-update)                                           | Updates the database to the last migration or to a specified migration      |
| [dotnet ef dbcontext info](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-dbcontext-info)                                             | Gets information about a `DbContext` type.                                  |
| [dotnet ef dbcontext list](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-dbcontext-list)                                             | Lists available `DbContext` types.                                          |
| [dotnet ef dbcontext optimize](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-dbcontext-optimize)                                     | Generates a compiled version of the model used by the `DbContext`.          |
| [dotnet ef dbcontext scaffold](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-dbcontext-scaffold)                                     | Generates a `DbContext` and entity type classes for a specified database.   |
| [dotnet ef dbcontext script](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-dbcontext-script)                                         | Generates a SQL script from the `DbContext`. Bypasses any migrations.       |
| [dotnet ef migrations add](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-migrations-add)                                             | Adds a new migration.                                                       |
| [dotnet ef migrations bundle](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-migrations-bundle)                                       | Creates an executable to update the database.                               |
| [dotnet ef migrations has-pending-model-changes](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-migrations-has-pending-model-changes) | Checks if any changes have been made to the model since the last migration. |
| [dotnet ef migrations list](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-migrations-list)                                           | Lists available migrations.                                                 |
| [dotnet ef migrations remove](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-migrations-remove)                                       | Removes the last migration.                                                 |
| [dotnet ef migrations script](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-migrations-script)                                       | Generates a SQL script from the migrations.                                 |

## Getting started with EF Core

See [Getting started with EF Core](https://learn.microsoft.com/ef/core/get-started/overview/install) for more information about EF NuGet packages, including which to install when getting started.

## Feedback

If you encounter a bug or issues with this package,you can [open an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).