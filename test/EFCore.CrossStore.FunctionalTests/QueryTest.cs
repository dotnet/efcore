// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace Microsoft.EntityFrameworkCore;

public class QueryTest
{
    public static IEnumerable<object[]> IsAsyncData = new object[][] { [false], [true] };

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task AsSplitQuery_does_not_throw_for_InMemory(bool async)
    {
        using var context = new InMemoryQueryContext();
        var query = context.Blogs.Include(e => e.Posts).AsSplitQuery();
        if (async)
        {
            await query.ToListAsync();
        }
        else
        {
            query.ToList();
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task AsSingleQuery_does_not_throw_for_InMemory(bool async)
    {
        using var context = new InMemoryQueryContext();
        var query = context.Blogs.Include(e => e.Posts).AsSingleQuery();
        if (async)
        {
            await query.ToListAsync();
        }
        else
        {
            query.ToList();
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task FromSqlRaw_throws_for_InMemory(bool async)
    {
        using var context = new InMemoryQueryContext();
        var query = RelationalQueryableExtensions.FromSqlRaw(context.Blogs, "Select 1");

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(FromSqlQueryRootExpression)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Cosmos_FromSqlRaw_throws_for_InMemory(bool async)
    {
        using var context = new InMemoryQueryContext();
        var query = CosmosQueryableExtensions.FromSqlRaw(context.Blogs, "Select 1");

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(FromSqlQueryRootExpression)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task FromSqlInterpolated_throws_for_InMemory(bool async)
    {
        using var context = new InMemoryQueryContext();
        var query = context.Blogs.FromSqlInterpolated($"Select 1");

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(FromSqlQueryRootExpression)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task FromSql_throws_for_InMemory(bool async)
    {
        using var context = new InMemoryQueryContext();
        var query = context.Blogs.FromSql($"Select 1");

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(FromSqlQueryRootExpression)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TemporalAsOf_throws_for_InMemory(bool async)
    {
        using var context = new InMemoryQueryContext();
        var query = context.Blogs.TemporalAsOf(DateTime.Now);

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(TemporalAsOfQueryRootExpression)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TemporalAll_throws_for_InMemory(bool async)
    {
        using var context = new InMemoryQueryContext();
        var query = context.Blogs.TemporalAll();

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(TemporalAllQueryRootExpression)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TemporalBetween_throws_for_InMemory(bool async)
    {
        using var context = new InMemoryQueryContext();
        var query = context.Blogs.TemporalBetween(DateTime.Now, DateTime.Now.AddDays(7));

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(TemporalBetweenQueryRootExpression)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TemporalContainedIn_throws_for_InMemory(bool async)
    {
        using var context = new InMemoryQueryContext();
        var query = context.Blogs.TemporalContainedIn(DateTime.Now, DateTime.Now.AddDays(7));

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(TemporalContainedInQueryRootExpression)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TemporalFromTo_throws_for_InMemory(bool async)
    {
        using var context = new InMemoryQueryContext();
        var query = context.Blogs.TemporalFromTo(DateTime.Now, DateTime.Now.AddDays(7));

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(TemporalFromToQueryRootExpression)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TemporalAsOf_throws_for_Sqlite(bool async)
    {
        using var context = new SqliteQueryContext();
        var query = context.Blogs.TemporalAsOf(DateTime.Now);

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(TemporalAsOfQueryRootExpression)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TemporalAll_throws_for_Sqlite(bool async)
    {
        using var context = new SqliteQueryContext();
        var query = context.Blogs.TemporalAll();

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(TemporalAllQueryRootExpression)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TemporalBetween_throws_for_Sqlite(bool async)
    {
        using var context = new SqliteQueryContext();
        var query = context.Blogs.TemporalBetween(DateTime.Now, DateTime.Now.AddDays(7));

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(TemporalBetweenQueryRootExpression)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TemporalContainedIn_throws_for_Sqlite(bool async)
    {
        using var context = new SqliteQueryContext();
        var query = context.Blogs.TemporalContainedIn(DateTime.Now, DateTime.Now.AddDays(7));

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(TemporalContainedInQueryRootExpression)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TemporalFromTo_throws_for_Sqlite(bool async)
    {
        using var context = new SqliteQueryContext();
        var query = context.Blogs.TemporalFromTo(DateTime.Now, DateTime.Now.AddDays(7));

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal(CoreStrings.QueryUnhandledQueryRootExpression(nameof(TemporalFromToQueryRootExpression)), message);
    }

    private class Blog
    {
        public int Id { get; set; }
        public List<Post> Posts { get; set; }
    }

    private class Post
    {
        public int Id { get; set; }
        public Blog Blog { get; set; }
    }

    private abstract class QueryContextBase : DbContext
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
        }
    }

    private class InMemoryQueryContext : QueryContextBase
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(InMemoryQueryContext));
    }

    private class SqliteQueryContext : QueryContextBase
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlite(((RelationalTestStore)SqliteTestStoreFactory.Instance.Create(nameof(SqliteQueryContext))).ConnectionString);
    }

    private class SqlServerQueryContext : QueryContextBase
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlite(
                    ((RelationalTestStore)SqlServerTestStoreFactory.Instance.Create(nameof(SqlServerQueryContext))).ConnectionString);
    }
}
