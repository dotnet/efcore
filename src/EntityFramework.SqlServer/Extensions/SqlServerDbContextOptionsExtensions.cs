// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.SqlServer.Extensions;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqlServerDbContextOptionsExtensions
    {
        public static SqlServerDbContextOptions UseSqlServer([NotNull] this DbContextOptions options)
        {
            Check.NotNull(options, nameof(options));

            ((IDbContextOptions)options)
                .AddOrUpdateExtension<SqlServerOptionsExtension>(x => { });

            return new SqlServerDbContextOptions(options);
        }

        public static SqlServerDbContextOptions UseSqlServer([NotNull] this DbContextOptions options, [NotNull] string connectionString)
        {
            Check.NotNull(options, nameof(options));
            Check.NotEmpty(connectionString, nameof(connectionString));

            ((IDbContextOptions)options)
                .AddOrUpdateExtension<SqlServerOptionsExtension>(x => x.ConnectionString = connectionString);

            return new SqlServerDbContextOptions(options);
        }

        public static SqlServerDbContextOptions UseSqlServer<T>([NotNull] this DbContextOptions<T> options, [NotNull] string connectionString)
        {
            return UseSqlServer((DbContextOptions)options, connectionString);
        }

        // Note: Decision made to use DbConnection not SqlConnection: Issue #772
        public static SqlServerDbContextOptions UseSqlServer([NotNull] this DbContextOptions options, [NotNull] DbConnection connection)
        {
            Check.NotNull(options, nameof(options));
            Check.NotNull(connection, nameof(connection));

            ((IDbContextOptions)options)
                .AddOrUpdateExtension<SqlServerOptionsExtension>(x => x.Connection = connection);

            return new SqlServerDbContextOptions(options);
        }

        // Note: Decision made to use DbConnection not SqlConnection: Issue #772
        public static SqlServerDbContextOptions UseSqlServer<T>([NotNull] this DbContextOptions<T> options, [NotNull] DbConnection connection)
        {
            return UseSqlServer((DbContextOptions)options, connection);
        }
    }
}
