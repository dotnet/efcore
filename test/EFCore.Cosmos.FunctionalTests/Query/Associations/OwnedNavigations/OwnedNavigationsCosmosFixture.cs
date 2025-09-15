// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsCosmosFixture : OwnedNavigationsFixtureBase
{
    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder)
            .ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined));

    public Task NoSyncTest(bool async, Func<bool, Task> testCode)
        => CosmosTestHelpers.Instance.NoSyncTest(async, testCode);

    public void NoSyncTest(Action testCode)
        => CosmosTestHelpers.Instance.NoSyncTest(testCode);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Ignore<RootReferencingEntity>();

        modelBuilder.Entity<RootEntity>()
            .ToContainer("RootEntities")
            .HasNoDiscriminator();
    }

    // We need to override the following asserters because of #36577:
    // the Cosmos provider incorrectly returns null for empty collections in some cases
    protected override void AssertRootEntity(RootEntity e, RootEntity a)
    {
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);

        NullSafeAssert<RelatedType>(e.RequiredRelated, a.RequiredRelated, AssertRelatedType);
        NullSafeAssert<RelatedType>(e.OptionalRelated, a.OptionalRelated, AssertRelatedType);

        if (e.RelatedCollection is not null && a.RelatedCollection is not null)
        {
            Assert.Equal(e.RelatedCollection.Count, a.RelatedCollection.Count);

            var (orderedExpected, orderedActual) = (e.RelatedCollection, a.RelatedCollection);

            for (var i = 0; i < e.RelatedCollection.Count; i++)
            {
                AssertRelatedType(orderedExpected[i], orderedActual[i]);
            }
        }
        else
        {
            // #36577: the Cosmos provider incorrectly returns null for empty collections in some cases
            if (e.RelatedCollection is [] && a.RelatedCollection is null)
            {
                return;
            }

            Assert.Equal(e.RelatedCollection, a.RelatedCollection);
        }
    }

    protected override void AssertRelatedType(RelatedType e, RelatedType a)
    {
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);

        Assert.Equal(e.Int, a.Int);
        Assert.Equal(e.String, a.String);

        NullSafeAssert<NestedType>(e.RequiredNested, a.RequiredNested, AssertNestedType);
        NullSafeAssert<NestedType>(e.OptionalNested, a.OptionalNested, AssertNestedType);

        if (e.NestedCollection is not null && a.NestedCollection != null)
        {
            Assert.Equal(e.NestedCollection.Count, a.NestedCollection.Count);

            var (orderedExpected, orderedActual) = (e.NestedCollection, a.NestedCollection);

            for (var i = 0; i < e.NestedCollection.Count; i++)
            {
                AssertNestedType(orderedExpected[i], orderedActual[i]);
            }
        }
        else
        {
            // #36577: the Cosmos provider incorrectly returns null for empty collections in some cases
            if (e.NestedCollection is [] && a.NestedCollection is null)
            {
                return;
            }

            Assert.Equal(e.NestedCollection, a.NestedCollection);
        }
    }

    private static void NullSafeAssert<T>(object? e, object? a, Action<T, T> assertAction)
    {
        if (e is T ee && a is T aa)
        {
            assertAction(ee, aa);
            return;
        }

        Assert.Equal(e, a);
    }
}
