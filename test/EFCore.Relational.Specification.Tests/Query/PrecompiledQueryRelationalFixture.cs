// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;
using static Microsoft.EntityFrameworkCore.TestUtilities.PrecompiledQueryTestHelpers;
using Blog = Microsoft.EntityFrameworkCore.Query.PrecompiledQueryRelationalTestBase.Blog;
using Post = Microsoft.EntityFrameworkCore.Query.PrecompiledQueryRelationalTestBase.Post;
using JsonRoot = Microsoft.EntityFrameworkCore.Query.PrecompiledQueryRelationalTestBase.JsonRoot;
using JsonBranch = Microsoft.EntityFrameworkCore.Query.PrecompiledQueryRelationalTestBase.JsonBranch;

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
        var blog1 = new Blog
        {
            Id = 8,
            Name = "Blog1",
            Json = []
        };
        var blog2 = new Blog
        {
            Id = 9,
            Name = "Blog2",
            Json =
            [
                new JsonRoot
                {
                    Number = 1,
                    Text = "One",
                    Inner = new JsonBranch { Date = new DateTime(2001, 1, 1) }
                },
                new JsonRoot
                {
                    Number = 2,
                    Text = "Two",
                    Inner = new JsonBranch { Date = new DateTime(2002, 2, 2) }
                },
            ]
        };

        context.Blogs.AddRange(blog1, blog2);

        var post11 = new Post
        {
            Id = 11,
            Title = "Post11",
            Blog = blog1
        };
        var post12 = new Post
        {
            Id = 12,
            Title = "Post12",
            Blog = blog1
        };
        var post21 = new Post
        {
            Id = 21,
            Title = "Post21",
            Blog = blog2
        };
        var post22 = new Post
        {
            Id = 22,
            Title = "Post22",
            Blog = blog2
        };
        var post23 = new Post
        {
            Id = 23,
            Title = "Post23",
            Blog = blog2
        };

        context.Posts.AddRange(post11, post12, post21, post22, post23);
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
