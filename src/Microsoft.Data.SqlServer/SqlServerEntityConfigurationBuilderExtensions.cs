// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    public static class SqlServerEntityConfigurationBuilderExtensions
    {
        public static EntityConfigurationBuilder SqlServerConnectionString(
            [NotNull] this EntityConfigurationBuilder builder, [NotNull] string connectionString)
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(connectionString, "connectionString");

            builder.AddBuildAction(c => c.AddOrUpdateExtension<SqlServerConfigurationExtension>(x => x.ConnectionString = connectionString));

            return builder;
        }

        // TODO: Use SqlConnection instead of DbConnection?
        public static EntityConfigurationBuilder SqlServerConnection(
            [NotNull] this EntityConfigurationBuilder builder, [NotNull] DbConnection connection)
        {
            Check.NotNull(builder, "builder");
            Check.NotNull(connection, "connection");

            builder.AddBuildAction(c => c.AddOrUpdateExtension<SqlServerConfigurationExtension>(x => x.Connection = connection));

            return builder;
        }
    }
}
