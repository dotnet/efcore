// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.TestModels
{
    public class SqlServerNorthwindContext : NorthwindContext
    {
        public static readonly string DatabaseName = StoreName;
        public static readonly string ConnectionString = SqlServerTestStore.CreateConnectionString(DatabaseName);

        public SqlServerNorthwindContext(DbContextOptions options,
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
            : base(options, queryTrackingBehavior)
        {
        }

        public static SqlServerTestStore GetSharedStore()
            => SqlServerTestStore.GetOrCreateShared(
                DatabaseName,
                () => SqlServerTestStore.ExecuteScript(DatabaseName, @"Northwind.sql"),
                cleanDatabase: false);
    }
}
