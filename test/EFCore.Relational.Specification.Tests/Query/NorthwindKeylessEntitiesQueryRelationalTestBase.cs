// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindKeylessEntitiesQueryRelationalTestBase<TFixture> : NorthwindKeylessEntitiesQueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindKeylessEntitiesQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Projecting_collection_correlated_with_keyless_entity_throws(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<CustomerQuery>().Select(
                    cq => new
                    {
                        cq.City,
                        cq.CompanyName,
                        OrderDetailIds = ss.Set<Customer>().Where(c => c.City == cq.City).ToList()
                    }).OrderBy(x => x.City).Take(2)))).Message;

        Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Collection_of_entities_projecting_correlated_collection_of_keyless_entities(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(
                    c => new
                    {
                        c.City, Collection = ss.Set<CustomerQuery>().Where(cq => cq.City == c.City).ToList(),
                    })))).Message;

        Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
    }

    public override async Task KeylessEntity_with_included_navs_multi_level(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.KeylessEntity_with_included_navs_multi_level(async))).Message;

        Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
    }

    public override async Task KeylessEntity_with_defining_query_and_correlated_collection(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.KeylessEntity_with_defining_query_and_correlated_collection(async))).Message;

        Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
    }

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
