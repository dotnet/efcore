// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexTableSplitting;

public class ComplexTableSplittingProjectionSqliteTest
    : ComplexTableSplittingProjectionRelationalTestBase<ComplexTableSplittingSqliteFixture>
{
    public ComplexTableSplittingProjectionSqliteTest(ComplexTableSplittingSqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
