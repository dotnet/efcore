// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore
{
    public class SqliteTestStore : RelationalTestStore
    {
        public const int CommandTimeout = 30;

        public static SqliteTestStore GetOrCreate(string name)
            => new SqliteTestStore(name);

        public static SqliteTestStore GetOrCreateInitialized(string name)
            => new SqliteTestStore(name).InitializeSqlite(null, null, null);

        public static SqliteTestStore GetExisting(string name)
            => new SqliteTestStore(name, seed: false);

        private readonly bool _seed;

        private SqliteTestStore(string name, bool seed = true)
            : base(name)
        {
            _seed = seed;

            ConnectionString = new SqliteConnectionStringBuilder
            {
                DataSource = Name + ".db",
                Cache = SqliteCacheMode.Shared
            }.ToString();

            Connection = new SqliteConnection(ConnectionString);
        }

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder.UseSqlite(Connection, b => b.CommandTimeout(CommandTimeout));

        public override TestStore Initialize(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
            => InitializeSqlite(serviceProvider, createContext, seed);

        public SqliteTestStore InitializeSqlite(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
        {
            ServiceProvider = serviceProvider;
            if (createContext == null)
            {
                createContext = CreateDefaultContext;
            }
            if (seed == null)
            {
                seed = c => { };
            }

            GlobalTestStoreIndex.CreateShared(typeof(SqliteTestStore).Name + Name, () =>
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
                });

            // Open the connection after initializing to ensure FK enforcement is on
            OpenConnection();

            return this;
        }

        public override void Clean(DbContext context)
            => context.Database.EnsureClean();

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
