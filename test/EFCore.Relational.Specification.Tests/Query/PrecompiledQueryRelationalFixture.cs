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
            .AddScoped<IQueryCompiler, NonCompilingQueryCompiler>();

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
}
