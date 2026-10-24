// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ExecuteMergeModel;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#pragma warning disable EF9107 // ExecuteMerge is experimental

public abstract class ExecuteMergeTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : ExecuteMergeFixtureBase, new()
{
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Do_nothing_inserts_new_and_ignores_conflict(bool async)
        => AssertMerge(
            async context =>
            {
                var affected = await Merge(
                    async, context.Blogs,
                    [new Blog { Id = 1, Name = "ignored" }, new Blog { Id = 3, Name = "Blog3" }],
                    merge => { });

                Assert.Equal(1, affected);
                var blogs = await context.Blogs.OrderBy(b => b.Id).ToListAsync();
                Assert.Equal(["Blog1", "Blog2", "Blog3"], blogs.Select(b => b.Name));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Do_nothing_match_on_unique_column(bool async)
        => AssertMerge(
            async context =>
            {
                var affected = await Merge(
                    async, context.Blogs,
                    [new Blog { Id = 5, Name = "Blog1" }, new Blog { Id = 3, Name = "Blog3" }],
                    merge => merge.Match(t => t.Name, s => s.Name));

                Assert.Equal(1, affected);
                var blogs = await context.Blogs.OrderBy(b => b.Id).ToListAsync();
                Assert.Equal([1, 2, 3], blogs.Select(b => b.Id));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_updates_and_inserts(bool async)
        => AssertMerge(
            async context =>
            {
                var affected = await Merge(
                    async, context.Visits,
                    [new DailyVisit { UserId = 1, Day = "mon", Visits = 3 }, new DailyVisit { UserId = 2, Day = "tue", Visits = 7 }],
                    merge => merge
                        .Match(t => new { t.UserId, t.Day }, s => new { s.UserId, s.Day })
                        .WhenMatched(u => u.SetProperty(t => t.Visits, (t, s) => s.Visits)));

                Assert.Equal(2, affected);
                var visits = await context.Visits.OrderBy(v => v.UserId).ToListAsync();
                Assert.Equal([3, 7], visits.Select(v => v.Visits));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task With_multiple_match_calls(bool async)
        => AssertMerge(
            async context =>
            {
                var affected = await Merge(
                    async, context.Visits,
                    [new DailyVisit { UserId = 1, Day = "mon", Visits = 3 }, new DailyVisit { UserId = 2, Day = "tue", Visits = 7 }],
                    merge => merge
                        .Match(t => t.UserId, s => s.UserId)
                        .Match(t => t.Day, s => s.Day)
                        .WhenMatched(u => u.SetProperty(t => t.Visits, (t, s) => s.Visits)));

                Assert.Equal(2, affected);
                var visits = await context.Visits.OrderBy(v => v.UserId).ToListAsync();
                Assert.Equal([3, 7], visits.Select(v => v.Visits));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Match_uses_source_key_selector(bool async)
        => AssertMerge(
            async context =>
            {
                // The Id value must come from ExternalId, otherwise the first row wouldn't match the existing account.
                var affected = await Merge(
                    async, context.Accounts,
                    [new Account { Id = 999, ExternalId = 1, Balance = 50 }, new Account { Id = 888, ExternalId = 2, Balance = 70 }],
                    merge => merge
                        .Match(t => t.Id, s => s.ExternalId)
                        .WhenMatched(u => u.SetProperty(t => t.Balance, (t, s) => s.Balance)));

                Assert.Equal(2, affected);
                var accounts = await context.Accounts.OrderBy(a => a.Id).ToListAsync();
                Assert.Equal([1, 2], accounts.Select(a => a.Id));
                Assert.Equal([50, 70], accounts.Select(a => a.Balance));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_reads_custom_match_source_member(bool async)
        => AssertMerge(
            async context =>
            {
                // s.ExternalId is inserted unchanged into the Id match column, so WhenMatched can read it.
                var affected = await Merge(
                    async, context.Accounts,
                    [new Account { Id = 999, ExternalId = 1, Balance = 50 }, new Account { Id = 888, ExternalId = 2, Balance = 70 }],
                    merge => merge
                        .Match(t => t.Id, s => s.ExternalId)
                        .WhenMatched(u => u.SetProperty(t => t.ExternalId, (t, s) => s.ExternalId)));

                Assert.Equal(2, affected);
                var accounts = await context.Accounts.OrderBy(a => a.Id).ToListAsync();
                Assert.Equal([1, 2], accounts.Select(a => a.Id));
                Assert.Equal([1, 2], accounts.Select(a => a.ExternalId));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_not_matched_uses_explicit_insert_setters(bool async)
        => AssertMerge(
            async context =>
            {
                var affected = await Merge(
                    async, context.Blogs,
                    [new Blog { Id = 1, Name = "ignored" }, new Blog { Id = 3, Name = "Blog3", Note = "not inserted" }],
                    merge => merge.WhenNotMatched(insert => insert
                        .SetProperty(t => t.Id, s => s.Id)
                        .SetProperty(t => t.Name, s => s.Name)));

                Assert.Equal(1, affected);
                var blog = await context.Blogs.SingleAsync(b => b.Id == 3);
                Assert.Equal("Blog3", blog.Name);
                Assert.Null(blog.Note);
                Assert.Equal("Blog1", (await context.Blogs.SingleAsync(b => b.Id == 1)).Name);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_and_when_not_matched_together(bool async)
        => AssertMerge(
            async context =>
            {
                var affected = await Merge(
                    async, context.Blogs,
                    [new Blog { Id = 1, Name = "Updated" }, new Blog { Id = 3, Name = "Blog3" }],
                    merge => merge
                        .WhenMatched(u => u.SetProperty(t => t.Name, (t, s) => s.Name))
                        .WhenNotMatched(insert => insert
                            .SetProperty(t => t.Id, s => s.Id)
                            .SetProperty(t => t.Name, s => s.Name)
                            .SetProperty(t => t.Note, "INS")));

                Assert.Equal(2, affected);
                var blogs = await context.Blogs.Where(b => b.Id != 2).OrderBy(b => b.Id).ToListAsync();
                Assert.Equal(["Updated", "Blog3"], blogs.Select(b => b.Name));
                Assert.Equal([null, "INS"], blogs.Select(b => b.Note));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_reads_dto_only_source_member(bool async)
        => AssertMerge(
            async context =>
            {
                // Title only exists on the DTO; since it's inserted unchanged into Name, WhenMatched can reference it.
                var affected = await Merge(
                    async, context.Blogs,
                    new BlogImport[] { new() { Id = 1, Title = "Updated" }, new() { Id = 3, Title = "Blog3" } },
                    merge => merge
                        .WhenMatched(u => u.SetProperty(t => t.Name, (t, s) => s.Title))
                        .WhenNotMatched(insert => insert
                            .SetProperty(t => t.Id, s => s.Id)
                            .SetProperty(t => t.Name, s => s.Title)));

                Assert.Equal(2, affected);
                var blogs = await context.Blogs.OrderBy(b => b.Id).ToListAsync();
                Assert.Equal(["Updated", "Blog2", "Blog3"], blogs.Select(b => b.Name));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Source_member_used_for_converted_and_unconverted_properties(bool async)
        => AssertMerge(
            async context =>
            {
                // s.Value is inserted both through ConvertedValue's value converter and as-is into Value, so each WhenMatched setter
                // must read it back from the inserted column with its own conversion.
                var affected = await Merge(
                    async, context.Measurements,
                    [new Measurement { Id = 1, Value = 5 }, new Measurement { Id = 2, Value = 7 }],
                    merge => merge
                        .WhenMatched(u => u
                            .SetProperty(t => t.Value, (t, s) => s.Value)
                            .SetProperty(t => t.ConvertedValue, (t, s) => s.Value))
                        .WhenNotMatched(insert => insert
                            .SetProperty(t => t.Id, s => s.Id)
                            .SetProperty(t => t.ConvertedValue, s => s.Value)
                            .SetProperty(t => t.Value, s => s.Value)));

                Assert.Equal(2, affected);
                var measurements = await context.Measurements.OrderBy(m => m.Id).ToListAsync();
                Assert.Equal([5, 7], measurements.Select(m => m.Value));
                Assert.Equal([5, 7], measurements.Select(m => m.ConvertedValue));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task With_empty_when_matched_leaves_matched_rows_unchanged(bool async)
        => AssertMerge(
            async context =>
            {
                var affected = await Merge(
                    async, context.Blogs,
                    [new Blog { Id = 1, Name = "ignored" }, new Blog { Id = 3, Name = "Blog3" }],
                    merge => merge.WhenMatched(_ => { }));

                Assert.Equal(1, affected);
                var blogs = await context.Blogs.OrderBy(b => b.Id).ToListAsync();
                Assert.Equal(["Blog1", "Blog2", "Blog3"], blogs.Select(b => b.Name));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_with_constant_value(bool async)
        => AssertMerge(
            async context =>
            {
                var affected = await Merge(
                    async, context.Visits,
                    [new DailyVisit { UserId = 1, Day = "mon", Visits = 3 }],
                    merge => merge
                        .Match(t => new { t.UserId, t.Day }, s => new { s.UserId, s.Day })
                        .WhenMatched(u => u.SetProperty(t => t.Visits, 0)));

                Assert.Equal(1, affected);
                Assert.Equal(0, (await context.Visits.SingleAsync()).Visits);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_not_matched_with_constant_value(bool async)
        => AssertMerge(
            async context =>
            {
                var affected = await Merge(
                    async, context.Blogs,
                    [new Blog { Id = 3, Name = "ignored" }],
                    merge => merge.WhenNotMatched(insert => insert
                        .SetProperty(t => t.Id, s => s.Id)
                        .SetProperty(t => t.Name, "CONST")));

                Assert.Equal(1, affected);
                Assert.Equal("CONST", (await context.Blogs.SingleAsync(b => b.Id == 3)).Name);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Default_insert_reads_same_named_members_from_dto_source(bool async)
        => AssertMerge(
            async context =>
            {
                var affected = await Merge(
                    async, context.Blogs,
                    new BlogRow[] { new() { Id = 1, Name = "ignored" }, new() { Id = 3, Name = "Blog3", Note = "hi" } },
                    merge => { });

                Assert.Equal(1, affected);
                var blog = await context.Blogs.SingleAsync(b => b.Id == 3);
                Assert.Equal("Blog3", blog.Name);
                Assert.Equal("hi", blog.Note);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task With_empty_source(bool async)
        => AssertMerge(
            async context =>
            {
                var affected = await Merge(async, context.Blogs, Array.Empty<Blog>(), merge => { });

                Assert.Equal(0, affected);
                Assert.Equal(2, await context.Blogs.CountAsync());
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Reused_source_collection_uses_current_values(bool async)
        => AssertMerge(
            async context =>
            {
                var source = new List<Blog> { new() { Id = 3, Name = "Blog3" } };
                Assert.Equal(1, await Merge(async, context.Blogs, source, merge => { }));

                // The same collection instance with different contents must not reuse the rows from the first execution.
                source.Clear();
                source.Add(new Blog { Id = 4, Name = "Blog4" });
                source.Add(new Blog { Id = 5, Name = "Blog5" });
                Assert.Equal(2, await Merge(async, context.Blogs, source, merge => { }));

                var blogs = await context.Blogs.OrderBy(b => b.Id).ToListAsync();
                Assert.Equal([1, 2, 3, 4, 5], blogs.Select(b => b.Id));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Queryable_source_inserts_rows(bool async)
        => AssertMerge(
            async context =>
            {
                var affected = await Merge(
                    async, context.Blogs, new[] { new Blog { Id = 3, Name = "Blog3" } }.AsQueryable(), merge => { });

                Assert.Equal(1, affected);
                Assert.Equal("Blog3", (await context.Blogs.SingleAsync(b => b.Id == 3)).Name);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Query_source_throws(bool async)
        => AssertMerge(
            async context =>
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => Merge(async, context.Blogs, context.Blogs.Where(b => b.Id > 0), merge => { }));

                Assert.Equal(CoreStrings.ExecuteMergeQuerySource, exception.Message);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Returning_with_queryable_source(bool async)
        => AssertMerge(
            async context =>
            {
                var ids = await MergeReturning(
                    async, context.Blogs, new[] { new Blog { Id = 3, Name = "Blog3" } }.AsQueryable(), merge => { }, t => t.Id);

                Assert.Equal([3], ids);
            });

    [Fact]
    public virtual Task Returning_does_not_enumerate_source_until_executed()
        => AssertMerge(
            context =>
            {
                var source = new List<Blog> { new() { Id = 3, Name = "Blog3" } };
                var results = context.Blogs.ExecuteMergeReturning(source.AsQueryable(), merge => { }, t => t.Id);

                // The row added after the call must still be merged, since the source is only read when the results are enumerated.
                source.Add(new Blog { Id = 4, Name = "Blog4" });

                Assert.Equal([3, 4], results.ToList().Order());
                return Task.CompletedTask;
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Returning_projects_affected_rows(bool async)
        => AssertMerge(
            async context =>
            {
                var results = await MergeReturning(
                    async, context.Visits,
                    [new DailyVisit { UserId = 1, Day = "mon", Visits = 3 }, new DailyVisit { UserId = 2, Day = "tue", Visits = 7 }],
                    merge => merge
                        .Match(t => new { t.UserId, t.Day }, s => new { s.UserId, s.Day })
                        .WhenMatched(u => u.SetProperty(t => t.Visits, (t, s) => s.Visits)),
                    t => new { t.UserId, t.Visits });

                Assert.Equal([(1, 3), (2, 7)], results.OrderBy(r => r.UserId).Select(r => (r.UserId, r.Visits)));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Returning_projects_scalars(bool async)
        => AssertMerge(
            async context =>
            {
                var ids = await MergeReturning(
                    async, context.Blogs,
                    [new Blog { Id = 1, Name = "Updated" }, new Blog { Id = 3, Name = "Blog3" }],
                    merge => merge.WhenMatched(u => u.SetProperty(t => t.Name, (t, s) => s.Name)),
                    t => t.Id);

                Assert.Equal([1, 3], ids.Order());
                Assert.Equal("Updated", (await context.Blogs.SingleAsync(b => b.Id == 1)).Name);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Returning_with_do_nothing_returns_only_inserted_rows(bool async)
        => AssertMerge(
            async context =>
            {
                var ids = await MergeReturning(
                    async, context.Blogs,
                    [new Blog { Id = 1, Name = "Blog1" }, new Blog { Id = 3, Name = "Blog3" }],
                    merge => { },
                    t => t.Id);

                Assert.Equal([3], ids);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Returning_reads_null_column_as_default(bool async)
        => AssertMerge(
            async context =>
            {
                var notes = await MergeReturning(
                    async, context.Blogs,
                    [new Blog { Id = 3, Name = "Blog3" }],
                    merge => { },
                    t => t.Note);

                Assert.Null(Assert.Single(notes));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Returning_with_buffering(bool async)
        => AssertMerge(
            async context =>
            {
                // A retrying execution strategy makes the query buffer the returned rows.
                var results = await new BufferingExecutionStrategy(context).ExecuteAsync(
                    () => MergeReturning(
                        async, context.Blogs,
                        [new Blog { Id = 1, Name = "Updated" }, new Blog { Id = 3, Name = "Blog3" }],
                        merge => merge.WhenMatched(u => u.SetProperty(t => t.Name, (t, s) => s.Name)),
                        t => new { t.Id, t.Name, t.Note }));

                Assert.Equal([(1, "Updated"), (3, "Blog3")], results.OrderBy(r => r.Id).Select(r => (r.Id, r.Name)));
                Assert.All(results, r => Assert.Null(r.Note));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Empty_match_throws(bool async)
        => AssertMerge(
            async context =>
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => Merge(
                        async, context.Blogs,
                        [new Blog { Id = 3, Name = "Blog3" }],
                        merge => merge.Match(t => new { }, s => new { })));

                Assert.Equal(CoreStrings.ExecuteMergeEmptyMatch, exception.InnerException!.Message);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_when_matched_throws(bool async)
        => AssertMerge(
            async context =>
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => Merge(
                        async, context.Blogs,
                        [new Blog { Id = 1, Name = "x" }],
                        merge => merge
                            .WhenMatched(u => u.SetProperty(t => t.Name, (t, s) => s.Name))
                            .WhenMatched(u => u.SetProperty(t => t.Note, "y"))));

                Assert.Equal(CoreStrings.ExecuteMergeDuplicateClause("WhenMatched"), exception.Message);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_when_not_matched_throws(bool async)
        => AssertMerge(
            async context =>
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => Merge(
                        async, context.Blogs,
                        [new Blog { Id = 3, Name = "x" }],
                        merge => merge
                            .WhenNotMatched(insert => insert.SetProperty(t => t.Id, s => s.Id).SetProperty(t => t.Name, s => s.Name))
                            .WhenNotMatched(insert => insert.SetProperty(t => t.Id, s => s.Id).SetProperty(t => t.Name, "y"))));

                Assert.Equal(CoreStrings.ExecuteMergeDuplicateClause("WhenNotMatched"), exception.Message);
            });

    protected virtual Task AssertMerge(Func<ExecuteMergeContext, Task> test)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(Fixture.CreateContext, Fixture.UseTransaction, test);

    protected static async Task<int> Merge<TTarget, TSource>(
        bool async,
        IQueryable<TTarget> target,
        IEnumerable<TSource> source,
        Action<MergeBuilder<TTarget, TSource>> buildMerge)
        => async
            ? await target.ExecuteMergeAsync(source, buildMerge)
            : target.ExecuteMerge(source, buildMerge);

    protected static async Task<List<TResult>> MergeReturning<TTarget, TSource, TResult>(
        bool async,
        IQueryable<TTarget> target,
        IEnumerable<TSource> source,
        Action<MergeBuilder<TTarget, TSource>> buildMerge,
        Expression<Func<TTarget, TResult>> returning)
    {
        if (!async)
        {
            return target.ExecuteMergeReturning(source, buildMerge, returning).ToList();
        }

        var results = new List<TResult>();
        await foreach (var result in target.ExecuteMergeReturningAsync(source, buildMerge, returning))
        {
            results.Add(result);
        }

        return results;
    }

    // Retries on failure, which makes queries buffer their results, but never actually retries, so it can run in the test's transaction.
    protected sealed class BufferingExecutionStrategy(DbContext context)
        : ExecutionStrategy(context, maxRetryCount: 1, maxRetryDelay: TimeSpan.Zero)
    {
        protected override void OnFirstExecution()
        {
        }

        protected override bool ShouldRetryOn(Exception exception)
            => false;
    }
}
