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
        public static SqlServerDbContextOptionsBuilder UseSqlServer([NotNull] this DbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            ((IOptionsBuilderExtender)optionsBuilder).AddOrUpdateExtension(GetOrCreateExtension(optionsBuilder));

            return new SqlServerDbContextOptionsBuilder(optionsBuilder);
        }

        public static SqlServerDbContextOptionsBuilder UseSqlServer([NotNull] this DbContextOptionsBuilder optionsBuilder, [NotNull] string connectionString)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var extension = GetOrCreateExtension(optionsBuilder);

            // TODO: Don't mutate
            extension.ConnectionString = connectionString;

            ((IOptionsBuilderExtender)optionsBuilder).AddOrUpdateExtension(extension);

            return new SqlServerDbContextOptionsBuilder(optionsBuilder);
        }

        // Note: Decision made to use DbConnection not SqlConnection: Issue #772
        public static SqlServerDbContextOptionsBuilder UseSqlServer([NotNull] this DbContextOptionsBuilder optionsBuilder, [NotNull] DbConnection connection)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var extension = GetOrCreateExtension(optionsBuilder);

            // TODO: Don't mutate
            extension.Connection = connection;

            ((IOptionsBuilderExtender)optionsBuilder).AddOrUpdateExtension(extension);

            return new SqlServerDbContextOptionsBuilder(optionsBuilder);
        }

        private static SqlServerOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.Options.FindExtension<SqlServerOptionsExtension>()
               ?? new SqlServerOptionsExtension(optionsBuilder.Options);
    }
}
