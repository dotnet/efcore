// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public abstract class NorthwindBulkUpdatesFixture<TModelCustomizer> : NorthwindQueryFixtureBase<TModelCustomizer>,
    IBulkUpdatesFixtureBase
    where TModelCustomizer : ITestModelCustomizer, new()
{
    protected override string StoreName
        => "BulkUpdatesNorthwind";

    public abstract void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction);
}
