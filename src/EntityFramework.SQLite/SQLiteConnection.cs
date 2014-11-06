// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.SQLite;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteConnection : RelationalConnection
    {
        public SQLiteConnection([NotNull] DbContextConfiguration configuration, [NotNull] ILoggerFactory loggerFactory)
            : base(configuration, loggerFactory)
        {
        }

        protected override DbConnection CreateDbConnection()
        {
            return new Data.SQLite.SQLiteConnection(ConnectionString);
        }

        public virtual Data.SQLite.SQLiteConnection CreateConnectionReadWriteCreate()
        {
            // TODO: Handle uris
            var builder = new SQLiteConnectionStringBuilder(ConnectionString) { Mode = "RWC" };

            return new Data.SQLite.SQLiteConnection(builder.ConnectionString);
        }

        public virtual Data.SQLite.SQLiteConnection CreateConnectionReadWrite()
        {
            // TODO: Handle in-memory & uris
            var builder = new SQLiteConnectionStringBuilder(ConnectionString) { Mode = "RW" };

            return new Data.SQLite.SQLiteConnection(builder.ConnectionString);
        }

        public virtual Data.SQLite.SQLiteConnection CreateConnectionReadOnly()
        {
            // TODO: Handle in-memory & uris
            var builder = new SQLiteConnectionStringBuilder(ConnectionString) { Mode = "RO" };

            return new Data.SQLite.SQLiteConnection(builder.ConnectionString);
        }
    }
}
