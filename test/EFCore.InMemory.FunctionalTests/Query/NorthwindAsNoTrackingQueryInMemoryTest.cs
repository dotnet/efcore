// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindAsNoTrackingQueryInMemoryTest : NorthwindAsNoTrackingQueryTestBase<
    NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
{
    public NorthwindAsNoTrackingQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
        : base(fixture)
    {
    }
}
