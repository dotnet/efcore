// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindEFPropertyIncludeQueryInMemoryTest : NorthwindEFPropertyIncludeQueryTestBase<
    NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
{
    public NorthwindEFPropertyIncludeQueryInMemoryTest(
        NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        //TestLoggerFactory.TestOutputHelper = testOutputHelper;
    }
}
