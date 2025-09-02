// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class SharedTypeQueryInMemoryTest : SharedTypeQueryTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Can_use_shared_type_entity_type_in_ToInMemoryQuery(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextInMemory24601>(
            seed: c => c.SeedAsync());

        using var context = contextFactory.CreateContext();

        var data = context.Set<ViewQuery24601>();

        Assert.Equal("Maumar", Assert.Single(data).Value);
    }

    private class MyContextInMemory24601(DbContextOptions options) : MyContext24601(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SharedTypeEntity<Dictionary<string, object>>(
                "STET",
                b =>
                {
                    b.IndexerProperty<int>("Id");
                    b.IndexerProperty<string>("Value");
                });

            modelBuilder.Entity<ViewQuery24601>().HasNoKey()
                .ToInMemoryQuery(
                    () => Set<Dictionary<string, object>>("STET").Select(e => new ViewQuery24601 { Value = (string)e["Value"] }));
        }
    }
}
