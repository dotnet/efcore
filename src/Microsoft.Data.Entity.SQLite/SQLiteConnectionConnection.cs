// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.SQLite;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteConnectionConnection : RelationalConnection
    {
        public SQLiteConnectionConnection([NotNull] DbContextConfiguration configuration)
            : base(configuration)
        {
        }

        protected override DbConnection CreateDbConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }

        public virtual SQLiteConnection CreateConnectionWithCreate()
        {
            // TODO: Handle uris
            var builder = new SQLiteConnectionStringBuilder(ConnectionString) { Mode = "RWC" };

            return new SQLiteConnection(builder.ConnectionString);
        }

        public virtual SQLiteConnection CreateConnectionWithoutCreate()
        {
            // TODO: Handle in-memory & uris
            var builder = new SQLiteConnectionStringBuilder(ConnectionString) { Mode = "RO" };

            return new SQLiteConnection(builder.ConnectionString);
        }
    }
}
