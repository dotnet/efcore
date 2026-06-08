# AI Triage

The below is an AI-generated analysis and may contain inaccuracies.

## Summary

This does not look like a new bug in owned-type mapping. The repro assigns the same `CustomerName` instance to two different owned navigations:

- `Order.OrdererName`
- `Order.AccountDetail.CustomerName`

For owned entity types, the defining navigation is part of the owned type identity, and instances of owned entity types cannot be shared by multiple owners/navigations. Because of that, EF Core throws while tracking the graph.

I reproduced this locally with the issue's model on **EF Core 10.0.7** and **SQLite**. The exception is thrown on `db.Add(order)` exactly as reported. When I changed the repro to use two separate `CustomerName` instances with the same shape, the exception no longer occurred.

## Classification

- **Issue type:** Task / duplicate follow-up rather than a new bug
- **Area labels:** `area-owned-entities`, `area-change-tracking`
- **Provider scope:** not provider-specific; no provider label needed
- **Security:** no security concern found

## Regression

I do not think this is a regression.

This appears to be the same long-standing owned-entity limitation already documented by EF Core and discussed in older issues:

- [#12345](https://github.com/dotnet/efcore/issues/12345) - sharing owned entity instances is not supported
- [#31540](https://github.com/dotnet/efcore/issues/31540) - improve the exception message for this scenario
- [#37251](https://github.com/dotnet/efcore/issues/37251) - same exception pattern, closed as duplicate

The EF docs also explicitly say: *"Instances of owned entity types cannot be shared by multiple owners"*.

## Guidance / workaround

If `CustomerName` is intended to behave as a value object, the immediate workaround is to avoid reusing the same object instance across owned navigations; create separate instances instead.

If shared-instance/value-object semantics are desired, complex types are likely the better fit than owned entity types.

## Minimal repro

<details>
<summary>minimal repro</summary>

```csharp
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;

var connection = new SqliteConnection("Filename=:memory:");
connection.Open();

var options = new DbContextOptionsBuilder<MyDbContext>()
    .UseSqlite(connection)
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging()
    .Options;

using var db = new MyDbContext(options);
db.Database.EnsureCreated();

CustomerName customerName = new();
AccountDetail accountInfo = new() { CustomerName = customerName };
Order order = new() { OrdererName = customerName, OrderItems = [new OrderItem()], AccountDetail = accountInfo };

db.Add(order);

public class OrderItem
{
}

public record CustomerName;

public record AccountDetail
{
    public CustomerName CustomerName { get; set; } = null!;
}

public class Order
{
    public AccountDetail AccountDetail { get; set; } = null!;
    public CustomerName OrdererName { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}

public class OrderTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property<Guid>("Id");
        builder.OwnsOne(order => order.OrdererName);
        builder.OwnsOne(order => order.AccountDetail, accountDetail => accountDetail.OwnsOne(a => a.CustomerName));
        builder.OwnsMany(order => order.OrderItems);
    }
}

public class MyDbContext(DbContextOptions<MyDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => new OrderTypeConfiguration().Configure(modelBuilder.Entity<Order>());
}
```

</details>

## Suggested final disposition

Close as duplicate of [#31540](https://github.com/dotnet/efcore/issues/31540) (and/or reference [#12345](https://github.com/dotnet/efcore/issues/12345) for the underlying by-design limitation).
