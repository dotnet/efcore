// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class GraphUpdatesIdentityResolutionInMemoryTest(
    GraphUpdatesIdentityResolutionInMemoryTest.InMemoryIdentityResolutionFixture fixture)
    : GraphUpdatesInMemoryTestBase<GraphUpdatesIdentityResolutionInMemoryTest.InMemoryIdentityResolutionFixture>(fixture)
{
    [ConditionalFact]
    public Task Can_attach_full_required_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadRequiredGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(await QueryRequiredGraph(context).AsNoTracking().SingleAsync(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, await context.SaveChangesAsync());
            });

    [ConditionalFact]
    public Task Can_attach_full_optional_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadOptionalGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(await QueryOptionalGraph(context).AsNoTracking().SingleAsync(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, await context.SaveChangesAsync());
            });

    [ConditionalFact]
    public Task Can_attach_full_required_non_PK_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadRequiredNonPkGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(await QueryRequiredNonPkGraph(context).AsNoTracking().SingleAsync(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, await context.SaveChangesAsync());
            });

    [ConditionalFact]
    public Task Can_attach_full_required_AK_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadRequiredAkGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(await QueryRequiredAkGraph(context).AsNoTracking().SingleAsync(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, await context.SaveChangesAsync());
            });

    [ConditionalFact]
    public Task Can_attach_full_optional_AK_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadOptionalAkGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(await QueryOptionalAkGraph(context).AsNoTracking().SingleAsync(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, await context.SaveChangesAsync());
            });

    [ConditionalFact]
    public Task Can_attach_full_required_non_PK_AK_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadRequiredNonPkAkGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(await QueryRequiredNonPkAkGraph(context).AsNoTracking().SingleAsync(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, await context.SaveChangesAsync());
            });

    [ConditionalFact]
    public Task Can_attach_full_required_one_to_many_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadOptionalOneToManyGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(await QueryOptionalOneToManyGraph(context).AsNoTracking().SingleAsync(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, await context.SaveChangesAsync());
            });

    [ConditionalFact]
    public Task Can_attach_full_required_composite_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadRequiredCompositeGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(await QueryRequiredCompositeGraph(context).AsNoTracking().SingleAsync(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, await context.SaveChangesAsync());
            });

    public class InMemoryIdentityResolutionFixture : GraphUpdatesInMemoryFixtureBase
    {
        protected override string StoreName
            => "GraphUpdatesIdentityResolutionTest";

        public override bool HasIdentityResolution
            => true;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).AddInterceptors(new UpdatingIdentityResolutionInterceptor());
    }
}
