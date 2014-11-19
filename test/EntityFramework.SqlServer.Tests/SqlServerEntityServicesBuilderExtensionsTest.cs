// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Update;
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
            services
                .AddEntityFramework()
                .AddSqlServer();

            // Relational
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDatabaseBuilder)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(RelationalObjectArrayValueReaderFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(RelationalTypedValueReaderFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ModificationCommandComparer)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(GraphFactory)));

            // SQL Server dingletones
            Assert.True(services.Any(sd => sd.ServiceType == typeof(DataStoreSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerSqlGenerator)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlStatementExecutor)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerTypeMapper)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerBatchExecutor)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerModificationCommandBatchFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerCommandBatchPreparer)));

            // SQL Server scoped
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerDataStore)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SqlServerConnection)));
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
            var services = new ServiceCollection();
            services
                .AddEntityFramework()
                .AddSqlServer();

            var serviceProvider = services.BuildServiceProvider();

            var context = new DbContext(
                serviceProvider,
                new DbContextOptions().UseSqlServer("goo=boo"));

            var scopedProvider = ((IDbContextServices)context).ScopedServiceProvider;

            var databaseBuilder = scopedProvider.GetRequiredService<SqlServerDatabaseBuilder>();
            var arrayReaderFactory = scopedProvider.GetRequiredService<RelationalObjectArrayValueReaderFactory>();
            var typedReaderFactory = scopedProvider.GetRequiredService<RelationalTypedValueReaderFactory>();
            var batchPreparer = scopedProvider.GetRequiredService<SqlServerCommandBatchPreparer>();
            var modificationCommandComparer = scopedProvider.GetRequiredService<ModificationCommandComparer>();
            var graphFactory = scopedProvider.GetRequiredService<GraphFactory>();

            var sqlServerDataStoreSource = scopedProvider.GetRequiredService<DataStoreSource>() as SqlServerDataStoreSource;
            var sqlServerSqlGenerator = scopedProvider.GetRequiredService<SqlServerSqlGenerator>();
            var sqlStatementExecutor = scopedProvider.GetRequiredService<SqlStatementExecutor>();
            var sqlTypeMapper = scopedProvider.GetRequiredService<SqlServerTypeMapper>();
            var sqlServerBatchExecutor = scopedProvider.GetRequiredService<SqlServerBatchExecutor>();
            var sqlServerModificationCommandBatchFactory = scopedProvider.GetRequiredService<SqlServerModificationCommandBatchFactory>();

            var sqlServerDataStore = scopedProvider.GetRequiredService<SqlServerDataStore>();
            var sqlServerConnection = scopedProvider.GetRequiredService<SqlServerConnection>();
            var modelDiffer = scopedProvider.GetRequiredService<SqlServerModelDiffer>();
            var serverMigrationOperationSqlGeneratorFactory = scopedProvider.GetRequiredService<SqlServerMigrationOperationSqlGeneratorFactory>();
            var sqlServerDataStoreCreator = scopedProvider.GetRequiredService<SqlServerDataStoreCreator>();
            var migrationAssembly = scopedProvider.GetRequiredService<MigrationAssembly>();
            var historyRepository = scopedProvider.GetRequiredService<HistoryRepository>();
            var sqlServerMigrator = scopedProvider.GetRequiredService<SqlServerMigrator>();

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

            scopedProvider = ((IDbContextServices)context).ScopedServiceProvider;

            // Dingletons
            Assert.Same(databaseBuilder, scopedProvider.GetRequiredService<SqlServerDatabaseBuilder>());
            Assert.Same(arrayReaderFactory, scopedProvider.GetRequiredService<RelationalObjectArrayValueReaderFactory>());
            Assert.Same(typedReaderFactory, scopedProvider.GetRequiredService<RelationalTypedValueReaderFactory>());
            Assert.Same(modificationCommandComparer, scopedProvider.GetRequiredService<ModificationCommandComparer>());
            Assert.Same(graphFactory, scopedProvider.GetRequiredService<GraphFactory>());

            Assert.Same(sqlServerSqlGenerator, scopedProvider.GetRequiredService<SqlServerSqlGenerator>());
            Assert.Same(sqlStatementExecutor, scopedProvider.GetRequiredService<SqlStatementExecutor>());
            Assert.Same(sqlTypeMapper, scopedProvider.GetRequiredService<SqlServerTypeMapper>());
            Assert.Same(batchPreparer, scopedProvider.GetRequiredService<SqlServerCommandBatchPreparer>());
            Assert.Same(sqlServerModificationCommandBatchFactory, scopedProvider.GetRequiredService<SqlServerModificationCommandBatchFactory>());

            // Scoped
            Assert.NotSame(sqlServerBatchExecutor, scopedProvider.GetRequiredService<SqlServerBatchExecutor>());
            Assert.NotSame(sqlServerDataStoreSource, scopedProvider.GetRequiredService<DataStoreSource>());
            Assert.NotSame(sqlServerDataStore, scopedProvider.GetRequiredService<SqlServerDataStore>());
            Assert.NotSame(sqlServerConnection, scopedProvider.GetRequiredService<SqlServerConnection>());
            Assert.NotSame(modelDiffer, scopedProvider.GetRequiredService<SqlServerModelDiffer>());
            Assert.NotSame(serverMigrationOperationSqlGeneratorFactory, scopedProvider.GetRequiredService<SqlServerMigrationOperationSqlGeneratorFactory>());
            Assert.NotSame(sqlServerDataStoreCreator, scopedProvider.GetRequiredService<SqlServerDataStoreCreator>());
            Assert.NotSame(migrationAssembly, scopedProvider.GetRequiredService<MigrationAssembly>());
            Assert.NotSame(historyRepository, scopedProvider.GetRequiredService<HistoryRepository>());
            Assert.NotSame(sqlServerMigrator, scopedProvider.GetRequiredService<SqlServerMigrator>());

            context.Dispose();
        }
    }
}
