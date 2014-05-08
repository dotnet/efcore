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

using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.SqlServer.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Framework.DependencyInjection
{
    public static class SqlServerEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddSqlServer([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.AddRelational().ServiceCollection
                // TODO: Need to be able to pick the appropriate identity generator for the data store in use
                .AddSingleton<IdentityGeneratorFactory, SqlServerIdentityGeneratorFactory>()
                .AddSingleton<DataStoreSource, SqlServerDataStoreSource>()
                .AddSingleton<SqlServerSqlGenerator, SqlServerSqlGenerator>()
                .AddSingleton<SqlStatementExecutor, SqlStatementExecutor>()
                .AddScoped<SqlServerDataStore, SqlServerDataStore>()
                .AddScoped<SqlServerConnection, SqlServerConnection>()
                .AddScoped<SqlServerBatchExecutor, SqlServerBatchExecutor>()
                .AddScoped<ModelDiffer, ModelDiffer>()
                .AddScoped<SqlServerMigrationOperationSqlGenerator, SqlServerMigrationOperationSqlGenerator>()
                .AddScoped<SqlServerDataStoreCreator, SqlServerDataStoreCreator>();

            return builder;
        }
    }
}
