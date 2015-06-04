// Copyright (c) .NET Foundation. All rights reserved.
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
    public static class SqlServerEntityOptionsExtensions
    {
        public static SqlServerEntityOptionsBuilder UseSqlServer([NotNull] this EntityOptionsBuilder optionsBuilder, [NotNull] string connectionString)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var extension = GetOrCreateExtension(optionsBuilder);

            extension.ConnectionString = connectionString;

            ((IOptionsBuilderExtender)optionsBuilder).AddOrUpdateExtension(extension);

            return new SqlServerEntityOptionsBuilder(optionsBuilder);
        }

        // Note: Decision made to use DbConnection not SqlConnection: Issue #772
        public static SqlServerEntityOptionsBuilder UseSqlServer([NotNull] this EntityOptionsBuilder optionsBuilder, [NotNull] DbConnection connection)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var extension = GetOrCreateExtension(optionsBuilder);

            extension.Connection = connection;

            ((IOptionsBuilderExtender)optionsBuilder).AddOrUpdateExtension(extension);

            return new SqlServerEntityOptionsBuilder(optionsBuilder);
        }

        private static SqlServerOptionsExtension GetOrCreateExtension(EntityOptionsBuilder optionsBuilder)
        {
            var existing = optionsBuilder.Options.FindExtension<SqlServerOptionsExtension>();
            return existing != null
                ? new SqlServerOptionsExtension(existing)
                : new SqlServerOptionsExtension();
        }
    }
}
