// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexNavigationsCollectionsQueryInMemoryTest
    : ComplexNavigationsCollectionsQueryTestBase<ComplexNavigationsQueryInMemoryFixture>
{
    public ComplexNavigationsCollectionsQueryInMemoryTest(
        ComplexNavigationsQueryInMemoryFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        //TestLoggerFactory.TestOutputHelper = testOutputHelper;
    }
}
