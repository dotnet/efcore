// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class CosmosConcurrencyTest(CosmosConcurrencyTest.CosmosFixture fixture) : IClassFixture<CosmosConcurrencyTest.CosmosFixture>
{
    private const string DatabaseName = "CosmosConcurrencyTest";

    protected CosmosFixture Fixture { get; } = fixture;

    [ConditionalFact]
    public virtual Task Adding_the_same_entity_twice_results_in_DbUpdateException()
        => ConcurrencyTestAsync<DbUpdateException>(
            ctx =>
            {
                ctx.Customers.Add(
                    new Customer
                    {
                        Id = "1", Name = "CreatedTwice",
                    });
                return Task.CompletedTask;
            });

    [ConditionalFact]
    public virtual Task Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException()
        => ConcurrencyTestAsync<DbUpdateConcurrencyException>(
            ctx =>
            {
                ctx.Customers.Add(
                    new Customer
                    {
                        Id = "2", Name = "Added",
                    });
                return Task.CompletedTask;
            }, async ctx => (await ctx.Customers.SingleAsync(c => c.Id == "2")).Name = "Updated",
            async ctx => ctx.Customers.Remove(await ctx.Customers.SingleAsync(c => c.Id == "2")));

    [ConditionalFact]
    public virtual Task Updating_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException()
        => ConcurrencyTestAsync<DbUpdateConcurrencyException>(
            ctx =>
            {
                ctx.Customers.Add(
                    new Customer
                    {
                        Id = "3", Name = "Added",
                    });
                return Task.CompletedTask;
            }, async ctx => (await ctx.Customers.SingleAsync(c => c.Id == "3")).Name = "Updated",
            async ctx => (await ctx.Customers.SingleAsync(c => c.Id == "3")).Name = "Updated");

    [ConditionalTheory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Etag_is_updated_in_entity_after_SaveChanges(bool? contentResponseOnWriteEnabled)
    {
        var options = new DbContextOptionsBuilder(Fixture.CreateOptions())
            .UseCosmos(
                o =>
                {
                    if (contentResponseOnWriteEnabled != null)
                    {
                        o.ContentResponseOnWriteEnabled(contentResponseOnWriteEnabled.Value);
                    }
                })
            .Options;

        var customer = new Customer
        {
            Id = "5",
            Name = "Theon",
            Children = { new DummyChild { Id = "0" } }
        };

        string etag = null;
        await using (var context = new ConcurrencyContext(options))
        {
            await Fixture.TestStore.CleanAsync(context);

            await context.AddAsync(customer);

            await context.SaveChangesAsync();

            etag = customer.ETag;
        }

        await using (var context = new ConcurrencyContext(options))
        {
            var customerFromStore = await context.Set<Customer>().SingleAsync();

            Assert.NotEmpty(customerFromStore.ETag);
            Assert.Equal(etag, customerFromStore.ETag);

            customerFromStore.Children.Add(new DummyChild { Id = "1" });

            await context.SaveChangesAsync();

            Assert.NotEmpty(customerFromStore.ETag);
            Assert.NotEqual(etag, customerFromStore.ETag);

            customerFromStore.Children.Add(new DummyChild { Id = "2" });

            Assert.NotEmpty(customerFromStore.ETag);
            Assert.NotEqual(etag, customerFromStore.ETag);

            customerFromStore.Children.Add(new DummyChild { Id = "3" });

            await context.SaveChangesAsync();

            Assert.NotEmpty(customerFromStore.ETag);
            Assert.NotEqual(etag, customerFromStore.ETag);
        }
    }

    /// <summary>
    ///     Runs the two actions with two different contexts and calling
    ///     SaveChanges such that storeChange will succeed and the store will reflect this change, and
    ///     then clientChange will result in a concurrency exception.
    ///     After the exception is caught the resolver action is called, after which SaveChanges is called
    ///     again. Finally, a new context is created and the validator is called so that the state of
    ///     the database at the end of the process can be validated.
    /// </summary>
    protected virtual Task ConcurrencyTestAsync<TException>(
        Func<ConcurrencyContext, Task> change)
        where TException : DbUpdateException
        => ConcurrencyTestAsync<TException>(
            null, change, change);

    /// <summary>
    ///     Runs the two actions with two different contexts and calling
    ///     SaveChanges such that storeChange will succeed and the store will reflect this change, and
    ///     then clientChange will result in a concurrency exception.
    ///     After the exception is caught the resolver action is called, after which SaveChanges is called
    ///     again. Finally, a new context is created and the validator is called so that the state of
    ///     the database at the end of the process can be validated.
    /// </summary>
    protected virtual async Task ConcurrencyTestAsync<TException>(
        Func<ConcurrencyContext, Task> seedAction,
        Func<ConcurrencyContext, Task> storeChange,
        Func<ConcurrencyContext, Task> clientChange)
        where TException : DbUpdateException
    {
        using var outerContext = CreateContext();
        await Fixture.TestStore.CleanAsync(outerContext);

        if (seedAction != null)
        {
            await seedAction(outerContext);
        }

        await outerContext.SaveChangesAsync();

        if (clientChange != null)
        {
            await clientChange(outerContext);
        }

        using (var innerContext = CreateContext())
        {
            if (storeChange != null)
            {
                await storeChange(innerContext);
            }

            await innerContext.SaveChangesAsync();
        }

        var updateException =
            await Assert.ThrowsAnyAsync<TException>(() => outerContext.SaveChangesAsync());

        var entry = updateException.Entries.Single();
        Assert.IsAssignableFrom<Customer>(entry.Entity);
    }

    protected ConcurrencyContext CreateContext()
        => Fixture.CreateContext();

    public class CosmosFixture : SharedStoreFixtureBase<ConcurrencyContext>
    {
        protected override string StoreName
            => DatabaseName;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }

    public class ConcurrencyContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
            => builder.Entity<Customer>(
                b =>
                {
                    b.HasKey(c => c.Id);
                    b.Property(c => c.ETag).IsETagConcurrency();
                    b.OwnsMany(x => x.Children);
                });
    }

    public class Customer
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ETag { get; set; }

        public ICollection<DummyChild> Children { get; } = new HashSet<DummyChild>();
    }

    public class DummyChild
    {
        public string Id { get; init; }
    }
}
