// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindSetOperationsQueryInMemoryTest : NorthwindSetOperationsQueryTestBase<
    NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
{
    public NorthwindSetOperationsQueryInMemoryTest(
        NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
        ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
        : base(fixture)
    {
        //TestLoggerFactory.TestOutputHelper = testOutputHelper;
    }

    public override async Task Collection_projection_before_set_operation_fails(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Collection_projection_before_set_operation_fails(async))).Message;

        Assert.Equal(InMemoryStrings.SetOperationsNotAllowedAfterClientEvaluation, message);
    }
}
