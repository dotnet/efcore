// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public static class SqlServerDatabaseFacadeExtensions
    {
        public static void EnsureClean(this DatabaseFacade databaseFacade)
            => databaseFacade.CreateExecutionStrategy()
                .Execute(databaseFacade, database => new SqlServerDatabaseCleaner().Clean(database));
    }
}
