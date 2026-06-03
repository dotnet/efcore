// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class QueryFilterFuncletizationInMemoryTest(QueryFilterFuncletizationInMemoryTest.QueryFilterFuncletizationInMemoryFixture fixture)
    : QueryFilterFuncletizationTestBase<QueryFilterFuncletizationInMemoryTest.QueryFilterFuncletizationInMemoryFixture>(fixture)
{
    public override void DbContext_list_is_parameterized()
    {
        using var context = CreateContext();
        // Default value of TenantIds is null InExpression over null values throws
        Assert.Throws<ArgumentNullException>(() => context.Set<ListFilter>().ToList());

        context.TenantIds = [];
        var query = context.Set<ListFilter>().ToList();
        Assert.Empty(query);

        context.TenantIds = [1];
        query = context.Set<ListFilter>().ToList();
        Assert.Single(query);

        context.TenantIds = [2, 3];
        query = context.Set<ListFilter>().ToList();
        Assert.Equal(2, query.Count);
    }

    public class QueryFilterFuncletizationInMemoryFixture : QueryFilterFuncletizationFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
