// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public abstract class OwnedJsonNoTrackingProjectionTestBase<TFixture>(TFixture fixture)
    : OwnedJsonProjectionTestBase<TFixture>(fixture)
        where TFixture : OwnedJsonRelationshipsFixtureBase, new()
{
    private readonly TrackingRewriter _trackingRewriter = new();

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        var rewritten = _trackingRewriter.Visit(serverQueryExpression);

        return rewritten;
    }
}
