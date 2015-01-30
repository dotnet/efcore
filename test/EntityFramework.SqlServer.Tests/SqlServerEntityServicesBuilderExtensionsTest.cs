// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
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
            services
                .AddEntityFramework()
                .AddSqlServer();

            // Relational
            Assert.True(services.Any(sd => sd.ServiceType == typeof(RelationalObjectArrayValueReaderFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(RelationalTypedValueReaderFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ModificationCommandComparer)));

            // SQL Server dingletones
            Assert.True(services.Any(sd => sd.ServiceType == typeof(DataStoreSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerValueGeneratorCache)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerValueGeneratorSelector)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SimpleValueGeneratorFactory<SequentialGuidValueGenerator>)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerSequenceValueGeneratorFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerSqlGenerator)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlStatementExecutor)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerTypeMapper)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerModificationCommandBatchFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerCommandBatchPreparer)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerMetadataExtensionProvider)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerMigrationOperationFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerModelSource)));

            // SQL Server scoped
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerBatchExecutor)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStoreServices)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStore)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerConnection)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerMigrationOperationProcessor)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerModelDiffer)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDatabase)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerMigrationOperationSqlGeneratorFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStoreCreator)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerMigrator)));

            // Migrations
            Assert.True(services.Any(sd => sd.ServiceType == typeof(MigrationAssembly)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(HistoryRepository)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(DbContextService<Migrator>)));
        }

        [Fact]
        public void Services_wire_up_correctly()
        {
            var serviceProvider = SqlServerTestHelpers.Instance.CreateServiceProvider();
            using (var context = SqlServerTestHelpers.Instance.CreateContext(serviceProvider))
            {
                var scopedProvider = ((IDbContextServices)context).ScopedServiceProvider;

                var arrayReaderFactory = serviceProvider.GetService<RelationalObjectArrayValueReaderFactory>();
                var typedReaderFactory = serviceProvider.GetService<RelationalTypedValueReaderFactory>();
                var modificationCommandComparer = serviceProvider.GetService<ModificationCommandComparer>();

                var sqlServerValueGeneratorCache = serviceProvider.GetRequiredService<SqlServerValueGeneratorCache>();
                var sqlServerValueGeneratorSelector = serviceProvider.GetRequiredService<SqlServerValueGeneratorSelector>();
                var simpleValueGeneratorFactory = serviceProvider.GetRequiredService<SimpleValueGeneratorFactory<SequentialGuidValueGenerator>>();
                var sqlServerSequenceValueGeneratorFactory = serviceProvider.GetRequiredService<SqlServerSequenceValueGeneratorFactory>();
                var sqlServerSqlGenerator = serviceProvider.GetRequiredService<SqlServerSqlGenerator>();
                var sqlStatementExecutor = serviceProvider.GetRequiredService<SqlStatementExecutor>();
                var sqlTypeMapper = serviceProvider.GetRequiredService<SqlServerTypeMapper>();
                var sqlServerModificationCommandBatchFactory = serviceProvider.GetRequiredService<SqlServerModificationCommandBatchFactory>();
                var batchPreparer = serviceProvider.GetService<SqlServerCommandBatchPreparer>();
                var sqlServerMetadataExtensionProvider = serviceProvider.GetService<SqlServerMetadataExtensionProvider>();
                var sqlServerMigrationOperationFactory = serviceProvider.GetService<SqlServerMigrationOperationFactory>();
                var sqlServerModelSource = serviceProvider.GetService<SqlServerModelSource>();

                var sqlServerDataStoreSource = scopedProvider.GetRequiredService<DataStoreSource>() as SqlServerDataStoreSource;
                var sqlServerBatchExecutor = scopedProvider.GetRequiredService<SqlServerBatchExecutor>();
                var sqlServerDataStoreServices = scopedProvider.GetService<SqlServerDataStoreServices>();
                var sqlServerDataStore = scopedProvider.GetService<SqlServerDataStore>();
                var sqlServerConnection = scopedProvider.GetService<SqlServerConnection>();
                var sqlServerMigrationOperationProcessor = scopedProvider.GetService<SqlServerMigrationOperationProcessor>();
                var modelDiffer = scopedProvider.GetService<SqlServerModelDiffer>();
                var sqlServerDatabase = scopedProvider.GetService<SqlServerDatabase>();
                var serverMigrationOperationSqlGeneratorFactory = scopedProvider.GetService<SqlServerMigrationOperationSqlGeneratorFactory>();
                var sqlServerDataStoreCreator = scopedProvider.GetService<SqlServerDataStoreCreator>();
                var sqlServerMigrator = scopedProvider.GetService<SqlServerMigrator>();

                var migrationAssembly = scopedProvider.GetService<MigrationAssembly>();
                var historyRepository = scopedProvider.GetService<HistoryRepository>();
                var migrator = scopedProvider.GetService<DbContextService<Migrator>>().Service as SqlServerMigrator;

                Assert.NotNull(arrayReaderFactory);
                Assert.NotNull(typedReaderFactory);
                Assert.NotNull(modificationCommandComparer);

                Assert.NotNull(sqlServerValueGeneratorCache);
                Assert.NotNull(sqlServerValueGeneratorSelector);
                Assert.NotNull(simpleValueGeneratorFactory);
                Assert.NotNull(sqlServerSequenceValueGeneratorFactory);
                Assert.NotNull(sqlServerSqlGenerator);
                Assert.NotNull(sqlStatementExecutor);
                Assert.NotNull(sqlTypeMapper);
                Assert.NotNull(sqlServerModificationCommandBatchFactory);
                Assert.NotNull(batchPreparer);
                Assert.NotNull(sqlServerMetadataExtensionProvider);
                Assert.NotNull(sqlServerMigrationOperationFactory);
                Assert.NotNull(sqlServerModelSource);

                Assert.NotNull(sqlServerDataStoreSource);
                Assert.NotNull(sqlServerBatchExecutor);
                Assert.NotNull(sqlServerDataStoreServices);
                Assert.NotNull(sqlServerDataStore);
                Assert.NotNull(sqlServerConnection);
                Assert.NotNull(sqlServerMigrationOperationProcessor);
                Assert.NotNull(modelDiffer);
                Assert.NotNull(sqlServerDatabase);
                Assert.NotNull(serverMigrationOperationSqlGeneratorFactory);
                Assert.NotNull(sqlServerDataStoreCreator);
                Assert.NotNull(sqlServerMigrator);

                Assert.NotNull(migrationAssembly);
                Assert.NotNull(historyRepository);
                Assert.NotNull(migrator);

                // Dingletons
                Assert.Same(arrayReaderFactory, scopedProvider.GetService<RelationalObjectArrayValueReaderFactory>());
                Assert.Same(typedReaderFactory, scopedProvider.GetService<RelationalTypedValueReaderFactory>());
                Assert.Same(modificationCommandComparer, scopedProvider.GetService<ModificationCommandComparer>());

                Assert.Same(sqlServerValueGeneratorCache, serviceProvider.GetRequiredService<SqlServerValueGeneratorCache>());
                Assert.Same(sqlServerValueGeneratorSelector, serviceProvider.GetRequiredService<SqlServerValueGeneratorSelector>());
                Assert.Same(simpleValueGeneratorFactory, serviceProvider.GetRequiredService<SimpleValueGeneratorFactory<SequentialGuidValueGenerator>>());
                Assert.Same(sqlServerSequenceValueGeneratorFactory, serviceProvider.GetRequiredService<SqlServerSequenceValueGeneratorFactory>());
                Assert.Same(sqlServerSqlGenerator, scopedProvider.GetRequiredService<SqlServerSqlGenerator>());
                Assert.Same(sqlStatementExecutor, scopedProvider.GetRequiredService<SqlStatementExecutor>());
                Assert.Same(sqlTypeMapper, scopedProvider.GetRequiredService<SqlServerTypeMapper>());
                Assert.Same(sqlServerModificationCommandBatchFactory, scopedProvider.GetRequiredService<SqlServerModificationCommandBatchFactory>());
                Assert.Same(batchPreparer, scopedProvider.GetRequiredService<SqlServerCommandBatchPreparer>());
                Assert.Same(sqlServerMetadataExtensionProvider, scopedProvider.GetService<SqlServerMetadataExtensionProvider>());
                Assert.Same(sqlServerMigrationOperationFactory, scopedProvider.GetService<SqlServerMigrationOperationFactory>());
                Assert.Same(sqlServerModelSource, serviceProvider.GetService<SqlServerModelSource>());

                // Scoped
                Assert.Same(sqlServerDataStoreSource, scopedProvider.GetService<DataStoreSource>());
                Assert.Same(sqlServerBatchExecutor, scopedProvider.GetService<SqlServerBatchExecutor>());
                Assert.Same(sqlServerDataStoreServices, scopedProvider.GetService<SqlServerDataStoreServices>());
                Assert.Same(sqlServerDataStore, scopedProvider.GetService<SqlServerDataStore>());
                Assert.Same(sqlServerConnection, scopedProvider.GetService<SqlServerConnection>());
                Assert.Same(sqlServerMigrationOperationProcessor, scopedProvider.GetService<SqlServerMigrationOperationProcessor>());
                Assert.Same(modelDiffer, scopedProvider.GetService<SqlServerModelDiffer>());
                Assert.Same(sqlServerDatabase, scopedProvider.GetService<SqlServerDatabase>());
                Assert.Same(serverMigrationOperationSqlGeneratorFactory, scopedProvider.GetService<SqlServerMigrationOperationSqlGeneratorFactory>());
                Assert.Same(sqlServerDataStoreCreator, scopedProvider.GetService<SqlServerDataStoreCreator>());
                Assert.Same(sqlServerMigrator, scopedProvider.GetService<SqlServerMigrator>());

                Assert.Same(migrationAssembly, scopedProvider.GetService<MigrationAssembly>());
                Assert.Same(historyRepository, scopedProvider.GetService<HistoryRepository>());
                Assert.Same(migrator, scopedProvider.GetService<DbContextService<Migrator>>().Service);

                using (var secondContext = SqlServerTestHelpers.Instance.CreateContext(serviceProvider))
                {
                    scopedProvider = ((IDbContextServices)secondContext).ScopedServiceProvider;

                    Assert.NotSame(sqlServerDataStoreSource, scopedProvider.GetService<DataStoreSource>());
                    Assert.NotSame(sqlServerBatchExecutor, scopedProvider.GetService<SqlServerBatchExecutor>());
                    Assert.NotSame(sqlServerDataStoreServices, scopedProvider.GetService<SqlServerDataStoreServices>());
                    Assert.NotSame(sqlServerDataStore, scopedProvider.GetService<SqlServerDataStore>());
                    Assert.NotSame(sqlServerConnection, scopedProvider.GetService<SqlServerConnection>());
                    Assert.NotSame(sqlServerMigrationOperationProcessor, scopedProvider.GetService<SqlServerMigrationOperationProcessor>());
                    Assert.NotSame(modelDiffer, scopedProvider.GetService<SqlServerModelDiffer>());
                    Assert.NotSame(sqlServerDatabase, scopedProvider.GetService<SqlServerDatabase>());
                    Assert.NotSame(serverMigrationOperationSqlGeneratorFactory, scopedProvider.GetService<SqlServerMigrationOperationSqlGeneratorFactory>());
                    Assert.NotSame(sqlServerDataStoreCreator, scopedProvider.GetService<SqlServerDataStoreCreator>());
                    Assert.NotSame(sqlServerMigrator, scopedProvider.GetService<SqlServerMigrator>());

                    Assert.NotSame(migrationAssembly, scopedProvider.GetService<MigrationAssembly>());
                    Assert.NotSame(historyRepository, scopedProvider.GetService<HistoryRepository>());
                    Assert.NotSame(migrator, scopedProvider.GetService<DbContextService<Migrator>>().Service);
                }
            }
        }

        [Fact]
        public void AddSqlServer_does_not_replace_services_already_registered()
        {
            var services = new ServiceCollection()
                .AddSingleton<SqlServerDataStore, FakeSqlServerDataStore>();

            services.AddEntityFramework().AddSqlServer();

            var serviceProvider = services.BuildServiceProvider();

            Assert.IsType<FakeSqlServerDataStore>(serviceProvider.GetRequiredService<SqlServerDataStore>());
        }

        private class FakeSqlServerDataStore : SqlServerDataStore
        {
        }
    }
}
