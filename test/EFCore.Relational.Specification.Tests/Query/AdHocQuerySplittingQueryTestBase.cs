// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class AdHocQuerySplittingQueryTestBase : NonSharedModelTestBase
{
    protected override string StoreName
        => "AdHocQuerySplittingQueryTests";

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected abstract DbContextOptionsBuilder SetQuerySplittingBehavior(
        DbContextOptionsBuilder optionsBuilder,
        QuerySplittingBehavior splittingBehavior);

    protected abstract DbContextOptionsBuilder ClearQuerySplittingBehavior(DbContextOptionsBuilder optionsBuilder);

    #region 21355

    [ConditionalFact]
    public virtual async Task Can_configure_SingleQuery_at_context_level()
    {
        var contextFactory = await InitializeAsync<Context21355>(
            seed: c => c.SeedAsync(),
            onConfiguring: o => SetQuerySplittingBehavior(o, QuerySplittingBehavior.SingleQuery));

        using (var context = contextFactory.CreateContext())
        {
            var result = context.Parents.Include(p => p.Children1).ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            var result = context.Parents.Include(p => p.Children1).AsSplitQuery().ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            context.Parents.Include(p => p.Children1).Include(p => p.Children2).ToList();
        }
    }

    [ConditionalFact]
    public virtual async Task Can_configure_SplitQuery_at_context_level()
    {
        var contextFactory = await InitializeAsync<Context21355>(
            seed: c => c.SeedAsync(),
            onConfiguring: o => SetQuerySplittingBehavior(o, QuerySplittingBehavior.SplitQuery));

        using (var context = contextFactory.CreateContext())
        {
            var result = context.Parents.Include(p => p.Children1).ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            var result = context.Parents.Include(p => p.Children1).AsSingleQuery().ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            context.Parents.Include(p => p.Children1).Include(p => p.Children2).ToList();
        }
    }

    [ConditionalFact]
    public virtual async Task Unconfigured_query_splitting_behavior_throws_a_warning()
    {
        var contextFactory = await InitializeAsync<Context21355>(
            seed: c => c.SeedAsync(),
            onConfiguring: o => ClearQuerySplittingBehavior(o));

        using (var context = contextFactory.CreateContext())
        {
            context.Parents.Include(p => p.Children1).Include(p => p.Children2).AsSplitQuery().ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            Assert.Contains(
                RelationalResources.LogMultipleCollectionIncludeWarning(new TestLogger<TestRelationalLoggingDefinitions>())
                    .GenerateMessage(),
                Assert.Throws<InvalidOperationException>(
                    () => context.Parents.Include(p => p.Children1).Include(p => p.Children2).ToList()).Message);
        }
    }

    [ConditionalFact]
    public virtual async Task Using_AsSingleQuery_without_context_configuration_does_not_throw_warning()
    {
        var contextFactory = await InitializeAsync<Context21355>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        context.Parents.Include(p => p.Children1).Include(p => p.Children2).AsSingleQuery().ToList();
    }

    [ConditionalFact]
    public virtual async Task SplitQuery_disposes_inner_data_readers()
    {
        var contextFactory = await InitializeAsync<Context21355>(seed: c => c.SeedAsync());

        ((RelationalTestStore)contextFactory.TestStore).CloseConnection();

        using (var context = contextFactory.CreateContext())
        {
            context.Parents.Include(p => p.Children1).Include(p => p.Children2).AsSplitQuery().ToList();

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }

        using (var context = contextFactory.CreateContext())
        {
            await context.Parents.Include(p => p.Children1).Include(p => p.Children2).AsSplitQuery().ToListAsync();

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }

        using (var context = contextFactory.CreateContext())
        {
            context.Parents.Include(p => p.Children1).Include(p => p.Children2).OrderBy(e => e.Id).AsSplitQuery().Single();

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }

        using (var context = contextFactory.CreateContext())
        {
            await context.Parents.Include(p => p.Children1).Include(p => p.Children2).OrderBy(e => e.Id).AsSplitQuery().SingleAsync();

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }
    }

    protected class Context21355(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Parent> Parents { get; set; }

        public async Task SeedAsync()
        {
            Add(new Parent { Id = "Parent1", Children1 = [new(), new()] });
            await SaveChangesAsync();
        }

        public class Parent
        {
            public string Id { get; set; }
            public List<Child> Children1 { get; set; }
            public List<AnotherChild> Children2 { get; set; }
        }

        public class Child
        {
            public int Id { get; set; }
            public string ParentId { get; set; }
            public Parent Parent { get; set; }
        }

        public class AnotherChild
        {
            public int Id { get; set; }
            public string ParentId { get; set; }
            public Parent Parent { get; set; }
        }
    }

    #endregion

    #region 25225

    [ConditionalFact]
    public virtual async Task Can_query_with_nav_collection_in_projection_with_split_query_in_parallel_async()
    {
        var (context1, context2) = await CreateTwoContext25225();
        var task1 = QueryAsync(context1, Context25225.Parent1Id, Context25225.Collection1Id);
        var task2 = QueryAsync(context2, Context25225.Parent2Id, Context25225.Collection2Id);
        await Task.WhenAll(task1, task2);

        async Task QueryAsync(Context25225 context, Guid parentId, Guid collectionId)
        {
            ClearLog();
            for (var i = 0; i < 100; i++)
            {
                var parent = await SelectParent25225(context, parentId).SingleAsync();
                AssertParent25225(parentId, collectionId, parent);
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_with_nav_collection_in_projection_with_split_query_in_parallel_sync()
    {
        var (context1, context2) = await CreateTwoContext25225();
        var task1 = Task.Run(() => Query(context1, Context25225.Parent1Id, Context25225.Collection1Id));
        var task2 = Task.Run(() => Query(context2, Context25225.Parent2Id, Context25225.Collection2Id));
        await Task.WhenAll(task1, task2);

        void Query(Context25225 context, Guid parentId, Guid collectionId)
        {
            ClearLog();
            for (var i = 0; i < 10; i++)
            {
                var parent = SelectParent25225(context, parentId).Single();
                AssertParent25225(parentId, collectionId, parent);
            }
        }
    }

    private async Task<(Context25225, Context25225)> CreateTwoContext25225()
    {
        var context1 = (await CreateContext25225Async()).CreateContext();
        var context2 = (await CreateContext25225Async()).CreateContext();

        // Can't run in parallel with the same connection instance. Issue #22921
        Assert.NotSame(context1.Database.GetDbConnection(), context2.Database.GetDbConnection());

        return (context1, context2);
    }

    private async Task<ContextFactory<Context25225>> CreateContext25225Async()
        => await InitializeAsync<Context25225>(
            seed: c => c.SeedAsync(),
            onConfiguring: o => SetQuerySplittingBehavior(o, QuerySplittingBehavior.SplitQuery),
            createTestStore: () => CreateTestStore25225());

    protected virtual Task<TestStore> CreateTestStore25225()
        => Task.FromResult(base.CreateTestStore());

    private static IQueryable<Context25225.ParentViewModel> SelectParent25225(Context25225 context, Guid parentId)
        => context
            .Parents
            .Where(x => x.Id == parentId)
            .Select(
                p => new Context25225.ParentViewModel
                {
                    Id = p.Id,
                    Collection = p
                        .Collection
                        .Select(
                            c => new Context25225.CollectionViewModel
                            {
                                Id = c.Id, ParentId = c.ParentId,
                            })
                        .ToArray()
                });

    private static void AssertParent25225(Guid expectedParentId, Guid expectedCollectionId, Context25225.ParentViewModel actualParent)
    {
        Assert.Equal(expectedParentId, actualParent.Id);
        Assert.Collection(
            actualParent.Collection,
            c => Assert.Equal(expectedCollectionId, c.Id)
        );
    }

    protected class Context25225(DbContextOptions options) : DbContext(options)
    {
        public static readonly Guid Parent1Id = new("d6457b52-690a-419e-8982-a1a8551b4572");
        public static readonly Guid Parent2Id = new("e79c82f4-3ae7-4c65-85db-04e08cba6fa7");
        public static readonly Guid Collection1Id = new("7ce625fb-863d-41b3-b42e-e4e4367f7548");
        public static readonly Guid Collection2Id = new("d347bbd5-003a-441f-a148-df8ab8ac4a29");
        public DbSet<Parent> Parents { get; set; }

        public async Task SeedAsync()
        {
            var parent1 = new Parent { Id = Parent1Id, Collection = new List<Collection> { new() { Id = Collection1Id, } } };
            var parent2 = new Parent { Id = Parent2Id, Collection = new List<Collection> { new() { Id = Collection2Id, } } };
            AddRange(parent1, parent2);
            await SaveChangesAsync();
        }

        public class Parent
        {
            public Guid Id { get; set; }
            public ICollection<Collection> Collection { get; set; }
        }

        public class Collection
        {
            public Guid Id { get; set; }
            public Guid ParentId { get; set; }
            public Parent Parent { get; set; }
        }

        public class ParentViewModel
        {
            public Guid Id { get; set; }
            public ICollection<CollectionViewModel> Collection { get; set; }
        }

        public class CollectionViewModel
        {
            public Guid Id { get; set; }
            public Guid ParentId { get; set; }
        }
    }

    #endregion

    #region 25400

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task NoTracking_split_query_creates_only_required_instances(bool async)
    {
        var contextFactory = await InitializeAsync<Context25400>(
            seed: c => c.SeedAsync(),
            onConfiguring: o => SetQuerySplittingBehavior(o, QuerySplittingBehavior.SplitQuery));

        using var context = contextFactory.CreateContext();
        Context25400.Test.ConstructorCallCount = 0;

        var query = context.Set<Context25400.Test>().AsNoTracking().OrderBy(e => e.Id);
        var test = async
            ? await query.FirstOrDefaultAsync()
            : query.FirstOrDefault();

        Assert.Equal(1, Context25400.Test.ConstructorCallCount);
    }

    private class Context25400(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Test> Tests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Test>().HasKey(e => e.Id);

        public async Task SeedAsync()
        {
            Tests.Add(new Test(15));

            await SaveChangesAsync();
        }

        public class Test
        {
            public static int ConstructorCallCount;

            public Test()
            {
                ++ConstructorCallCount;
            }

            public Test(int value)
            {
                Value = value;
            }

            public int Id { get; set; }
            public int Value { get; set; }
        }
    }

    #endregion
}
