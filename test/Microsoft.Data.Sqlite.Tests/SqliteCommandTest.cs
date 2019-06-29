// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite.Properties;
using Xunit;

using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    public class SqliteCommandTest
    {
        [Fact]
        public void Ctor_sets_values()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.DefaultTimeout = 1;
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var command = new SqliteCommand("SELECT 1;", connection, transaction);

                    Assert.Equal("SELECT 1;", command.CommandText);
                    Assert.Same(connection, command.Connection);
                    Assert.Equal(1, command.CommandTimeout);
                    Assert.Same(transaction, command.Transaction);
                }
            }
        }

        [Fact]
        public void CommandText_throws_when_set_when_open_reader()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    var ex = Assert.Throws<InvalidOperationException>(() => command.CommandText = "SELECT 2;");

                    Assert.Equal(Resources.SetRequiresNoOpenReader("CommandText"), ex.Message);
                }
            }
        }

        [Fact]
        public void Connection_can_be_unset()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";
                command.Prepare();

                command.Connection = null;
                Assert.Null(command.Connection);
            }
        }

        [Fact]
        public void Connection_throws_when_set_when_open_reader()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    var ex = Assert.Throws<InvalidOperationException>(() => command.Connection = new SqliteConnection());

                    Assert.Equal(Resources.SetRequiresNoOpenReader("Connection"), ex.Message);
                }
            }
        }

        [Fact]
        public void CommandType_text_by_default()
        {
            Assert.Equal(CommandType.Text, new SqliteCommand().CommandType);
        }

        [Theory]
        [InlineData(CommandType.StoredProcedure)]
        [InlineData(CommandType.TableDirect)]
        public void CommandType_validates_value(CommandType commandType)
        {
            var ex = Assert.Throws<ArgumentException>(() => new SqliteCommand().CommandType = commandType);

            Assert.Equal(Resources.InvalidCommandType(commandType), ex.Message);
        }

        [Fact]
        public void Parameters_works()
        {
            var command = new SqliteCommand();

            var result = command.Parameters;

            Assert.NotNull(result);
            Assert.Same(result, command.Parameters);
        }

        [Fact]
        public void CreateParameter_works()
        {
            Assert.NotNull(new SqliteCommand().CreateParameter());
        }

        [Fact]
        public void Prepare_throws_when_no_connection()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new SqliteCommand().Prepare());

            Assert.Equal(Resources.CallRequiresOpenConnection("Prepare"), ex.Message);
        }

        [Fact]
        public void Prepare_throws_when_connection_closed()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var ex = Assert.Throws<InvalidOperationException>(() => connection.CreateCommand().Prepare());

                Assert.Equal(Resources.CallRequiresOpenConnection("Prepare"), ex.Message);
            }
        }

        [Fact]
        public void Prepare_throws_when_no_command_text()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => connection.CreateCommand().Prepare());

                Assert.Equal(Resources.CallRequiresSetCommandText("Prepare"), ex.Message);
            }
        }

        [Fact]
        public void Prepare_throws_when_command_text_contains_dependent_commands()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE Data (Value); SELECT * FROM Data;";
                var ex = Assert.Throws<SqliteException>(() => command.Prepare());

                Assert.Equal(Resources.SqliteNativeError(SQLITE_ERROR, "no such table: Data"), ex.Message);
            }
        }

        [Fact]
        private void Multiple_command_executes_works()
        {
            const int INSERTS = 3;

            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.ExecuteNonQuery("CREATE TABLE Data (ID integer PRIMARY KEY, Value integer);");

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Data (Value) VALUES (@value);";
                    var valueParam = command.Parameters.AddWithValue("@value", -1);

                    Assert.Equal(1, command.ExecuteNonQuery());

                    for (var i = 0; i < INSERTS; i++)
                    {
                        valueParam.Value = i;
                        Assert.Equal(1, command.ExecuteNonQuery());
                    }

                    Assert.Equal(1, command.ExecuteNonQuery());

                    command.CommandText = "SELECT Value FROM Data ORDER BY ID";
                    using (var reader = command.ExecuteReader())
                    {
                        Assert.True(reader.Read());
                        Assert.Equal(-1, reader.GetInt32(0));

                        for (var i = 0; i < INSERTS; i++)
                        {
                            Assert.True(reader.Read());
                            Assert.Equal(i, reader.GetInt32(0));
                        }

                        Assert.True(reader.Read());
                        Assert.Equal(INSERTS - 1, reader.GetInt32(0));
                    }
                }
            }
        }

        [Fact]
        public void ExecuteReader_throws_when_no_connection()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new SqliteCommand().ExecuteReader());

            Assert.Equal(Resources.CallRequiresOpenConnection("ExecuteReader"), ex.Message);
        }

        [Fact]
        public void ExecuteReader_throws_when_connection_closed()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var ex = Assert.Throws<InvalidOperationException>(() => connection.CreateCommand().ExecuteReader());

                Assert.Equal(Resources.CallRequiresOpenConnection("ExecuteReader"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteReader_throws_when_no_command_text()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => connection.CreateCommand().ExecuteReader());

                Assert.Equal(Resources.CallRequiresSetCommandText("ExecuteReader"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteReader_throws_on_error()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "INVALID";
                connection.Open();

                var ex = Assert.Throws<SqliteException>(() => command.ExecuteReader());

                Assert.Equal(SQLITE_ERROR, ex.SqliteErrorCode);
            }
        }

        [Fact]
        public void ExecuteScalar_throws_when_no_connection()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new SqliteCommand().ExecuteScalar());

            Assert.Equal(Resources.CallRequiresOpenConnection("ExecuteScalar"), ex.Message);
        }

        [Fact]
        public void ExecuteScalar_throws_when_connection_closed()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var ex = Assert.Throws<InvalidOperationException>(() => connection.CreateCommand().ExecuteScalar());

                Assert.Equal(Resources.CallRequiresOpenConnection("ExecuteScalar"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteReader_throws_when_transaction_required()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";
                connection.Open();

                using (connection.BeginTransaction())
                {
                    var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteReader());

                    Assert.Equal(Resources.TransactionRequired, ex.Message);
                }
            }
        }

        [Fact]
        public void ExecuteScalar_throws_when_no_command_text()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => connection.CreateCommand().ExecuteScalar());

                Assert.Equal(Resources.CallRequiresSetCommandText("ExecuteScalar"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteScalar_processes_dependent_commands()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE Data (Value); SELECT * FROM Data;";

                Assert.Null(command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_null_when_empty()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1 WHERE 0 = 1;";
                connection.Open();

                Assert.Null(command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_null_when_non_query()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE Data (Value);";
                connection.Open();

                Assert.Null(command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_long_when_integer()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";
                connection.Open();

                Assert.Equal(1L, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_double_when_real()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 3.14;";
                connection.Open();

                Assert.Equal(3.14, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_string_when_text()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 'test';";
                connection.Open();

                Assert.Equal("test", command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_byte_array_when_blob()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT x'7e57';";
                connection.Open();

                Assert.Equal(new byte[] { 0x7e, 0x57 }, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_DBNull_when_null()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT NULL;";
                connection.Open();

                Assert.Equal(DBNull.Value, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteReader_binds_parameters()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT @Parameter;";
                command.Parameters.AddWithValue("@Parameter", 1);
                connection.Open();

                Assert.Equal(1L, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteReader_throws_when_parameter_unset()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT @Parameter, @Parameter2;";
                command.Parameters.AddWithValue("@Parameter", 1);
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteScalar());
                Assert.Equal(Resources.MissingParameters("@Parameter2"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteReader_throws_when_reader_open()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteReader());
                    Assert.Equal(Resources.DataReaderOpen, ex.Message);
                }
            }
        }

        [Fact]
        public void ExecuteScalar_returns_long_when_batching()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 42; SELECT 43;";
                connection.Open();

                Assert.Equal(42L, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_long_when_multiple_columns()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 42, 43;";
                connection.Open();

                Assert.Equal(42L, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_long_when_multiple_rows()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 42 UNION SELECT 43;";
                connection.Open();

                Assert.Equal(42L, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteNonQuery_throws_when_no_connection()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new SqliteCommand().ExecuteNonQuery());

            Assert.Equal(Resources.CallRequiresOpenConnection("ExecuteNonQuery"), ex.Message);
        }

        [Fact]
        public void ExecuteNonQuery_throws_when_connection_closed()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var ex = Assert.Throws<InvalidOperationException>(() => connection.CreateCommand().ExecuteNonQuery());

                Assert.Equal(Resources.CallRequiresOpenConnection("ExecuteNonQuery"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteNonQuery_throws_when_no_command_text()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => connection.CreateCommand().ExecuteNonQuery());

                Assert.Equal(Resources.CallRequiresSetCommandText("ExecuteNonQuery"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteReader_throws_when_transaction_mismatched()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";
                connection.Open();

                using (var otherConnection = new SqliteConnection("Data Source=:memory:"))
                {
                    otherConnection.Open();

                    using (var transaction = otherConnection.BeginTransaction())
                    {
                        command.Transaction = transaction;

                        var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteReader());

                        Assert.Equal(Resources.TransactionConnectionMismatch, ex.Message);
                    }
                }
            }
        }

        [Fact]
        public void ExecuteReader_throws_when_transaction_completed_externally()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    connection.ExecuteNonQuery("ROLLBACK;");

                    var ex = Assert.Throws<InvalidOperationException>(() => connection.ExecuteNonQuery("SELECT 1;"));

                    Assert.Equal(Resources.TransactionCompleted, ex.Message);
                }
            }
        }

        [Fact]
        public void ExecuteNonQuery_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";
                connection.Open();

                Assert.Equal(-1, command.ExecuteNonQuery());
            }
        }

        [Fact]
        public void ExecuteReader_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    Assert.NotNull(reader);
                }
            }
        }

        [Fact]
        public void ExecuteReader_skips_DML_statements()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.ExecuteNonQuery("CREATE TABLE Test(Value);");

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Test VALUES(1);
                    SELECT 1;";

                using (var reader = command.ExecuteReader())
                {
                    var hasData = reader.Read();
                    Assert.True(hasData);

                    Assert.Equal(1L, reader.GetInt64(0));
                }
            }
        }

        [Fact]
        public void ExecuteReader_works_when_comments()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "-- TODO: Write SQL";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    Assert.False(reader.HasRows);
                    Assert.Equal(-1, reader.RecordsAffected);
                }
            }
        }

        [Fact]
        public void ExecuteReader_works_when_trailing_comments()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 0; -- My favorite number";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    var hasResult = reader.NextResult();
                    Assert.False(hasResult);
                }
            }
        }

        [Fact]
        public void Cancel_does_nothing()
        {
            new SqliteCommand().Cancel();
        }

        [Fact]
        public void ExecuteReader_supports_SequentialAccess()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 0;";
                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    var hasResult = reader.NextResult();
                    Assert.False(hasResult);
                }
            }
        }

        [Fact]
        public void ExecuteReader_supports_SingleResult()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 0;";
                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var hasResult = reader.NextResult();
                    Assert.False(hasResult);
                }
            }
        }

        [Fact]
        public void ExecuteReader_supports_SingleRow()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 0;";
                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    var hasResult = reader.NextResult();
                    Assert.False(hasResult);
                }
            }
        }

        [Fact]
        public void ExecuteReader_supports_CloseConnection()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 0;";
                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    var hasResult = reader.NextResult();
                    Assert.False(hasResult);
                }

                Assert.Equal(ConnectionState.Closed, connection.State);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public Task ExecuteReader_retries_when_locked(bool extendedErrorCode)
        {
            const string connectionString = "Data Source=locked;Mode=Memory;Cache=Shared";

            var selectedSignal = new AutoResetEvent(initialState: false);

            return Task.WhenAll(
                Task.Run(
                    async () =>
                    {
                        using (var connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();
                            if (extendedErrorCode)
                            {
                                sqlite3_extended_result_codes(connection.Handle, 1);
                            }

                            connection.ExecuteNonQuery(
                                "CREATE TABLE Data (Value); INSERT INTO Data VALUES (0);");

                            using (connection.ExecuteReader("SELECT * FROM Data;"))
                            {
                                selectedSignal.Set();

                                await Task.Delay(1000);
                            }
                        }
                    }),
                Task.Run(
                    () =>
                    {
                        using (var connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();
                            if (extendedErrorCode)
                            {
                                sqlite3_extended_result_codes(connection.Handle, 1);
                            }

                            selectedSignal.WaitOne();

                            var command = connection.CreateCommand();
                            command.CommandText = "DROP TABLE Data;";

                            command.ExecuteNonQuery();
                        }
                    }));
        }

        [Fact]
        public async Task ExecuteReader_retries_when_busy()
        {
            const string connectionString = "Data Source=busy.db";

            var selectedSignal = new AutoResetEvent(initialState: false);

            try
            {
                await Task.WhenAll(
                    Task.Run(
                        async () =>
                        {
                            using (var connection = new SqliteConnection(connectionString))
                            {
                                connection.Open();

                                connection.ExecuteNonQuery(
                                    "CREATE TABLE Data (Value); INSERT INTO Data VALUES (0);");

                                using (connection.ExecuteReader("SELECT * FROM Data;"))
                                {
                                    selectedSignal.Set();

                                    await Task.Delay(1000);
                                }
                            }
                        }),
                    Task.Run(
                        () =>
                        {
                            using (var connection = new SqliteConnection(connectionString))
                            {
                                connection.Open();

                                selectedSignal.WaitOne();

                                var command = connection.CreateCommand();
                                command.CommandText = "DROP TABLE Data;";

                                command.ExecuteNonQuery();
                            }
                        }));
            }
            finally
            {
                File.Delete("busy.db");
            }
        }

        [Fact]
        public void ExecuteReader_honors_CommandTimeout()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                connection.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (0);");

                using (connection.ExecuteReader("SELECT * FROM Data;"))
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "DROP TABLE Data;";
                    command.CommandTimeout = 1;

                    var stopwatch = Stopwatch.StartNew();
                    Assert.Throws<SqliteException>(() => command.ExecuteNonQuery());
                    stopwatch.Stop();

                    Assert.InRange(stopwatch.ElapsedMilliseconds, 1000, 1999);
                }
            }
        }

        [Fact]
        public void Can_get_results_from_nonreadonly_statements()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var result = connection.ExecuteScalar<string>("PRAGMA journal_mode;");

                Assert.NotNull(result);
            }
        }

        [Fact]
        public void ExecuteReader_works_when_subsequent_DML()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.ExecuteNonQuery(@"
                    CREATE TABLE Test(Value);
                    INSERT INTO Test VALUES(1), (2);");

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Value FROM Test;
                    DELETE FROM Test";

                using (var reader = command.ExecuteReader())
                {
                    var hasData = reader.Read();
                    Assert.True(hasData);

                    Assert.Equal(1L, reader.GetInt64(0));

                    hasData = reader.Read();
                    Assert.True(hasData);

                    Assert.Equal(2L, reader.GetInt64(0));
                }
            }
        }
    }
}
