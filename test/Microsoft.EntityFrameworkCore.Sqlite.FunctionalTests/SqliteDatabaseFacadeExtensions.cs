// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public static class SqliteDatabaseFacadeExtensions
    {
        public static void EnsureClean(this DatabaseFacade databaseFacade)
            => new SqliteDatabaseCleaner().Clean(databaseFacade);
    }
}
