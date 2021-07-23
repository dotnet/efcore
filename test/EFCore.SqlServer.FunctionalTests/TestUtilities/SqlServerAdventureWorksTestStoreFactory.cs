﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class SqlServerAdventureWorksTestStoreFactory : SqlServerTestStoreFactory
    {
        public static new SqlServerAdventureWorksTestStoreFactory Instance { get; } = new();

        protected SqlServerAdventureWorksTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => SqlServerTestStore.GetOrCreate(
                "adventureworks",
                Path.Combine("SqlAzure", "adventureworks.sql"));
    }
}
