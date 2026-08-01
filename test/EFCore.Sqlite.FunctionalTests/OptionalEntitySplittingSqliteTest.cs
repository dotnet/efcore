// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

// Whether an optional entity-splitting fragment's row needs to be inserted, updated, or left alone when the entity
// is deleted is decided from the entry's tracked original values rather than from provider-specific SQL, so these
// scenarios work identically on every relational provider, including ones like Sqlite that have no special support
// for conditional upserts/deletes.
public class OptionalEntitySplittingSqliteTest : NonSharedModelTestBase, IClassFixture<NonSharedFixture>
{
    public OptionalEntitySplittingSqliteTest(NonSharedFixture fixture)
        : base(fixture)
    {
    }

    protected override string NonSharedStoreName
        => "OptionalEntitySplittingSaveChangesTest";

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    private static void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Customer>(b =>
        {
            b.ToTable("Customers");
            b.Property(c => c.Id).ValueGeneratedNever();
            b.SplitToTable(
                "CustomerDetails", t =>
                {
                    t.IsOptional();
                    t.Property(c => c.Description);
                });
        });

    private async Task<ContextFactory<CustomerContext>> InitializeContextAsync()
        => await InitializeNonSharedTest<CustomerContext>(OnModelCreating);

    [Fact]
    public async Task Insert_with_all_null_optional_payload_does_not_touch_fragment_table()
    {
        var contextFactory = await InitializeContextAsync();

        await using (var context = contextFactory.CreateDbContext())
        {
            context.Customers.Add(new Customer { Id = 1, Name = "Alice" });

            // No exception: the optional-fragment INSERT is skipped entirely because every payload value mapped
            // to it is null.
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var customer = await context.Customers.SingleAsync();
            Assert.Equal("Alice", customer.Name);
            Assert.Null(customer.Description);
        }
    }

    [Fact]
    public async Task Insert_with_non_null_optional_payload_inserts_fragment_row()
    {
        var contextFactory = await InitializeContextAsync();

        await using (var context = contextFactory.CreateDbContext())
        {
            context.Customers.Add(new Customer { Id = 1, Name = "Alice", Description = "Some details" });

            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var customer = await context.Customers.SingleAsync();
            Assert.Equal("Some details", customer.Description);
        }
    }

    [Fact]
    public async Task Setting_previously_absent_optional_fragment_inserts_row()
    {
        var contextFactory = await InitializeContextAsync();

        await using (var context = contextFactory.CreateDbContext())
        {
            context.Customers.Add(new Customer { Id = 1, Name = "Alice" });
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var customer = await context.Customers.SingleAsync();
            customer.Description = "Some details";

            // The fragment row was never inserted, so this is treated as an INSERT rather than an UPDATE.
            // If it were sent as an UPDATE, it would affect zero rows and throw DbUpdateConcurrencyException.
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var customer = await context.Customers.SingleAsync();
            Assert.Equal("Some details", customer.Description);
        }
    }

    [Fact]
    public async Task Updating_previously_present_optional_fragment_updates_row()
    {
        var contextFactory = await InitializeContextAsync();

        await using (var context = contextFactory.CreateDbContext())
        {
            context.Customers.Add(new Customer { Id = 1, Name = "Alice", Description = "Some details" });
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var customer = await context.Customers.SingleAsync();
            customer.Description = "Other details";

            // The fragment row already exists, so this must be sent as an UPDATE. If it were sent as an INSERT,
            // it would violate the primary key and throw.
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var customer = await context.Customers.SingleAsync();
            Assert.Equal("Other details", customer.Description);
        }
    }

    [Fact]
    public async Task Deleting_entity_with_absent_optional_fragment_completes_without_error()
    {
        var contextFactory = await InitializeContextAsync();

        await using (var context = contextFactory.CreateDbContext())
        {
            context.Customers.Add(new Customer { Id = 1, Name = "Alice" });
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var customer = await context.Customers.SingleAsync();
            context.Customers.Remove(customer);

            // The fragment row was never inserted, so its DELETE is skipped. If it were sent, it would affect
            // zero rows and throw DbUpdateConcurrencyException.
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            Assert.Empty(await context.Customers.ToListAsync());
        }
    }

    [Fact]
    public async Task Deleting_entity_with_present_optional_fragment_deletes_row()
    {
        var contextFactory = await InitializeContextAsync();

        await using (var context = contextFactory.CreateDbContext())
        {
            context.Customers.Add(new Customer { Id = 1, Name = "Alice", Description = "Some details" });
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            var customer = await context.Customers.SingleAsync();
            context.Customers.Remove(customer);

            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateDbContext())
        {
            Assert.Empty(await context.Customers.ToListAsync());
        }
    }

    protected class CustomerContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }
    }

    protected class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
