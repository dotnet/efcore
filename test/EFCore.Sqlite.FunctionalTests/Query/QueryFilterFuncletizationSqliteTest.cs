// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query;

public class QueryFilterFuncletizationSqliteTest : QueryFilterFuncletizationTestBase<
    QueryFilterFuncletizationSqliteTest.QueryFilterFuncletizationSqliteFixture>
{
    public QueryFilterFuncletizationSqliteTest(
        QueryFilterFuncletizationSqliteFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override void DbContext_list_is_parameterized()
    {
        using var context = CreateContext();
        // Default value of TenantIds is null InExpression over null values throws
        Assert.Throws<NullReferenceException>(() => context.Set<ListFilter>().ToList());

        context.TenantIds = new List<int>();
        var query = context.Set<ListFilter>().ToList();
        Assert.Empty(query);

        context.TenantIds = new List<int> { 1 };
        query = context.Set<ListFilter>().ToList();
        Assert.Single(query);

        context.TenantIds = new List<int> { 2, 3 };
        query = context.Set<ListFilter>().ToList();
        Assert.Equal(2, query.Count);
    }

    public class QueryFilterFuncletizationSqliteFixture : QueryFilterFuncletizationRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
