// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDatabase : RelationalDatabase
    {
        public SqlServerDatabase(
            [NotNull] DbContextService<DbContext> context,
            [NotNull] SqlServerDataStoreCreator dataStoreCreator,
            [NotNull] SqlServerConnection connection,
            [NotNull] Migrator migrator,
            [NotNull] ILoggerFactory loggerFactory)
            : base(context, dataStoreCreator, connection, migrator, loggerFactory)
        {
        }

        public new virtual SqlServerConnection Connection => (SqlServerConnection)base.Connection;
    }
}
