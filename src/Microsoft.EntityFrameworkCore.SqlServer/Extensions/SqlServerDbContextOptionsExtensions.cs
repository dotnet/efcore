// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Infrastructure.Internal;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqlServerDbContextOptionsExtensions
    {
        /// <summary>
        ///     Configures the context to connect to a Microsoft SQL Server database.
        /// </summary>
        /// <param name="optionsBuilder"> The options for the context. </param>
        /// <param name="connectionString"> The connection string of the database to connect to. </param>
        /// <returns> An options builder to allow additional SQL Server specific configuration. </returns>
        public static SqlServerDbContextOptionsBuilder UseSqlServer(
            [NotNull] this DbContextOptionsBuilder optionsBuilder, [NotNull] string connectionString)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var extension = GetOrCreateExtension(optionsBuilder);
            extension.ConnectionString = connectionString;
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            return new SqlServerDbContextOptionsBuilder(optionsBuilder);
        }

        // Note: Decision made to use DbConnection not SqlConnection: Issue #772
        /// <summary>
        ///     Configures the context to connect to a Microsoft SQL Server database.
        /// </summary>
        /// <param name="optionsBuilder"> The options for the context. </param>
        /// <param name="connection">
        ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
        ///     in the open state then EF will not open or close the connection. If the connection is in the closed
        ///     state then EF will open and close the connection as needed.
        /// </param>
        /// <returns> An options builder to allow additional SQL Server specific configuration. </returns>
        public static SqlServerDbContextOptionsBuilder UseSqlServer(
            [NotNull] this DbContextOptionsBuilder optionsBuilder, [NotNull] DbConnection connection)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var extension = GetOrCreateExtension(optionsBuilder);
            extension.Connection = connection;
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            return new SqlServerDbContextOptionsBuilder(optionsBuilder);
        }

        private static SqlServerOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        {
            var existing = optionsBuilder.Options.FindExtension<SqlServerOptionsExtension>();
            return existing != null
                ? new SqlServerOptionsExtension(existing)
                : new SqlServerOptionsExtension();
        }
    }
}
