// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedTableSplitting;

public abstract class OwnedTableSplittingProjectionRelationalTestBase<TFixture>
    : OwnedNavigationsProjectionTestBase<TFixture>
        where TFixture : OwnedTableSplittingRelationalFixtureBase, new()
{
    public OwnedTableSplittingProjectionRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // // The following are tests projecting out collections, which aren't supported with table splitting.
    // // Collection properties are ignored in the model, and since these tests project we client-eval and get an assertion failure.
    // public override Task Select_related_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    //     => Assert.ThrowsAsync<FailException>(() => base.Select_related_collection(async, queryTrackingBehavior));

    // public override Task Select_optional_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    //     => Assert.ThrowsAsync<FailException>(() => base.Select_optional_related_nested_collection(async, queryTrackingBehavior));
}
