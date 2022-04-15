// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class SqlServerDatabaseFacadeExtensions
{
    public static void EnsureClean(this DatabaseFacade databaseFacade)
        => databaseFacade.CreateExecutionStrategy()
            .Execute(databaseFacade, database => new SqlServerDatabaseCleaner().Clean(database));
}
