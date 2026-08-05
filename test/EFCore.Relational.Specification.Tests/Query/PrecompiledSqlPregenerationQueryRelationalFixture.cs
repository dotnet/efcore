// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;
using Blog = Microsoft.EntityFrameworkCore.Query.PrecompiledSqlPregenerationQueryRelationalTestBase.Blog;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class PrecompiledSqlPregenerationQueryRelationalFixture
    : SharedStoreFixtureBase<PrecompiledSqlPregenerationQueryRelationalTestBase.PrecompiledQueryContext>, ITestSqlLoggerFactory
{
    protected override string StoreName
        => "PrecompiledSqlPregenerationQueryTest";

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            // Bomb if any query is executed that wasn't precompiled
            .AddScoped<IQueryCompiler, PrecompiledQueryTestHelpers.NonCompilingQueryCompiler>();

    protected override async Task SeedAsync(PrecompiledSqlPregenerationQueryRelationalTestBase.PrecompiledQueryContext context)
    {
        context.Blogs.AddRange(
            new Blog { Id = 8, Name = "Blog1" },
            new Blog { Id = 9, Name = "Blog2" });
        await context.SaveChangesAsync();
    }

    public abstract PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers { get; }
}
