// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class ComplexNavigationsCollectionsSplitSharedTypeQuerySqlServerTest
    : ComplexNavigationsCollectionsSplitSharedTypeQueryRelationalTestBase<ComplexNavigationsSharedTypeQuerySqlServerFixture>
{
    public ComplexNavigationsCollectionsSplitSharedTypeQuerySqlServerTest(
        ComplexNavigationsSharedTypeQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
