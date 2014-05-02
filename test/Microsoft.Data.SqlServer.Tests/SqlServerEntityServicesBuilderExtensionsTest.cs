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

using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Migrations;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Update;
using Xunit;

namespace Microsoft.Data.SqlServer.Tests
{
    public class SqlServerEntityServicesBuilderExtensionsTest
    {
        [Fact]
        public void Can_get_default_services()
        {
            var services = new ServiceCollection().AddEntityFramework(s => s.AddSqlServer());

            // Relational
            Assert.True(services.Any(sd => sd.ServiceType == typeof(DatabaseBuilder)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(RelationalObjectArrayValueReaderFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(RelationalTypedValueReaderFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(CommandBatchPreparer)));

            // SQL Server dingletones
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IdentityGeneratorFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(DataStoreSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerSqlGenerator)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlStatementExecutor)));

            // SQL Server scoped
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStore)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerConnection)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerBatchExecutor)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ModelDiffer)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerMigrationOperationSqlGenerator)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStoreCreator)));
        }

        [Fact]
        public void Services_wire_up_correctly()
        {
            var serviceProvider = new ServiceCollection().AddEntityFramework(s => s.AddSqlServer()).BuildServiceProvider();

            var context = new DbContext(
                serviceProvider,
                new EntityConfigurationBuilder().SqlServerConnectionString("goo").BuildConfiguration());

            var scopedProvider = context.Configuration.Services.ServiceProvider;

            var databaseBuilder = scopedProvider.GetService<DatabaseBuilder>();
            var arrayReaderFactory = scopedProvider.GetService<RelationalObjectArrayValueReaderFactory>();
            var typedReaderFactory = scopedProvider.GetService<RelationalTypedValueReaderFactory>();
            var batchPreparer = scopedProvider.GetService<CommandBatchPreparer>();

            var identityGeneratorFactory = scopedProvider.GetService<IdentityGeneratorFactory>();
            var dataStoreSource = scopedProvider.GetService<DataStoreSource>();
            var sqlServerSqlGenerator = scopedProvider.GetService<SqlServerSqlGenerator>();
            var sqlStatementExecutor = scopedProvider.GetService<SqlStatementExecutor>();

            var sqlServerDataStore = scopedProvider.GetService<SqlServerDataStore>();
            var sqlServerConnection = scopedProvider.GetService<SqlServerConnection>();
            var sqlServerBatchExecutor = scopedProvider.GetService<SqlServerBatchExecutor>();
            var modelDiffer = scopedProvider.GetService<ModelDiffer>();
            var sqlServerMigrationOperationSqlGenerator = scopedProvider.GetService<SqlServerMigrationOperationSqlGenerator>();
            var sqlServerDataStoreCreator = scopedProvider.GetService<SqlServerDataStoreCreator>();

            Assert.NotNull(databaseBuilder);
            Assert.NotNull(arrayReaderFactory);
            Assert.NotNull(typedReaderFactory);
            Assert.NotNull(batchPreparer);

            Assert.NotNull(identityGeneratorFactory);
            Assert.NotNull(dataStoreSource);
            Assert.NotNull(sqlServerSqlGenerator);
            Assert.NotNull(sqlStatementExecutor);

            Assert.NotNull(sqlServerDataStore);
            Assert.NotNull(sqlServerConnection);
            Assert.NotNull(sqlServerBatchExecutor);
            Assert.NotNull(modelDiffer);
            Assert.NotNull(sqlServerMigrationOperationSqlGenerator);
            Assert.NotNull(sqlServerDataStoreCreator);

            context.Dispose();

            context = new DbContext(
                serviceProvider,
                new EntityConfigurationBuilder().SqlServerConnectionString("goo").BuildConfiguration());

            scopedProvider = context.Configuration.Services.ServiceProvider;

            // Dingletons
            Assert.Same(databaseBuilder, scopedProvider.GetService<DatabaseBuilder>());
            Assert.Same(arrayReaderFactory, scopedProvider.GetService<RelationalObjectArrayValueReaderFactory>());
            Assert.Same(typedReaderFactory, scopedProvider.GetService<RelationalTypedValueReaderFactory>());
            Assert.Same(batchPreparer, scopedProvider.GetService<CommandBatchPreparer>());

            Assert.Same(identityGeneratorFactory, scopedProvider.GetService<IdentityGeneratorFactory>());
            Assert.Same(dataStoreSource, scopedProvider.GetService<DataStoreSource>());
            Assert.Same(sqlServerSqlGenerator, scopedProvider.GetService<SqlServerSqlGenerator>());
            Assert.Same(sqlStatementExecutor, scopedProvider.GetService<SqlStatementExecutor>());

            // Scoped
            Assert.NotSame(sqlServerDataStore, scopedProvider.GetService<SqlServerDataStore>());
            Assert.NotSame(sqlServerConnection, scopedProvider.GetService<SqlServerConnection>());
            Assert.NotSame(sqlServerBatchExecutor, scopedProvider.GetService<SqlServerBatchExecutor>());
            Assert.NotSame(modelDiffer, scopedProvider.GetService<ModelDiffer>());
            Assert.NotSame(sqlServerMigrationOperationSqlGenerator, scopedProvider.GetService<SqlServerMigrationOperationSqlGenerator>());
            Assert.NotSame(sqlServerDataStoreCreator, scopedProvider.GetService<SqlServerDataStoreCreator>());

            context.Dispose();
        }
    }
}
