// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class SqlServerDatabaseCreatorTest
    {
        [Fact]
        public async Task Exists_returns_false_when_database_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task Exists_returns_false_when_database_with_filename_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: false, file: true);
        }

        [Fact]
        public async Task ExistsAsync_returns_false_when_database_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task ExistsAsync_returns_false_when_database_with_filename_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: true, file: true);
        }

        private static async Task Exists_returns_false_when_database_doesnt_exist_test(bool async, bool file)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false, useFileName: file))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IRelationalDatabaseCreator>();

                    Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }

        [Fact]
        public async Task Exists_returns_true_when_database_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task Exists_returns_true_when_database_with_filename_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: false, file: true);
        }

        [Fact]
        public async Task ExistsAsync_returns_true_when_database_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task ExistsAsync_returns_true_when_database_with_filename_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: true, file: true);
        }

        private static async Task Exists_returns_true_when_database_exists_test(bool async, bool file)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true, useFileName: file))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IRelationalDatabaseCreator>();

                    Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }

        [Fact]
        public async Task EnsureDeleted_will_delete_database()
        {
            await EnsureDeleted_will_delete_database_test(async: false, open: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureDeleted_will_delete_database_with_filename()
        {
            await EnsureDeleted_will_delete_database_test(async: false, open: false, file: true);
        }

        [Fact]
        public async Task EnsureDeletedAsync_will_delete_database()
        {
            await EnsureDeleted_will_delete_database_test(async: true, open: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureDeletedAsync_will_delete_database_with_filename()
        {
            await EnsureDeleted_will_delete_database_test(async: true, open: false, file: true);
        }

        [Fact]
        public async Task EnsureDeleted_will_delete_database_with_opened_connections()
        {
            await EnsureDeleted_will_delete_database_test(async: false, open: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureDeleted_will_delete_database_with_filename_with_opened_connections()
        {
            await EnsureDeleted_will_delete_database_test(async: false, open: true, file: true);
        }

        [Fact]
        public async Task EnsureDeletedAsync_will_delete_database_with_opened_connections()
        {
            await EnsureDeleted_will_delete_database_test(async: true, open: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureDeletedAsync_will_delete_database_with_filename_with_opened_connections()
        {
            await EnsureDeleted_will_delete_database_test(async: true, open: true, file: true);
        }

        private static async Task EnsureDeleted_will_delete_database_test(bool async, bool open, bool file)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true, useFileName: file))
            {
                if (!open)
                {
                    testDatabase.Connection.Close();
                }

                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IRelationalDatabaseCreator>();

                    Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

                    if (async)
                    {
                        Assert.True(await context.Database.EnsureDeletedAsync());
                    }
                    else
                    {
                        Assert.True(context.Database.EnsureDeleted());
                    }

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                    Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }

        [Fact]
        public async Task EnsuredDeleted_noop_when_database_doesnt_exist()
        {
            await EnsuredDeleted_noop_when_database_doesnt_exist_test(async: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsuredDeleted_noop_when_database_with_filename_doesnt_exist()
        {
            await EnsuredDeleted_noop_when_database_doesnt_exist_test(async: false, file: true);
        }

        [Fact]
        public async Task EnsuredDeletedAsync_noop_when_database_doesnt_exist()
        {
            await EnsuredDeleted_noop_when_database_doesnt_exist_test(async: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsuredDeletedAsync_noop_when_database_with_filename_doesnt_exist()
        {
            await EnsuredDeleted_noop_when_database_doesnt_exist_test(async: true, file: true);
        }

        private static async Task EnsuredDeleted_noop_when_database_doesnt_exist_test(bool async, bool file)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false, useFileName: file))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IRelationalDatabaseCreator>();

                    Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

                    if (async)
                    {
                        Assert.False(await creator.EnsureDeletedAsync());
                    }
                    else
                    {
                        Assert.False(creator.EnsureDeleted());
                    }

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                    Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }

        [Fact]
        public async Task EnsureCreated_can_create_schema_in_existing_database()
        {
            await EnsureCreated_can_create_schema_in_existing_database_test(async: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureCreated_can_create_schema_in_existing_database_with_filename()
        {
            await EnsureCreated_can_create_schema_in_existing_database_test(async: false, file: true);
        }

        [Fact]
        public async Task EnsureCreatedAsync_can_create_schema_in_existing_database()
        {
            await EnsureCreated_can_create_schema_in_existing_database_test(async: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureCreatedAsync_can_create_schema_in_existing_database_with_filename()
        {
            await EnsureCreated_can_create_schema_in_existing_database_test(async: true, file: true);
        }

        private static async Task EnsureCreated_can_create_schema_in_existing_database_test(bool async, bool file)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(useFileName: file))
            {
                await RunDatabaseCreationTest(testDatabase, async);
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
        public async Task EnsureCreated_can_create_physical_database_and_schema()
        {
            await EnsureCreated_can_create_physical_database_and_schema_test(async: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureCreated_can_create_physical_database_with_filename_and_schema()
        {
            await EnsureCreated_can_create_physical_database_and_schema_test(async: false, file: true);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
        public async Task EnsureCreatedAsync_can_create_physical_database_and_schema()
        {
            await EnsureCreated_can_create_physical_database_and_schema_test(async: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsureCreatedAsync_can_create_physical_database_with_filename_and_schema()
        {
            await EnsureCreated_can_create_physical_database_and_schema_test(async: true, file: true);
        }

        private static Task EnsureCreated_can_create_physical_database_and_schema_test(bool async, bool file)
        {
            return SqlServerTestStore.GetExecutionStrategy().ExecuteAsync(async state =>
            {
                using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false, useFileName: state.file))
                {
                    await RunDatabaseCreationTest(testDatabase, state.async);
                }
            }, new { async, file });
        }

        private static async Task RunDatabaseCreationTest(SqlServerTestStore testStore, bool async)
        {
            using (var context = new BloggingContext(testStore))
            {
                var creator = context.GetService<IRelationalDatabaseCreator>();

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                if (async)
                {
                    Assert.True(await creator.EnsureCreatedAsync());
                }
                else
                {
                    Assert.True(creator.EnsureCreated());
                }

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                if (testStore.Connection.State != ConnectionState.Open)
                {
                    await testStore.Connection.OpenAsync();
                }

                var tables = await testStore.QueryAsync<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'");
                Assert.Equal(1, tables.Count());
                Assert.Equal("Blogs", tables.Single());

                var columns = (await testStore.QueryAsync<string>(
                    "SELECT TABLE_NAME + '.' + COLUMN_NAME + ' (' + DATA_TYPE + ')' FROM INFORMATION_SCHEMA.COLUMNS  WHERE TABLE_NAME = 'Blogs' ORDER BY TABLE_NAME, COLUMN_NAME")).ToArray();
                Assert.Equal(15, columns.Length);

                Assert.Equal(
                    new[]
                    {
                        "Blogs.AndChew (varbinary)",
                        "Blogs.AndRow (timestamp)",
                        "Blogs.Cheese (nvarchar)",
                        "Blogs.CupOfChar (int)",
                        "Blogs.ErMilan (int)",
                        "Blogs.Fuse (smallint)",
                        "Blogs.George (bit)",
                        "Blogs.Key1 (nvarchar)",
                        "Blogs.Key2 (varbinary)",
                        "Blogs.NotFigTime (datetime2)",
                        "Blogs.On (real)",
                        "Blogs.OrNothing (float)",
                        "Blogs.TheGu (uniqueidentifier)",
                        "Blogs.ToEat (tinyint)",
                        "Blogs.WayRound (bigint)"
                    },
                    columns);
            }
        }

        [Fact]
        public async Task EnsuredCreated_is_noop_when_database_exists_and_has_schema()
        {
            await EnsuredCreated_is_noop_when_database_exists_and_has_schema_test(async: false, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsuredCreated_is_noop_when_database_with_filename_exists_and_has_schema()
        {
            await EnsuredCreated_is_noop_when_database_exists_and_has_schema_test(async: false, file: true);
        }

        [Fact]
        public async Task EnsuredCreatedAsync_is_noop_when_database_exists_and_has_schema()
        {
            await EnsuredCreated_is_noop_when_database_exists_and_has_schema_test(async: true, file: false);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsAttach)]
        public async Task EnsuredCreatedAsync_is_noop_when_database_with_filename_exists_and_has_schema()
        {
            await EnsuredCreated_is_noop_when_database_exists_and_has_schema_test(async: true, file: true);
        }

        private static async Task EnsuredCreated_is_noop_when_database_exists_and_has_schema_test(bool async, bool file)
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false, useFileName: file))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    context.Database.EnsureCreated();

                    if (async)
                    {
                        Assert.False(await context.Database.EnsureCreatedAsync());
                    }
                    else
                    {
                        Assert.False(context.Database.EnsureCreated());
                    }

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
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
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false))
            {
                await ((TestDatabaseCreator)GetDatabaseCreator(testDatabase)).ExecutionStrategyFactory.Create()
                    .ExecuteAsync(async creator =>
                        {
                            var errorNumber = async
                                ? (await Assert.ThrowsAsync<SqlException>(() => creator.HasTablesAsyncBase())).Number
                                : Assert.Throws<SqlException>(() => creator.HasTablesBase()).Number;

                            if (errorNumber != 233) // skip if no-process transient failure
                            {
                                Assert.Equal(
                                    4060, // Login failed error number
                                    errorNumber);
                            }
                        }, (TestDatabaseCreator)GetDatabaseCreator(testDatabase));
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
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true))
            {
                var creator = GetDatabaseCreator(testDatabase);

                Assert.False(async
                    ? await ((TestDatabaseCreator)creator).HasTablesAsyncBase()
                    : ((TestDatabaseCreator)creator).HasTablesBase());
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
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true))
            {
                await testDatabase.ExecuteNonQueryAsync("CREATE TABLE SomeTable (Id uniqueidentifier)");

                var creator = GetDatabaseCreator(testDatabase);

                Assert.True(async ? await ((TestDatabaseCreator)creator).HasTablesAsyncBase() : ((TestDatabaseCreator)creator).HasTablesBase());
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
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true))
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
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false))
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
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true))
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .BuildServiceProvider();

                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(serviceProvider)
                    .UseSqlServer(testDatabase.ConnectionString, b => b.ApplyConfiguration());

                using (var context = new BloggingContext(testDatabase))
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
                    Assert.Equal("Blogs", tables.Single());

                    var columns = await testDatabase.QueryAsync<string>("SELECT TABLE_NAME + '.' + COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Blogs'");
                    Assert.Equal(15, columns.Count());
                    Assert.True(columns.Any(c => c == "Blogs.Key1"));
                    Assert.True(columns.Any(c => c == "Blogs.Key2"));
                    Assert.True(columns.Any(c => c == "Blogs.Cheese"));
                    Assert.True(columns.Any(c => c == "Blogs.ErMilan"));
                    Assert.True(columns.Any(c => c == "Blogs.George"));
                    Assert.True(columns.Any(c => c == "Blogs.TheGu"));
                    Assert.True(columns.Any(c => c == "Blogs.NotFigTime"));
                    Assert.True(columns.Any(c => c == "Blogs.ToEat"));
                    Assert.True(columns.Any(c => c == "Blogs.CupOfChar"));
                    Assert.True(columns.Any(c => c == "Blogs.OrNothing"));
                    Assert.True(columns.Any(c => c == "Blogs.Fuse"));
                    Assert.True(columns.Any(c => c == "Blogs.WayRound"));
                    Assert.True(columns.Any(c => c == "Blogs.On"));
                    Assert.True(columns.Any(c => c == "Blogs.AndChew"));
                    Assert.True(columns.Any(c => c == "Blogs.AndRow"));
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
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false))
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
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false))
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
                        "'")));
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
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: true))
            {
                var creator = GetDatabaseCreator(testDatabase);

                var ex = async
                    ? await Assert.ThrowsAsync<SqlException>(() => creator.CreateAsync())
                    : Assert.Throws<SqlException>(() => creator.Create());
                Assert.Equal(
                    1801, // Database with given name already exists
                    ex.Number);
            }
        }

        private static IServiceProvider CreateContextServices(SqlServerTestStore testStore)
            => new BloggingContext(testStore).GetInfrastructure();

        private static IRelationalDatabaseCreator GetDatabaseCreator(SqlServerTestStore testStore)
            => CreateContextServices(testStore).GetRequiredService<IRelationalDatabaseCreator>();

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestSqlServerExecutionStrategyFactory : SqlServerExecutionStrategyFactory
        {
            public TestSqlServerExecutionStrategyFactory(ExecutionStrategyContextDependencies dependencies)
                : base(dependencies)
            {
            }

            protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyContext context) => NoopExecutionStrategy.Instance;
        }

        private static IServiceProvider CreateServiceProvider()
            => new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .AddScoped<IExecutionStrategyFactory, TestSqlServerExecutionStrategyFactory>()
            .AddScoped<IRelationalDatabaseCreator, TestDatabaseCreator>()
            .BuildServiceProvider();

        private class BloggingContext : DbContext
        {
            private readonly SqlServerTestStore _testStore;

            public BloggingContext(SqlServerTestStore testStore)
            {
                _testStore = testStore;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseSqlServer(_testStore.ConnectionString, b => b.ApplyConfiguration().CommandTimeout(600))
                    .UseInternalServiceProvider(CreateServiceProvider());

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>(b =>
                {
                    b.HasKey(e => new { e.Key1, e.Key2 });
                    b.Property(e => e.AndRow).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                });
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        public class Blog
        {
            public string Key1 { get; set; }
            public byte[] Key2 { get; set; }
            public string Cheese { get; set; }
            public int ErMilan { get; set; }
            public bool George { get; set; }
            public Guid TheGu { get; set; }
            public DateTime NotFigTime { get; set; }
            public byte ToEat { get; set; }
            public char CupOfChar { get; set; }
            public double OrNothing { get; set; }
            public short Fuse { get; set; }
            public long WayRound { get; set; }
            public float On { get; set; }
            public byte[] AndChew { get; set; }
            public byte[] AndRow { get; set; }
        }
        public class TestDatabaseCreator : SqlServerDatabaseCreator
        {
            public TestDatabaseCreator(
                RelationalDatabaseCreatorDependencies dependencies,
                ISqlServerConnection connection,
                IRawSqlCommandBuilder rawSqlCommandBuilder)
                : base(dependencies, connection, rawSqlCommandBuilder)
            {
            }

            public bool HasTablesBase() => HasTables();

            public Task<bool> HasTablesAsyncBase(CancellationToken cancellationToken = default(CancellationToken))
                => HasTablesAsync(cancellationToken);

            public IExecutionStrategyFactory ExecutionStrategyFactory => Dependencies.ExecutionStrategyFactory;
        }
    }
}
