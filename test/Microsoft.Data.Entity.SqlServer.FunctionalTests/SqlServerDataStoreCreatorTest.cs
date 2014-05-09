// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerDataStoreCreatorTest
    {
        [Fact]
        public async Task Exists_returns_false_when_database_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: false);
        }

        [Fact]
        public async Task ExistsAsync_returns_false_when_database_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: true);
        }

        private static async Task Exists_returns_false_when_database_doesnt_exist_test(bool async)
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: false))
            {
                var creator = GetDataStoreCreator(testDatabase);

                Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
            }
        }

        [Fact]
        public async Task Exists_returns_true_when_database_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: false);
        }

        [Fact]
        public async Task ExistsAsync_returns_true_when_database_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: true);
        }

        private static async Task Exists_returns_true_when_database_exists_test(bool async)
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: true))
            {
                var creator = GetDataStoreCreator(testDatabase);

                Assert.True(async ? await creator.ExistsAsync() : creator.Exists());
            }
        }

        [Fact]
        public async Task Delete_will_delete_database()
        {
            await Delete_will_delete_database_test(async: false);
        }

        [Fact]
        public async Task DeleteAsync_will_delete_database()
        {
            await Delete_will_delete_database_test(async: true);
        }

        private static async Task Delete_will_delete_database_test(bool async)
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: true))
            {
                testDatabase.Connection.Close();

                var creator = GetDataStoreCreator(testDatabase);

                Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

                if (async)
                {
                    await creator.DeleteAsync();
                }
                else
                {
                    creator.Delete();
                }

                Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
            }
        }

        [Fact]
        public async Task Delete_noop_when_database_doesnt_exist()
        {
            await Delete_noop_when_database_doesnt_exist_test(async: false);
        }

        [Fact]
        public async Task DeleteAsync_noop_when_database_doesnt_exist()
        {
            await Delete_noop_when_database_doesnt_exist_test(async: true);
        }

        private static async Task Delete_noop_when_database_doesnt_exist_test(bool async)
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: false))
            {
                var creator = GetDataStoreCreator(testDatabase);

                Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

                if (async)
                {
                    await creator.DeleteAsync();
                }
                else
                {
                    creator.Delete();
                }

                Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
            }
        }

        [Fact]
        public async Task Can_create_schema_in_existing_database()
        {
            await Can_create_schema_in_existing_database_test(async: false);
        }

        [Fact]
        public async Task Can_create_schema_in_existing_database_async()
        {
            await Can_create_schema_in_existing_database_test(async: true);
        }

        private static async Task Can_create_schema_in_existing_database_test(bool async)
        {
            using (var testDatabase = await TestDatabase.Scratch())
            {
                await RunDatabaseCreationTest(testDatabase, async);
            }
        }

        [Fact]
        public async Task Can_create_physical_database_and_schema()
        {
            await Can_create_physical_database_and_schema_test(async: false);
        }

        [Fact]
        public async Task Can_create_physical_database_and_schema_async()
        {
            await Can_create_physical_database_and_schema_test(async: true);
        }

        private static async Task Can_create_physical_database_and_schema_test(bool async)
        {
            using (var testDatabase = await TestDatabase.Scratch(createDatabase: false))
            {
                await RunDatabaseCreationTest(testDatabase, async);
            }
        }

        private static DbContextConfiguration CreateConfiguration(TestDatabase testDatabase)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework().AddSqlServer();
            return new DbContext(
                serviceCollection.BuildServiceProvider(),
                new DbContextOptions()
                    .UseSqlServer(testDatabase.Connection.ConnectionString)
                    .BuildConfiguration())
                .Configuration;
        }

        private static SqlServerDataStoreCreator GetDataStoreCreator(TestDatabase testDatabase)
        {
            return CreateConfiguration(testDatabase).Services.ServiceProvider.GetService<SqlServerDataStoreCreator>();
        }

        private static async Task RunDatabaseCreationTest(TestDatabase testDatabase, bool async)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework().AddSqlServer();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var configuration = new DbContextOptions()
                .UseSqlServer(testDatabase.Connection.ConnectionString)
                .BuildConfiguration();

            using (var context = new BloggingContext(serviceProvider, configuration))
            {
                var creator = context.Configuration.DataStoreCreator;

                if (async)
                {
                    await creator.CreateAsync(context.Model);
                }
                else
                {
                    creator.Create(context.Model);
                }

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

        private class BloggingContext : DbContext
        {
            public BloggingContext(IServiceProvider serviceProvider, ImmutableDbContextOptions configuration)
                : base(serviceProvider, configuration)
            {
            }

            public DbSet<Blog> Blogs { get; set; }

            public class Blog
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
        }
    }
}
