// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class SqlServerTestStoreFactory : ITestStoreFactory<SqlServerTestStore>
    {
        public static SqlServerTestStoreFactory Instance { get; } = new SqlServerTestStoreFactory();

        protected SqlServerTestStoreFactory()
        {
        }

        public virtual SqlServerTestStore CreateShared(string storeName)
            => SqlServerTestStore.GetOrCreateShared(storeName);
    }
}
