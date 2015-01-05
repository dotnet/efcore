// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDatabase : MigrationsEnabledDatabase
    {
        public SqlServerDatabase(
            [NotNull] DbContextService<IModel> model,
            [NotNull] SqlServerDataStoreCreator dataStoreCreator,
            [NotNull] SqlServerConnection connection,
            [NotNull] SqlServerMigrator migrator,
            [NotNull] ILoggerFactory loggerFactory)
            : base(model, dataStoreCreator, connection, migrator, loggerFactory)
        {
        }

        public new virtual SqlServerConnection Connection
        {
            get { return (SqlServerConnection)base.Connection; }
        }
    }
}
