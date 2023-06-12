﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class SqliteTestStore : RelationalTestStore
{
    public const int CommandTimeout = 30;

    public static SqliteTestStore GetOrCreate(string name, bool sharedCache = false)
        => new(name, sharedCache: sharedCache);

    public static SqliteTestStore GetOrCreateInitialized(string name)
        => new SqliteTestStore(name).InitializeSqlite(
            new ServiceCollection().AddEntityFrameworkSqlite().BuildServiceProvider(validateScopes: true),
            (Func<DbContext>)null,
            null);

    public static SqliteTestStore GetExisting(string name)
        => new(name, seed: false);

    public static SqliteTestStore Create(string name)
        => new(name, shared: false);

    private readonly bool _seed;

    private SqliteTestStore(string name, bool seed = true, bool sharedCache = false, bool shared = true)
        : base(name, shared)
    {
        _seed = seed;

        ConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Name + ".db", Cache = sharedCache ? SqliteCacheMode.Shared : SqliteCacheMode.Private
        }.ToString();

        var connection = new SqliteConnection(ConnectionString);
        Connection = connection;
    }

    public virtual DbContextOptionsBuilder AddProviderOptions(
        DbContextOptionsBuilder builder,
        Action<SqliteDbContextOptionsBuilder> configureSqlite)
        => builder.UseSqlite(
            Connection, b =>
            {
                b.CommandTimeout(CommandTimeout);
                b.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                configureSqlite?.Invoke(b);
            });

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
        => AddProviderOptions(builder, configureSqlite: null);

    public SqliteTestStore InitializeSqlite(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
        => (SqliteTestStore)Initialize(serviceProvider, createContext, seed);

    public SqliteTestStore InitializeSqlite(
        IServiceProvider serviceProvider,
        Func<SqliteTestStore, DbContext> createContext,
        Action<DbContext> seed)
        => (SqliteTestStore)Initialize(serviceProvider, () => createContext(this), seed);

    protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed, Action<DbContext> clean)
    {
        if (!_seed)
        {
            return;
        }

        using var context = createContext();
        if (!context.Database.EnsureCreated())
        {
            clean?.Invoke(context);
            Clean(context);
        }

        seed?.Invoke(context);
    }

    public override void Clean(DbContext context)
        => context.Database.EnsureClean();

    public int ExecuteNonQuery(string sql, params object[] parameters)
    {
        using var command = CreateCommand(sql, parameters);
        return command.ExecuteNonQuery();
    }

    public T ExecuteScalar<T>(string sql, params object[] parameters)
    {
        using var command = CreateCommand(sql, parameters);
        return (T)command.ExecuteScalar();
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
}
