// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class SqliteTestStore : RelationalTestStore
    {
        public const int CommandTimeout = 30;

        public static SqliteTestStore GetOrCreate(string name, bool sharedCache = true)
            => new SqliteTestStore(name, sharedCache: sharedCache);

        public static SqliteTestStore GetOrCreateInitialized(string name)
            => new SqliteTestStore(name).InitializeSqlite(null, (Func<DbContext>)null, null);

        public static SqliteTestStore GetExisting(string name)
            => new SqliteTestStore(name, seed: false);

        public static SqliteTestStore Create(string name, bool sharedCache = true)
            => new SqliteTestStore(name, sharedCache: sharedCache, shared: false);

        public static SqliteTestStore CreateInitialized(string name)
            => new SqliteTestStore(name, shared: false).InitializeSqlite(null, (Func<DbContext>)null, null);

        private readonly bool _seed;

        private SqliteTestStore(string name, bool seed = true, bool sharedCache = true, bool shared = true)
            : base(name, shared)
        {
            _seed = seed;

            ConnectionString = new SqliteConnectionStringBuilder
            {
                DataSource = Name + ".db",
                Cache = sharedCache ? SqliteCacheMode.Shared : SqliteCacheMode.Private
            }.ToString();

            Connection = new SqliteConnection(ConnectionString);
        }

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder.UseSqlite(Connection, b => b.CommandTimeout(CommandTimeout));

        public SqliteTestStore InitializeSqlite(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
            => (SqliteTestStore)Initialize(serviceProvider, createContext, seed);

        public SqliteTestStore InitializeSqlite(IServiceProvider serviceProvider, Func<SqliteTestStore, DbContext> createContext, Action<DbContext> seed)
            => (SqliteTestStore)Initialize(serviceProvider, () => createContext(this), seed);

        protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed)
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
        }

        public override void Clean(DbContext context)
            => context.Database.EnsureClean();

        public override void OpenConnection()
        {
            Connection.Open();

#if !Test21
            ((SqliteConnection)Connection).EnableExtensions();
            SpatialiteLoader.TryLoad(Connection);
#endif

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
}
