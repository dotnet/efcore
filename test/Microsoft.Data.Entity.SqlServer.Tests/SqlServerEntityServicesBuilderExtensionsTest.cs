// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerEntityServicesBuilderExtensionsTest
    {
        [Fact]
        public void Can_get_default_services()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddSqlServer();

            // Relational
            Assert.True(services.Any(sd => sd.ServiceType == typeof(DatabaseBuilder)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(RelationalObjectArrayValueReaderFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(RelationalTypedValueReaderFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(CommandBatchPreparer)));

            // SQL Server dingletones
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
            var services = new ServiceCollection();
            services.AddEntityFramework().AddSqlServer();
            var serviceProvider = services.BuildServiceProvider();

            var context = new DbContext(
                serviceProvider,
                new DbContextOptions().UseSqlServer("goo").BuildConfiguration());

            var scopedProvider = context.Configuration.Services.ServiceProvider;

            var databaseBuilder = scopedProvider.GetService<DatabaseBuilder>();
            var arrayReaderFactory = scopedProvider.GetService<RelationalObjectArrayValueReaderFactory>();
            var typedReaderFactory = scopedProvider.GetService<RelationalTypedValueReaderFactory>();
            var batchPreparer = scopedProvider.GetService<CommandBatchPreparer>();

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
                new DbContextOptions().UseSqlServer("goo").BuildConfiguration());

            scopedProvider = context.Configuration.Services.ServiceProvider;

            // Dingletons
            Assert.Same(databaseBuilder, scopedProvider.GetService<DatabaseBuilder>());
            Assert.Same(arrayReaderFactory, scopedProvider.GetService<RelationalObjectArrayValueReaderFactory>());
            Assert.Same(typedReaderFactory, scopedProvider.GetService<RelationalTypedValueReaderFactory>());
            Assert.Same(batchPreparer, scopedProvider.GetService<CommandBatchPreparer>());

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
