// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindQueryFiltersQueryInMemoryTest : NorthwindQueryFiltersQueryTestBase<
    NorthwindQueryInMemoryFixture<NorthwindQueryFiltersCustomizer>>
{
    public NorthwindQueryFiltersQueryInMemoryTest(
        NorthwindQueryInMemoryFixture<NorthwindQueryFiltersCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        //TestLoggerFactory.TestOutputHelper = testOutputHelper;
    }
}
