// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Services;
using Microsoft.Data.Migrations;
using Microsoft.Data.Relational;
using Xunit;

namespace Microsoft.Data.SqlServer.FunctionalTests
{
    public class SqlServerDataStoreCreatorTest
    {
        [Fact]
        public async Task Exists_returns_false_when_database_doesnt_exist()
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: false))
            {
                var store = CreateStore(testDatabase);

                var creator = new SqlServerDataStoreCreator(
                    store,
                    new ModelDiffer(),
                    new SqlServerMigrationOperationSqlGenerator(),
                    new SqlStatementExecutor());

                Assert.False(await creator.ExistsAsync());
            }
        }

        [Fact]
        public async Task Exists_returns_true_when_database_exists()
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: true))
            {
                var store = CreateStore(testDatabase);

                var creator = new SqlServerDataStoreCreator(
                    store,
                    new ModelDiffer(),
                    new SqlServerMigrationOperationSqlGenerator(),
                    new SqlStatementExecutor());

                Assert.True(await creator.ExistsAsync());
            }
        }

        [Fact]
        public async Task Delete_will_delete_database()
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: true))
            {
                testDatabase.Connection.Close();

                var store = CreateStore(testDatabase);

                var creator = new SqlServerDataStoreCreator(
                    store,
                    new ModelDiffer(),
                    new SqlServerMigrationOperationSqlGenerator(),
                    new SqlStatementExecutor());

                Assert.True(await creator.ExistsAsync());

                await creator.DeleteAsync();

                Assert.False(await creator.ExistsAsync());
            }
        }

        [Fact]
        public async Task Delete_noop_when_database_doesnt_exist()
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: false))
            {
                var store = CreateStore(testDatabase);

                var creator = new SqlServerDataStoreCreator(
                    store,
                    new ModelDiffer(),
                    new SqlServerMigrationOperationSqlGenerator(),
                    new SqlStatementExecutor());

                Assert.False(await creator.ExistsAsync());

                await creator.DeleteAsync();

                Assert.False(await creator.ExistsAsync());
            }
        }

        [Fact]
        public async Task Can_create_schema_in_existing_database()
        {
            using (var testDatabase = await TestDatabase.Scratch())
            {
                await RunDatabaseCreationTest(testDatabase);
            }
        }

        [Fact]
        public async Task Can_create_physical_database_and_schema()
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: false))
            {
                await RunDatabaseCreationTest(testDatabase);
            }
        }

        private static ContextConfiguration CreateConfiguration(TestDatabase testDatabase)
        {
            return new EntityContext(
                new ServiceCollection()
                    .AddEntityFramework(s => s.AddSqlServer())
                    .BuildServiceProvider(),
                new EntityConfigurationBuilder()
                    .SqlServerConnectionString(testDatabase.Connection.ConnectionString)
                    .BuildConfiguration())
                .Configuration;
        }

        private static SqlServerDataStore CreateStore(TestDatabase testDatabase)
        {
            var store = new SqlServerDataStore(CreateConfiguration(testDatabase), new NullLoggerFactory(), new SqlServerSqlGenerator());
            return store;
        }

        private static async Task RunDatabaseCreationTest(TestDatabase testDatabase)
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework(s => s.AddSqlServer())
                .BuildServiceProvider();

            var configuration = new EntityConfigurationBuilder()
                .SqlServerConnectionString(testDatabase.Connection.ConnectionString)
                .BuildConfiguration();

            using (var context = new BloggingContext(serviceProvider, configuration))
            {
                var creator = new SqlServerDataStoreCreator(
                    (SqlServerDataStore)context.Configuration.DataStore,
                    new ModelDiffer(),
                    new SqlServerMigrationOperationSqlGenerator(),
                    new SqlStatementExecutor());

                await creator.CreateAsync(context.Model);

                if (testDatabase.Connection.State != ConnectionState.Open)
                {
                    await testDatabase.Connection.OpenAsync();
                }

                var tables = await testDatabase.QueryAsync<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES");
                Assert.Equal(1, tables.Count());
                Assert.Equal("Blog", tables.Single());

                var columns = await testDatabase.QueryAsync<string>("SELECT TABLE_NAME + '.' + COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS");
                Assert.Equal(2, columns.Count());
                Assert.True(columns.Any(c => c == "Blog.Id"));
                Assert.True(columns.Any(c => c == "Blog.Name"));
            }
        }

        private class BloggingContext : EntityContext
        {
            public BloggingContext(IServiceProvider serviceProvider, EntityConfiguration configuration)
                : base(serviceProvider, configuration)
            {
            }

            public EntitySet<Blog> Blogs { get; set; }

            public class Blog
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
        }
    }
}
