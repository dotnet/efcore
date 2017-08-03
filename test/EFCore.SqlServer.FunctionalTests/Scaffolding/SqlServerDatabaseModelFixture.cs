// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class SqlServerDatabaseModelFixture : IDisposable
    {
        public SqlServerDatabaseModelFixture()
        {
            TestStore = SqlServerTestStore.CreateScratch();
            TestStore.ExecuteNonQuery("CREATE SCHEMA db2");
            TestStore.ExecuteNonQuery("CREATE SCHEMA [db.2]");
        }

        public SqlServerTestStore TestStore { get; }

        public void Dispose() => TestStore.Dispose();
    }
}
