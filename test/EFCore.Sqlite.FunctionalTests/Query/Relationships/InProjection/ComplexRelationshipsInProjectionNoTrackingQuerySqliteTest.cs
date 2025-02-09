// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public class ComplexRelationshipsInProjectionNoTrackingQuerySqliteTest
    : ComplexRelationshipsInProjectionNoTrackingQueryRelationalTestBase<ComplexRelationshipsQuerySqliteFixture>
{
    public ComplexRelationshipsInProjectionNoTrackingQuerySqliteTest(ComplexRelationshipsQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
