// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class PartitionKeyTest : IClassFixture<PartitionKeyTest.CosmosPartitionKeyFixture>
{
    private const string DatabaseName = nameof(PartitionKeyTest);

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    protected CosmosPartitionKeyFixture Fixture { get; }

    public PartitionKeyTest(CosmosPartitionKeyFixture fixture)
    {
        Fixture = fixture;
        ClearLog();
    }

    [ConditionalFact]
    public virtual async Task Can_add_update_delete_end_to_end_with_partition_key()
    {
        const string readSql =
            """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["PartitionKey"]
OFFSET 0 LIMIT 1
""";

        await PartitionKeyTestAsync(
            ctx => ctx.Customers.OrderBy(c => c.PartitionKey).FirstAsync(),
            readSql,
            ctx => ctx.Customers.OrderBy(c => c.PartitionKey).LastAsync(),
            ctx => ctx.Customers.OrderBy(c => c.PartitionKey).ToListAsync(),
            2);
    }

    [ConditionalFact]
    public virtual async Task Can_add_update_delete_end_to_end_with_with_partition_key_extension()
    {
        const string readSql =
            """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
OFFSET 0 LIMIT 1
""";

        await PartitionKeyTestAsync(
            ctx => ctx.Customers.WithPartitionKey("1").FirstAsync(),
            readSql,
            ctx => ctx.Customers.WithPartitionKey("2").LastAsync(),
            ctx => ctx.Customers.WithPartitionKey("2").ToListAsync(),
            1);
    }

    [ConditionalFact]
    public async Task Can_query_with_implicit_partition_key_filter()
    {
        const string readSql =
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((c["Id"] = 42) OR (c["Name"] = "John Snow")))
OFFSET 0 LIMIT 1
""";

        await PartitionKeyTestAsync(
            ctx => ctx.Customers
                .Where(b => (b.Id == 42 || b.Name == "John Snow") && b.PartitionKey == 1)
                .FirstAsync(),
            readSql,
            ctx => ctx.Customers.WithPartitionKey("2").LastAsync(),
            ctx => ctx.Customers
                .Where(b => b.Id == 42 && (b.PartitionKey == 1 || b.PartitionKey == 2))
                .ToListAsync(),
            2);
    }

    protected virtual async Task PartitionKeyTestAsync(
        Func<PartitionKeyContext, Task<Customer>> readSingleTask,
        string readSql,
        Func<PartitionKeyContext, Task<Customer>> readLastTask,
        Func<PartitionKeyContext, Task<List<Customer>>> readListTask,
        int listCount)
    {
        var customer1 = new Customer
        {
            Id = 42,
            Name = "Theon",
            PartitionKey = 1
        };

        var customer2 = new Customer
        {
            Id = 42,
            Name = "Theon Twin",
            PartitionKey = 2
        };

        await using (var innerContext = CreateContext())
        {
            await innerContext.Database.EnsureCreatedAsync();

            await innerContext.AddAsync(customer1);
            await innerContext.AddAsync(customer2);
            await innerContext.SaveChangesAsync();
        }

        // Read & update
        await using (var innerContext = CreateContext())
        {
            var customerFromStore = await readSingleTask(innerContext);

            AssertSql(readSql);

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);
            Assert.Equal(1, customerFromStore.PartitionKey);

            customerFromStore.Name = "Theon Greyjoy";

            await innerContext.SaveChangesAsync();
        }

        // Read list
        await using (var innerContext = CreateContext())
        {
            var customerFromStore = await readListTask(innerContext);

            Assert.Equal(listCount, customerFromStore.Count);
        }

        // Test exception
        await using (var innerContext = CreateContext())
        {
            var customerFromStore = await readSingleTask(innerContext);
            customerFromStore.PartitionKey = 2;

            Assert.Equal(
                CoreStrings.KeyReadOnly(nameof(Customer.PartitionKey), nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() => innerContext.SaveChanges()).Message);
        }

        // Read update & delete
        await using (var innerContext = CreateContext())
        {
            var customerFromStore = await readSingleTask(innerContext);

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);
            Assert.Equal(1, customerFromStore.PartitionKey);

            innerContext.Remove(customerFromStore);

            innerContext.Remove(await readLastTask(innerContext));

            await innerContext.SaveChangesAsync();
        }

        await using (var innerContext = CreateContext())
        {
            Assert.Empty(await readListTask(innerContext));
        }
    }

    protected PartitionKeyContext CreateContext()
        => Fixture.CreateContext();

    public class CosmosPartitionKeyFixture : SharedStoreFixtureBase<PartitionKeyContext>
    {
        protected override string StoreName
            => DatabaseName;

        protected override bool UsePooling
            => false;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
    }

    public class PartitionKeyContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        public virtual DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>(
                cb =>
                {
                    cb.HasPartitionKey(c => c.PartitionKey);
                    cb.Property(c => c.PartitionKey).HasConversion<string>();
                    cb.HasKey(c => new { c.Id, c.PartitionKey });
                });
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PartitionKey { get; set; }
    }
}
