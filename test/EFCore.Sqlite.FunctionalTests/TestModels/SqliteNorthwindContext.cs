// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests.TestModels
{
    public class SqliteNorthwindContext : NorthwindContext
    {
        public SqliteNorthwindContext(DbContextOptions options,
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
            : base(options, queryTrackingBehavior)
        {
        }

        public static SqliteTestStore GetSharedStore() => SqliteTestStore.GetOrCreateShared("northwind", () => { });
    }
}
