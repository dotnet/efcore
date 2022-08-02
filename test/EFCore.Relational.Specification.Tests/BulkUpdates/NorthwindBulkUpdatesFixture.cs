﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public abstract class NorthwindBulkUpdatesFixture<TModelCustomizer> : NorthwindQueryRelationalFixture<TModelCustomizer>, IBulkUpdatesFixtureBase
    where TModelCustomizer : IModelCustomizer, new()
{
    protected override string StoreName => "BulkUpdatesNorthwind";

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
