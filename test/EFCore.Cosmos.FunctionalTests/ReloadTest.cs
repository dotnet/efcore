// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ReloadTest : IClassFixture<ReloadTest.CosmosReloadTestFixture>
{
    public static IEnumerable<object[]> IsAsyncData = [[false], [true]];

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    protected CosmosReloadTestFixture Fixture { get; }

    public ReloadTest(CosmosReloadTestFixture fixture)
    {
        Fixture = fixture;
        ClearLog();
    }

    [ConditionalFact]
    public async Task Entity_reference_can_be_reloaded()
    {
        using var context = CreateContext();

        var entry = await context.AddAsync(new Item { Id = 1337 });

        await context.SaveChangesAsync();

        var itemJson = entry.Property<JObject>("__jObject").CurrentValue;
        itemJson["unmapped"] = 2;

        await entry.ReloadAsync();

        itemJson = entry.Property<JObject>("__jObject").CurrentValue;
        Assert.Null(itemJson["unmapped"]);
    }

    protected ReloadTestContext CreateContext()
        => Fixture.CreateContext();

    public class CosmosReloadTestFixture : SharedStoreFixtureBase<ReloadTestContext>
    {
        protected override string StoreName
            => nameof(ReloadTest);

        protected override bool UsePooling
            => false;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
    }

    public class ReloadTestContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>(
                b =>
                {
                    b.HasPartitionKey(e => e.Id);
                });
        }

        public DbSet<Item> Items { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
    }
}
