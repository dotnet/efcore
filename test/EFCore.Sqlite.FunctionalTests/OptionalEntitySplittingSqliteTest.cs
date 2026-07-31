// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

// Sqlite doesn't implement the conditional upsert/delete required to write optional entity-splitting fragments;
// these tests verify that this is surfaced as a clear provider-capability error rather than silently wrong SQL,
// and that operations which never need to touch the optional fragment (e.g. inserting/deleting an all-null
// fragment) keep working, since the "should we touch this fragment at all" decision is provider-neutral.
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

            // No exception: the optional-fragment INSERT is skipped entirely (provider-neutral decision)
            // because every payload value mapped to it is null.
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
    public async Task Update_of_optional_fragment_throws_provider_capability_error()
    {
        var contextFactory = await InitializeContextAsync();

        await using var context = contextFactory.CreateDbContext();
        context.Customers.Add(new Customer { Id = 1, Name = "Alice" });
        await context.SaveChangesAsync();

        var customer = await context.Customers.SingleAsync();
        customer.Description = "Some details";

        var exception = await Assert.ThrowsAsync<NotSupportedException>(() => context.SaveChangesAsync());
        Assert.Equal(RelationalStrings.OptionalEntitySplittingNotSupported, exception.Message);
    }

    [Fact]
    public async Task Delete_of_entity_with_optional_fragment_throws_provider_capability_error()
    {
        var contextFactory = await InitializeContextAsync();

        await using var context = contextFactory.CreateDbContext();
        context.Customers.Add(new Customer { Id = 1, Name = "Alice" });
        await context.SaveChangesAsync();

        var customer = await context.Customers.SingleAsync();
        context.Customers.Remove(customer);

        var exception = await Assert.ThrowsAsync<NotSupportedException>(() => context.SaveChangesAsync());
        Assert.Equal(RelationalStrings.OptionalEntitySplittingNotSupported, exception.Message);
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
