// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
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
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ModificationCommandComparer)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(GraphFactory)));

            // SQL Server dingletones
            Assert.True(services.Any(sd => sd.ServiceType == typeof(DataStoreSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerSqlGenerator)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlStatementExecutor)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerTypeMapper)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerBatchExecutor)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ModificationCommandBatchFactory)));

            // SQL Server scoped
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStore)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerConnection)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ModelDiffer)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerMigrationOperationSqlGeneratorFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStoreCreator)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(MigrationAssembly)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(HistoryRepository)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(Migrator)));
        }

        [Fact]
        public void Services_wire_up_correctly()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddSqlServer();
            var serviceProvider = services.BuildServiceProvider();

            var context = new DbContext(
                serviceProvider,
                new DbContextOptions().UseSqlServer("goo=boo"));

            var scopedProvider = context.Configuration.Services.ServiceProvider;

            var databaseBuilder = scopedProvider.GetService<DatabaseBuilder>();
            var arrayReaderFactory = scopedProvider.GetService<RelationalObjectArrayValueReaderFactory>();
            var typedReaderFactory = scopedProvider.GetService<RelationalTypedValueReaderFactory>();
            var batchPreparer = scopedProvider.GetService<CommandBatchPreparer>();
            var modificationCommandComparer = scopedProvider.GetService<ModificationCommandComparer>();
            var graphFactory = scopedProvider.GetService<GraphFactory>();

            var sqlServerDataStoreSource = scopedProvider.GetService<DataStoreSource>() as SqlServerDataStoreSource;
            var sqlServerSqlGenerator = scopedProvider.GetService<SqlServerSqlGenerator>();
            var sqlStatementExecutor = scopedProvider.GetService<SqlStatementExecutor>();
            var sqlTypeMapper = scopedProvider.GetService<SqlServerTypeMapper>();
            var sqlServerBatchExecutor = scopedProvider.GetService<SqlServerBatchExecutor>();
            var sqlServerModificationCommandBatchFactory = scopedProvider.GetService<ModificationCommandBatchFactory>() as SqlServerModificationCommandBatchFactory;

            var sqlServerDataStore = scopedProvider.GetService<SqlServerDataStore>();
            var sqlServerConnection = scopedProvider.GetService<SqlServerConnection>();
            var modelDiffer = scopedProvider.GetService<ModelDiffer>();
            var serverMigrationOperationSqlGeneratorFactory = scopedProvider.GetService<SqlServerMigrationOperationSqlGeneratorFactory>();
            var sqlServerDataStoreCreator = scopedProvider.GetService<SqlServerDataStoreCreator>();
            var migrationAssembly = scopedProvider.GetService<MigrationAssembly>();
            var historyRepository = scopedProvider.GetService<HistoryRepository>();
            var sqlServerMigrator = scopedProvider.GetService<Migrator>() as SqlServerMigrator;

            Assert.NotNull(databaseBuilder);
            Assert.NotNull(arrayReaderFactory);
            Assert.NotNull(typedReaderFactory);
            Assert.NotNull(batchPreparer);
            Assert.NotNull(modificationCommandComparer);
            Assert.NotNull(graphFactory);

            Assert.NotNull(sqlServerDataStoreSource);
            Assert.NotNull(sqlServerSqlGenerator);
            Assert.NotNull(sqlStatementExecutor);
            Assert.NotNull(sqlTypeMapper);
            Assert.NotNull(sqlServerBatchExecutor);
            Assert.NotNull(sqlServerModificationCommandBatchFactory);

            Assert.NotNull(sqlServerDataStore);
            Assert.NotNull(sqlServerConnection);
            Assert.NotNull(modelDiffer);
            Assert.NotNull(serverMigrationOperationSqlGeneratorFactory);
            Assert.NotNull(sqlServerDataStoreCreator);
            Assert.NotNull(migrationAssembly);
            Assert.NotNull(historyRepository);
            Assert.NotNull(sqlServerMigrator);

            context.Dispose();

            context = new DbContext(
                serviceProvider,
                new DbContextOptions().UseSqlServer("goo=boo"));

            scopedProvider = context.Configuration.Services.ServiceProvider;

            // Dingletons
            Assert.Same(databaseBuilder, scopedProvider.GetService<DatabaseBuilder>());
            Assert.Same(arrayReaderFactory, scopedProvider.GetService<RelationalObjectArrayValueReaderFactory>());
            Assert.Same(typedReaderFactory, scopedProvider.GetService<RelationalTypedValueReaderFactory>());
            Assert.Same(modificationCommandComparer, scopedProvider.GetService<ModificationCommandComparer>());
            Assert.Same(graphFactory, scopedProvider.GetService<GraphFactory>());

            Assert.Same(sqlServerSqlGenerator, scopedProvider.GetService<SqlServerSqlGenerator>());
            Assert.Same(sqlStatementExecutor, scopedProvider.GetService<SqlStatementExecutor>());
            Assert.Same(sqlTypeMapper, scopedProvider.GetService<SqlServerTypeMapper>());
            Assert.Same(sqlServerBatchExecutor, scopedProvider.GetService<SqlServerBatchExecutor>());

            // Scoped
            Assert.NotSame(batchPreparer, scopedProvider.GetService<CommandBatchPreparer>());
            Assert.NotSame(sqlServerModificationCommandBatchFactory, scopedProvider.GetService<ModificationCommandBatchFactory>());
            Assert.NotSame(sqlServerDataStoreSource, scopedProvider.GetService<DataStoreSource>());
            Assert.NotSame(sqlServerDataStore, scopedProvider.GetService<SqlServerDataStore>());
            Assert.NotSame(sqlServerConnection, scopedProvider.GetService<SqlServerConnection>());
            Assert.NotSame(modelDiffer, scopedProvider.GetService<ModelDiffer>());
            Assert.NotSame(serverMigrationOperationSqlGeneratorFactory, scopedProvider.GetService<SqlServerMigrationOperationSqlGeneratorFactory>());
            Assert.NotSame(sqlServerDataStoreCreator, scopedProvider.GetService<SqlServerDataStoreCreator>());
            Assert.NotSame(migrationAssembly, scopedProvider.GetService<MigrationAssembly>());
            Assert.NotSame(historyRepository, scopedProvider.GetService<HistoryRepository>());
            Assert.NotSame(sqlServerMigrator, scopedProvider.GetService<Migrator>());

            context.Dispose();
        }
    }
}
