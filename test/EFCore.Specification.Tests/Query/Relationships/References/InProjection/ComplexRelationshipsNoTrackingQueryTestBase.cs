// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.References.InProjection;

public abstract class ComplexRelationshipsInProjectionNoTrackingQueryTestBase<TFixture>(TFixture fixture)
    : ComplexRelationshipsInProjectionQueryTestBase<TFixture>(fixture)
        where TFixture : ComplexRelationshipsQueryFixtureBase, new()
{
    private readonly TrackingRewriter _trackingRewriter = new();

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        var rewritten = _trackingRewriter.Visit(serverQueryExpression);

        return rewritten;
    }
}
