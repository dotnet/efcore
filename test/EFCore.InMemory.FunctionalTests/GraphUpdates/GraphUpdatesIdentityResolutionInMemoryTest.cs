// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class GraphUpdatesIdentityResolutionInMemoryTest
    : GraphUpdatesInMemoryTestBase<GraphUpdatesIdentityResolutionInMemoryTest.InMemoryIdentityResolutionFixture>
{
    public GraphUpdatesIdentityResolutionInMemoryTest(InMemoryIdentityResolutionFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public void Can_attach_full_required_graph_of_duplicates()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var trackedRoot = LoadRequiredGraph(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryRequiredGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public void Can_attach_full_optional_graph_of_duplicates()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var trackedRoot = LoadOptionalGraph(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryOptionalGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public void Can_attach_full_required_non_PK_graph_of_duplicates()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var trackedRoot = LoadRequiredNonPkGraph(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryRequiredNonPkGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public void Can_attach_full_required_AK_graph_of_duplicates()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var trackedRoot = LoadRequiredAkGraph(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryRequiredAkGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public void Can_attach_full_optional_AK_graph_of_duplicates()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var trackedRoot = LoadOptionalAkGraph(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryOptionalAkGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public void Can_attach_full_required_non_PK_AK_graph_of_duplicates()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var trackedRoot = LoadRequiredNonPkAkGraph(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryRequiredNonPkAkGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public void Can_attach_full_required_one_to_many_graph_of_duplicates()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var trackedRoot = LoadOptionalOneToManyGraph(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryOptionalOneToManyGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public void Can_attach_full_required_composite_graph_of_duplicates()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var trackedRoot = LoadRequiredCompositeGraph(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryRequiredCompositeGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertNavigations(trackedRoot);

                Assert.Equal(0, context.SaveChanges());
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
