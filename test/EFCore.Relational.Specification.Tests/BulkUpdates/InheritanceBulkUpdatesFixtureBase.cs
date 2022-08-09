// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public abstract class InheritanceBulkUpdatesFixtureBase : InheritanceQueryFixtureBase, IBulkUpdatesFixtureBase
{
    protected override string StoreName => "InheritanceBulkUpdatesTest";

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
