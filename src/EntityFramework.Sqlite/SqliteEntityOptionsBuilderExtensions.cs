// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Sqlite;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public static class SqliteEntityOptionsBuilderExtensions
    {
        public static RelationalEntityOptionsBuilder UseSqlite([NotNull] this EntityOptionsBuilder options, [NotNull] string connectionString)
        {
            Check.NotNull(options, nameof(options));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var extension = GetOrCreateExtension(options);
            extension.ConnectionString = connectionString;
            ((IOptionsBuilderExtender)options).AddOrUpdateExtension(extension);

            return new RelationalEntityOptionsBuilder(options);
        }

        public static RelationalEntityOptionsBuilder UseSqlite([NotNull] this EntityOptionsBuilder options, [NotNull] DbConnection connection)
        {
            Check.NotNull(options, nameof(options));
            Check.NotNull(connection, nameof(connection));

            var extension = GetOrCreateExtension(options);
            extension.Connection = connection;
            ((IOptionsBuilderExtender)options).AddOrUpdateExtension(extension);

            return new RelationalEntityOptionsBuilder(options);
        }

        private static SqliteOptionsExtension GetOrCreateExtension(EntityOptionsBuilder options)
        {
            var existingExtension = options.Options.FindExtension<SqliteOptionsExtension>();

            return existingExtension != null
                ? new SqliteOptionsExtension(existingExtension)
                : new SqliteOptionsExtension();
        }
    }
}
