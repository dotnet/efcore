// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class TestInfrastructureTest
{
    [ConditionalFact]
    public async Task Waits_indefinitely()
        => await Task.Delay(Timeout.InfiniteTimeSpan);
}
