// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Oracle.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    // Tests are split into classes to enable parralel execution
    public class OracleDatabaseCreatorExistsTest
    {
        [ConditionalFact]
        public Task Returns_false_when_database_does_not_exist()
        {
            return Returns_false_when_database_does_not_exist_test(async: false);
        }

        [ConditionalFact]
        public Task Async_returns_false_when_database_does_not_exist()
        {
            return Returns_false_when_database_does_not_exist_test(async: true);
        }

        private static async Task Returns_false_when_database_does_not_exist_test(bool async)
        {
            using (var testDatabase = OracleTestStore.Create("NonExisting"))
            {
                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
                {
                    var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(context);

                    Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }

        [ConditionalFact]
        public Task Returns_true_when_database_exists()
        {
            return Returns_true_when_database_exists_test(async: false);
        }

        [ConditionalFact]
        public Task Async_returns_true_when_database_exists()
        {
            return Returns_true_when_database_exists_test(async: true);
        }

        private static async Task Returns_true_when_database_exists_test(bool async)
        {
            using (var testDatabase = OracleTestStore.GetOrCreateInitialized("ExistingBlogging"))
            {
                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
                {
                    var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(context);

                    Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

                    Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
                }
            }
        }
    }

    public class OracleDatabaseCreatorEnsureDeletedTest
    {
        [ConditionalFact]
        public Task Deletes_database()
        {
            return Delete_database_test(async: false, open: false);
        }

        [ConditionalFact]
        public Task Async_deletes_database()
        {
            return Delete_database_test(async: true, open: false);
        }

        [ConditionalFact]
        public Task Deletes_database_with_opened_connections()
        {
            return Delete_database_test(async: false, open: true);
        }

        [ConditionalFact]
        public Task Async_deletes_database_with_opened_connections()
        {
            return Delete_database_test(async: true, open: true);
        }

        private static async Task Delete_database_test(bool async, bool open)
        {
            using (var testDatabase = OracleTestStore.CreateInitialized("EnsureDeleteBlogging"))
            {
                if (!open)
                {
                    testDatabase.CloseConnection();
                }

                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
                {
                    var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(context);

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

        [ConditionalFact]
        public Task Noop_when_database_does_not_exist()
        {
            return Noop_when_database_does_not_exist_test(async: false);
        }

        [ConditionalFact]
        public Task Async_is_noop_when_database_does_not_exist()
        {
            return Noop_when_database_does_not_exist_test(async: true);
        }

        private static async Task Noop_when_database_does_not_exist_test(bool async)
        {
            using (var testDatabase = OracleTestStore.Create("NonExisting"))
            {
                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
                {
                    var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(context);

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
    }

    public class OracleDatabaseCreatorEnsureCreatedTest
    {
        [ConditionalFact]
        public Task Creates_schema_in_existing_database()
        {
            return Creates_schema_in_existing_database_test(async: false);
        }

        [ConditionalFact]
        public Task Async_creates_schema_in_existing_database()
        {
            return Creates_schema_in_existing_database_test(async: true);
        }

        [ConditionalFact]
        public Task Creates_physical_database_and_schema()
        {
            return Creates_new_physical_database_and_schema_test(async: false);
        }

        [ConditionalFact]
        public Task Async_creates_physical_database_and_schema()
        {
            return Creates_new_physical_database_and_schema_test(async: true);
        }

        private static Task Creates_schema_in_existing_database_test(bool async)
            => Creates_physical_database_and_schema_test((true, async));

        private static Task Creates_new_physical_database_and_schema_test(bool async)
            => Creates_physical_database_and_schema_test((false, async));

        private static async Task Creates_physical_database_and_schema_test(
            (bool CreateDatabase, bool Async) options)
        {
            (var createDatabase, var async) = options;

            using (var testDatabase = OracleTestStore.Create("EnsureCreatedTest"))
            {
                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
                {
                    if (createDatabase)
                    {
                        testDatabase.Initialize(null, (Func<DbContext>)null, null);
                    }
                    else
                    {
                        testDatabase.DeleteDatabase();
                    }

                    var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(context);

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

                    if (testDatabase.ConnectionState != ConnectionState.Open)
                    {
                        await testDatabase.OpenConnectionAsync();
                    }

                    var tables = (await testDatabase.QueryAsync<string>(
                        "SELECT table_name FROM user_tables")).ToList();

                    Assert.Equal(1, tables.Count);
                    Assert.Equal("Blogs", tables.Single());

                    var columns = (await testDatabase.QueryAsync<string>(
                        "SELECT table_name || '.' || column_name || ' (' || data_type || ')' "
                        + "FROM user_tab_columns WHERE table_name = 'Blogs' "
                        + "ORDER BY table_name, column_name")).ToArray();

                    Assert.Equal(14, columns.Length);

                    Assert.Equal(
                        new[]
                        {
                            "Blogs.AndChew (BLOB)",
                            "Blogs.AndRow (RAW)",
                            "Blogs.Cheese (NVARCHAR2)",
                            "Blogs.ErMilan (NUMBER)",
                            "Blogs.Fuse (NUMBER)",
                            "Blogs.George (NUMBER)",
                            "Blogs.Key1 (NVARCHAR2)",
                            "Blogs.Key2 (RAW)",
                            "Blogs.NotFigTime (TIMESTAMP(6))",
                            "Blogs.On (FLOAT)",
                            "Blogs.OrNothing (FLOAT)",
                            "Blogs.TheGu (RAW)",
                            "Blogs.ToEat (NUMBER)",
                            "Blogs.WayRound (NUMBER)"
                        },
                        columns);
                }
            }
        }

        [ConditionalFact]
        public Task Noop_when_database_exists_and_has_schema()
        {
            return Noop_when_database_exists_and_has_schema_test(async: false);
        }

        [ConditionalFact]
        public Task Async_is_noop_when_database_exists_and_has_schema()
        {
            return Noop_when_database_exists_and_has_schema_test(async: true);
        }

        private static async Task Noop_when_database_exists_and_has_schema_test(bool async)
        {
            using (var testDatabase = OracleTestStore.CreateInitialized("InitializedBlogging"))
            {
                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
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
    }

    public class OracleDatabaseCreatorHasTablesTest
    {
        [ConditionalFact]
        public Task Throws_when_database_does_not_exist()
        {
            return Throws_when_database_does_not_exist_test(async: false);
        }

        [ConditionalFact]
        public Task Async_throws_when_database_does_not_exist()
        {
            return Throws_when_database_does_not_exist_test(async: true);
        }

        private static async Task Throws_when_database_does_not_exist_test(bool async)
        {
            using (var testDatabase = OracleTestStore.GetOrCreate("NonExisting"))
            {
                var databaseCreator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);
                await databaseCreator.ExecutionStrategyFactory.Create().ExecuteAsync(
                    databaseCreator,
                    async creator =>
                        {
                            var errorNumber = async
                                ? (await Assert.ThrowsAsync<OracleException>(() => creator.HasTablesAsyncBase())).Number
                                : Assert.Throws<OracleException>(() => creator.HasTablesBase()).Number;

                            if (errorNumber != 233) // skip if no-process transient failure
                            {
                                Assert.Equal(
                                    1017, // Login failed error number
                                    errorNumber);
                            }
                        });
            }
        }

        [ConditionalFact]
        public Task Returns_false_when_database_exists_but_has_no_tables()
        {
            return Returns_false_when_database_exists_but_has_no_tables_test(async: false);
        }

        [ConditionalFact]
        public Task Async_returns_false_when_database_exists_but_has_no_tables()
        {
            return Returns_false_when_database_exists_but_has_no_tables_test(async: true);
        }

        private static async Task Returns_false_when_database_exists_but_has_no_tables_test(bool async)
        {
            using (var testDatabase = OracleTestStore.GetOrCreateInitialized("Empty"))
            {
                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);
                Assert.False(async ? await creator.HasTablesAsyncBase() : creator.HasTablesBase());
            }
        }

        [ConditionalFact]
        public Task Returns_true_when_database_exists_and_has_any_tables()
        {
            return Returns_true_when_database_exists_and_has_any_tables_test(async: false);
        }

        [ConditionalFact]
        public Task Async_returns_true_when_database_exists_and_has_any_tables()
        {
            return Returns_true_when_database_exists_and_has_any_tables_test(async: true);
        }

        private static async Task Returns_true_when_database_exists_and_has_any_tables_test(bool async)
        {
            using (var testDatabase = OracleTestStore.GetOrCreate("ExistingTables")
                .InitializeOracle(null, t => new OracleDatabaseCreatorTest.BloggingContext(t), null))
            {
                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);
                Assert.True(async ? await creator.HasTablesAsyncBase() : creator.HasTablesBase());
            }
        }
    }

    public class OracleDatabaseCreatorDeleteTest
    {
        [ConditionalFact]
        public async Task Deletes_database()
        {
            await Deletes_database_test(async: false);
        }

        [ConditionalFact]
        public async Task Async_deletes_database()
        {
            await Deletes_database_test(async: true);
        }

        private static async Task Deletes_database_test(bool async)
        {
            using (var testDatabase = OracleTestStore.CreateInitialized("DeleteBlogging"))
            {
                testDatabase.CloseConnection();

                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);

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

        [ConditionalFact]
        public Task Throws_when_database_does_not_exist()
        {
            return Throws_when_database_does_not_exist_test(async: false);
        }

        [ConditionalFact]
        public Task Async_throws_when_database_does_not_exist()
        {
            return Throws_when_database_does_not_exist_test(async: true);
        }

        private static async Task Throws_when_database_does_not_exist_test(bool async)
        {
            using (var testDatabase = OracleTestStore.GetOrCreate("NonExistingBlogging"))
            {
                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);

                if (async)
                {
                    await Assert.ThrowsAsync<OracleException>(() => creator.DeleteAsync());
                }
                else
                {
                    Assert.Throws<OracleException>(() => creator.Delete());
                }
            }
        }
    }

    public class OracleDatabaseCreatorCreateTablesTest
    {
        [ConditionalFact]
        public Task Creates_schema_in_existing_database()
        {
            return Creates_schema_in_existing_database_test(async: false);
        }

        [ConditionalFact]
        public Task Async_creates_schema_in_existing_database()
        {
            return Creates_schema_in_existing_database_test(async: true);
        }

        private static async Task Creates_schema_in_existing_database_test(bool async)
        {
            using (var testDatabase = OracleTestStore.GetOrCreateInitialized("ExistingBlogging" + (async ? "Async" : "")))
            {
                using (var context = new OracleDatabaseCreatorTest.BloggingContext(testDatabase))
                {
                    var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(context);

                    if (async)
                    {
                        await creator.CreateTablesAsync();
                    }
                    else
                    {
                        creator.CreateTables();
                    }

                    if (testDatabase.ConnectionState != ConnectionState.Open)
                    {
                        await testDatabase.OpenConnectionAsync();
                    }

                    var tables = (await testDatabase.QueryAsync<string>(
                        "SELECT table_name FROM user_tables")).ToList();

                    Assert.Equal(1, tables.Count);
                    Assert.Equal("Blogs", tables.Single());

                    var columns = (await testDatabase.QueryAsync<string>(
                        "SELECT table_name || '.' || column_name || ' (' || data_type || ')' "
                        + "FROM user_tab_columns WHERE table_name = 'Blogs' "
                        + "ORDER BY table_name, column_name")).ToArray();

                    Assert.Equal(14, columns.Length);

                    Assert.Equal(
                        new[]
                        {
                            "Blogs.AndChew (BLOB)",
                            "Blogs.AndRow (RAW)",
                            "Blogs.Cheese (NVARCHAR2)",
                            "Blogs.ErMilan (NUMBER)",
                            "Blogs.Fuse (NUMBER)",
                            "Blogs.George (NUMBER)",
                            "Blogs.Key1 (NVARCHAR2)",
                            "Blogs.Key2 (RAW)",
                            "Blogs.NotFigTime (TIMESTAMP(6))",
                            "Blogs.On (FLOAT)",
                            "Blogs.OrNothing (FLOAT)",
                            "Blogs.TheGu (RAW)",
                            "Blogs.ToEat (NUMBER)",
                            "Blogs.WayRound (NUMBER)"
                        },
                        columns);
                }
            }
        }

        [ConditionalFact]
        public Task Throws_if_database_does_not_exist()
        {
            return Throws_if_database_does_not_exist_test(async: false);
        }

        [ConditionalFact]
        public Task Async_throws_if_database_does_not_exist()
        {
            return Throws_if_database_does_not_exist_test(async: true);
        }

        private static async Task Throws_if_database_does_not_exist_test(bool async)
        {
            using (var testDatabase = OracleTestStore.GetOrCreate("NonExisting"))
            {
                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);

                var errorNumber
                    = async
                        ? (await Assert.ThrowsAsync<OracleException>(() => creator.CreateTablesAsync())).Number
                        : Assert.Throws<OracleException>(() => creator.CreateTables()).Number;

                if (errorNumber != 233) // skip if no-process transient failure
                {
                    Assert.Equal(
                        1017, // Login failed error number
                        errorNumber);
                }
            }
        }
    }

    public class OracleDatabaseCreatorCreateTest
    {
        [ConditionalFact]
        public Task Creates_physical_database_but_not_tables()
        {
            return Creates_physical_database_but_not_tables_test(async: false);
        }

        [ConditionalFact]
        public Task Async_creates_physical_database_but_not_tables()
        {
            return Creates_physical_database_but_not_tables_test(async: true);
        }

        private static async Task Creates_physical_database_but_not_tables_test(bool async)
        {
            using (var testDatabase = OracleTestStore.GetOrCreate("CreateTest"))
            {
                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);

                creator.EnsureDeleted();

                if (async)
                {
                    await creator.CreateAsync();
                }
                else
                {
                    creator.Create();
                }

                Assert.True(creator.Exists());

                if (testDatabase.ConnectionState != ConnectionState.Open)
                {
                    await testDatabase.OpenConnectionAsync();
                }

                Assert.Equal(
                    0, (await testDatabase.QueryAsync<string>(
                        "SELECT table_name FROM user_tables")).Count());
            }
        }

        [ConditionalFact]
        public Task Throws_if_database_already_exists()
        {
            return Throws_if_database_already_exists_test(async: false);
        }

        [ConditionalFact]
        public Task Async_throws_if_database_already_exists()
        {
            return Throws_if_database_already_exists_test(async: true);
        }

        private static async Task Throws_if_database_already_exists_test(bool async)
        {
            using (var testDatabase = OracleTestStore.GetOrCreateInitialized("ExistingBlogging"))
            {
                var creator = OracleDatabaseCreatorTest.GetDatabaseCreator(testDatabase);

                var ex = async
                    ? await Assert.ThrowsAsync<OracleException>(() => creator.CreateAsync())
                    : Assert.Throws<OracleException>(() => creator.Create());
                Assert.Equal(
                    1920, // Database with given name already exists
                    ex.Number);
            }
        }
    }

    public static class OracleDatabaseCreatorTest
    {
        public static TestDatabaseCreator GetDatabaseCreator(OracleTestStore testStore)
            => GetDatabaseCreator(testStore.ConnectionString);

        public static TestDatabaseCreator GetDatabaseCreator(string connectionString)
            => GetDatabaseCreator(new BloggingContext(connectionString));

        public static TestDatabaseCreator GetDatabaseCreator(BloggingContext context)
            => (TestDatabaseCreator)context.GetService<IRelationalDatabaseCreator>();

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestOracleExecutionStrategyFactory : OracleExecutionStrategyFactory
        {
            public TestOracleExecutionStrategyFactory(ExecutionStrategyDependencies dependencies)
                : base(dependencies)
            {
            }

            protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyDependencies dependencies)
                => new NoopExecutionStrategy(dependencies);
        }

        private static IServiceProvider CreateServiceProvider()
            => new ServiceCollection()
                .AddEntityFrameworkOracle()
                .AddScoped<IExecutionStrategyFactory, TestOracleExecutionStrategyFactory>()
                .AddScoped<IRelationalDatabaseCreator, TestDatabaseCreator>()
                .BuildServiceProvider();

        public class BloggingContext : DbContext
        {
            private readonly string _connectionString;

            public BloggingContext(OracleTestStore testStore)
                : this(testStore.ConnectionString)
            {
            }

            public BloggingContext(string connectionString)
            {
                _connectionString = connectionString;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseOracle(_connectionString, b => b.ApplyConfiguration().CommandTimeout(OracleTestStore.CommandTimeout))
                    .UseInternalServiceProvider(CreateServiceProvider());

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>(
                    b =>
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
            public double OrNothing { get; set; }
            public short Fuse { get; set; }
            public long WayRound { get; set; }
            public float On { get; set; }
            public byte[] AndChew { get; set; }
            public byte[] AndRow { get; set; }
        }

        public class TestDatabaseCreator : OracleDatabaseCreator
        {
            public TestDatabaseCreator(
                RelationalDatabaseCreatorDependencies dependencies,
                IOracleConnection connection,
                IRawSqlCommandBuilder rawSqlCommandBuilder)
                : base(dependencies, connection, rawSqlCommandBuilder)
            {
            }

            public bool HasTablesBase() => HasTables();

            public Task<bool> HasTablesAsyncBase(CancellationToken cancellationToken = default)
                => HasTablesAsync(cancellationToken);

            public IExecutionStrategyFactory ExecutionStrategyFactory => Dependencies.ExecutionStrategyFactory;
        }
    }
}
