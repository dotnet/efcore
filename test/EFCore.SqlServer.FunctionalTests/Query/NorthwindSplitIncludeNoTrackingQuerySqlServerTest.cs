// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindSplitIncludeNoTrackingQuerySqlServerTest : NorthwindSplitIncludeNoTrackingQueryTestBase<
    NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindSplitIncludeNoTrackingQuerySqlServerTest(
        NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory(Skip = "Issue#21202")]
    public override async Task Include_collection_skip_take_no_order_by(bool async)
        => await base.Include_collection_skip_take_no_order_by(async);

    [ConditionalTheory(Skip = "Issue#21202")]
    public override async Task Include_collection_skip_no_order_by(bool async)
        => await base.Include_collection_skip_no_order_by(async);
}
