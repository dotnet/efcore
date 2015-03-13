// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.SqlServer.Extensions;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqlServerDbContextOptionsExtensions
    {
        public static SqlServerDbContextOptionsBuilder UseSqlServer([NotNull] this DbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            string connectionString;
            optionsBuilder.Options.RawOptions.TryGetValue("ConnectionString", out connectionString);

            return optionsBuilder.UseConnectionString(connectionString);
        }

        public static SqlServerDbContextOptionsBuilder UseSqlServer([NotNull] this DbContextOptionsBuilder optionsBuilder, [NotNull] string connectionString)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            return optionsBuilder.UseConnectionString(connectionString);
        }

        private static SqlServerDbContextOptionsBuilder UseConnectionString(this DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            var extension = GetOrCreateExtension(optionsBuilder);

            if (connectionString != null)
            {
                extension.ConnectionString = optionsBuilder.Options.Configuration.ResolveConnectionString(connectionString);
            }

            ((IOptionsBuilderExtender)optionsBuilder).AddOrUpdateExtension(extension);

            return new SqlServerDbContextOptionsBuilder(optionsBuilder);
        }

        // Note: Decision made to use DbConnection not SqlConnection: Issue #772
        public static SqlServerDbContextOptionsBuilder UseSqlServer([NotNull] this DbContextOptionsBuilder optionsBuilder, [NotNull] DbConnection connection)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var extension = GetOrCreateExtension(optionsBuilder);

            extension.Connection = connection;

            ((IOptionsBuilderExtender)optionsBuilder).AddOrUpdateExtension(extension);

            return new SqlServerDbContextOptionsBuilder(optionsBuilder);
        }

        private static SqlServerOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        {
            var existing = optionsBuilder.Options.FindExtension<SqlServerOptionsExtension>();
            return existing != null
                ? new SqlServerOptionsExtension(existing)
                : new SqlServerOptionsExtension(optionsBuilder.Options);
        }
    }
}
