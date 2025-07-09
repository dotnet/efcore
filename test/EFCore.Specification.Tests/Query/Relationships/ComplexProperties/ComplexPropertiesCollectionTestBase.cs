// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexProperties;

public abstract class ComplexPropertiesCollectionTestBase<TFixture>(TFixture fixture)
    : RelationshipsCollectionTestBase<TFixture>(fixture)
    where TFixture : ComplexPropertiesFixtureBase, new()
{
    // Complex collection indexing currently fails because SubqueryMemberPushdownExpressionVisitor moves the Int member access to before the
    // ElementAt (making a Select()), this interferes with our translation. See #36335.
    public override Task Index_constant(bool async)
        => Assert.ThrowsAsync<EqualException>(() => base.Index_constant(async));

    public override Task Index_parameter(bool async)
        => Assert.ThrowsAsync<EqualException>(() => base.Index_parameter(async));

    public override Task Index_column(bool async)
        => Assert.ThrowsAsync<EqualException>(() => base.Index_column(async));

    // public override Task Index_out_of_bounds()
    //     => Assert.ThrowsAsync<EqualException>(() => base.Index_out_of_bounds());

    // TODO: the following is temporary until change tracking is implemented for complex JSON types (#35962)
    private readonly TrackingRewriter _trackingRewriter = new(QueryTrackingBehavior.NoTracking);

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        var rewritten = _trackingRewriter.Visit(serverQueryExpression);

        return rewritten;
    }
}
