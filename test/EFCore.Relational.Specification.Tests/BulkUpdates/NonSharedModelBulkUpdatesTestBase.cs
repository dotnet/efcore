// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public abstract class NonSharedModelBulkUpdatesTestBase : NonSharedModelTestBase
{
    protected override string StoreName
        => "NonSharedModelBulkUpdatesTests";

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Delete_aggregate_root_when_eager_loaded_owned_collection(bool async)
    {
        var contextFactory = await InitializeAsync<Context28671>(onModelCreating: mb => mb.Entity<Owner>().Ignore(e => e.OwnedReference));
        await AssertDelete(async, contextFactory.CreateContext,
            context => context.Set<Owner>(), rowsAffectedCount: 0);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Delete_aggregate_root_when_table_sharing_with_owned(bool async)
    {
        var contextFactory = await InitializeAsync<Context28671>();
        await AssertDelete(async, contextFactory.CreateContext,
            context => context.Set<Owner>(), rowsAffectedCount: 0);
    }

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
            () => AssertDelete(async, contextFactory.CreateContext,
            context => context.Set<Owner>(), rowsAffectedCount: 0),
            RelationalStrings.ExecuteDeleteOnTableSplitting(nameof(Owner)));
    }

    protected class Context28671 : DbContext
    {
        public Context28671(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Owner>(
                b =>
                {
                    b.OwnsOne(e => e.OwnedReference);
                    b.OwnsMany(e => e.OwnedCollections);
                });
        }
    }

    public class Owner
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public OwnedReference OwnedReference { get; set; }
        public List<OwnedCollection> OwnedCollections { get; set; }
    }

    public class OwnedReference
    {
        public int Number { get; set; }
        public string Value { get; set; }
    }

    public class OwnedCollection
    {
        public string Value { get; set; }
    }

    public class OtherReference
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
    }

#nullable enable
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Delete_predicate_based_on_optional_navigation(bool async)
    {
        var contextFactory = await InitializeAsync<Context28745>();
        await AssertDelete(
            async, contextFactory.CreateContext,
            context => context.Posts.Where(p => p.Blog!.Title!.StartsWith("Arthur")), rowsAffectedCount: 1);
    }

    protected class Context28745 : DbContext
    {
        public Context28745(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Blog> Blogs
            => Set<Blog>();

        public DbSet<Post> Posts
            => Set<Post>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>()
                .HasData(new Blog { Id = 1, Title = "Arthur" }, new Blog { Id = 2, Title = "Brice" });

            modelBuilder.Entity<Post>()
                .HasData(
                    new { Id = 1, BlogId = 1 },
                    new { Id = 2, BlogId = 2 },
                    new { Id = 3, BlogId = 2 });
        }
    }

    public class Blog
    {
        public int Id { get; set; }
        public string? Title { get; set; }

        public virtual ICollection<Post> Posts { get; } = new List<Post>();
    }

    public class Post
    {
        public int Id { get; set; }
        public virtual Blog? Blog { get; set; }
    }

#nullable disable

    #region HelperMethods

    public async Task AssertDelete<TContext, TResult>(
        bool async,
        Func<TContext> contextCreator,
        Func<TContext, IQueryable<TResult>> query,
        int rowsAffectedCount)
        where TContext : DbContext
    {
        if (async)
        {
            await TestHelpers.ExecuteWithStrategyInTransactionAsync(
                contextCreator, UseTransaction,
                async context =>
                {
                    var processedQuery = query(context);

                    var result = await processedQuery.ExecuteDeleteAsync();

                    Assert.Equal(rowsAffectedCount, result);
                });
        }
        else
        {
            TestHelpers.ExecuteWithStrategyInTransaction(
                contextCreator, UseTransaction,
                context =>
                {
                    var processedQuery = query(context);

                    var result = processedQuery.ExecuteDelete();

                    Assert.Equal(rowsAffectedCount, result);
                });
        }
    }

    public async Task AssertUpdate<TContext, TResult>(
        bool async,
        Func<TContext> contextCreator,
        Func<TContext, IQueryable<TResult>> query,
        Expression<Func<SetPropertyCalls<TResult>, SetPropertyCalls<TResult>>> setPropertyCalls,
        int rowsAffectedCount)
        where TResult : class
        where TContext : DbContext
    {
        if (async)
        {
            await TestHelpers.ExecuteWithStrategyInTransactionAsync(
                contextCreator, UseTransaction,
                async context =>
                {
                    var processedQuery = query(context);

                    var result = await processedQuery.ExecuteUpdateAsync(setPropertyCalls);

                    Assert.Equal(rowsAffectedCount, result);
                });
        }
        else
        {
            TestHelpers.ExecuteWithStrategyInTransaction(
                contextCreator, UseTransaction,
                context =>
                {
                    var processedQuery = query(context);

                    var result = processedQuery.ExecuteUpdate(setPropertyCalls);

                    Assert.Equal(rowsAffectedCount, result);
                });
        }
    }

    protected static async Task AssertTranslationFailedWithDetails(Func<Task> query, string details)
        => Assert.Contains(
            RelationalStrings.NonQueryTranslationFailedWithDetails("", details)[21..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    #endregion
}
