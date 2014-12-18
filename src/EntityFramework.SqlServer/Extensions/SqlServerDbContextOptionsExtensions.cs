// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqlServerDbContextOptionsExtensions
    {
        public static void UseSqlServer([NotNull] this DbContextOptions options)
        {
            Check.NotNull(options, "options");

            ((IDbContextOptions)options)
                .AddOrUpdateExtension<SqlServerOptionsExtension>(x => { });
        }

        public static void UseSqlServer([NotNull] this DbContextOptions options, [NotNull] string connectionString)
        {
            Check.NotNull(options, "options");
            Check.NotEmpty(connectionString, "connectionString");

            ((IDbContextOptions)options)
                .AddOrUpdateExtension<SqlServerOptionsExtension>(x => x.ConnectionString = connectionString);
        }

        public static void UseSqlServer<T>([NotNull] this DbContextOptions<T> options, [NotNull] string connectionString)
        {
            UseSqlServer((DbContextOptions)options, connectionString);
        }

        // Note: Decision made to use DbConnection not SqlConnection: Issue #772
        public static void UseSqlServer([NotNull] this DbContextOptions options, [NotNull] DbConnection connection)
        {
            Check.NotNull(options, "options");
            Check.NotNull(connection, "connection");

            ((IDbContextOptions)options)
                .AddOrUpdateExtension<SqlServerOptionsExtension>(x => x.Connection = connection);
        }

        // Note: Decision made to use DbConnection not SqlConnection: Issue #772
        public static void UseSqlServer<T>([NotNull] this DbContextOptions<T> options, [NotNull] DbConnection connection)
        {
            UseSqlServer((DbContextOptions)options, connection);
        }
    }
}
