// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public abstract class JsonRelationshipsInProjectionNoTrackingQueryTestBase<TFixture>(TFixture fixture)
    : JsonRelationshipsInProjectionQueryTestBase<TFixture>(fixture)
        where TFixture : JsonRelationshipsQueryFixtureBase, new()
{
    private readonly NoTrackingRewriter _noTrackingRewriter = new();

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        var rewritten = _noTrackingRewriter.Visit(serverQueryExpression);

        return rewritten;
    }
}
