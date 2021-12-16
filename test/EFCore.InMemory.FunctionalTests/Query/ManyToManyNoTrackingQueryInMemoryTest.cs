// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ManyToManyNoTrackingQueryInMemoryTest
    : ManyToManyNoTrackingQueryTestBase<ManyToManyQueryInMemoryFixture>
{
    public ManyToManyNoTrackingQueryInMemoryTest(ManyToManyQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        //TestLoggerFactory.TestOutputHelper = testOutputHelper;
    }
}
