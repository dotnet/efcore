// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

// Only adding NoTracking version - no point to do both and most of the tests don't work in tracking (projecting without owner)
[SqlServerCondition(SqlServerCondition.SupportsJsonType)]
public class JsonTypeRelationshipsInProjectionNoTrackingQuerySqlServerTest
    : RelationshipsInProjectionQueryTestBase<JsonTypeRelationshipsQuerySqlServerFixture>
{
    private readonly NoTrackingRewriter _noTrackingRewriter = new();

    public JsonTypeRelationshipsInProjectionNoTrackingQuerySqlServerTest(JsonTypeRelationshipsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        var rewritten = _noTrackingRewriter.Visit(serverQueryExpression);

        return rewritten;
    }
}
