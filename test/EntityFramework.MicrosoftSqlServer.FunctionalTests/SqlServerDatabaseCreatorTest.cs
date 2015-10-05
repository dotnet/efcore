// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerDatabaseCreatorTest
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
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: false))
            {
                var creator = GetDatabaseCreator(testDatabase);

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
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: true))
            {
                var creator = GetDatabaseCreator(testDatabase);

                Assert.True(async ? await creator.ExistsAsync() : creator.Exists());
            }
        }

        [Fact]
        public async Task HasTables_throws_when_database_doesnt_exist()
        {
            await HasTables_throws_when_database_doesnt_exist_test(async: false);
        }

        [Fact]
        public async Task HasTablesAsync_throws_when_database_doesnt_exist()
        {
            await HasTables_throws_when_database_doesnt_exist_test(async: true);
        }

        private static async Task HasTables_throws_when_database_doesnt_exist_test(bool async)
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: false))
            {
                var creator = GetDatabaseCreator(testDatabase);

                var errorNumber = async
                    ? (await Assert.ThrowsAsync<SqlException>(() => ((TestDatabaseCreator)creator).HasTablesAsync())).Number
                    : Assert.Throws<SqlException>(() => ((TestDatabaseCreator)creator).HasTables()).Number;

                if (errorNumber != 233) // skip if no-process transient failure
                {
                    Assert.Equal(
                        4060, // Login failed error number
                        errorNumber);
                }
            }
        }

        [Fact]
        public async Task HasTables_returns_false_when_database_exists_but_has_no_tables()
        {
            await HasTables_returns_false_when_database_exists_but_has_no_tables_test(async: false);
        }

        [Fact]
        public async Task HasTablesAsync_returns_false_when_database_exists_but_has_no_tables()
        {
            await HasTables_returns_false_when_database_exists_but_has_no_tables_test(async: true);
        }

        private static async Task HasTables_returns_false_when_database_exists_but_has_no_tables_test(bool async)
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: true))
            {
                var creator = GetDatabaseCreator(testDatabase);

                Assert.False(async ? await ((TestDatabaseCreator)creator).HasTablesAsync() : ((TestDatabaseCreator)creator).HasTables());
            }
        }

        [Fact]
        public async Task HasTables_returns_true_when_database_exists_and_has_any_tables()
        {
            await HasTables_returns_true_when_database_exists_and_has_any_tables_test(async: false);
        }

        [Fact]
        public async Task HasTablesAsync_returns_true_when_database_exists_and_has_any_tables()
        {
            await HasTables_returns_true_when_database_exists_and_has_any_tables_test(async: true);
        }

        private static async Task HasTables_returns_true_when_database_exists_and_has_any_tables_test(bool async)
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: true))
            {
                await testDatabase.ExecuteNonQueryAsync("CREATE TABLE SomeTable (Id uniqueidentifier)");

                var creator = GetDatabaseCreator(testDatabase);

                Assert.True(async ? await ((TestDatabaseCreator)creator).HasTablesAsync() : ((TestDatabaseCreator)creator).HasTables());
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
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: true))
            {
                testDatabase.Connection.Close();

                var creator = GetDatabaseCreator(testDatabase);

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
        public async Task Delete_throws_when_database_doesnt_exist()
        {
            await Delete_throws_when_database_doesnt_exist_test(async: false);
        }

        [Fact]
        public async Task DeleteAsync_throws_when_database_doesnt_exist()
        {
            await Delete_throws_when_database_doesnt_exist_test(async: true);
        }

        private static async Task Delete_throws_when_database_doesnt_exist_test(bool async)
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: false))
            {
                var creator = GetDatabaseCreator(testDatabase);

                if (async)
                {
                    await Assert.ThrowsAsync<SqlException>(() => creator.DeleteAsync());
                }
                else
                {
                    Assert.Throws<SqlException>(() => creator.Delete());
                }
            }
        }

        [Fact]
        public async Task CreateTables_creates_schema_in_existing_database()
        {
            await CreateTables_creates_schema_in_existing_database_test(async: false);
        }

        [Fact]
        public async Task CreateTablesAsync_creates_schema_in_existing_database()
        {
            await CreateTables_creates_schema_in_existing_database_test(async: true);
        }

        private static async Task CreateTables_creates_schema_in_existing_database_test(bool async)
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync())
            {
                var serviceCollection = new ServiceCollection();
                serviceCollection
                    .AddEntityFramework()
                    .AddSqlServer();

                var serviceProvider = serviceCollection.BuildServiceProvider();

                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlServer(testDatabase.Connection.ConnectionString);

                using (var context = new BloggingContext(serviceProvider, optionsBuilder.Options))
                {
                    var creator = (RelationalDatabaseCreator)context.GetService<IDatabaseCreator>();

                    if (async)
                    {
                        await creator.CreateTablesAsync();
                    }
                    else
                    {
                        creator.CreateTables();
                    }

                    if (testDatabase.Connection.State != ConnectionState.Open)
                    {
                        await testDatabase.Connection.OpenAsync();
                    }

                    var tables = await testDatabase.QueryAsync<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'");
                    Assert.Equal(1, tables.Count());
                    Assert.Equal("Blog", tables.Single());

                    var columns = await testDatabase.QueryAsync<string>("SELECT TABLE_NAME + '.' + COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Blog'");
                    Assert.Equal(2, columns.Count());
                    Assert.True(columns.Any(c => c == "Blog.Id"));
                    Assert.True(columns.Any(c => c == "Blog.Name"));
                }
            }
        }

        [Fact]
        public async Task CreateTables_throws_if_database_does_not_exist()
        {
            await CreateTables_throws_if_database_does_not_exist_test(async: false);
        }

        [Fact]
        public async Task CreateTablesAsync_throws_if_database_does_not_exist()
        {
            await CreateTables_throws_if_database_does_not_exist_test(async: true);
        }

        private static async Task CreateTables_throws_if_database_does_not_exist_test(bool async)
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: false))
            {
                var creator = GetDatabaseCreator(testDatabase);

                var errorNumber
                    = async
                        ? (await Assert.ThrowsAsync<SqlException>(() => creator.CreateTablesAsync())).Number
                        : Assert.Throws<SqlException>(() => creator.CreateTables()).Number;

                if (errorNumber != 233) // skip if no-process transient failure
                {
                    Assert.Equal(
                        4060, // Login failed error number
                        errorNumber);
                }
            }
        }

        [Fact]
        public async Task Create_creates_physical_database_but_not_tables()
        {
            await Create_creates_physical_database_but_not_tables_test(async: false);
        }

        [Fact]
        public async Task CreateAsync_creates_physical_database_but_not_tables()
        {
            await Create_creates_physical_database_but_not_tables_test(async: true);
        }

        private static async Task Create_creates_physical_database_but_not_tables_test(bool async)
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: false))
            {
                var creator = GetDatabaseCreator(testDatabase);

                Assert.False(creator.Exists());

                if (async)
                {
                    await creator.CreateAsync();
                }
                else
                {
                    creator.Create();
                }

                Assert.True(creator.Exists());

                if (testDatabase.Connection.State != ConnectionState.Open)
                {
                    await testDatabase.Connection.OpenAsync();
                }

                Assert.Equal(0, (await testDatabase.QueryAsync<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'")).Count());

                Assert.True(await testDatabase.ExecuteScalarAsync<bool>(
                    string.Concat(
                        "SELECT is_read_committed_snapshot_on FROM sys.databases WHERE name='",
                        testDatabase.Connection.Database,
                        "'"),
                    CancellationToken.None));
            }
        }

        [Fact]
        public async Task Create_throws_if_database_already_exists()
        {
            await Create_throws_if_database_already_exists_test(async: false);
        }

        [Fact]
        public async Task CreateAsync_throws_if_database_already_exists()
        {
            await Create_throws_if_database_already_exists_test(async: true);
        }

        private static async Task Create_throws_if_database_already_exists_test(bool async)
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: true))
            {
                var creator = GetDatabaseCreator(testDatabase);

                Assert.Equal(
                    1801, // Database with given name already exists
                    async
                        ? (await Assert.ThrowsAsync<SqlException>(() => creator.CreateAsync())).Number
                        : Assert.Throws<SqlException>(() => creator.Create()).Number);
            }
        }

        private static IServiceProvider CreateContextServices(SqlServerTestStore testStore)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework()
                .AddSqlServer();

            serviceCollection.AddScoped<SqlServerDatabaseCreator, TestDatabaseCreator>();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(testStore.Connection.ConnectionString);

            return ((IAccessor<IServiceProvider>)new BloggingContext(
                serviceCollection.BuildServiceProvider(),
                optionsBuilder.Options))
                .Service;
        }

        private static IRelationalDatabaseCreator GetDatabaseCreator(SqlServerTestStore testStore)
            => CreateContextServices(testStore).GetRequiredService<IRelationalDatabaseCreator>();

        private class BloggingContext : DbContext
        {
            public BloggingContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
            }

            public DbSet<Blog> Blogs { get; set; }

            public class Blog
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
        }

        public class TestDatabaseCreator : SqlServerDatabaseCreator
        {
            public TestDatabaseCreator(
                ISqlServerConnection connection,
                IMigrationsModelDiffer modelDiffer,
                IMigrationsSqlGenerator sqlGenerator,
                ISqlStatementExecutor statementExecutor,
                IModel model)
                : base(connection, modelDiffer, sqlGenerator, statementExecutor, model)
            {
            }

            public new bool HasTables() => base.HasTables();

            public new Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
                => base.HasTablesAsync(cancellationToken);
        }
    }
}
