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

    public override async Task Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(bool async)
        // Nested collection with ToList. Issue #23303.
        => await Assert.ThrowsAsync<ArgumentNullException>(
            () => base.Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(async));
}
