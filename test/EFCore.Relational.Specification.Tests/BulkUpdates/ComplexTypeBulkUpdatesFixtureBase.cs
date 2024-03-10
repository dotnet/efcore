// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public abstract class ComplexTypeBulkUpdatesFixtureBase : ComplexTypeQueryRelationalFixtureBase, IBulkUpdatesFixtureBase
{
    protected override string StoreName
        => "ComplexTypeBulkUpdatesTest";

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
