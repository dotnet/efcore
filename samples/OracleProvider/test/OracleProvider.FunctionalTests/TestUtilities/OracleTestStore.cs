// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Oracle.Storage.Internal;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable SuggestBaseTypeForParameter
namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class OracleTestStore : RelationalTestStore
    {
        // One time Oracle setup:
        //
        //   -- Create a pluggable database that will contain EF test schemas (users).
        //   CREATE PLUGGABLE DATABASE ef
        //   ADMIN USER ef_pdb_admin IDENTIFIED BY ef_pdb_admin
        //   ROLES = (DBA)
        //   FILE_NAME_CONVERT = ('\pdbseed\', '\pdb_ef\');
        //
        //   ALTER PLUGGABLE DATABASE ef OPEN;

        public const int CommandTimeout = 600;
        private static string BaseDirectory => AppContext.BaseDirectory;

        public static OracleTestStore GetNorthwindStore()
            => (OracleTestStore)OracleNorthwindTestStoreFactory.Instance
                .GetOrCreate(OracleNorthwindTestStoreFactory.Name).Initialize(null, (Func<DbContext>)null, null);

        public static OracleTestStore GetOrCreate(string name)
            => new OracleTestStore(name);

        public static OracleTestStore GetOrCreateInitialized(string name, string scriptPath = null)
            => new OracleTestStore(name, scriptPath).InitializeOracle(null, (Func<DbContext>)null, null);

        public static OracleTestStore GetOrCreate(string name, string scriptPath)
            => new OracleTestStore(name, scriptPath: scriptPath);

        public static OracleTestStore Create(string name)
            => new OracleTestStore(name, shared: false);

        public static OracleTestStore CreateInitialized(string name)
            => new OracleTestStore(name, shared: false)
                .InitializeOracle(null, (Func<DbContext>)null, null);

        private readonly string _scriptPath;

        private OracleTestStore(
            string name,
            string scriptPath = null,
            bool shared = true)
            : base(name.Substring(0, Math.Min(name.Length, 30)), shared)
        {
            if (scriptPath != null)
            {
                _scriptPath = Path.Combine(Path.GetDirectoryName(typeof(OracleTestStore).GetTypeInfo().Assembly.Location), scriptPath);
            }

            ConnectionString = CreateConnectionString(Name);
            Connection = new OracleConnection(ConnectionString);
        }

        public OracleTestStore InitializeOracle(
            IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
            => (OracleTestStore)Initialize(serviceProvider, createContext, seed);

        public OracleTestStore InitializeOracle(
            IServiceProvider serviceProvider, Func<OracleTestStore, DbContext> createContext, Action<DbContext> seed)
            => InitializeOracle(serviceProvider, () => createContext(this), seed);

        protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed)
        {
            if (CreateDatabase())
            {
                if (_scriptPath != null)
                {
                    ExecuteScript(_scriptPath);
                }
                else
                {
                    using (var context = createContext())
                    {
                        context.Database.EnsureCreated();
                        seed(context);
                    }
                }
            }
        }

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder.UseOracle(Connection, b => b.ApplyConfiguration().CommandTimeout(CommandTimeout));

        private bool CreateDatabase()
        {
            if (UserExists(Name))
            {
                if (_scriptPath != null)
                {
                    return false;
                }

                Clean(Name);
            }

            using (var master
                = new OracleConnection(CreateConnectionString(OracleRelationalConnection.EFPDBAdminUser)))
            {
                ExecuteNonQuery(
                    master,
                    $@"BEGIN
                             EXECUTE IMMEDIATE 'CREATE USER {Name} IDENTIFIED BY {Name}';
                             EXECUTE IMMEDIATE 'GRANT DBA TO {Name}';
                           END;");
            }

            return true;
        }

        private static bool UserExists(string name)
        {
            using (var connection
                = new OracleConnection(CreateConnectionString(OracleRelationalConnection.EFPDBAdminUser)))
            {
                return ExecuteScalar<int>(
                           connection,
                           $"SELECT COUNT(*) FROM all_users WHERE username = '{name.ToUpperInvariant()}'") > 0;
            }
        }

        private static void Clean(string name)
        {
            DropUser(name);
        }

        private void DropUser()
        {
            OracleConnection.ClearPool((OracleConnection)Connection);

            DropUser(Name);
        }

        private static void DropUser(string name)
        {
            using (var connection
                = new OracleConnection(CreateConnectionString(OracleRelationalConnection.EFPDBAdminUser)))
            {
                retry:
                try
                {
                    OracleConnection.ClearAllPools();

                    ExecuteNonQuery(
                        connection,
                        $@"BEGIN
                         FOR v_cur IN (SELECT sid, serial# FROM v$session WHERE username = '{name.ToUpperInvariant()}') LOOP
                            EXECUTE IMMEDIATE ('ALTER SYSTEM KILL SESSION ''' || v_cur.sid || ',' || v_cur.serial# || ''' IMMEDIATE');
                         END LOOP;
                         EXECUTE IMMEDIATE 'DROP USER {name} CASCADE';
                         EXCEPTION
                           WHEN OTHERS THEN
                             IF SQLCODE != -01918 THEN
                               RAISE;
                             END IF;
                       END;");
                }
                catch (OracleException e)
                {
                    if (e.Number == 1940
                        || e.Number == 31
                        || e.Number == 30
                        || e.Number == 26)
                    {
                        // ORA-01940: cannot drop a user that is currently connected
                        // ORA-00031: session marked for kill
                        // ORA-00030: User session ID does not exist
                        // ORA-00026: missing or invalid session ID

                        goto retry;
                    }

                    throw;
                }
            }
        }

        public override void Clean(DbContext context)
            => throw new NotImplementedException();

        public void ExecuteScript(string scriptPath)
        {
            // HACK: Probe for script file as current dir
            // is different between k build and VS run.
            if (File.Exists(@"..\..\" + scriptPath))
            {
                //executing in VS - so path is relative to bin\<config> dir
                scriptPath = @"..\..\" + scriptPath;
            }
            else
            {
                scriptPath = Path.Combine(BaseDirectory, scriptPath);
            }

            var script = File.ReadAllText(scriptPath);

            Execute(
                Connection, command =>
                    {
                        var statements = Regex.Split(script, @";[\r?\n]\s+", RegexOptions.Multiline);

                        foreach (var statement in statements)
                        {
                            if (string.IsNullOrWhiteSpace(statement)
                                || statement.StartsWith("SET ", StringComparison.Ordinal))
                            {
                                continue;
                            }

                            command.CommandText = statement;
                            command.ExecuteNonQuery();
                        }

                        return 0;
                    }, "");
        }

        public void DeleteDatabase()
        {
            DropUser();
        }

        public override void OpenConnection()
        {
            Connection.Open();
        }

        public override Task OpenConnectionAsync()
            => Connection.OpenAsync();

        public T ExecuteScalar<T>(string sql, params object[] parameters)
            => ExecuteScalar<T>(Connection, sql, parameters);

        private static T ExecuteScalar<T>(DbConnection connection, string sql, params object[] parameters)
            => Execute(connection, command => (T)Convert.ChangeType(command.ExecuteScalar(), typeof(T)), sql, false, parameters);

        public Task<T> ExecuteScalarAsync<T>(string sql, params object[] parameters)
            => ExecuteScalarAsync<T>(Connection, sql, parameters);

        private static Task<T> ExecuteScalarAsync<T>(DbConnection connection, string sql, IReadOnlyList<object> parameters = null)
            => ExecuteAsync(connection, async command => (T)Convert.ChangeType(await command.ExecuteScalarAsync(), typeof(T)), sql, false, parameters);

        public int ExecuteNonQuery(string sql, params object[] parameters)
            => ExecuteNonQuery(Connection, sql, parameters);

        private static int ExecuteNonQuery(DbConnection connection, string sql, object[] parameters = null)
            => Execute(connection, command => command.ExecuteNonQuery(), sql, false, parameters);

        public Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
            => ExecuteNonQueryAsync(Connection, sql, parameters);

        private static Task<int> ExecuteNonQueryAsync(DbConnection connection, string sql, IReadOnlyList<object> parameters = null)
            => ExecuteAsync(connection, command => command.ExecuteNonQueryAsync(), sql, false, parameters);

        public IEnumerable<T> Query<T>(string sql, params object[] parameters)
            => Query<T>(Connection, sql, parameters);

        private static IEnumerable<T> Query<T>(DbConnection connection, string sql, object[] parameters = null)
            => Execute(
                connection, command =>
                    {
                        using (var dataReader = command.ExecuteReader())
                        {
                            var results = Enumerable.Empty<T>();
                            while (dataReader.Read())
                            {
                                results = results.Concat(new[] { dataReader.GetFieldValue<T>(0) });
                            }
                            return results;
                        }
                    }, sql, false, parameters);

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] parameters)
            => QueryAsync<T>(Connection, sql, parameters);

        private static Task<IEnumerable<T>> QueryAsync<T>(DbConnection connection, string sql, object[] parameters = null)
            => ExecuteAsync(
                connection, async command =>
                    {
                        using (var dataReader = await command.ExecuteReaderAsync())
                        {
                            var results = Enumerable.Empty<T>();
                            while (await dataReader.ReadAsync())
                            {
                                results = results.Concat(new[] { await dataReader.GetFieldValueAsync<T>(0) });
                            }
                            return results;
                        }
                    }, sql, false, parameters);

        private static T Execute<T>(
            DbConnection connection, Func<DbCommand, T> execute, string sql,
            bool useTransaction = false, object[] parameters = null)
            => ExecuteCommand(connection, execute, sql, useTransaction, parameters);

        private static T ExecuteCommand<T>(
            DbConnection connection, Func<DbCommand, T> execute, string sql, bool useTransaction, object[] parameters)
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }

            connection.Open();

            try
            {
                using (var transaction = useTransaction ? connection.BeginTransaction() : null)
                {
                    T result;
                    using (var command = CreateCommand(connection, sql, parameters))
                    {
                        command.Transaction = transaction;
                        result = execute(command);
                    }
                    transaction?.Commit();

                    return result;
                }
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        private static Task<T> ExecuteAsync<T>(
            DbConnection connection, Func<DbCommand, Task<T>> executeAsync, string sql,
            bool useTransaction = false, IReadOnlyList<object> parameters = null)
            => ExecuteCommandAsync(connection, executeAsync, sql, useTransaction, parameters);

        private static async Task<T> ExecuteCommandAsync<T>(
            DbConnection connection, Func<DbCommand, Task<T>> executeAsync, string sql, bool useTransaction, IReadOnlyList<object> parameters)
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }

            await connection.OpenAsync();

            try
            {
                using (var transaction = useTransaction ? connection.BeginTransaction() : null)
                {
                    T result;
                    using (var command = CreateCommand(connection, sql, parameters))
                    {
                        result = await executeAsync(command);
                    }
                    transaction?.Commit();

                    return result;
                }
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        private static DbCommand CreateCommand(
            DbConnection connection, string commandText, IReadOnlyList<object> parameters = null)
        {
            var command = (OracleCommand)connection.CreateCommand();

            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            if (parameters != null)
            {
                for (var i = 0; i < parameters.Count; i++)
                {
                    command.Parameters.Add("p" + i, parameters[i]);
                }
            }

            return command;
        }

        public static string CreateConnectionString(string user)
        {
            var oracleConnectionStringBuilder = new OracleConnectionStringBuilder
            {
                DataSource = "//localhost:1521/ef.localdomain",
                UserID = user,
                Password = user
            };

            return oracleConnectionStringBuilder.ToString();
        }
    }
}
