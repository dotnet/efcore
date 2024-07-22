﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public abstract class NonSharedModelBulkUpdatesRelationalTestBase : NonSharedModelBulkUpdatesTestBase
{
    protected override string StoreName
        => "NonSharedModelBulkUpdatesTests";

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Delete_aggregate_root_when_table_sharing_with_non_owned_throws(bool async)
    {
        var contextFactory = await InitializeAsync<Context28671>(
            onModelCreating: mb =>
            {
                mb.Entity<Owner>().HasOne<OtherReference>().WithOne().HasForeignKey<OtherReference>(e => e.Id);
                mb.Entity<OtherReference>().ToTable(nameof(Owner));
            });

        await AssertTranslationFailedWithDetails(
            () => AssertDelete(
                async, contextFactory.CreateContext,
                context => context.Set<Owner>(), rowsAffectedCount: 0),
            RelationalStrings.ExecuteDeleteOnTableSplitting(nameof(Owner)));
    }
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Update_main_table_in_entity_with_entity_splitting(bool async)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            onModelCreating: mb => mb.Entity<Blog>()
                .ToTable("Blogs")
                .SplitToTable(
                    "BlogsPart1", tb =>
                    {
                        tb.Property(b => b.Title);
                        tb.Property(b => b.Rating);
                    }),
            seed: async context =>
            {
                context.Set<Blog>().Add(new Blog { Title = "SomeBlog" });
                await context.SaveChangesAsync();
            });

        await AssertUpdate(
            async,
            contextFactory.CreateContext,
            ss => ss.Set<Blog>(),
            s => s.SetProperty(b => b.CreationTimestamp, b => new DateTime(2020, 1, 1)),
            rowsAffectedCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Update_non_main_table_in_entity_with_entity_splitting(bool async)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            onModelCreating: mb => mb.Entity<Blog>()
                .ToTable("Blogs")
                .SplitToTable(
                    "BlogsPart1", tb =>
                    {
                        tb.Property(b => b.Title);
                        tb.Property(b => b.Rating);
                    }),
            seed: async context =>
            {
                context.Set<Blog>().Add(new Blog { Title = "SomeBlog" });
                await context.SaveChangesAsync();
            });

        await AssertUpdate(
            async,
            contextFactory.CreateContext,
            ss => ss.Set<Blog>(),
            s => s
                .SetProperty(b => b.Title, b => b.Rating.ToString())
                .SetProperty(b => b.Rating, b => b.Title!.Length),
            rowsAffectedCount: 1);
    }

    #region HelperMethods

    protected static async Task AssertTranslationFailedWithDetails(Func<Task> query, string details)
        => Assert.Contains(
            CoreStrings.NonQueryTranslationFailedWithDetails("", details)[21..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    #endregion
}
