# AI Triage

The below is an AI-generated analysis and may contain inaccuracies.

Issue [#39073](https://github.com/dotnet/efcore/issues/39073) still reproduces on `main` at commit `980c0d2` (`Update XUnitV3Version to 4.0.1 (#39079)`).

Running the minimal repro against the repository's current `EFCore.SqlServer` project throws the reported `ArgumentOutOfRangeException` from `InternalComplexCollectionEntry.GetCollection(Boolean original)` while `DbContext.Update` marks the tracked graph as modified.

Therefore, no PR has fixed this issue on `main`.

The related fixes do not cover this state transition:

- [#37702](https://github.com/dotnet/efcore/pull/37702) fixed [#37585](https://github.com/dotnet/efcore/issues/37585), where accepting changes after `SaveChanges` failed after deleting an element from a nested complex collection.
- [#37729](https://github.com/dotnet/efcore/pull/37729) fixed [#37724](https://github.com/dotnet/efcore/issues/37724), where changing an entity with complex collections from `Deleted` to `Unchanged` failed.
- #39073 instead fails immediately when `Update` is called on an already tracked entity after its outer complex collection has become shorter.

This is a provider-independent change-tracking bug. Suggested classification:

- Type: `Bug`
- Labels: `customer-reported`, `area-change-tracking`, `area-complex-types`

<details>
<summary>minimal repro</summary>

```csharp
using Microsoft.EntityFrameworkCore;

using var context = new AppContext();
var order = new Order { Id = 1, Lines = [new Line { Name = "a" }, new Line { Name = "b" }] };
context.Attach(order);
order.Lines = [new Line { Name = "a" }];
context.Update(order);

public class AppContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Server=unused;Database=unused");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Order>().ComplexCollection(
            o => o.Lines,
            lines =>
            {
                lines.ToJson();
                lines.ComplexCollection(l => l.Parts);
            });
}

public class Order
{
    public int Id { get; set; }
    public List<Line> Lines { get; set; } = [];
}

public class Line
{
    public string Name { get; set; } = "";
    public List<Part> Parts { get; set; } = [];
}

public class Part
{
    public string Code { get; set; } = "";
}
```

</details>
