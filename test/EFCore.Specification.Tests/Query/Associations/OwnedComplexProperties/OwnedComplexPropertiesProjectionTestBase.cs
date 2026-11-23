// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public abstract class OwnedComplexPropertiesProjectionTestBase<TFixture>(TFixture fixture)
    : AssociationsProjectionTestBase<TFixture>(fixture)
    where TFixture : OwnedComplexPropertiesFixtureBase, new()
{
    public override Task Select_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_associate(queryTrackingBehavior));

    public override Task Select_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_optional_associate(queryTrackingBehavior));

    public override Task Select_unmapped_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_unmapped_associate_scalar_property(queryTrackingBehavior));

    public override Task Select_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_associate_collection(queryTrackingBehavior));

    public override Task SelectMany_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.SelectMany_associate_collection(queryTrackingBehavior));

    public override Task Select_associate_and_target_to_index_based_binding_via_closure(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_associate_and_target_to_index_based_binding_via_closure(queryTrackingBehavior));

    public override Task Select_required_associate_duplicated(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_required_associate_duplicated(queryTrackingBehavior));

    public override Task Select_required_associate_and_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_required_associate_and_optional_associate(queryTrackingBehavior));

    public override Task Select_optional_associate_and_ints(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_optional_associate_and_ints(queryTrackingBehavior));

    public override Task Select_subquery_FirstOrDefault_complex_collection(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_subquery_FirstOrDefault_complex_collection(queryTrackingBehavior));

    protected virtual async Task AssertOwnedTrackingQuery(QueryTrackingBehavior queryTrackingBehavior, Func<Task> test)
    {
        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message;

            Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);

            return;
        }

        await test();
    }
}
