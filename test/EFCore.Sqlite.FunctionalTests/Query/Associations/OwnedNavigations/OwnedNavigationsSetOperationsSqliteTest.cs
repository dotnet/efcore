// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsSetOperationsSqliteTest(OwnedNavigationsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedNavigationsSetOperationsRelationalTestBase<OwnedNavigationsSqliteFixture>(fixture, testOutputHelper)
{
    // SQL APPLY not supported in SQLite - different exception message from the one expected in the base class
    public override Task On_related_projected(QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<EqualException>(() => base.On_related_projected(queryTrackingBehavior));
}
