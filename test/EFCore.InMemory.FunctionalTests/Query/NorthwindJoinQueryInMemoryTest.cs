// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindJoinQueryInMemoryTest : NorthwindJoinQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
{
    public NorthwindJoinQueryInMemoryTest(
        NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
        ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
        : base(fixture)
    {
        //TestLoggerFactory.TestOutputHelper = testOutputHelper;
    }

    [ConditionalTheory(Skip = "Issue#21200")]
    public override Task SelectMany_with_client_eval(bool async)
        => base.SelectMany_with_client_eval(async);

    [ConditionalTheory(Skip = "Issue#21200")]
    public override Task SelectMany_with_client_eval_with_collection_shaper(bool async)
        => base.SelectMany_with_client_eval_with_collection_shaper(async);

    [ConditionalTheory(Skip = "Issue#21200")]
    public override Task SelectMany_with_client_eval_with_collection_shaper_ignored(bool async)
        => base.SelectMany_with_client_eval_with_collection_shaper_ignored(async);

    [ConditionalTheory(Skip = "Issue#21200")]
    public override Task SelectMany_with_client_eval_with_constructor(bool async)
        => base.SelectMany_with_client_eval_with_constructor(async);
}
