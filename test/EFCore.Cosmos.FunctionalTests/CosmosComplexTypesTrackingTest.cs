// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore;

public class CosmosComplexTypesTrackingTest(CosmosComplexTypesTrackingTest.CosmosFixture fixture) : ComplexTypesTrackingTestBase<CosmosComplexTypesTrackingTest.CosmosFixture>(fixture)
{
    [ConditionalFact]
    public async Task Can_reorder_complex_collection_elements()
    {
        await using var context = CreateContext();
        var pub = CreatePubWithCollections(context);
        await context.AddAsync(pub);
        await context.SaveChangesAsync();

        pub.Activities.Reverse();
        var first = pub.Activities[0];
        var last = pub.Activities.Last();
        await context.SaveChangesAsync();

        await using var assertContext = CreateContext();
        var dbPub = await assertContext.Set<PubWithCollections>().FirstAsync(x => x.Id == pub.Id);
        Assert.Equivalent(first, dbPub.Activities[0]);
        Assert.Equivalent(last, dbPub.Activities.Last());
    }

    [ConditionalFact]
    public async Task Can_change_complex_collection_element()
    {
        await using var context = CreateContext();
        var pub = CreatePubWithCollections(context);
        await context.AddAsync(pub);
        await context.SaveChangesAsync();

        pub.Activities[0].Name = "Changed123";
        await context.SaveChangesAsync();

        await using var assertContext = CreateContext();
        var dbPub = await assertContext.Set<PubWithCollections>().FirstAsync(x => x.Id == pub.Id);
        Assert.Equivalent("Changed123", dbPub.Activities[0].Name);
    }

    [ConditionalFact]
    public async Task Can_change_complex_collection_element_complex_collection()
    {
        await using var context = CreateContext();
        var pub = CreatePubWithCollections(context);
        await context.AddAsync(pub);
        await context.SaveChangesAsync();

        pub.Activities[0].Teams.Add(new Team { Name = "NewTeam" });
        await context.SaveChangesAsync();

        await using var assertContext = CreateContext();
        var dbPub = await assertContext.Set<PubWithCollections>().FirstAsync(x => x.Id == pub.Id);
        Assert.Equal("NewTeam", dbPub.Activities[0].Teams.Last().Name);
    }

    [ConditionalFact]
    public async Task Can_add_complex_collection_element()
    {
        await using var context = CreateContext();
        var pub = CreatePubWithCollections(context);
        await context.AddAsync(pub);
        await context.SaveChangesAsync();

        pub.Activities.Add(new ActivityWithCollection { Name = "NewActivity" });
        await context.SaveChangesAsync();

        await using var assertContext = CreateContext();
        var dbPub = await assertContext.Set<PubWithCollections>().FirstAsync(x => x.Id == pub.Id);
        Assert.Equivalent("NewActivity", dbPub.Activities.Last().Name);
        Assert.Equivalent(pub.Activities.Count, dbPub.Activities.Count);
    }

    public override Task Can_save_null_second_level_complex_property_with_required_properties(bool async)
        => async
            ? base.Can_save_null_second_level_complex_property_with_required_properties(async)
            : throw SkipException.ForSkip("Cosmos does not support synchronous operations.");

    public override Task Can_save_null_third_level_complex_property_with_all_optional_properties(bool async)
        => async
            ? base.Can_save_null_third_level_complex_property_with_all_optional_properties(async)
            : throw SkipException.ForSkip("Cosmos does not support synchronous operations.");

    protected override Task TrackAndSaveTest<TEntity>(EntityState state, bool async, Func<DbContext, TEntity> createPub)
        => async
            ? base.TrackAndSaveTest(state, async, createPub)
            : throw SkipException.ForSkip("Cosmos does not support synchronous operations.");

    public override Task Can_change_state_from_Deleted_with_complex_collection(EntityState newState, bool async)
        => async
            ? base.Can_change_state_from_Deleted_with_complex_collection(newState, async)
            : throw SkipException.ForSkip("Cosmos does not support synchronous operations.");

    public override Task Can_change_state_from_Deleted_with_complex_record_collection(EntityState newState, bool async)
        => async
            ? base.Can_change_state_from_Deleted_with_complex_record_collection(newState, async)
            : throw SkipException.ForSkip("Cosmos does not support synchronous operations.");

    public override Task Can_change_state_from_Deleted_with_complex_field_collection(EntityState newState, bool async)
        => async
            ? base.Can_change_state_from_Deleted_with_complex_field_collection(newState, async)
            : throw SkipException.ForSkip("Cosmos does not support synchronous operations.");

    public override Task Can_change_state_from_Deleted_with_complex_field_record_collection(EntityState newState, bool async)
        => async
            ? base.Can_change_state_from_Deleted_with_complex_field_record_collection(newState, async)
            : throw SkipException.ForSkip("Cosmos does not support synchronous operations.");

    public override Task Can_save_default_values_in_optional_complex_property_with_multiple_properties(bool async)
        => async
            ? base.Can_save_default_values_in_optional_complex_property_with_multiple_properties(async)
            : throw SkipException.ForSkip("Cosmos does not support synchronous operations.");

    public override Task Can_null_complex_property_with_default_values_and_multiple_properties(bool async)
        => async
            ? base.Can_null_complex_property_with_default_values_and_multiple_properties(async)
            : throw SkipException.ForSkip("Cosmos does not support synchronous operations.");

    protected override async Task ExecuteWithStrategyInTransactionAsync(Func<DbContext, Task> testOperation, Func<DbContext, Task>? nestedTestOperation1 = null, Func<DbContext, Task>? nestedTestOperation2 = null, Func<DbContext, Task>? nestedTestOperation3 = null)
    {
        using var c = CreateContext();
        try
        {
            await c.Database.CreateExecutionStrategy().ExecuteAsync(
                c,
                async context =>
                {
                    using (var innerContext = CreateContext())
                    {
                        await testOperation(innerContext);
                    }

                    if (nestedTestOperation1 == null)
                    {
                        return;
                    }

                    using (var innerContext1 = CreateContext())
                    {
                        await nestedTestOperation1(innerContext1);
                    }

                    if (nestedTestOperation2 == null)
                    {
                        return;
                    }

                    using (var innerContext2 = CreateContext())
                    {
                        await nestedTestOperation2(innerContext2);
                    }

                    if (nestedTestOperation3 == null)
                    {
                        return;
                    }

                    using (var innerContext3 = CreateContext())
                    {
                        await nestedTestOperation3(innerContext3);
                    }
                }
            );
        }
        finally
        {
            // Cosmos does not support rolling back transactions, so clean the database instead
            await Fixture.TestStore.CleanAsync(c);
        }
    }

    public class CosmosFixture : FixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);
            modelBuilder.Entity<Pub>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<PubWithStructs>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<PubWithReadonlyStructs>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<PubWithRecords>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<PubWithCollections>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<PubWithRecordCollections>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<PubWithArrayCollections>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<PubWithRecordArrayCollections>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<PubWithPropertyBagCollections>().HasPartitionKey(x => x.Id);
            modelBuilder.Entity<EntityWithOptionalMultiPropComplex>().HasPartitionKey(x => x.Id);
            if (!UseProxies)
            {
                modelBuilder.Entity<FieldPub>().HasPartitionKey(x => x.Id);
                modelBuilder.Entity<FieldPubWithStructs>().HasPartitionKey(x => x.Id);
                modelBuilder.Entity<FieldPubWithRecords>().HasPartitionKey(x => x.Id);
                modelBuilder.Entity<FieldPubWithCollections>().HasPartitionKey(x => x.Id);
                modelBuilder.Entity<FieldPubWithRecordCollections>().HasPartitionKey(x => x.Id);
                modelBuilder.Entity<Yogurt>().HasPartitionKey(x => x.Id);
            }
        }
    }
}
