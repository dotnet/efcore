// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Sqlite;
using Microsoft.Data.Entity.Sqlite.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqliteDbContextOptionsExtensions
    {
        public static DbContextOptions UseSqlite([NotNull] this DbContextOptions options, [NotNull] string connectionString)
        {
            Check.NotNull(options, "options");
            Check.NotEmpty(connectionString, "connectionString");

            ((IDbContextOptions)options)
                .AddOrUpdateExtension<SqliteOptionsExtension>(x => x.ConnectionString = connectionString);

            return options;
        }

        public static DbContextOptions<T> UseSqlite<T>([NotNull] this DbContextOptions<T> options, [NotNull] string connectionString)
        {
            return (DbContextOptions<T>)UseSqlite((DbContextOptions)options, connectionString);
        }

        // Note: Decision made to use DbConnection not SqliteConnection: Issue #772
        public static DbContextOptions UseSqlite([NotNull] this DbContextOptions options, [NotNull] DbConnection connection)
        {
            Check.NotNull(options, "options");
            Check.NotNull(connection, "connection");

            ((IDbContextOptions)options)
                .AddOrUpdateExtension<SqliteOptionsExtension>(x => x.Connection = connection);

            return options;
        }

        // Note: Decision made to use DbConnection not SqliteConnection: Issue #772
        public static DbContextOptions<T> UseSqlite<T>([NotNull] this DbContextOptions<T> options, [NotNull] DbConnection connection)
        {
            return (DbContextOptions<T>)UseSqlite((DbContextOptions)options, connection);
        }
    }
}
