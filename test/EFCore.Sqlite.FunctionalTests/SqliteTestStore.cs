// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public class SqliteTestStore : RelationalTestStore<SqliteConnection>
    {
        public const int CommandTimeout = 30;
        private static int _scratchCount;

        private static string BaseDirectory => AppContext.BaseDirectory;

        public static SqliteTestStore GetNorthwindStore() => GetOrCreateShared("northwind", () => { });

        public static SqliteTestStore GetOrCreateShared(string name, bool sharedCache, Action initializeDatabase = null) =>
            new SqliteTestStore(name).InitializeShared(initializeDatabase, sharedCache);

        public static SqliteTestStore GetShared(string name)
            => new SqliteTestStore(name);

        public static SqliteTestStore GetExisting(string name)
            => new SqliteTestStore(name, seed: false);

        public static SqliteTestStore GetOrCreateShared(string name, Action initializeDatabase = null) =>
            GetOrCreateShared(name, false, initializeDatabase);

        public static SqliteTestStore CreateScratch(bool sharedCache = false)
        {
            string name;
            do
            {
                name = "scratch-" + Interlocked.Increment(ref _scratchCount);
            }
            while (File.Exists(name + ".db")
                   || File.Exists(Path.Combine(BaseDirectory, name + ".db")));

            return new SqliteTestStore(name).InitializeTransient(sharedCache);
        }

        private readonly bool _seed;
        private bool _deleteDatabase;

        private SqliteTestStore(string name, bool seed = true)
            : base(name)
        {
            _seed = seed;
        }

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkSqlite()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder.UseSqlite(Connection, b => b.CommandTimeout(CommandTimeout));

        public override TestStore Initialize(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
            => InitializeShared(() =>
                {
                    if (!_seed)
                    {
                        return;
                    }
                    using (var context = createContext())
                    {
                        if (!context.Database.EnsureCreated())
                        {
                            Clean(context);
                        }
                        seed(context);
                    }
                },
                sharedCache: false);

        public override void Clean(DbContext context)
        {
            context.Database.EnsureClean();
        }

        private SqliteTestStore InitializeShared(Action initializeDatabase, bool sharedCache)
        {
            CreateConnection(sharedCache);

            GlobalTestStoreIndex.CreateShared(typeof(SqliteTestStore).Name + Name, initializeDatabase);

            // Open the connection after initializing to ensure FK enforcement is on
            OpenConnection();

            return this;
        }

        private SqliteTestStore InitializeTransient(bool sharedCache)
        {
            CreateConnection(sharedCache);
            OpenConnection();

            _deleteDatabase = true;
            return this;
        }

        private void CreateConnection(bool sharedCache = false)
        {
            ConnectionString = CreateConnectionString(Name, sharedCache);
            Connection = new SqliteConnection(ConnectionString);
        }

        public override void OpenConnection()
        {
            Connection.Open();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys=ON;";
                command.ExecuteNonQuery();
            }
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return command.ExecuteNonQuery();
            }
        }

        private DbCommand CreateCommand(string commandText, object[] parameters)
        {
            var command = Connection.CreateCommand();

            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            for (var i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue("@p" + i, parameters[i]);
            }

            return command;
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_deleteDatabase)
            {
                File.Delete(Name + ".db");
            }
        }

        public static string CreateConnectionString(string name, bool sharedCache = false) =>
            new SqliteConnectionStringBuilder
            {
                DataSource = name + ".db",
                Cache = sharedCache ? SqliteCacheMode.Shared : SqliteCacheMode.Private
            }.ToString();
    }
}
