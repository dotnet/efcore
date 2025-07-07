// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class QueryExpressionInterceptionTestBase(InterceptionTestBase.InterceptionFixtureBase fixture)
    : InterceptionTestBase(fixture)
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Intercept_query_passively(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<TestQueryExpressionInterceptor>(inject: true);

        using var _ = context;

        var query = context.Set<Singularity>().Where(e => e.Type == "Black Hole");
        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal("Black Hole", result.Type);

        AssertNormalOutcome(context, interceptor);

        Assert.Contains(""".Where(e => e.Type == "Black Hole")""", interceptor.QueryExpression);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Intercept_query_with_multiple_interceptors(bool async)
    {
        var interceptor1 = new TestQueryExpressionInterceptor();
        var interceptor2 = new QueryChangingExpressionInterceptor();

        using var context = await CreateContextAsync(
            appInterceptor: null,
            [interceptor1, interceptor2]);

        using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

        var query = context.Set<Singularity>().Where(e => e.Type == "Bing Bang");
        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal("Bing Bang", result.Type);

        AssertNormalOutcome(context, interceptor1);
        AssertNormalOutcome(context, interceptor2);

        listener.AssertEventsInOrder(
            CoreEventId.QueryCompilationStarting.Name,
            CoreEventId.QueryExecutionPlanned.Name);

        _ = async ? await query.ToListAsync() : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Intercept_to_change_query_expression(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<QueryChangingExpressionInterceptor>(inject: true);

        using var _ = context;

        var query = context.Set<Singularity>().Where(e => e.Type == "Black Hole");
        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal("Bing Bang", result.Type);

        AssertNormalOutcome(context, interceptor);

        Assert.Contains(""".Where(e => e.Type == "Bing Bang")""", interceptor.QueryExpression);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Interceptor_does_not_leak_across_contexts(bool async)
    {
        // Create one context with QueryChangingExpressionInterceptor, and another with TestQueryExpressionInterceptor (which is a no-op).
        // Note that we don't use the regular suite infra for creating the contexts, as that creates separate service providers for each
        // one, but that's exactly what we want to test here.
        using var context1 = new UniverseContext(
            Fixture.AddOptions(
                Fixture.TestStore.AddProviderOptions(
                    new DbContextOptionsBuilder<DbContext>().AddInterceptors(new QueryChangingExpressionInterceptor())))
            .Options);
        using var context2 = new UniverseContext(
            Fixture.AddOptions(
                Fixture.TestStore.AddProviderOptions(
                    new DbContextOptionsBuilder<DbContext>().AddInterceptors(new TestQueryExpressionInterceptor())))
            .Options);

        var query1 = context1.Set<Singularity>().Where(e => e.Type == "Black Hole");
        var result1 = async ? await query1.SingleAsync() : query1.Single();
        Assert.Equal("Bing Bang", result1.Type);

        var query2 = context2.Set<Singularity>().Where(e => e.Type == "Black Hole");
        var result2 = async ? await query2.SingleAsync() : query2.Single();
        Assert.Equal("Black Hole", result2.Type);
    }

    protected class QueryChangingExpressionInterceptor : TestQueryExpressionInterceptor
    {
        public override Expression QueryCompilationStarting(
            Expression queryExpression,
            QueryExpressionEventData eventData)
            => base.QueryCompilationStarting(new SingularityTypeExpressionVisitor().Visit(queryExpression), eventData);

        private class SingularityTypeExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitBinary(BinaryExpression node)
                => node.Right is ConstantExpression { Value: "Black Hole" }
                    ? Expression.Equal(node.Left, Expression.Constant("Bing Bang"))
                    : base.VisitBinary(node);
        }
    }

    protected static void AssertNormalOutcome(DbContext context, TestQueryExpressionInterceptor interceptor)
    {
        Assert.Same(context, interceptor.Context);
        Assert.True(interceptor.QueryCompilationStartingCalled);
        Assert.True(interceptor.QueryCompilationStartingCalled);
    }

    protected class TestQueryExpressionInterceptor : IQueryExpressionInterceptor
    {
        public bool QueryCompilationStartingCalled { get; set; }
        public string QueryExpression { get; set; }
        public DbContext Context { get; set; }

        public virtual Expression QueryCompilationStarting(
            Expression queryExpression,
            QueryExpressionEventData eventData)
        {
            QueryCompilationStartingCalled = true;
            Context = eventData.Context;
            QueryExpression = eventData.ExpressionPrinter.PrintExpression(queryExpression);

            return queryExpression;
        }
    }

    public static readonly IEnumerable<object[]> IsAsyncData = [[false], [true]];
}
