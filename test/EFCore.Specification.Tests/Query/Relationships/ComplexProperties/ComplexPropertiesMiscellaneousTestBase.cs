// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexProperties;

public abstract class ComplexPropertiesMiscellaneousTestBase<TFixture>(TFixture fixture)
    : RelationshipsMiscellaneousTestBase<TFixture>(fixture)
        where TFixture : ComplexPropertiesFixtureBase, new()
{
    // TODO: the following is temporary until change tracking is implemented for complex JSON types (#35962)
    private readonly TrackingRewriter _trackingRewriter = new(QueryTrackingBehavior.NoTracking);

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        var rewritten = _trackingRewriter.Visit(serverQueryExpression);

        return rewritten;
    }
}
