// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDatabase : RelationalDatabase
    {
        public SqlServerDatabase(
            [NotNull] DbContext context,
            [NotNull] ISqlServerDataStoreCreator dataStoreCreator,
            [NotNull] ISqlServerConnection connection,
            [NotNull] IMigrator migrator,
            [NotNull] ILoggerFactory loggerFactory)
            : base(context, dataStoreCreator, connection, migrator, loggerFactory)
        {
        }

        public new virtual ISqlServerConnection Connection => (ISqlServerConnection)base.Connection;
    }
}
