// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDataStoreSource : DataStoreSource<SqlServerDataStore, ISqlServerDataStoreServices, SqlServerOptionsExtension>
    {
        public override void AutoConfigure(DbContextOptionsBuilder optionsBuilder)
        {
        }
    }
}
