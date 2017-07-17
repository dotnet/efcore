// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public class SqliteTestStore : RelationalTestStore<SqliteConnection>
    {
        private static int _scratchCount;

        public static SqliteTestStore GetNorthwindStore() => GetOrCreateShared("northwind", () => { });

        public static SqliteTestStore GetOrCreateShared(string name, bool sharedCache, Action initializeDatabase = null) =>
            new SqliteTestStore(name).CreateShared(initializeDatabase, sharedCache);

        public static SqliteTestStore GetOrCreateShared(
            string name,
            IServiceProvider serviceProvider,
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> addOptions,
            Func<DbContextOptions, DbContext> createContext,
            Action<DbContext> seed)
            => new SqliteTestStore(name, serviceProvider, addOptions, createContext).CreateShared(seed);

        public static SqliteTestStore GetOrCreateShared(string name, Action initializeDatabase = null) =>
            GetOrCreateShared(name, false, initializeDatabase);

        private static string BaseDirectory => AppContext.BaseDirectory;

        public static SqliteTestStore CreateScratch(bool sharedCache = false)
        {
            string name;
            do
            {
                name = "scratch-" + Interlocked.Increment(ref _scratchCount);
            }
            while (File.Exists(name + ".db")
                   || File.Exists(Path.Combine(BaseDirectory, name + ".db")));

            return new SqliteTestStore(name).CreateTransient(sharedCache);
        }

        public const int CommandTimeout = 30;
        private bool _deleteDatabase;

        private SqliteTestStore(string name,
            IServiceProvider serviceProvider = null,
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> addOptions = null,
            Func<DbContextOptions, DbContext> createContext = null)
            : base(name,
                serviceProvider ??
                SqliteTestStoreFactory.Instance.AddProviderServices(new ServiceCollection()).BuildServiceProvider(),
                addOptions,
                createContext)
        {
        }

        protected override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder
                .UseSqlite(Connection, b => b.CommandTimeout(CommandTimeout))
                .UseInternalServiceProvider(ServiceProvider);

        private SqliteTestStore CreateShared(Action<DbContext> seed)
            => CreateShared(() =>
                {
                    using (var context = CreateContext())
                    {
                        if (!context.Database.EnsureCreated())
                        {
                            context.Database.EnsureClean();
                        }
                        seed(context);
                    }
                    ((TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>()).Clear();
                },
                sharedCache: false);

        private SqliteTestStore CreateShared(Action initializeDatabase, bool sharedCache)
        {
            CreateConnection(sharedCache);

            CreateShared(typeof(SqliteTestStore).Name + Name, initializeDatabase);

            return this;
        }

        private SqliteTestStore CreateTransient(bool sharedCache)
        {
            CreateConnection(sharedCache);

            _deleteDatabase = true;
            return this;
        }

        private void CreateConnection(bool sharedCache = false)
        {
            ConnectionString = CreateConnectionString(Name, sharedCache);
            Connection = new SqliteConnection(ConnectionString);

            OpenConnection();
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
