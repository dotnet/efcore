// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

// TODO: Consider removing these in favor of ReadItemPartitionKeyQueryTest
public class HierarchicalPartitionKeyTest : IClassFixture<HierarchicalPartitionKeyTest.CosmosHierarchicalPartitionKeyFixture>
{
    private const string DatabaseName = nameof(HierarchicalPartitionKeyTest);

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    protected CosmosHierarchicalPartitionKeyFixture Fixture { get; }

    public HierarchicalPartitionKeyTest(CosmosHierarchicalPartitionKeyFixture fixture)
    {
        Fixture = fixture;
        ClearLog();
    }

    [ConditionalFact]
    public virtual async Task Can_add_update_delete_end_to_end_with_partition_key()
    {
        const string read1Sql =
            """
SELECT VALUE c
FROM root c
ORDER BY c["PartitionKey1"]
OFFSET 0 LIMIT 1
""";

        const string read2Sql =
            """
@__p_0='1'

SELECT VALUE c
FROM root c
ORDER BY c["PartitionKey1"]
OFFSET @__p_0 LIMIT 1
""";

        await PartitionKeyTestAsync(
            ctx => ctx.Customers.OrderBy(c => c.PartitionKey1).FirstAsync(),
            read1Sql,
            ctx => ctx.Customers.OrderBy(c => c.PartitionKey1).Skip(1).FirstAsync(),
            read2Sql,
            ctx => ctx.Customers.OrderBy(c => c.PartitionKey1).LastAsync(),
            ctx => ctx.Customers.OrderBy(c => c.PartitionKey1).ToListAsync(),
            2);
    }

    [ConditionalFact]
    public virtual async Task Can_add_update_delete_end_to_end_with_with_partition_key_extension()
    {
        const string readSql =
            """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""";

        await PartitionKeyTestAsync(
            ctx => ctx.Customers.WithPartitionKey("A", 1.1, true).SingleAsync(),
            readSql,
            ctx => ctx.Customers.WithPartitionKey("B", 2.1, false).SingleAsync(),
            readSql,
            ctx => ctx.Customers.WithPartitionKey("B", 2.1, false).LastAsync(),
            ctx => ctx.Customers.WithPartitionKey("B", 2.1, false).ToListAsync(),
            1);
    }

    [ConditionalFact]
    public async Task Can_query_with_implicit_partition_key_filter()
    {
        const string readSql =
            """
SELECT VALUE c
FROM root c
WHERE ((c["Id"] = 42) OR (c["Name"] = "John Snow"))
OFFSET 0 LIMIT 2
""";

        await PartitionKeyTestAsync(
            ctx => ctx.Customers
                .Where(
                    b => (b.Id == 42 || b.Name == "John Snow")
                        && b.PartitionKey1 == "A"
                        && b.PartitionKey2 == 1.1
                        && b.PartitionKey3)
                .SingleAsync(),
            readSql,
            ctx => ctx.Customers
                .Where(
                    b => (b.Id == 42 || b.Name == "John Snow")
                        && b.PartitionKey1 == "B"
                        && b.PartitionKey2 == 2.1
                        && !b.PartitionKey3)
                .SingleAsync(),
            readSql,
            ctx => ctx.Customers.WithPartitionKey("B", 2.1, false).LastAsync(),
            ctx => ctx.Customers
                .Where(
                    b => b.Id == 42
                        && ((b.PartitionKey1 == "A" && b.PartitionKey2 == 1.1 && b.PartitionKey3)
                            || (b.PartitionKey1 == "B" && b.PartitionKey2 == 2.1 && !b.PartitionKey3)))
                .ToListAsync(),
            2);
    }

    protected virtual async Task PartitionKeyTestAsync(
        Func<HierarchicalPartitionKeyContext, Task<Customer>> read1SingleTask,
        string read1Sql,
        Func<HierarchicalPartitionKeyContext, Task<Customer>> read2SingleTask,
        string read2Sql,
        Func<HierarchicalPartitionKeyContext, Task<Customer>> readLastTask,
        Func<HierarchicalPartitionKeyContext, Task<List<Customer>>> readListTask,
        int listCount)
    {
        var customer1 = new Customer
        {
            Id = 42,
            Name = "Theon",
            PartitionKey1 = "A",
            PartitionKey2 = 1.1,
            PartitionKey3 = true,
        };

        var customer2 = new Customer
        {
            Id = 42,
            Name = "Theon Twin",
            PartitionKey1 = "B",
            PartitionKey2 = 2.1,
            PartitionKey3 = false,
        };

        await using (var innerContext = CreateContext())
        {
            await innerContext.Database.EnsureCreatedAsync();

            await innerContext.AddAsync(customer1);
            await innerContext.AddAsync(customer2);
            await innerContext.SaveChangesAsync();
        }

        // Read & update in first partition
        await using (var innerContext = CreateContext())
        {
            var customerFromStore = await read1SingleTask(innerContext);

            AssertSql(read1Sql);

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);
            Assert.Equal("A", customerFromStore.PartitionKey1);
            Assert.Equal(1.1, customerFromStore.PartitionKey2);
            Assert.True(customerFromStore.PartitionKey3);

            customerFromStore.Name = "Theon Greyjoy";

            await innerContext.SaveChangesAsync();
        }

        // Read & update in second partition
        await using (var innerContext = CreateContext())
        {
            var customerFromStore = await read2SingleTask(innerContext);

            AssertSql(read2Sql);

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon Twin", customerFromStore.Name);
            Assert.Equal("B", customerFromStore.PartitionKey1);
            Assert.Equal(2.1, customerFromStore.PartitionKey2);
            Assert.False(customerFromStore.PartitionKey3);

            customerFromStore.Name = "Theon Bluejoy";

            await innerContext.SaveChangesAsync();
        }

        // Read list from all partitions
        await using (var innerContext = CreateContext())
        {
            var customerFromStore = await readListTask(innerContext);

            Assert.Equal(listCount, customerFromStore.Count);
        }

        // Test exceptions
        await using (var innerContext = CreateContext())
        {
            var customerFromStore = await read1SingleTask(innerContext);
            customerFromStore.PartitionKey1 = "C";

            Assert.Equal(
                CoreStrings.KeyReadOnly(nameof(Customer.PartitionKey1), nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() => innerContext.SaveChanges()).Message);
        }

        await using (var innerContext = CreateContext())
        {
            var customerFromStore = await read1SingleTask(innerContext);
            customerFromStore.PartitionKey2 = 2.1;

            Assert.Equal(
                CoreStrings.KeyReadOnly(nameof(Customer.PartitionKey2), nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() => innerContext.SaveChanges()).Message);
        }

        await using (var innerContext = CreateContext())
        {
            var customerFromStore = await read1SingleTask(innerContext);
            customerFromStore.PartitionKey3 = false;

            Assert.Equal(
                CoreStrings.KeyReadOnly(nameof(Customer.PartitionKey3), nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() => innerContext.SaveChanges()).Message);
        }

        // Read update & delete
        await using (var innerContext = CreateContext())
        {
            var customerFromStore = await read1SingleTask(innerContext);

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);
            Assert.Equal("A", customerFromStore.PartitionKey1);
            Assert.Equal(1.1, customerFromStore.PartitionKey2);
            Assert.True(customerFromStore.PartitionKey3);

            innerContext.Remove(customerFromStore);

            var lastTask = await readLastTask(innerContext);

            innerContext.Remove(lastTask);

            await innerContext.SaveChangesAsync();
        }

        await using (var innerContext = CreateContext())
        {
            Assert.Empty(await readListTask(innerContext));
        }
    }

    protected HierarchicalPartitionKeyContext CreateContext()
        => Fixture.CreateContext();

    public class CosmosHierarchicalPartitionKeyFixture : SharedStoreFixtureBase<HierarchicalPartitionKeyContext>
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

    public class HierarchicalPartitionKeyContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        public virtual DbSet<Customer> Customers
            => Set<Customer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>(
                cb =>
                {
                    cb.HasPartitionKey(
                        c => new
                        {
                            c.PartitionKey1,
                            c.PartitionKey2,
                            c.PartitionKey3
                        });
                    cb.HasKey(
                        c => new
                        {
                            c.Id,
                            c.PartitionKey1,
                            c.PartitionKey2,
                            c.PartitionKey3
                        });
                });
    }

    public class Customer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? PartitionKey1 { get; set; }
        public double PartitionKey2 { get; set; }
        public bool PartitionKey3 { get; set; }
    }
}
