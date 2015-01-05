// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
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
            services
                .AddEntityFramework()
                .AddSqlServer();

            // Relational
            Assert.True(services.Any(sd => sd.ServiceType == typeof(RelationalObjectArrayValueReaderFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(RelationalTypedValueReaderFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ModificationCommandComparer)));

            // SQL Server dingletones
            Assert.True(services.Any(sd => sd.ServiceType == typeof(DataStoreSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerSqlGenerator)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlStatementExecutor)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerTypeMapper)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerBatchExecutor)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerModificationCommandBatchFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerCommandBatchPreparer)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerMetadataExtensionProvider)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerMigrationOperationFactory)));

            // SQL Server scoped
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStore)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerConnection)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerMigrationOperationProcessor)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerModelDiffer)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerMigrationOperationSqlGeneratorFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStoreCreator)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(MigrationAssembly)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(HistoryRepository)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerMigrator)));
        }

        [Fact]
        public void Services_wire_up_correctly()
        {
            var serviceProvider = TestHelpers.CreateServiceProvider();
            var context = TestHelpers.CreateContext(serviceProvider);
            var scopedProvider = ((IDbContextServices)context).ScopedServiceProvider;

            var arrayReaderFactory = scopedProvider.GetService<RelationalObjectArrayValueReaderFactory>();
            var typedReaderFactory = scopedProvider.GetService<RelationalTypedValueReaderFactory>();
            var batchPreparer = scopedProvider.GetService<SqlServerCommandBatchPreparer>();
            var modificationCommandComparer = scopedProvider.GetService<ModificationCommandComparer>();

            var sqlServerDataStoreSource = scopedProvider.GetRequiredService<DataStoreSource>() as SqlServerDataStoreSource;
            var sqlServerSqlGenerator = scopedProvider.GetRequiredService<SqlServerSqlGenerator>();
            var sqlStatementExecutor = scopedProvider.GetRequiredService<SqlStatementExecutor>();
            var sqlTypeMapper = scopedProvider.GetRequiredService<SqlServerTypeMapper>();
            var sqlServerBatchExecutor = scopedProvider.GetRequiredService<SqlServerBatchExecutor>();
            var sqlServerModificationCommandBatchFactory = scopedProvider.GetRequiredService<SqlServerModificationCommandBatchFactory>();
            var sqlServerMetadataExtensionProvider = scopedProvider.GetService<SqlServerMetadataExtensionProvider>();
            var sqlServerMigrationOperationFactory = scopedProvider.GetService<SqlServerMigrationOperationFactory>();

            var sqlServerDataStore = scopedProvider.GetService<SqlServerDataStore>();
            var sqlServerConnection = scopedProvider.GetService<SqlServerConnection>();
            var sqlServerMigrationOperationProcessor = scopedProvider.GetService<SqlServerMigrationOperationProcessor>();
            var modelDiffer = scopedProvider.GetService<SqlServerModelDiffer>();
            var serverMigrationOperationSqlGeneratorFactory = scopedProvider.GetService<SqlServerMigrationOperationSqlGeneratorFactory>();
            var sqlServerDataStoreCreator = scopedProvider.GetService<SqlServerDataStoreCreator>();
            var migrationAssembly = scopedProvider.GetService<MigrationAssembly>();
            var historyRepository = scopedProvider.GetService<HistoryRepository>();
            var sqlServerMigrator = scopedProvider.GetService<SqlServerMigrator>();

            Assert.NotNull(arrayReaderFactory);
            Assert.NotNull(typedReaderFactory);
            Assert.NotNull(batchPreparer);
            Assert.NotNull(modificationCommandComparer);

            Assert.NotNull(sqlServerDataStoreSource);
            Assert.NotNull(sqlServerSqlGenerator);
            Assert.NotNull(sqlStatementExecutor);
            Assert.NotNull(sqlTypeMapper);
            Assert.NotNull(sqlServerBatchExecutor);
            Assert.NotNull(sqlServerModificationCommandBatchFactory);
            Assert.NotNull(sqlServerMetadataExtensionProvider);
            Assert.NotNull(sqlServerMigrationOperationFactory);

            Assert.NotNull(sqlServerDataStore);
            Assert.NotNull(sqlServerConnection);
            Assert.NotNull(sqlServerMigrationOperationProcessor);
            Assert.NotNull(modelDiffer);
            Assert.NotNull(serverMigrationOperationSqlGeneratorFactory);
            Assert.NotNull(sqlServerDataStoreCreator);
            Assert.NotNull(migrationAssembly);
            Assert.NotNull(historyRepository);
            Assert.NotNull(sqlServerMigrator);

            context.Dispose();

            context = TestHelpers.CreateContext(serviceProvider);
            scopedProvider = ((IDbContextServices)context).ScopedServiceProvider;

            // Dingletons
            Assert.Same(arrayReaderFactory, scopedProvider.GetService<RelationalObjectArrayValueReaderFactory>());
            Assert.Same(typedReaderFactory, scopedProvider.GetService<RelationalTypedValueReaderFactory>());
            Assert.Same(modificationCommandComparer, scopedProvider.GetService<ModificationCommandComparer>());

            Assert.Same(sqlServerSqlGenerator, scopedProvider.GetRequiredService<SqlServerSqlGenerator>());
            Assert.Same(sqlStatementExecutor, scopedProvider.GetRequiredService<SqlStatementExecutor>());
            Assert.Same(sqlTypeMapper, scopedProvider.GetRequiredService<SqlServerTypeMapper>());
            Assert.Same(batchPreparer, scopedProvider.GetRequiredService<SqlServerCommandBatchPreparer>());
            Assert.Same(sqlServerModificationCommandBatchFactory, scopedProvider.GetRequiredService<SqlServerModificationCommandBatchFactory>());
            Assert.Same(sqlServerMetadataExtensionProvider, scopedProvider.GetService<SqlServerMetadataExtensionProvider>());
            Assert.Same(sqlServerMigrationOperationFactory, scopedProvider.GetService<SqlServerMigrationOperationFactory>());

            // Scoped
            Assert.NotSame(sqlServerBatchExecutor, scopedProvider.GetService<SqlServerBatchExecutor>());
            Assert.NotSame(sqlServerDataStoreSource, scopedProvider.GetService<DataStoreSource>());
            Assert.NotSame(sqlServerDataStore, scopedProvider.GetService<SqlServerDataStore>());
            Assert.NotSame(sqlServerConnection, scopedProvider.GetService<SqlServerConnection>());
            Assert.NotSame(sqlServerMigrationOperationProcessor, scopedProvider.GetService<SqlServerMigrationOperationProcessor>());
            Assert.NotSame(modelDiffer, scopedProvider.GetService<SqlServerModelDiffer>());
            Assert.NotSame(serverMigrationOperationSqlGeneratorFactory, scopedProvider.GetService<SqlServerMigrationOperationSqlGeneratorFactory>());
            Assert.NotSame(sqlServerDataStoreCreator, scopedProvider.GetService<SqlServerDataStoreCreator>());
            Assert.NotSame(migrationAssembly, scopedProvider.GetService<MigrationAssembly>());
            Assert.NotSame(historyRepository, scopedProvider.GetService<HistoryRepository>());
            Assert.NotSame(sqlServerMigrator, scopedProvider.GetService<SqlServerMigrator>());

            context.Dispose();
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
