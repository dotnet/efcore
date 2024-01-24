// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using IsolationLevel = System.Data.IsolationLevel;

namespace Microsoft.EntityFrameworkCore;

public class RelationalDatabaseFacadeExtensionsTest
{
    [ConditionalFact]
    public void Return_true_if_relational()
    {
        using var context = FakeRelationalTestHelpers.Instance.CreateContext();
        Assert.True(context.Database.IsRelational());
    }

    [ConditionalFact]
    public void Return_false_if_inMemory()
    {
        using var context = InMemoryTestHelpers.Instance.CreateContext();
        Assert.False(context.Database.IsRelational());
    }

    [ConditionalFact]
    public void GetDbConnection_returns_the_current_connection()
    {
        var dbConnection = new FakeDbConnection("A=B");
        var context = FakeRelationalTestHelpers.Instance.CreateContext();

        ((FakeRelationalConnection)context.GetService<IRelationalConnection>()).UseConnection(dbConnection);

        Assert.Same(dbConnection, context.Database.GetDbConnection());
    }

    [ConditionalFact]
    public void Relational_specific_methods_throws_when_non_relational_provider_is_in_use()
    {
        var optionsBuilder = new DbContextOptionsBuilder()
            .UseInternalServiceProvider(
                new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider(validateScopes: true))
            .UseInMemoryDatabase(Guid.NewGuid().ToString());
        var context = new DbContext(optionsBuilder.Options);

        Assert.Equal(
            RelationalStrings.RelationalNotInUse,
            Assert.Throws<InvalidOperationException>(() => context.Database.GetDbConnection()).Message);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Can_open_the_underlying_connection(bool async)
    {
        var dbConnection = new FakeDbConnection("A=B");
        var context = FakeRelationalTestHelpers.Instance.CreateContext();

        ((FakeRelationalConnection)context.GetService<IRelationalConnection>()).UseConnection(dbConnection);

        if (async)
        {
            await context.Database.OpenConnectionAsync();
            Assert.Equal(1, dbConnection.OpenAsyncCount);
        }
        else
        {
            context.Database.OpenConnection();
            Assert.Equal(1, dbConnection.OpenCount);
        }
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Can_close_the_underlying_connection(bool async)
    {
        var dbConnection = new FakeDbConnection("A=B");
        var context = FakeRelationalTestHelpers.Instance.CreateContext();

        ((FakeRelationalConnection)context.GetService<IRelationalConnection>()).UseConnection(dbConnection);

        if (async)
        {
            await context.Database.OpenConnectionAsync();
            await context.Database.CloseConnectionAsync();
        }
        else
        {
            context.Database.OpenConnection();
            context.Database.CloseConnection();
        }

        Assert.Equal(1, dbConnection.CloseCount);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Can_begin_transaction_with_isolation_level(bool async)
    {
        var dbConnection = new FakeDbConnection("A=B");
        var context = FakeRelationalTestHelpers.Instance.CreateContext();
        ((FakeRelationalConnection)context.GetService<IRelationalConnection>()).UseConnection(dbConnection);

        var transaction = async
            ? await context.Database.BeginTransactionAsync(IsolationLevel.Chaos)
            : context.Database.BeginTransaction(IsolationLevel.Chaos);

        Assert.Same(dbConnection.DbTransactions.Single(), transaction.GetDbTransaction());
        Assert.Equal(IsolationLevel.Chaos, transaction.GetDbTransaction().IsolationLevel);
    }

    [ConditionalFact]
    public void Can_use_transaction()
    {
        var dbConnection = new FakeDbConnection("A=B");
        var context = FakeRelationalTestHelpers.Instance.CreateContext();
        ((FakeRelationalConnection)context.GetService<IRelationalConnection>()).UseConnection(dbConnection);
        var transaction = new FakeDbTransaction(dbConnection, IsolationLevel.Chaos);

        Assert.Same(transaction, context.Database.UseTransaction(transaction).GetDbTransaction());
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Begin_transaction_ignores_isolation_level_on_non_relational_provider(bool async)
    {
        var context = InMemoryTestHelpers.Instance.CreateContext(
            new ServiceCollection().AddScoped<IDbContextTransactionManager, FakeDbContextTransactionManager>());

        var transactionManager = (FakeDbContextTransactionManager)context.GetService<IDbContextTransactionManager>();

        if (async)
        {
            await context.Database.BeginTransactionAsync(IsolationLevel.Chaos);
            Assert.Equal(1, transactionManager.BeginAsyncCount);
        }
        else
        {
            context.Database.BeginTransaction(IsolationLevel.Chaos);
            Assert.Equal(1, transactionManager.BeginCount);
        }
    }

    private class FakeDbContextTransactionManager : IDbContextTransactionManager
    {
        public int BeginCount { get; set; }
        public int BeginAsyncCount { get; set; }

        public void ResetState()
        {
        }

        public Task ResetStateAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public IDbContextTransaction BeginTransaction()
        {
            BeginCount++;
            return new InMemoryTransaction();
        }

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            BeginAsyncCount++;
            return Task.FromResult<IDbContextTransaction>(new InMemoryTransaction());
        }

        public void CommitTransaction()
        {
        }

        public void RollbackTransaction()
        {
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public IDbContextTransaction CurrentTransaction { get; }

        public Transaction EnlistedTransaction { get; }

        public void EnlistTransaction(Transaction transaction)
        {
        }
    }

    [ConditionalFact]
    public void use_transaction_throws_on_non_relational_provider()
    {
        var transaction = new FakeDbTransaction(new FakeDbConnection("A=B"));
        var context = InMemoryTestHelpers.Instance.CreateContext();

        Assert.Equal(
            RelationalStrings.RelationalNotInUse,
            Assert.Throws<InvalidOperationException>(
                () => context.Database.UseTransaction(transaction)).Message);
    }

    [ConditionalFact]
    public void GetMigrations_works()
    {
        var migrations = new[] { "00000000000001_One", "00000000000002_Two", "00000000000003_Three" };

        var migrationsAssembly = new FakeIMigrationsAssembly { Migrations = migrations.ToDictionary(x => x, x => default(TypeInfo)) };

        var db = FakeRelationalTestHelpers.Instance.CreateContext(
            new ServiceCollection().AddSingleton<IMigrationsAssembly>(migrationsAssembly));

        Assert.Equal(migrations, db.Database.GetMigrations());
    }

    private class FakeIMigrationsAssembly : IMigrationsAssembly
    {
        public IReadOnlyDictionary<string, TypeInfo> Migrations { get; set; }
        public ModelSnapshot ModelSnapshot { get; set; }
        public Assembly Assembly { get; }

        public string FindMigrationId(string nameOrId)
            => throw new NotImplementedException();

        public Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
            => throw new NotImplementedException();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetAppliedMigrations_works(bool async)
    {
        var migrations = new[] { "00000000000001_One", "00000000000002_Two" };

        var repository = new FakeHistoryRepository { AppliedMigrations = migrations.Select(id => new HistoryRow(id, "1.1.0")).ToList() };

        var context = FakeRelationalTestHelpers.Instance.CreateContext(
            new ServiceCollection().AddSingleton<IHistoryRepository>(repository));

        Assert.Equal(
            migrations,
            async
                ? await context.Database.GetAppliedMigrationsAsync()
                : context.Database.GetAppliedMigrations());
    }

    [ConditionalFact]
    public void HasPendingModelChanges_has_no_migrations_has_dbcontext_changes_returns_true()
    {
        // This project has NO existing migrations right now but does have information in the DbContext
        var migrationsAssembly = new FakeIMigrationsAssembly
        {
            ModelSnapshot = null, Migrations = new Dictionary<string, TypeInfo>(),
        };

        var testHelper = FakeRelationalTestHelpers.Instance;

        var contextOptions = testHelper.CreateOptions(
            testHelper.CreateServiceProvider(new ServiceCollection().AddSingleton<IMigrationsAssembly>(migrationsAssembly)));

        var testContext = new TestDbContext(contextOptions);

        Assert.True(testContext.Database.HasPendingModelChanges());
    }

    [ConditionalFact]
    public void HasPendingModelChanges_has_migrations_and_no_new_context_changes_returns_false()
    {
        var fakeModelSnapshot = new FakeModelSnapshot(
            builder =>
            {
                builder.Entity(
                    "Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensionsTests.TestDbContext.Simple", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasColumnType("default_int_mapping");

                        b.HasKey("Id");

                        b.ToTable("Simples");
                    });
            });
        var migrationsAssembly = new FakeIMigrationsAssembly
        {
            ModelSnapshot = fakeModelSnapshot, Migrations = new Dictionary<string, TypeInfo>(),
        };

        var testHelper = FakeRelationalTestHelpers.Instance;

        var contextOptions = testHelper.CreateOptions(
            testHelper.CreateServiceProvider(new ServiceCollection().AddSingleton<IMigrationsAssembly>(migrationsAssembly)));

        var testContext = new TestDbContext(contextOptions);

        Assert.False(testContext.Database.HasPendingModelChanges());
    }

    private class TestDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Simple> Simples { get; set; }

        public class Simple
        {
            public int Id { get; set; }
        }
    }

    private class FakeModelSnapshot(Action<ModelBuilder> buildModel) : ModelSnapshot
    {
        private readonly Action<ModelBuilder> _buildModel = buildModel;

        protected override void BuildModel(ModelBuilder modelBuilder)
            => _buildModel(modelBuilder);
    }

    private class FakeHistoryRepository : IHistoryRepository
    {
        public List<HistoryRow> AppliedMigrations { get; set; }

        public IReadOnlyList<HistoryRow> GetAppliedMigrations()
            => AppliedMigrations;

        public Task<IReadOnlyList<HistoryRow>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<HistoryRow>>(AppliedMigrations);

        public bool Exists()
            => throw new NotImplementedException();

        public Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public string GetCreateScript()
            => throw new NotImplementedException();

        public string GetCreateIfNotExistsScript()
            => throw new NotImplementedException();

        public string GetInsertScript(HistoryRow row)
            => throw new NotImplementedException();

        public string GetDeleteScript(string migrationId)
            => throw new NotImplementedException();

        public string GetBeginIfNotExistsScript(string migrationId)
            => throw new NotImplementedException();

        public string GetBeginIfExistsScript(string migrationId)
            => throw new NotImplementedException();

        public string GetEndIfScript()
            => throw new NotImplementedException();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetPendingMigrations_works(bool async)
    {
        var migrations = new[] { "00000000000001_One", "00000000000002_Two", "00000000000003_Three" };

        var appliedMigrations = new[] { "00000000000001_One", "00000000000002_Two" };

        var migrationsAssembly = new FakeIMigrationsAssembly { Migrations = migrations.ToDictionary(x => x, x => default(TypeInfo)) };

        var repository = new FakeHistoryRepository
        {
            AppliedMigrations = appliedMigrations.Select(id => new HistoryRow(id, "1.1.0")).ToList()
        };

        var context = FakeRelationalTestHelpers.Instance.CreateContext(
            new ServiceCollection()
                .AddSingleton<IHistoryRepository>(repository)
                .AddSingleton<IMigrationsAssembly>(migrationsAssembly));

        Assert.Equal(["00000000000003_Three"],
            async
                ? await context.Database.GetPendingMigrationsAsync()
                : context.Database.GetPendingMigrations());
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Can_pass_no_params(bool async, bool cancellation)
    {
        using var context = new ThudContext();
        var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

        if (async)
        {
            if (cancellation)
            {
                var cancellationToken = new CancellationToken();
                await context.Database.ExecuteSqlRawAsync("<Some query>", cancellationToken);
            }
            else
            {
                await context.Database.ExecuteSqlRawAsync("<Some query>");
            }
        }
        else
        {
            context.Database.ExecuteSqlRaw("<Some query>");
        }

        Assert.Equal("<Some query>", commandBuilder.Sql);
        Assert.Equal([], commandBuilder.Parameters);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Can_pass_array_of_int_params_as_object(bool async, bool cancellation)
    {
        using var context = new ThudContext();
        var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

        if (async)
        {
            if (cancellation)
            {
                var cancellationToken = new CancellationToken();
                await context.Database.ExecuteSqlRawAsync("<Some query>", [1, 2], cancellationToken);
            }
            else
            {
                await context.Database.ExecuteSqlRawAsync("<Some query>", 1, 2);
            }
        }
        else
        {
            context.Database.ExecuteSqlRaw("<Some query>", 1, 2);
        }

        Assert.Equal("<Some query>", commandBuilder.Sql);
        Assert.Equal([1, 2], commandBuilder.Parameters);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Can_pass_ints_as_params(bool async)
    {
        using var context = new ThudContext();
        var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

        if (async)
        {
            await context.Database.ExecuteSqlRawAsync("<Some query>", 1, 2);
        }
        else
        {
            context.Database.ExecuteSqlRaw("<Some query>", 1, 2);
        }

        Assert.Equal("<Some query>", commandBuilder.Sql);
        Assert.Equal([1, 2], commandBuilder.Parameters);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Can_pass_mixed_array_of_params(bool async, bool cancellation)
    {
        using var context = new ThudContext();
        var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

        if (async)
        {
            if (cancellation)
            {
                var cancellationToken = new CancellationToken();
                await context.Database.ExecuteSqlRawAsync("<Some query>", [1, "Cheese"], cancellationToken);
            }
            else
            {
                await context.Database.ExecuteSqlRawAsync("<Some query>", 1, "Cheese");
            }
        }
        else
        {
            context.Database.ExecuteSqlRaw("<Some query>", 1, "Cheese");
        }

        Assert.Equal("<Some query>", commandBuilder.Sql);
        Assert.Equal([1, "Cheese"], commandBuilder.Parameters);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Can_pass_list_of_int_params_as_object(bool async, bool cancellation)
    {
        using var context = new ThudContext();
        var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

        if (async)
        {
            if (cancellation)
            {
                var cancellationToken = new CancellationToken();
                await context.Database.ExecuteSqlRawAsync(
                    "<Some query>", [1, 2], cancellationToken);
            }
            else
            {
                await context.Database.ExecuteSqlRawAsync(
                    "<Some query>", new List<object> { 1, 2 });
            }
        }
        else
        {
            context.Database.ExecuteSqlRaw(
                "<Some query>", new List<object> { 1, 2 });
        }

        Assert.Equal("<Some query>", commandBuilder.Sql);
        Assert.Equal([1, 2], commandBuilder.Parameters);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Can_pass_mixed_list_of_params(bool async, bool cancellation)
    {
        using var context = new ThudContext();
        var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

        if (async)
        {
            if (cancellation)
            {
                var cancellationToken = new CancellationToken();
                await context.Database.ExecuteSqlRawAsync(
                    "<Some query>", [1, "Pickle"], cancellationToken);
            }
            else
            {
                await context.Database.ExecuteSqlRawAsync(
                    "<Some query>", new List<object> { 1, "Pickle" });
            }
        }
        else
        {
            context.Database.ExecuteSqlRaw(
                "<Some query>", new List<object> { 1, "Pickle" });
        }

        Assert.Equal("<Some query>", commandBuilder.Sql);
        Assert.Equal([1, "Pickle"], commandBuilder.Parameters);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Can_pass_single_int_as_object(bool async, bool cancellation)
    {
        using var context = new ThudContext();
        var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

        if (async)
        {
            if (cancellation)
            {
                var cancellationToken = new CancellationToken();
                await context.Database.ExecuteSqlRawAsync("<Some query>", [1], cancellationToken);
            }
            else
            {
                await context.Database.ExecuteSqlRawAsync("<Some query>", 1);
            }
        }
        else
        {
            context.Database.ExecuteSqlRaw("<Some query>", 1);
        }

        Assert.Equal("<Some query>", commandBuilder.Sql);
        Assert.Equal([1], commandBuilder.Parameters);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Can_pass_single_string(bool async, bool cancellation)
    {
        using var context = new ThudContext();
        var commandBuilder = (TestRawSqlCommandBuilder)context.GetService<IRawSqlCommandBuilder>();

        if (async)
        {
            if (cancellation)
            {
                var cancellationToken = new CancellationToken();
                await context.Database.ExecuteSqlRawAsync("<Some query>", new[] { "Branston" }, cancellationToken);
            }
            else
            {
                await context.Database.ExecuteSqlRawAsync("<Some query>", "Branston");
            }
        }
        else
        {
            context.Database.ExecuteSqlRaw("<Some query>", "Branston");
        }

        Assert.Equal("<Some query>", commandBuilder.Sql);
        Assert.Equal(["Branston"], commandBuilder.Parameters);
    }

    private class ThudContext : DbContext
    {
        public ThudContext()
            : base(
                FakeRelationalTestHelpers.Instance.CreateOptions(
                    FakeRelationalTestHelpers.Instance.CreateServiceProvider(
                        new ServiceCollection()
                            .AddScoped<IRawSqlCommandBuilder, TestRawSqlCommandBuilder>())))
        {
        }
    }

    private class TestRawSqlCommandBuilder(
        IRelationalCommandBuilderFactory relationalCommandBuilderFactory) : IRawSqlCommandBuilder
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory = relationalCommandBuilderFactory;

        public string Sql { get; private set; }
        public IEnumerable<object> Parameters { get; private set; }

        public IRelationalCommand Build(string sql)
            => throw new NotImplementedException();

        public RawSqlCommand Build(string sql, IEnumerable<object> parameters)
            => throw new NotImplementedException();

        public RawSqlCommand Build(string sql, IEnumerable<object> parameters, IModel model)
        {
            Sql = sql;
            Parameters = parameters;

            return new RawSqlCommand(_commandBuilderFactory.Create().Build(), new Dictionary<string, object>());
        }
    }
}
