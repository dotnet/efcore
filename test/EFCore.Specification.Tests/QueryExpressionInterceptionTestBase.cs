// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class QueryExpressionInterceptionTestBase : InterceptionTestBase
{
    protected QueryExpressionInterceptionTestBase(InterceptionFixtureBase fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual async Task Intercept_query_passively(bool async, bool inject)
    {
        var (context, interceptor) = await CreateContextAsync<TestQueryExpressionInterceptor>(inject);

        using var _ = context;

        var query = context.Set<Singularity>().Where(e => e.Type == "Black Hole");
        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Single(results);
        Assert.Equal("Black Hole", results[0].Type);

        AssertNormalOutcome(context, interceptor);

        Assert.Contains(@".Where(e => e.Type == ""Black Hole"")", interceptor.QueryExpression);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual async Task Intercept_query_with_multiple_interceptors(bool async, bool inject)
    {
        var interceptor1 = new TestQueryExpressionInterceptor();
        var interceptor2 = new QueryChangingExpressionInterceptor();
        var interceptor3 = new TestQueryExpressionInterceptor();
        var interceptor4 = new TestQueryExpressionInterceptor();

        using var context = await CreateContextAsync(
            new IInterceptor[] { new TestQueryExpressionInterceptor(), interceptor1, interceptor2 },
            new IInterceptor[] { interceptor3, interceptor4, new TestQueryExpressionInterceptor() });

        using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

        var query = context.Set<Singularity>().Where(e => e.Type == "Bing Bang");
        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Single(results);
        Assert.Equal("Bing Bang", results[0].Type);

        AssertNormalOutcome(context, interceptor1);
        AssertNormalOutcome(context, interceptor2);
        AssertNormalOutcome(context, interceptor3);
        AssertNormalOutcome(context, interceptor4);

        listener.AssertEventsInOrder(
            CoreEventId.QueryCompilationStarting.Name,
            CoreEventId.QueryExecutionPlanned.Name);

        _ = async ? await query.ToListAsync() : query.ToList();
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual async Task Intercept_to_change_query_expression(bool async, bool inject)
    {
        var (context, interceptor) = await CreateContextAsync<QueryChangingExpressionInterceptor>(inject);

        using var _ = context;

        var query = context.Set<Singularity>().Where(e => e.Type == "Black Hole");
        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Single(results);
        Assert.Equal("Bing Bang", results[0].Type);

        AssertNormalOutcome(context, interceptor);

        Assert.Contains(@".Where(e => e.Type == ""Bing Bang"")", interceptor.QueryExpression);
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
}
