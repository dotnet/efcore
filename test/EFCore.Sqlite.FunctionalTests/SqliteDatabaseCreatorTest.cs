// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.



// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class SqliteDatabaseCreatorTest
{
    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Exists_returns_false_when_database_doesnt_exist(bool async, bool useCanConnect)
    {
        var context = CreateContext("Data Source=doesnt-exist.db");

        if (useCanConnect)
        {
            Assert.False(async ? await context.Database.CanConnectAsync() : context.Database.CanConnect());
        }
        else
        {
            var creator = context.GetService<IRelationalDatabaseCreator>();
            Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HasTables_returns_false_when_database_is_empty(bool async)
    {
        using var testStore = SqliteTestStore.GetOrCreateInitialized("Empty");
        var context = CreateContext(testStore.ConnectionString);

        var creator = context.GetService<IRelationalDatabaseCreator>();
        Assert.False(async ? await creator.HasTablesAsync() : creator.HasTables());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HasTables_returns_true_when_database_is_not_empty(bool async)
    {
        using var testStore = SqliteTestStore.GetOrCreateInitialized($"HasATable{(async ? 'A' : 'S')}");
        var context = CreateContext(testStore.ConnectionString);
        context.Database.ExecuteSqlRaw("CREATE TABLE Dummy (Foo INTEGER)");

        var creator = context.GetService<IRelationalDatabaseCreator>();
        Assert.True(async ? await creator.HasTablesAsync() : creator.HasTables());
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Exists_returns_true_when_database_exists(bool async, bool useCanConnect)
    {
        using var testStore = SqliteTestStore.GetOrCreateInitialized("Empty");
        var context = CreateContext(testStore.ConnectionString);

        if (useCanConnect)
        {
            Assert.True(async ? await context.Database.CanConnectAsync() : context.Database.CanConnect());
        }
        else
        {
            var creator = context.GetService<IRelationalDatabaseCreator>();
            Assert.True(async ? await creator.ExistsAsync() : creator.Exists());
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Create_sets_journal_mode_to_wal(bool async)
    {
        using var testStore = SqliteTestStore.GetOrCreate("Create");
        using var context = CreateContext(testStore.ConnectionString);
        var creator = context.GetService<IRelationalDatabaseCreator>();

        if (async)
        {
            await creator.CreateAsync();
        }
        else
        {
            creator.Create();
        }

        testStore.OpenConnection();
        var journalMode = testStore.ExecuteScalar<string>("PRAGMA journal_mode;");
        Assert.Equal("wal", journalMode);
    }

    [ConditionalTheory(Skip = "Issues #25797 and #26016")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Delete_works_even_when_different_connection_exists_to_same_file(bool async)
    {
        using (var context = new BathtubContext("DataSource=bathtub.db"))
        {
            if (async)
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
            }
            else
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }

        using (var context = new BathtubContext("Command Timeout=60;DataSource=bathtub.db"))
        {
            var creator = context.GetService<IRelationalDatabaseCreator>();

            if (async)
            {
                await context.Database.EnsureDeletedAsync();
                Assert.False(await creator.ExistsAsync());
            }
            else
            {
                context.Database.EnsureDeleted();
                Assert.False(creator.Exists());
            }
        }
    }

    private class BathtubContext : DbContext
    {
        private readonly string _connectionString;

        public BathtubContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(_connectionString);
    }

    [ConditionalTheory]
    [InlineData("Data Source=:memory:")]
    [InlineData("Data Source=exists-memory;Mode=Memory;Cache=Shared")]
    public void Exists_returns_true_when_memory(string connectionString)
    {
        var context = CreateContext(connectionString);

        var creator = context.GetService<IRelationalDatabaseCreator>();
        Assert.True(creator.Exists());
    }

    private DbContext CreateContext(string connectionString)
        => new(
            new DbContextOptionsBuilder()
                .UseSqlite(connectionString)
                .UseInternalServiceProvider(
                    SqliteTestStoreFactory.Instance.AddProviderServices(new ServiceCollection())
                        .BuildServiceProvider(validateScopes: true))
                .Options);
}
