Entity Framework (EF) Core s a .NET library that provides an object-relational mapping (ORM) framework for .NET developers to work with databases. It supports multiple database providers, including SQL Server, MySQL, PostgreSQL, SQLite, and others.

## Getting started

### Prerequisites

Make sure to install the same version of all EF Core packages shipped by Microsoft. For example, if version 5.0.3 of Microsoft.EntityFrameworkCore.SqlServer is installed, then all other Microsoft.EntityFrameworkCore.* packages must also be at 5.0.3.

## Usage

To use Microsoft.EntityFrameworkCore in your application, you will typically need to create a class that inherits from DbContext, which represents your database context. You can then define classes that represent your database entities, and use LINQ queries to interact with the database.

Here's an example of how you might define a database context and an entity:

```c#
using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

You can then use the MyDbContext class to interact with the database:

```c#
using (var context = new MyDbContext())
{
    // Add a new customer
    context.Customers.Add(new Customer { Name = "John Doe" });
    context.SaveChanges();

    // Retrieve all customers
    var customers = context.Customers.ToList();
}
```

Microsoft.EntityFrameworkCore supports multiple database providers, including SQL Server, MySQL, PostgreSQL, SQLite, and others. You will need to install the provider package for your chosen database. For example, to use SQL Server, you would install the [Microsoft.EntityFrameworkCore.SqlServer package](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/7.0.3).

You would then configure your database context to use the SQL Server provider:

```c#
using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MyDatabase;");
    }

    public DbSet<Customer> Customers { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

## Additional documentation

- [Getting Started with Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli).
- Follow the [ASP.NET Core Tutorial](https://learn.microsoft.com/en-us/aspnet/core/data/ef-rp/intro?view=aspnetcore-7.0&tabs=visual-studio) to use EF Core in a web app.
- [Releases and planning(roadmap)](https://learn.microsoft.com/en-us/ef/core/what-is-new/)
- [How to write an EF Core provider](https://learn.microsoft.com/en-us/ef/core/providers/writing-a-provider)

## Feedback

If you have a specific question about using these projects, we encourage you to ask it on [Stack Overflow](https://stackoverflow.com/questions/tagged/entity-framework-core+or+entity-framework-core-2.1+or+entity-framework-core-3.1). If you encounter a bug or would like to request a feature, [submit an Github issue](https://github.com/dotnet/efcore/issues/new/choose). For more details, see [getting support](https://github.com/dotnet/efcore/blob/main/.github/SUPPORT.md).