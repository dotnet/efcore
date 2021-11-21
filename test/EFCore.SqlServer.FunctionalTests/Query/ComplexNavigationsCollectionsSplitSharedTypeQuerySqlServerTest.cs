// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexNavigationsCollectionsSplitSharedTypeQuerySqlServerTest
    : ComplexNavigationsCollectionsSplitSharedTypeQueryRelationalTestBase<ComplexNavigationsSharedTypeQuerySqlServerFixture>
{
    public ComplexNavigationsCollectionsSplitSharedTypeQuerySqlServerTest(
        ComplexNavigationsSharedTypeQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(bool async)
        // Nested collection with ToList. Issue #23303.
        => Assert.ThrowsAsync<ArgumentNullException>(
            () => base.Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(async));
}
