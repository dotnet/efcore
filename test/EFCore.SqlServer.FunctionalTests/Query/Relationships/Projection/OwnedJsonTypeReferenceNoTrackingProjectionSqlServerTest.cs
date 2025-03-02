// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

// Only adding NoTracking version - no point to do both and most of the tests don't work in tracking (projecting without owner)
[SqlServerCondition(SqlServerCondition.SupportsJsonType)]
public class OwnedJsonTypeReferenceNoTrackingProjectionSqlServerTest
    : ReferenceProjectionTestBase<OwnedJsonTypeRelationshipsSqlServerFixture>
{
    public OwnedJsonTypeReferenceNoTrackingProjectionSqlServerTest(OwnedJsonTypeRelationshipsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
