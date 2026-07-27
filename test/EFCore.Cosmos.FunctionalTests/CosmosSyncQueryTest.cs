// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

public class CosmosSyncQueryTest(CosmosSyncQueryTest.CosmosSyncQueryFixture fixture)
    : IClassFixture<CosmosSyncQueryTest.CosmosSyncQueryFixture>
{
    [Fact]
    public void GetEnumerator_throws()
        => AssertSyncQueryThrows(context =>
        {
            using var enumerator = ((IEnumerable<SyncQueryEntity>)context.Entities).GetEnumerator();
            enumerator.MoveNext();
        });

    [Fact]
    public void ToArray_throws()
        => AssertSyncQueryThrows(context => context.Entities.ToArray());

    [Fact]
    public void ToList_throws()
        => AssertSyncQueryThrows(context => context.Entities.ToList());

    [Fact]
    public void Load_throws()
        => AssertSyncQueryThrows(context => context.Entities.Load());

    [Fact]
    public void Any_throws()
        => AssertSyncQueryThrows(context => context.Entities.Any());

    [Fact]
    public void All_throws()
        => AssertSyncQueryThrows(context => ((IEnumerable<SyncQueryEntity>)context.Entities).All(e => e.Id > 0));

    [Fact]
    public void Count_throws()
        => AssertSyncQueryThrows(context => context.Entities.Count());

    [Fact]
    public void LongCount_throws()
        => AssertSyncQueryThrows(context => context.Entities.LongCount());

    [Fact]
    public void First_throws()
        => AssertSyncQueryThrows(context => context.Entities.OrderBy(e => e.Id).First());

    [Fact]
    public void FirstOrDefault_throws()
        => AssertSyncQueryThrows(context => context.Entities.OrderBy(e => e.Id).FirstOrDefault());

    [Fact]
    public void Single_throws()
        => AssertSyncQueryThrows(context => context.Entities.Where(e => e.Id == 1).Single());

    [Fact]
    public void SingleOrDefault_throws()
        => AssertSyncQueryThrows(context => context.Entities.Where(e => e.Id == 1).SingleOrDefault());

    [Fact]
    public void Last_throws()
        => AssertSyncQueryThrows(context => context.Entities.OrderBy(e => e.Id).Last());

    [Fact]
    public void LastOrDefault_throws()
        => AssertSyncQueryThrows(context => context.Entities.OrderBy(e => e.Id).LastOrDefault());

    [Fact]
    public void ElementAt_throws()
        => AssertSyncQueryThrows(context => ((IEnumerable<SyncQueryEntity>)context.Entities.OrderBy(e => e.Id)).ElementAt(0));

    [Fact]
    public void ElementAtOrDefault_throws()
        => AssertSyncQueryThrows(context => ((IEnumerable<SyncQueryEntity>)context.Entities.OrderBy(e => e.Id)).ElementAtOrDefault(0));

    [Fact]
    public void Min_throws()
        => AssertSyncQueryThrows(context => context.Entities.Min(e => e.Value));

    [Fact]
    public void Max_throws()
        => AssertSyncQueryThrows(context => context.Entities.Max(e => e.Value));

    [Fact]
    public void Sum_throws()
        => AssertSyncQueryThrows(context => context.Entities.Sum(e => e.Value));

    [Fact]
    public void Average_throws()
        => AssertSyncQueryThrows(context => context.Entities.Average(e => e.Value));

    private void AssertSyncQueryThrows(Action<SyncQueryContext> syncQuery)
    {
        using var context = fixture.CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(() => syncQuery(context));

        Assert.Equal(CosmosStrings.SyncNotSupported, exception.Message);
    }

    public class CosmosSyncQueryFixture : SharedStoreFixtureBase<SyncQueryContext>
    {
        protected override string StoreName
            => nameof(CosmosSyncQueryTest);

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        protected override Task SeedAsync(SyncQueryContext context)
        {
            context.Entities.AddRange(
                new SyncQueryEntity { Id = 1, PartitionKey = "1", Value = 8 },
                new SyncQueryEntity { Id = 2, PartitionKey = "2", Value = 13 });

            return context.SaveChangesAsync();
        }
    }

    public class SyncQueryContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<SyncQueryEntity> Entities
            => Set<SyncQueryEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<SyncQueryEntity>(b =>
            {
                b.ToContainer(nameof(Entities));
                b.HasPartitionKey(e => e.PartitionKey);
            });
    }

    public class SyncQueryEntity
    {
        public int Id { get; set; }
        public string PartitionKey { get; set; } = null!;
        public int Value { get; set; }
    }
}
