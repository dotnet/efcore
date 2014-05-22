// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.SqlServer.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity
{
    public static class SqlServerEntityConfigurationBuilderExtensions
    {
        public static DbContextOptions UseSqlServer(
            [NotNull] this DbContextOptions builder, [NotNull] string connectionString)
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(connectionString, "connectionString");

            builder.AddBuildAction(c => c.AddOrUpdateExtension<SqlServerConfigurationExtension>(x => x.ConnectionString = connectionString));

            return builder;
        }

        // TODO: Use SqlConnection instead of DbConnection?
        public static DbContextOptions UseSqlServer(
            [NotNull] this DbContextOptions builder, [NotNull] DbConnection connection)
        {
            Check.NotNull(builder, "builder");
            Check.NotNull(connection, "connection");

            builder.AddBuildAction(c => c.AddOrUpdateExtension<SqlServerConfigurationExtension>(x => x.Connection = connection));

            return builder;
        }
    }
}
