// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class ComplexProjectionNoTrackingSqliteTest
    : ComplexProjectionNoTrackingRelationalTestBase<ComplexRelationshipsSqliteFixture>
{
    public ComplexProjectionNoTrackingSqliteTest(ComplexRelationshipsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
