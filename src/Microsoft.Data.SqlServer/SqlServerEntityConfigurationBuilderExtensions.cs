// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
