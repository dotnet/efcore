// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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

            builder.AddBuildAction(c => c.AddExtension(new SqlServerConfigurationExtension { ConnectionString = connectionString }));

            return builder;
        }
    }
}
