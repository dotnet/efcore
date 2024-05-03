// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;
using static Microsoft.EntityFrameworkCore.TestUtilities.PrecompiledQueryTestHelpers;
using Blog = Microsoft.EntityFrameworkCore.Query.PrecompiledQueryRelationalTestBase.Blog;
namespace Microsoft.EntityFrameworkCore.Query;

public abstract class PrecompiledQueryRelationalFixture
    : SharedStoreFixtureBase<PrecompiledQueryRelationalTestBase.PrecompiledQueryContext>, ITestSqlLoggerFactory
{
    protected override string StoreName
        => "PrecompiledQueryTest";

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            // Bomb if any query is executed that wasn't precompiled
            .AddScoped<IQueryCompiler, NonCompilingQueryCompiler>()
            // Don't pregenerate SQLs to make sure we're testing quotability of SQL expressions
            .AddScoped<IShapedQueryCompilingExpressionVisitorFactory, NonSqlGeneratingShapedQueryCompilingExpressionVisitorFactory>();

    public new RelationalTestStore TestStore
        => (RelationalTestStore)base.TestStore;

    protected override async Task SeedAsync(PrecompiledQueryRelationalTestBase.PrecompiledQueryContext context)
    {
        context.Blogs.AddRange(
            new Blog { Id = 8, Name = "Blog1" },
            new Blog { Id = 9, Name = "Blog2" });
        await context.SaveChangesAsync();
    }

    public abstract PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers { get; }

    public class NonSqlGeneratingShapedQueryCompilingExpressionVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies)
        : IShapedQueryCompilingExpressionVisitorFactory
    {
        public ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
            => new NonSqlGeneratingShapedQueryCompilingExpressionVisitor(
                dependencies,
                relationalDependencies,
                queryCompilationContext);
    }

    /// <summary>
    ///     A replacement for <see cref="RelationalShapedQueryCompilingExpressionVisitor" /> which does not pregenerate
    ///     any SQL, ever. This means that we always generate the SQL as an expression tree in the interceptor, which allows us
    ///     to check that all SQL expressions are properly quotable.
    /// </summary>
    public class NonSqlGeneratingShapedQueryCompilingExpressionVisitor(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : RelationalShapedQueryCompilingExpressionVisitor(dependencies, relationalDependencies, queryCompilationContext)
    {
        protected override int MaxNullableParametersForPregeneratedSql
            => int.MinValue;
    }
}
