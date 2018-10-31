// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;
using Xunit;

#if !NETCOREAPP2_0
using System.Data.Common;
#endif

namespace Microsoft.Data.Sqlite
{
    public class SqliteConnectionTest
    {
        [Fact]
        public void Ctor_sets_connection_string()
        {
            var connectionString = "Data Source=test.db";

            var connection = new SqliteConnection(connectionString);

            Assert.Equal(connectionString, connection.ConnectionString);
        }

        [Fact]
        public void ConnectionString_setter_throws_when_open()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => connection.ConnectionString = "Data Source=test.db");

                Assert.Equal(Resources.ConnectionStringRequiresClosedConnection, ex.Message);
            }
        }

        [Fact]
        public void ConnectionString_gets_and_sets_value()
        {
            var connection = new SqliteConnection();
            var connectionString = "Data Source=test.db";

            connection.ConnectionString = connectionString;

            Assert.Equal(connectionString, connection.ConnectionString);
        }

        [Fact]
        public void Database_returns_value()
        {
            var connection = new SqliteConnection();

            Assert.Equal("main", connection.Database);
        }

        [Fact]
        public void DataSource_returns_connection_string_data_source_when_closed()
        {
            var connection = new SqliteConnection("Data Source=test.db");

            Assert.Equal("test.db", connection.DataSource);
        }

        [Fact]
        public void DataSource_returns_actual_filename_when_open()
        {
            using (var connection = new SqliteConnection("Data Source=test.db"))
            {
                connection.Open();

                var result = connection.DataSource;

                Assert.True(Path.IsPathRooted(result));
                Assert.Equal("test.db", Path.GetFileName(result));
            }
        }

        [Fact]
        public void DefaultTimeout_defaults_to_30()
        {
            var connection = new SqliteConnection();

            Assert.Equal(30, connection.DefaultTimeout);
        }

        [Fact]
        public void DefaultTimeout_works()
        {
            var connection = new SqliteConnection();
            connection.DefaultTimeout = 1;

            Assert.Equal(1, connection.DefaultTimeout);
        }

        [Fact]
        public void ServerVersion_returns_value()
        {
            var connection = new SqliteConnection();

            var version = connection.ServerVersion;

            Assert.StartsWith("3.", version);
        }

        [Fact]
        public void State_closed_by_default()
        {
            var connection = new SqliteConnection();

            Assert.Equal(ConnectionState.Closed, connection.State);
        }

        [Fact]
        public void Open_throws_when_no_connection_string()
        {
            var connection = new SqliteConnection();

            var ex = Assert.Throws<InvalidOperationException>(() => connection.Open());

            Assert.Equal(Resources.OpenRequiresSetConnectionString, ex.Message);
        }

        [Fact]
        public void Open_adjusts_relative_path()
        {
            using (var connection = new SqliteConnection("Data Source=local.db"))
            {
                connection.Open();

                Assert.Equal(Path.Combine(AppContext.BaseDirectory, "local.db"), connection.DataSource);
            }
        }

        [Fact]
        public void Open_throws_when_error()
        {
            using (var connection = new SqliteConnection("Data Source=file:data.db?mode=invalidmode"))
            {
                var ex = Assert.Throws<SqliteException>(() => connection.Open());

                Assert.Equal(raw.SQLITE_ERROR, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void Open_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var raised = false;
                StateChangeEventHandler handler = (sender, e) =>
                    {
                        raised = true;

                        Assert.Equal(connection, sender);
                        Assert.Equal(ConnectionState.Closed, e.OriginalState);
                        Assert.Equal(ConnectionState.Open, e.CurrentState);
                    };

                connection.StateChange += handler;
                try
                {
                    connection.Open();

                    Assert.True(raised);
                    Assert.Equal(ConnectionState.Open, connection.State);
                }
                finally
                {
                    connection.StateChange -= handler;
                }
            }
        }

        [Fact]
        public void Open_works_when_readonly()
        {
            using (var connection = new SqliteConnection("Data Source=readonly.db"))
            {
                connection.Open();

                connection.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS Idomic (Word TEXT);");
            }

            using (var connection = new SqliteConnection("Data Source=readonly.db;Mode=ReadOnly"))
            {
                connection.Open();

                var ex = Assert.Throws<SqliteException>(
                    () => connection.ExecuteNonQuery("INSERT INTO Idomic VALUES ('arimfexendrapuse');"));

                Assert.Equal(raw.SQLITE_READONLY, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void Open_works_when_readwrite()
        {
            using (var connection = new SqliteConnection("Data Source=readwrite.db;Mode=ReadWrite"))
            {
                var ex = Assert.Throws<SqliteException>(() => connection.Open());

                Assert.Equal(raw.SQLITE_CANTOPEN, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void Open_works_when_memory_shared()
        {
            var connectionString = "Data Source=people;Mode=Memory;Cache=Shared";

            using (var connection1 = new SqliteConnection(connectionString))
            {
                connection1.Open();

                connection1.ExecuteNonQuery(
                    "CREATE TABLE Person (Name TEXT);" +
                    "INSERT INTO Person VALUES ('Waldo');");

                using (var connection2 = new SqliteConnection(connectionString))
                {
                    connection2.Open();

                    var name = connection2.ExecuteScalar<string>("SELECT Name FROM Person;");
                    Assert.Equal("Waldo", name);
                }
            }
        }

        [Fact]
        public void BackupDatabase_works()
        {
            using (var connection1 = new SqliteConnection("Data Source=:memory:"))
            {
                connection1.Open();

                connection1.ExecuteNonQuery(
                    "CREATE TABLE Person (Name TEXT);" +
                    "INSERT INTO Person VALUES ('Waldo');");

                using (var connection2 = new SqliteConnection("Data Source=:memory:"))
                {
                    connection2.Open();
                    connection1.BackupDatabase(connection2);

                    var name = connection2.ExecuteScalar<string>("SELECT Name FROM Person;");
                    Assert.Equal("Waldo", name);
                }
            }
        }

        [Fact]
        public void BackupDatabase_works_when_destination_closed()
        {
            using (var source = new SqliteConnection("Data Source=:memory:"))
            using (var destination = new SqliteConnection("Data Source=:memory:"))
            {
                source.Open();
                source.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (0);");

                source.BackupDatabase(destination);
            }
        }

        [Fact]
        public void BackupDatabase_throws_when_closed()
        {
            var source = new SqliteConnection();
            var destination = new SqliteConnection();

            var ex = Assert.Throws<InvalidOperationException>(() => source.BackupDatabase(destination));

            Assert.Equal(Resources.CallRequiresOpenConnection("BackupDatabase"), ex.Message);
        }

        [Fact]
        public void BackupDatabase_throws_when_destination_null()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var ex = Assert.Throws<ArgumentNullException>(() => connection.BackupDatabase(null));

                Assert.Equal("destination", ex.ParamName);
            }
        }

        [Fact]
        public void BackupDatabase_throws_with_correct_message()
        {
            using (var source = new SqliteConnection("Data Source=:memory:"))
            using (var destination = new SqliteConnection("Data Source=:memory:"))
            {
                source.Open();
                source.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (0);");

                using (source.BeginTransaction())
                {
                    source.ExecuteNonQuery("UPDATE Data SET Value = 1;");

                    var ex = Assert.Throws<SqliteException>(() => source.BackupDatabase(destination));
                    Assert.Equal(raw.SQLITE_BUSY, ex.SqliteErrorCode);
                    Assert.Contains(raw.sqlite3_errstr(raw.SQLITE_BUSY), ex.Message);
                }
            }
        }

        [Fact]
        public void Open_works_when_uri()
        {
            using (var connection = new SqliteConnection("Data Source=file:readwrite.db?mode=rw"))
            {
                var ex = Assert.Throws<SqliteException>(() => connection.Open());

                Assert.Equal(raw.SQLITE_CANTOPEN, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void Close_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var raised = false;
                StateChangeEventHandler handler = (sender, e) =>
                    {
                        raised = true;

                        Assert.Equal(connection, sender);
                        Assert.Equal(ConnectionState.Open, e.OriginalState);
                        Assert.Equal(ConnectionState.Closed, e.CurrentState);
                    };

                connection.StateChange += handler;
                try
                {
                    connection.Close();

                    Assert.True(raised);
                    Assert.Equal(ConnectionState.Closed, connection.State);
                }
                finally
                {
                    connection.StateChange -= handler;
                }
            }
        }

        [Fact]
        public void Close_can_be_called_before_open()
        {
            var connection = new SqliteConnection();

            connection.Close();
        }

        [Fact]
        public void Close_can_be_called_more_than_once()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                connection.Close();
                connection.Close();
            }
        }

        [Fact]
        public void Dispose_closes_connection()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            connection.Dispose();

            Assert.Equal(ConnectionState.Closed, connection.State);
        }

        [Fact]
        public void Dispose_can_be_called_more_than_once()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            connection.Dispose();
            connection.Dispose();
        }

        [Fact]
        public void CreateCommand_returns_command()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.DefaultTimeout = 1;
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();

                    Assert.NotNull(command);
                    Assert.Same(connection, command.Connection);
                    Assert.Equal(1, command.CommandTimeout);
                    Assert.Same(transaction, command.Transaction);
                }
            }
        }

        [Fact]
        public void CreateCollation_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateCollation("MY_NOCASE", (s1, s2) => string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase));

                Assert.Equal(1L, connection.ExecuteScalar<long>("SELECT 'Νικοσ' = 'ΝΙΚΟΣ' COLLATE MY_NOCASE;"));
            }
        }

        [Fact]
        public void CreateCollation_with_null_comparer_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateCollation("MY_NOCASE", (s1, s2) => string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase));
                connection.CreateCollation("MY_NOCASE", null);

                var ex = Assert.Throws<SqliteException>(
                    () => connection.ExecuteScalar<long>("SELECT 'Νικοσ' = 'ΝΙΚΟΣ' COLLATE MY_NOCASE;"));

                Assert.Equal(raw.SQLITE_ERROR, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void CreateCollation_throws_when_closed()
        {
            var connection = new SqliteConnection();

            var ex = Assert.Throws<InvalidOperationException>(() => connection.CreateCollation("NOCOL", (s1, s2) => -1));

            Assert.Equal(Resources.CallRequiresOpenConnection("CreateCollation"), ex.Message);
        }

        [Fact]
        public void CreateCollation_throws_with_empty_name()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                var ex = Assert.Throws<ArgumentNullException>(() => connection.CreateCollation(null, null));

                Assert.Equal("name", ex.ParamName);
            }
        }

        [Fact]
        public void CreateCollation_works_with_state()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                var list = new List<string>();
                connection.CreateCollation(
                    "MY_NOCASE",
                    list,
                    (l, s1, s2) =>
                        {
                            l.Add("Invoked");
                            return string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase);
                        });

                Assert.Equal(1L, connection.ExecuteScalar<long>("SELECT 'Νικοσ' = 'ΝΙΚΟΣ' COLLATE MY_NOCASE;"));
                var item = Assert.Single(list);
                Assert.Equal("Invoked", item);
            }
        }

        [Fact]
        public void CreateFunction_throws_when_closed()
        {
            var connection = new SqliteConnection();

            var ex = Assert.Throws<InvalidOperationException>(() => connection.CreateFunction("test", () => 1L));

            Assert.Equal(Resources.CallRequiresOpenConnection("CreateFunction"), ex.Message);
        }

        [Fact]
        public void CreateFunction_throws_when_no_name()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                var ex = Assert.Throws<ArgumentNullException>(() => connection.CreateFunction(null, () => 1L));

                Assert.Equal("name", ex.ParamName);
            }
        }

        [Fact]
        public void CreateFunction_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateFunction("test", 1L, (long state, long x, int y) => $"{state} {x} {y}");

                var result = connection.ExecuteScalar<string>("SELECT test(2, 3);");

                Assert.Equal("1 2 3", result);
            }
        }

        [Fact]
        public void CreateFunction_works_when_params()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateFunction(
                    "test",
                    args => string.Join(", ", args.Select(a => a?.GetType().FullName ?? "(null)")));

                var result = connection.ExecuteScalar<string>("SELECT test(1, 3.1, 'A', X'7E57', NULL);");

                Assert.Equal("System.Int64, System.Double, System.String, System.Byte[], (null)", result);
            }
        }

        [Fact]
        public void CreateFunction_works_when_exception()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateFunction<long>("test", () => throw new Exception("Test"));

                var ex = Assert.Throws<SqliteException>(() => connection.ExecuteScalar<long>("SELECT test();"));

                Assert.Equal(Resources.SqliteNativeError(raw.SQLITE_ERROR, "Test"), ex.Message);
                Assert.Equal(raw.SQLITE_ERROR, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void CreateFunction_works_when_sqlite_exception()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateFunction<long>("test", () => throw new SqliteException("Test", 200));

                var ex = Assert.Throws<SqliteException>(() => connection.ExecuteScalar<long>("SELECT test();"));

                Assert.Equal(Resources.SqliteNativeError(200, "Test"), ex.Message);
                Assert.Equal(200, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void CreateFunction_works_when_null()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateFunction("test", () => 1L);
                connection.CreateFunction("test", default(Func<long>));

                var ex = Assert.Throws<SqliteException>(() => connection.ExecuteScalar<long>("SELECT test();"));

                Assert.Equal(Resources.SqliteNativeError(raw.SQLITE_ERROR, "no such function: test"), ex.Message);
                Assert.Equal(raw.SQLITE_ERROR, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void CreateFunction_works_when_result_null()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateFunction<object>("test", () => null);

                var result = connection.ExecuteScalar<object>("SELECT test();");

                Assert.Equal(DBNull.Value, result);
            }
        }

        [Fact]
        public void CreateFunction_works_when_result_DBNull()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateFunction<object>("test", () => DBNull.Value);

                var result = connection.ExecuteScalar<object>("SELECT test();");

                Assert.Equal(DBNull.Value, result);
            }
        }

        [Fact]
        public void CreateFunction_works_when_parameter_null_and_type_nullable()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateFunction("test", (long? x) => x == null);

                var result = connection.ExecuteScalar<long>("SELECT test(NULL);");

                Assert.Equal(1L, result);
            }
        }

        [Fact]
        public void CreateFunction_works_when_parameter_null_but_type_long()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateFunction("test", (long x) => x);

                var ex = Assert.Throws<SqliteException>(() => connection.ExecuteScalar<long>("SELECT test(NULL);"));

                Assert.Equal(
                    Resources.SqliteNativeError(raw.SQLITE_ERROR, Resources.UDFCalledWithNull("test", 0)),
                    ex.Message);
                Assert.Equal(raw.SQLITE_ERROR, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void CreateFunction_works_when_parameter_null_but_type_double()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateFunction("test", (double x) => x);

                var ex = Assert.Throws<SqliteException>(() => connection.ExecuteScalar<double>("SELECT test(NULL);"));

                Assert.Equal(
                    Resources.SqliteNativeError(raw.SQLITE_ERROR, Resources.UDFCalledWithNull("test", 0)),
                    ex.Message);
                Assert.Equal(raw.SQLITE_ERROR, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void CreateFunction_works_when_parameter_null_and_type_string()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateFunction("test", (string x) => x == null);

                var result = connection.ExecuteScalar<long>("SELECT test(NULL);");

                Assert.Equal(1L, result);
            }
        }

        [Fact]
        public void CreateFunction_works_when_parameter_null_and_type_byteArray()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateFunction("test", (byte[] x) => x == null);

                var result = connection.ExecuteScalar<long>("SELECT test(NULL);");

                Assert.Equal(1L, result);
            }
        }

        [Fact]
        public void CreateFunction_works_when_parameter_empty_blob()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.CreateFunction("test", (byte[] x) => x?.Length == 0);

                var result = connection.ExecuteScalar<long>("SELECT test(X'');");

                Assert.Equal(1L, result);
            }
        }

        [Fact]
        public void CreateFunction_is_non_deterministic()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                connection.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (0);");
                connection.CreateFunction("test", (double x) => x);

                var ex = Assert.Throws<SqliteException>(
                    () => connection.ExecuteNonQuery("CREATE INDEX InvalidIndex ON Data (Value) WHERE test(Value) = 0;"));

                Assert.Equal(
                    Resources.SqliteNativeError(raw.SQLITE_ERROR, "non-deterministic functions prohibited in partial index WHERE clauses"),
                    ex.Message);
                Assert.Equal(raw.SQLITE_ERROR, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void CreateFunction_deterministic_param_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                connection.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (0);");
                connection.CreateFunction("test", (double x) => x, true);

                Assert.Equal(1, connection.ExecuteNonQuery("CREATE INDEX InvalidIndex ON Data (Value) WHERE test(Value) = 0;"));
            }
        }

        [Fact]
        public void CreateAggregate_throws_when_closed()
        {
            var connection = new SqliteConnection();

            var ex = Assert.Throws<InvalidOperationException>(() => connection.CreateAggregate("test", (string a) => "A"));

            Assert.Equal(Resources.CallRequiresOpenConnection("CreateAggregate"), ex.Message);
        }

        [Fact]
        public void CreateAggregate_throws_when_no_name()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                var ex = Assert.Throws<ArgumentNullException>(() => connection.CreateAggregate(null, (string a) => "A"));

                Assert.Equal("name", ex.ParamName);
            }
        }

        [Fact]
        public void CreateAggregate_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.ExecuteNonQuery("CREATE TABLE dual2 (dummy1, dummy2); INSERT INTO dual2 (dummy1, dummy2) VALUES ('X', 1);");
                connection.CreateAggregate(
                    "test",
                    "A",
                    (string a, string x, int y) => a + x + y,
                    a => a + "Z");

                var result = connection.ExecuteScalar<string>("SELECT test(dummy1, dummy2) FROM dual2;");

                Assert.Equal("AX1Z", result);
            }
        }

        [Fact]
        public void CreateAggregate_works_when_params()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.ExecuteNonQuery("CREATE TABLE dual (dummy); INSERT INTO dual (dummy) VALUES ('X');");
                connection.CreateAggregate("test", (string a, object[] args) => a + string.Join(", ", args) + "; ");

                var result = connection.ExecuteScalar<string>("SELECT test(dummy) FROM dual;");

                Assert.Equal("X; ", result);
            }
        }

        [Fact]
        public void CreateAggregate_works_when_exception_during_step()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.ExecuteNonQuery("CREATE TABLE dual (dummy); INSERT INTO dual (dummy) VALUES ('X');");
                connection.CreateAggregate("test", (string a) => throw new Exception("Test"));

                var ex = Assert.Throws<SqliteException>(
                    () => connection.ExecuteScalar<string>("SELECT test() FROM dual;"));

                Assert.Equal(Resources.SqliteNativeError(raw.SQLITE_ERROR, "Test"), ex.Message);
                Assert.Equal(raw.SQLITE_ERROR, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void CreateAggregate_works_when_exception_during_final()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.ExecuteNonQuery("CREATE TABLE dual (dummy); INSERT INTO dual (dummy) VALUES ('X');");
                connection.CreateAggregate<string, string>("test", "A", a => "B", a => throw new Exception("Test"));

                var ex = Assert.Throws<SqliteException>(
                    () => connection.ExecuteScalar<string>("SELECT test() FROM dual;"));

                Assert.Equal(Resources.SqliteNativeError(raw.SQLITE_ERROR, "Test"), ex.Message);
                Assert.Equal(raw.SQLITE_ERROR, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void CreateAggregate_works_when_sqlite_exception()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.ExecuteNonQuery("CREATE TABLE dual (dummy); INSERT INTO dual (dummy) VALUES ('X');");
                connection.CreateAggregate("test", (string a) => throw new SqliteException("Test", 200));

                var ex = Assert.Throws<SqliteException>(
                    () => connection.ExecuteScalar<string>("SELECT test() FROM dual;"));

                Assert.Equal(Resources.SqliteNativeError(200, "Test"), ex.Message);
                Assert.Equal(200, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void CreateAggregate_works_when_null()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.ExecuteNonQuery("CREATE TABLE dual (dummy); INSERT INTO dual (dummy) VALUES ('X');");
                connection.CreateAggregate("test", (string a) => "A");
                connection.CreateAggregate("test", default(Func<string, string>));

                var ex = Assert.Throws<SqliteException>(
                    () => connection.ExecuteScalar<long>("SELECT test() FROM dual;"));

                Assert.Equal(Resources.SqliteNativeError(raw.SQLITE_ERROR, "no such function: test"), ex.Message);
                Assert.Equal(raw.SQLITE_ERROR, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void BeginTransaction_throws_when_closed()
        {
            var connection = new SqliteConnection();

            var ex = Assert.Throws<InvalidOperationException>(() => connection.BeginTransaction());

            Assert.Equal(Resources.CallRequiresOpenConnection("BeginTransaction"), ex.Message);
        }

        [Fact]
        public void BeginTransaction_throws_when_parallel_transaction()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (connection.BeginTransaction())
                {
                    var ex = Assert.Throws<InvalidOperationException>(() => connection.BeginTransaction());

                    Assert.Equal(Resources.ParallelTransactionsNotSupported, ex.Message);
                }
            }
        }

        [Fact]
        public void BeginTransaction_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    Assert.NotNull(transaction);
                    Assert.Equal(connection, transaction.Connection);
                    Assert.Equal(IsolationLevel.Serializable, transaction.IsolationLevel);
                }
            }
        }

        [Fact]
        public void ChangeDatabase_not_supported()
        {
            using (var connection = new SqliteConnection())
            {
                Assert.Throws<NotSupportedException>(() => connection.ChangeDatabase("new"));
            }
        }

        [Fact]
        public void Mars_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var command1 = connection.CreateCommand();
                command1.CommandText = "SELECT '1A' UNION SELECT '1B';";

                using (var reader1 = command1.ExecuteReader())
                {
                    reader1.Read();
                    Assert.Equal("1A", reader1.GetString(0));

                    var command2 = connection.CreateCommand();
                    command2.CommandText = "SELECT '2A' UNION SELECT '2B';";

                    using (var reader2 = command2.ExecuteReader())
                    {
                        reader2.Read();
                        Assert.Equal("2A", reader2.GetString(0));

                        reader1.Read();
                        Assert.Equal("1B", reader1.GetString(0));

                        reader2.Read();
                        Assert.Equal("2B", reader2.GetString(0));
                    }
                }
            }
        }

        [Fact]
        public void EnableExtensions_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var sql = "SELECT load_extension('unknown');";

                var ex = Assert.Throws<SqliteException>(() => connection.ExecuteNonQuery(sql));
                var originalError = ex.Message;

                connection.EnableExtensions();

                ex = Assert.Throws<SqliteException>(() => connection.ExecuteNonQuery(sql));
                var enabledError = ex.Message;

                connection.EnableExtensions(enable: false);

                ex = Assert.Throws<SqliteException>(() => connection.ExecuteNonQuery(sql));
                var disabledError = ex.Message;

                Assert.NotEqual(originalError, enabledError);
                Assert.Equal(originalError, disabledError);
            }
        }

        [Fact]
        public void EnableExtensions_throws_when_closed()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var ex = Assert.Throws<InvalidOperationException>(() => connection.EnableExtensions());
                Assert.Equal(Resources.CallRequiresOpenConnection("EnableExtensions"), ex.Message);
            }
        }

#if !NETCOREAPP2_0
        [Fact]
        public void DbProviderFactory_works()
        {
            var connection = new SqliteConnection();

            var result = DbProviderFactories.GetFactory(connection);

            Assert.Same(SqliteFactory.Instance, result);
        }
#endif
    }
}
