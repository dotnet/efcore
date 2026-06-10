// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public static class SqliteDatabaseFacadeTestExtensions
{
    public static void EnsureClean(this DatabaseFacade databaseFacade)
        => new SqliteDatabaseCleaner().Clean(databaseFacade);
}
