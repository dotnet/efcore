// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

    public override Task SelectMany_with_client_eval(bool async)
        // Joins between sources with client eval. Issue #21200.
        => Assert.ThrowsAsync<NotImplementedException>(() => base.SelectMany_with_client_eval(async));

    public override Task SelectMany_with_client_eval_with_collection_shaper(bool async)
        // Joins between sources with client eval. Issue #21200.
        => Assert.ThrowsAsync<NotImplementedException>(() => base.SelectMany_with_client_eval_with_collection_shaper(async));

    public override Task SelectMany_with_client_eval_with_collection_shaper_ignored(bool async)
        // Joins between sources with client eval. Issue #21200.
        => Assert.ThrowsAsync<NotImplementedException>(() => base.SelectMany_with_client_eval_with_collection_shaper_ignored(async));

    public override Task SelectMany_with_client_eval_with_constructor(bool async)
        // Joins between sources with client eval. Issue #21200.
        => Assert.ThrowsAsync<NotImplementedException>(() => base.SelectMany_with_client_eval_with_constructor(async));
}
