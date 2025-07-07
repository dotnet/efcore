// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class MaterializationInterceptionCosmosTest(NonSharedFixture fixture) :
    MaterializationInterceptionTestBase<MaterializationInterceptionCosmosTest.CosmosLibraryContext>(fixture)
{
    public override Task Intercept_query_materialization_with_owned_types_projecting_collection(bool async, bool usePooling)
        => Task.CompletedTask;

    public override Task Intercept_query_materialization_with_owned_types(bool async, bool usePooling)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Intercept_query_materialization_with_owned_types(a, usePooling));

    public class CosmosLibraryContext(DbContextOptions options) : LibraryContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedOnAdd();
                    b.HasPartitionKey(e => e.Title);
                    b.HasKey(e => new { e.Id, e.Title });
                });

            modelBuilder.Entity<Pamphlet>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedOnAdd();
                    b.HasPartitionKey(e => e.Title);
                    b.HasKey(e => new { e.Id, e.Title });
                });

            modelBuilder.Entity<TestEntity30244>(
                b =>
                {
                    b.HasPartitionKey(e => e.Title);
                    b.HasKey(e => new { e.Id, e.Title });
                });
        }
    }

    [ConditionalTheory(Skip = "Issue #33600 - flaky test")]
    public override Task Binding_interceptors_are_used_by_queries(bool inject, bool usePooling)
        => base.Binding_interceptors_are_used_by_queries(inject, usePooling);

    [ConditionalTheory(Skip = "Issue #33600 - flaky test")]
    public override Task Multiple_materialization_interceptors_can_be_used(bool inject, bool usePooling)
        => base.Multiple_materialization_interceptors_can_be_used(inject, usePooling);

    [ConditionalTheory(Skip = "Issue #33600 - flaky test")]
    public override Task Intercept_query_materialization_for_empty_constructor(bool inject, bool usePooling)
        => base.Intercept_query_materialization_for_empty_constructor(inject, usePooling);

    [ConditionalTheory(Skip = "Issue #33600 - flaky test")]
    public override Task Intercept_query_materialization_for_full_constructor(bool inject, bool usePooling)
        => base.Intercept_query_materialization_for_full_constructor(inject, usePooling);

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;
}
