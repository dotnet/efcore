// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public abstract class OwnedNavigationsCollectionTestBase<TFixture>(TFixture fixture) : AssociationsCollectionTestBase<TFixture>(fixture)
    where TFixture : OwnedNavigationsFixtureBase, new()
{
    public override Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Distinct_projected(queryTrackingBehavior));

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
