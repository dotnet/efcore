// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

// Tests are split into classes to enable parallel execution
// Some combinations are skipped to reduce run time
[SqlServerCondition(SqlServerCondition.IsNotCI)]
public class SqlServerDatabaseCreatorExistsTest : SqlServerDatabaseCreatorTest
{
    [ConditionalTheory]
    [InlineData(true, true, false)]
    [InlineData(false, false, false)]
    [InlineData(true, true, true)]
    [InlineData(false, false, true)]
    public Task Returns_false_when_database_does_not_exist(bool async, bool ambientTransaction, bool useCanConnect)
        => Returns_false_when_database_does_not_exist_test(async, ambientTransaction, useCanConnect, file: false);

    [ConditionalTheory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [SqlServerCondition(SqlServerCondition.SupportsAttach)]
    public Task Returns_false_when_database_with_filename_does_not_exist(bool async, bool ambientTransaction, bool useCanConnect)
        => Returns_false_when_database_does_not_exist_test(async, ambientTransaction, useCanConnect, file: true);

    private static async Task Returns_false_when_database_does_not_exist_test(
        bool async,
        bool ambientTransaction,
        bool useCanConnect,
        bool file)
    {
        using var testDatabase = SqlServerTestStore.Create("NonExisting", file);
        using var context = new BloggingContext(testDatabase);
        var creator = GetDatabaseCreator(context);

        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (CreateTransactionScope(ambientTransaction))
                {
                    if (useCanConnect)
                    {
                        Assert.False(async ? await creator.CanConnectAsync() : creator.CanConnect());
                    }
                    else
                    {
                        Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
                    }
                }
            });

        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    [ConditionalTheory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    public Task Returns_true_when_database_exists(bool async, bool ambientTransaction, bool useCanConnect)
        => Returns_true_when_database_exists_test(async, ambientTransaction, useCanConnect, file: false);

    [ConditionalTheory]
    [InlineData(true, true, false)]
    [InlineData(false, false, false)]
    [InlineData(true, true, true)]
    [InlineData(false, false, true)]
    [SqlServerCondition(SqlServerCondition.SupportsAttach)]
    public Task Returns_true_when_database_with_filename_exists(bool async, bool ambientTransaction, bool useCanConnect)
        => Returns_true_when_database_exists_test(async, ambientTransaction, useCanConnect, file: true);

    private static async Task Returns_true_when_database_exists_test(bool async, bool ambientTransaction, bool useCanConnect, bool file)
    {
        using var testDatabase = file
            ? await SqlServerTestStore.CreateInitializedAsync("ExistingBloggingFile", useFileName: true)
            : await SqlServerTestStore.GetOrCreateInitializedAsync("ExistingBlogging");
        using var context = new BloggingContext(testDatabase);
        var creator = GetDatabaseCreator(context);

        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (CreateTransactionScope(ambientTransaction))
                {
                    if (useCanConnect)
                    {
                        Assert.True(async ? await creator.CanConnectAsync() : creator.CanConnect());
                    }
                    else
                    {
                        Assert.True(async ? await creator.ExistsAsync() : creator.Exists());
                    }
                }
            });

        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }
}

[SqlServerCondition(SqlServerCondition.IsNotCI)]
public class SqlServerDatabaseCreatorEnsureDeletedTest : SqlServerDatabaseCreatorTest
{
    [ConditionalTheory]
    [InlineData(true, true, true)]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public Task Deletes_database(bool async, bool open, bool ambientTransaction)
        => Delete_database_test(async, open, ambientTransaction, file: false);

    [ConditionalTheory]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    [SqlServerCondition(SqlServerCondition.SupportsAttach)]
    public Task Deletes_database_with_filename(bool async, bool open, bool ambientTransaction)
        => Delete_database_test(async, open, ambientTransaction, file: true);

    private static async Task Delete_database_test(bool async, bool open, bool ambientTransaction, bool file)
    {
        using var testDatabase = await SqlServerTestStore.CreateInitializedAsync("EnsureDeleteBlogging" + (file ? "File" : ""), file);
        if (!open)
        {
            testDatabase.CloseConnection();
        }

        using var context = new BloggingContext(testDatabase);
        var creator = GetDatabaseCreator(context);

        Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

        await GetExecutionStrategy(testDatabase).ExecuteAsync(
            async () =>
            {
                using (CreateTransactionScope(ambientTransaction))
                {
                    if (async)
                    {
                        Assert.True(await context.Database.EnsureDeletedAsync());
                    }
                    else
                    {
                        Assert.True(context.Database.EnsureDeleted());
                    }
                }
            });

        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

        Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Noop_when_database_does_not_exist(bool async)
        => Noop_when_database_does_not_exist_test(async, file: false);

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    [SqlServerCondition(SqlServerCondition.SupportsAttach)]
    public Task Noop_when_database_with_filename_does_not_exist(bool async)
        => Noop_when_database_does_not_exist_test(async, file: true);

    private static async Task Noop_when_database_does_not_exist_test(bool async, bool file)
    {
        using var testDatabase = SqlServerTestStore.Create("NonExisting", file);
        using var context = new BloggingContext(testDatabase);
        var creator = GetDatabaseCreator(context);

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

[SqlServerCondition(SqlServerCondition.IsNotCI)]
public class SqlServerDatabaseCreatorEnsureCreatedTest : SqlServerDatabaseCreatorTest
{
    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public Task Creates_schema_in_existing_database(bool async, bool ambientTransaction)
        => Creates_schema_in_existing_database_test(async, ambientTransaction, file: false);

    [ConditionalTheory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [SqlServerCondition(SqlServerCondition.SupportsAttach)]
    public Task Creates_schema_in_existing_database_with_filename(bool async, bool ambientTransaction)
        => Creates_schema_in_existing_database_test(async, ambientTransaction, file: true);

    private static Task Creates_schema_in_existing_database_test(bool async, bool ambientTransaction, bool file)
        => TestEnvironment.IsSqlAzure
            ? new TestSqlServerRetryingExecutionStrategy().ExecuteAsync(
                (true, async, ambientTransaction, file), Creates_physical_database_and_schema_test)
            : Creates_physical_database_and_schema_test((true, async, ambientTransaction, file));

    [ConditionalTheory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
    public Task Creates_physical_database_and_schema(bool async, bool ambientTransaction)
        => Creates_new_physical_database_and_schema_test(async, ambientTransaction, file: false);

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [SqlServerCondition(SqlServerCondition.SupportsAttach)]
    public Task Creates_physical_database_with_filename_and_schema(bool async, bool ambientTransaction)
        => Creates_new_physical_database_and_schema_test(async, ambientTransaction, file: true);

    private static Task Creates_new_physical_database_and_schema_test(bool async, bool ambientTransaction, bool file)
        => TestEnvironment.IsSqlAzure
            ? new TestSqlServerRetryingExecutionStrategy().ExecuteAsync(
                (false, async, ambientTransaction, file), Creates_physical_database_and_schema_test)
            : Creates_physical_database_and_schema_test((false, async, ambientTransaction, file));

    private static async Task Creates_physical_database_and_schema_test(
        (bool CreateDatabase, bool Async, bool ambientTransaction, bool File) options)
    {
        var (createDatabase, async, ambientTransaction, file) = options;
        using var testDatabase = SqlServerTestStore.Create("EnsureCreatedTest" + (file ? "File" : ""), file);
        using var context = new BloggingContext(testDatabase);
        if (createDatabase)
        {
            await testDatabase.InitializeAsync(null, (Func<DbContext>)null);
        }
        else
        {
            testDatabase.DeleteDatabase();
        }

        var creator = GetDatabaseCreator(context);

        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

        using (CreateTransactionScope(ambientTransaction))
        {
            if (async)
            {
                Assert.True(await creator.EnsureCreatedAsync());
            }
            else
            {
                Assert.True(creator.EnsureCreated());
            }
        }

        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

        if (testDatabase.ConnectionState != ConnectionState.Open)
        {
            await testDatabase.OpenConnectionAsync();
        }

        var tables = testDatabase.Query<string>(
            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'").ToList();
        Assert.Single(tables);
        Assert.Equal("Blogs", tables.Single());

        var columns = testDatabase.Query<string>(
                "SELECT TABLE_NAME + '.' + COLUMN_NAME + ' (' + DATA_TYPE + ')' FROM INFORMATION_SCHEMA.COLUMNS  WHERE TABLE_NAME = 'Blogs' ORDER BY TABLE_NAME, COLUMN_NAME")
            .ToArray();
        Assert.Equal(14, columns.Length);

        Assert.Equal(
            new[]
            {
                "Blogs.AndChew (varbinary)",
                "Blogs.AndRow (timestamp)",
                "Blogs.Cheese (nvarchar)",
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

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Noop_when_database_exists_and_has_schema(bool async)
        => Noop_when_database_exists_and_has_schema_test(async, file: false);

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    [SqlServerCondition(SqlServerCondition.SupportsAttach)]
    public Task Noop_when_database_with_filename_exists_and_has_schema(bool async)
        => Noop_when_database_exists_and_has_schema_test(async, file: true);

    private static async Task Noop_when_database_exists_and_has_schema_test(bool async, bool file)
    {
        using var testDatabase = await SqlServerTestStore.CreateInitializedAsync("InitializedBlogging" + (file ? "File" : ""), file);
        using var context = new BloggingContext(testDatabase);
        context.Database.EnsureCreatedResiliently();

        if (async)
        {
            Assert.False(await context.Database.EnsureCreatedResilientlyAsync());
        }
        else
        {
            Assert.False(context.Database.EnsureCreatedResiliently());
        }

        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }
}

[SqlServerCondition(SqlServerCondition.IsNotCI)]
public class SqlServerDatabaseCreatorHasTablesTest : SqlServerDatabaseCreatorTest
{
    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Throws_when_database_does_not_exist(bool async)
    {
        using var testDatabase = SqlServerTestStore.GetOrCreate("NonExisting");
        var databaseCreator = GetDatabaseCreator(testDatabase);
        await databaseCreator.ExecutionStrategy.ExecuteAsync(
            databaseCreator,
            async creator =>
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
            });
    }

    [ConditionalTheory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Returns_false_when_database_exists_but_has_no_tables(bool async, bool ambientTransaction)
    {
        using var testDatabase = await SqlServerTestStore.GetOrCreateInitializedAsync("Empty");
        var creator = GetDatabaseCreator(testDatabase);

        await GetExecutionStrategy(testDatabase).ExecuteAsync(
            async () =>
            {
                using (CreateTransactionScope(ambientTransaction))
                {
                    Assert.False(async ? await creator.HasTablesAsyncBase() : creator.HasTablesBase());
                }
            });
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task Returns_true_when_database_exists_and_has_any_tables(bool async, bool ambientTransaction)
    {
        using var testDatabase = await SqlServerTestStore.GetOrCreate("ExistingTables")
            .InitializeSqlServerAsync(null, t => new BloggingContext(t), null);
        var creator = GetDatabaseCreator(testDatabase);

        await GetExecutionStrategy(testDatabase).ExecuteAsync(
            async () =>
            {
                using (CreateTransactionScope(ambientTransaction))
                {
                    Assert.True(async ? await creator.HasTablesAsyncBase() : creator.HasTablesBase());
                }
            });
    }
}

[SqlServerCondition(SqlServerCondition.IsNotCI)]
public class SqlServerDatabaseCreatorDeleteTest : SqlServerDatabaseCreatorTest
{
    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public static async Task Deletes_database(bool async, bool ambientTransaction)
    {
        using var testDatabase = await SqlServerTestStore.CreateInitializedAsync("DeleteBlogging");
        testDatabase.CloseConnection();

        var creator = GetDatabaseCreator(testDatabase);

        Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

        using (CreateTransactionScope(ambientTransaction))
        {
            if (async)
            {
                await creator.DeleteAsync();
            }
            else
            {
                creator.Delete();
            }
        }

        Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Throws_when_database_does_not_exist(bool async)
    {
        using var testDatabase = SqlServerTestStore.GetOrCreate("NonExistingBlogging");
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

    [ConditionalFact]
    public void Throws_when_no_initial_catalog()
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection);
        connectionStringBuilder.Remove("Initial Catalog");

        var creator = GetDatabaseCreator(connectionStringBuilder.ToString());

        var ex = Assert.Throws<InvalidOperationException>(() => creator.Delete());

        Assert.Equal(SqlServerStrings.NoInitialCatalog, ex.Message);
    }
}

[SqlServerCondition(SqlServerCondition.IsNotCI)]
public class SqlServerDatabaseCreatorCreateTablesTest : SqlServerDatabaseCreatorTest
{
    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task Creates_schema_in_existing_database_test(bool async, bool ambientTransaction)
    {
        using var testDatabase = await SqlServerTestStore.GetOrCreateInitializedAsync("ExistingBlogging" + (async ? "Async" : ""));
        using var context = new BloggingContext(testDatabase);
        var creator = GetDatabaseCreator(context);

        using (CreateTransactionScope(ambientTransaction))
        {
            if (async)
            {
                await creator.CreateTablesAsync();
            }
            else
            {
                creator.CreateTables();
            }
        }

        if (testDatabase.ConnectionState != ConnectionState.Open)
        {
            await testDatabase.OpenConnectionAsync();
        }

        var tables = (await testDatabase.QueryAsync<string>(
            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'")).ToList();
        Assert.Single(tables);
        Assert.Equal("Blogs", tables.Single());

        var columns = (await testDatabase.QueryAsync<string>(
            "SELECT TABLE_NAME + '.' + COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Blogs'")).ToList();
        Assert.Equal(14, columns.Count);
        Assert.Contains(columns, c => c == "Blogs.Key1");
        Assert.Contains(columns, c => c == "Blogs.Key2");
        Assert.Contains(columns, c => c == "Blogs.Cheese");
        Assert.Contains(columns, c => c == "Blogs.ErMilan");
        Assert.Contains(columns, c => c == "Blogs.George");
        Assert.Contains(columns, c => c == "Blogs.TheGu");
        Assert.Contains(columns, c => c == "Blogs.NotFigTime");
        Assert.Contains(columns, c => c == "Blogs.ToEat");
        Assert.Contains(columns, c => c == "Blogs.OrNothing");
        Assert.Contains(columns, c => c == "Blogs.Fuse");
        Assert.Contains(columns, c => c == "Blogs.WayRound");
        Assert.Contains(columns, c => c == "Blogs.On");
        Assert.Contains(columns, c => c == "Blogs.AndChew");
        Assert.Contains(columns, c => c == "Blogs.AndRow");
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Throws_if_database_does_not_exist(bool async)
    {
        using var testDatabase = SqlServerTestStore.GetOrCreate("NonExisting");
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

    [ConditionalFact]
    public void GenerateCreateScript_works()
    {
        using var context = new BloggingContext("Data Source=foo");
        var script = context.Database.GenerateCreateScript();
        Assert.Equal(
            "CREATE TABLE [Blogs] ("
            + _eol
            + "    [Key1] nvarchar(450) NOT NULL,"
            + _eol
            + "    [Key2] varbinary(900) NOT NULL,"
            + _eol
            + "    [Cheese] nvarchar(max) NULL,"
            + _eol
            + "    [ErMilan] int NOT NULL,"
            + _eol
            + "    [George] bit NOT NULL,"
            + _eol
            + "    [TheGu] uniqueidentifier NOT NULL,"
            + _eol
            + "    [NotFigTime] datetime2 NOT NULL,"
            + _eol
            + "    [ToEat] tinyint NOT NULL,"
            + _eol
            + "    [OrNothing] float NOT NULL,"
            + _eol
            + "    [Fuse] smallint NOT NULL,"
            + _eol
            + "    [WayRound] bigint NOT NULL,"
            + _eol
            + "    [On] real NOT NULL,"
            + _eol
            + "    [AndChew] varbinary(max) NULL,"
            + _eol
            + "    [AndRow] rowversion NULL,"
            + _eol
            + "    CONSTRAINT [PK_Blogs] PRIMARY KEY ([Key1], [Key2])"
            + _eol
            + ");"
            + _eol
            + "GO"
            + _eol
            + _eol
            + _eol,
            script);
    }

    private static readonly string _eol = Environment.NewLine;
}

[SqlServerCondition(SqlServerCondition.IsNotCI)]
public class SqlServerDatabaseCreatorCreateTest : SqlServerDatabaseCreatorTest
{
    [ConditionalTheory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Creates_physical_database_but_not_tables(bool async, bool ambientTransaction)
    {
        using var testDatabase = SqlServerTestStore.GetOrCreate("CreateTest");
        var creator = GetDatabaseCreator(testDatabase);

        creator.EnsureDeleted();

        await GetExecutionStrategy(testDatabase).ExecuteAsync(
            async () =>
            {
                using (CreateTransactionScope(ambientTransaction))
                {
                    if (async)
                    {
                        await creator.CreateAsync();
                    }
                    else
                    {
                        creator.Create();
                    }
                }
            });

        Assert.True(creator.Exists());

        if (testDatabase.ConnectionState != ConnectionState.Open)
        {
            await testDatabase.OpenConnectionAsync();
        }

        Assert.Empty(
            (await testDatabase.QueryAsync<string>(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'")));

        Assert.True(
            await testDatabase.ExecuteScalarAsync<bool>(
                string.Concat(
                    "SELECT is_read_committed_snapshot_on FROM sys.databases WHERE name='",
                    testDatabase.Name,
                    "'")));
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Throws_if_database_already_exists(bool async)
    {
        using var testDatabase = await SqlServerTestStore.GetOrCreateInitializedAsync("ExistingBlogging");
        var creator = GetDatabaseCreator(testDatabase);

        var ex = async
            ? await Assert.ThrowsAsync<SqlException>(() => creator.CreateAsync())
            : Assert.Throws<SqlException>(() => creator.Create());
        Assert.Equal(
            1801, // Database with given name already exists
            ex.Number);
    }
}

#pragma warning disable RCS1102 // Make class static.
[SqlServerCondition(SqlServerCondition.IsNotSqlAzure | SqlServerCondition.IsNotCI)]
public class SqlServerDatabaseCreatorTest
{
    protected static IDisposable CreateTransactionScope(bool useTransaction)
        => TestStore.CreateTransactionScope(useTransaction);

    protected static TestDatabaseCreator GetDatabaseCreator(SqlServerTestStore testStore)
        => GetDatabaseCreator(testStore.ConnectionString);

    protected static TestDatabaseCreator GetDatabaseCreator(string connectionString)
        => GetDatabaseCreator(new BloggingContext(connectionString));

    protected static TestDatabaseCreator GetDatabaseCreator(BloggingContext context)
        => (TestDatabaseCreator)context.GetService<IRelationalDatabaseCreator>();

    protected static IExecutionStrategy GetExecutionStrategy(SqlServerTestStore testStore)
        => new BloggingContext(testStore).GetService<IExecutionStrategyFactory>().Create();

    // ReSharper disable once ClassNeverInstantiated.Local
    private class TestSqlServerExecutionStrategyFactory(ExecutionStrategyDependencies dependencies)
        : SqlServerExecutionStrategyFactory(dependencies)
    {
        protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyDependencies dependencies)
            => new NonRetryingExecutionStrategy(dependencies);
    }

    private static IServiceProvider CreateServiceProvider()
        => new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .AddScoped<IExecutionStrategyFactory, TestSqlServerExecutionStrategyFactory>()
            .AddScoped<IRelationalDatabaseCreator, TestDatabaseCreator>()
            .BuildServiceProvider(validateScopes: true);

    protected class BloggingContext(string connectionString) : DbContext
    {
        private readonly string _connectionString = connectionString;

        public BloggingContext(SqlServerTestStore testStore)
            : this(testStore.ConnectionString)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(_connectionString, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(CreateServiceProvider());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Blog>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Key1, e.Key2 });
                    b.Property(e => e.AndRow).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                });

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

    public class TestDatabaseCreator(
        RelationalDatabaseCreatorDependencies dependencies,
        ISqlServerConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder) : SqlServerDatabaseCreator(dependencies, connection, rawSqlCommandBuilder)
    {
        public bool HasTablesBase()
            => HasTables();

        public Task<bool> HasTablesAsyncBase(CancellationToken cancellationToken = default)
            => HasTablesAsync(cancellationToken);

        public IExecutionStrategy ExecutionStrategy
            => Dependencies.ExecutionStrategy;
    }
}
