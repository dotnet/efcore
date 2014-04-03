// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Data.Migrations;
using Microsoft.Data.Relational;
using Xunit;

namespace Microsoft.Data.SqlServer.FunctionalTests
{
    public class SqlServerDataStoreCreatorTest
    {
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

        private static async Task RunDatabaseCreationTest(TestDatabase testDatabase)
        {
            var config = new EntityConfigurationBuilder()
                .UseSqlServer(testDatabase.Connection.ConnectionString)
                .BuildConfiguration();

            using (var context = new BloggingContext(config))
            {
                var migrator = new SqlServerDataStoreCreator(
                    (SqlServerDataStore)config.DataStore,
                    new ModelDiffer(),
                    new SqlServerMigrationOperationSqlGenerator(),
                    new SqlStatementExecutor());

                await migrator.CreateIfNotExistsAsync(context.Model);

                if (testDatabase.Connection.State != System.Data.ConnectionState.Open)
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
            public BloggingContext(EntityConfiguration config)
                :base(config)
            { }

            public EntitySet<Blog> Blogs { get; set; }

            public class Blog
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
        }
    }
}
