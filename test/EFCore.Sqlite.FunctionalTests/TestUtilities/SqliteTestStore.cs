// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class SqliteTestStore : RelationalTestStore
{
    public const int CommandTimeout = 30;

    public static SqliteTestStore GetOrCreate(string name, bool sharedCache = false)
        => new(name, sharedCache: sharedCache);

    public static async Task<SqliteTestStore> GetOrCreateInitializedAsync(string name)
        => await new SqliteTestStore(name).InitializeSqliteAsync(
            new ServiceCollection().AddEntityFrameworkSqlite().BuildServiceProvider(validateScopes: true),
            (Func<DbContext>?)null,
            null);

    public static SqliteTestStore GetExisting(string name)
        => new(name, seed: false);

    public static SqliteTestStore Create(string name)
        => new(name, shared: false);

    private readonly bool _seed;

    private SqliteTestStore(string name, bool seed = true, bool sharedCache = false, bool shared = true)
        : base(name, shared, CreateConnection(name, sharedCache))
        => _seed = seed;

    public virtual DbContextOptionsBuilder AddProviderOptions(
        DbContextOptionsBuilder builder,
        Action<SqliteDbContextOptionsBuilder>? configureSqlite)
        => builder.UseSqlite(
            Connection, b =>
            {
                b.CommandTimeout(CommandTimeout);
                b.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                configureSqlite?.Invoke(b);
            });

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
        => AddProviderOptions(builder, configureSqlite: null);

    public async Task<SqliteTestStore> InitializeSqliteAsync(
        IServiceProvider? serviceProvider,
        Func<DbContext>? createContext,
        Func<DbContext, Task>? seed)
        => (SqliteTestStore)await InitializeAsync(serviceProvider, createContext, seed);

    public async Task<SqliteTestStore> InitializeSqliteAsync(
        IServiceProvider serviceProvider,
        Func<SqliteTestStore, DbContext> createContext,
        Func<DbContext, Task> seed)
        => (SqliteTestStore)await InitializeAsync(serviceProvider, () => createContext(this), seed);

    protected override async Task InitializeAsync(Func<DbContext> createContext, Func<DbContext, Task>? seed, Func<DbContext, Task>? clean)
    {
        if (!_seed)
        {
            return;
        }

        using var context = createContext();
        if (!await context.Database.EnsureCreatedAsync())
        {
            if (clean != null)
            {
                await clean(context);
            }

            await CleanAsync(context);
        }

        if (seed != null)
        {
            await seed(context);
        }
    }

    public override Task CleanAsync(DbContext context)
    {
        context.Database.EnsureClean();
        return Task.CompletedTask;
    }

    public int ExecuteNonQuery(string sql, params object[] parameters)
    {
        using var command = CreateCommand(sql, parameters);
        return command.ExecuteNonQuery();
    }

    public T ExecuteScalar<T>(string sql, params object[] parameters)
    {
        using var command = CreateCommand(sql, parameters);
        return (T)command.ExecuteScalar()!;
    }

    private DbCommand CreateCommand(string commandText, object[] parameters)
    {
        var command = (SqliteCommand)Connection.CreateCommand();

        command.CommandText = commandText;
        command.CommandTimeout = CommandTimeout;

        for (var i = 0; i < parameters.Length; i++)
        {
            command.Parameters.AddWithValue("@p" + i, parameters[i]);
        }

        return command;
    }

    private static SqliteConnection CreateConnection(string name, bool sharedCache)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = name + ".db", Cache = sharedCache ? SqliteCacheMode.Shared : SqliteCacheMode.Private
        }.ToString();

        return new SqliteConnection(connectionString);
    }
}
