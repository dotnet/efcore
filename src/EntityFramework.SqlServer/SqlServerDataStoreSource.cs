// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDataStoreSource : DataStoreSource<SqlServerDataStoreServices, SqlServerOptionsExtension>
    {
        public SqlServerDataStoreSource([NotNull] DbContextConfiguration configuration)
            : base(configuration)
        {
        }

        public override string Name
        {
            get { return typeof(SqlServerDataStore).Name; }
        }

        public override void AutoConfigure()
        {
            ContextOptions.UseSqlServer();
        }
    }
}
