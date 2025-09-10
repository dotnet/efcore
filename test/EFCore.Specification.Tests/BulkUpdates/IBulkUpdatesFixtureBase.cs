// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public interface IBulkUpdatesFixtureBase : IQueryFixtureBase
{
    Action<DatabaseFacade, IDbContextTransaction> GetUseTransaction()
        => UseTransaction;

    void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction);
}
