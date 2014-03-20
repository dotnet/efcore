// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    public static class ApiExtensions
    {
        public static EntityConfigurationBuilder UseSqlServer(
            [NotNull] this EntityConfigurationBuilder builder, [NotNull] string connectionString)
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(connectionString, "connectionString");

            builder.UseDataStore(new SqlServerDataStore(connectionString));

            return builder;
        }
    }
}
