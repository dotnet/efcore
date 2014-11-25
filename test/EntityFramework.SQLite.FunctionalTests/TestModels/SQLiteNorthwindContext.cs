// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests.TestModels
{
    public class SqliteNorthwindContext : NorthwindContext
    {
        public const string DatabaseName = "Northwind";

        public SqliteNorthwindContext(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }

        /// <summary>
        ///     A transactional test database, pre-populated with Northwind schema/data
        /// </summary>
        public static Task<SqLiteTestStore> GetSharedStoreAsync()
        {
            return SqLiteTestStore.GetOrCreateSharedAsync("Filename=northwind.db");
        }
    }
}
