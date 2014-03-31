// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using Xunit;

namespace Microsoft.Data.SQLite
{
    public class SQLiteCommandTest
    {
        [Fact]
        public void Ctor_validates_arguments()
        {
            var ex = Assert.Throws<ArgumentException>(() => new SQLiteCommand(null));
            Assert.Equal(Strings.FormatArgumentIsNullOrWhitespace("commandText"), ex.Message);

            Assert.Throws<ArgumentNullException>("connection", () => new SQLiteCommand("SELECT 1", null));

            using (var connection = new SQLiteConnection())
                Assert.Throws<ArgumentNullException>(
                    "transaction",
                    () => new SQLiteCommand("SELECT 1", connection, null));
        }

        [Fact]
        public void CommandType_text_by_default()
        {
            using (var command = new SQLiteCommand())
            {
                Assert.Equal(CommandType.Text, command.CommandType);
            }
        }

        [Fact]
        public void CommandType_validates_value()
        {
            using (var command = new SQLiteCommand())
            {
                var ex = Assert.Throws<ArgumentException>(() => command.CommandType = 0);

                Assert.Equal(Strings.FormatInvalidCommandType(0), ex.Message);
            }
        }

        [Fact]
        public void CommandText_validates_value()
        {
            using (var command = new SQLiteCommand())
            {
                var ex = Assert.Throws<ArgumentException>(() => command.CommandText = null);

                Assert.Equal(Strings.FormatArgumentIsNullOrWhitespace("value"), ex.Message);
            }
        }

        [Fact]
        public void Connection_validates_value()
        {
            using (var command = new SQLiteCommand())
                Assert.Throws<ArgumentNullException>("value", () => command.Connection = null);
        }

        [Fact]
        public void Parameters_works()
        {
            using (var command = new SQLiteCommand())
            {
                var result = command.Parameters;

                Assert.NotNull(result);
                Assert.Same(result, command.Parameters);
            }
        }

        [Fact]
        public void CreateParameter_works()
        {
            using (var command = new SQLiteCommand())
            {
                Assert.NotNull(command.CreateParameter());
            }
        }

        [Fact]
        public void Prepare_can_be_called_when_disposed()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                command.Dispose();
                command.Connection = connection;
                connection.Open();

                command.Prepare();
            }
        }

        [Fact]
        public void Prepare_throws_when_open_reader()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                using (command.ExecuteReader())
                {
                    command.CommandText = "SELECT 2";

                    var ex = Assert.Throws<InvalidOperationException>(() => command.Prepare());
                    Assert.Equal(Strings.OpenReaderExists, ex.Message);
                }
            }
        }

        [Fact]
        public void Prepare_throws_when_no_connection()
        {
            using (var command = new SQLiteCommand())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => command.Prepare());

                Assert.Equal(Strings.FormatCallRequiresOpenConnection("Prepare"), ex.Message);
            }
        }

        [Fact]
        public void Prepare_throws_when_connection_closed()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => command.Prepare());

                Assert.Equal(Strings.FormatCallRequiresOpenConnection("Prepare"), ex.Message);
            }
        }

        [Fact]
        public void Prepare_throws_when_no_command_text()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => command.Prepare());

                Assert.Equal(Strings.FormatCallRequiresSetCommandText("Prepare"), ex.Message);
            }
        }

        [Fact]
        public void Prepare_throws_on_error()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INVALID";
                connection.Open();

                var ex = Assert.Throws<SQLiteException>(() => command.Prepare());

                Assert.Equal(1, ex.ErrorCode);
            }
        }

        [Fact]
        public void ExecuteScalar_throws_when_open_reader()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                using (command.ExecuteReader())
                {
                    var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteScalar());
                    Assert.Equal(Strings.OpenReaderExists, ex.Message);
                }
            }
        }

        [Fact]
        public void ExecuteScalar_throws_when_no_connection()
        {
            using (var command = new SQLiteCommand())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteScalar());

                Assert.Equal(Strings.FormatCallRequiresOpenConnection("ExecuteScalar"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteScalar_throws_when_connection_closed()
        {
            using (var connection = new SQLiteConnection())
            using (var command = connection.CreateCommand())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteScalar());

                Assert.Equal(Strings.FormatCallRequiresOpenConnection("ExecuteScalar"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteScalar_throws_when_no_command_text()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteScalar());

                Assert.Equal(Strings.FormatCallRequiresSetCommandText("ExecuteScalar"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteScalar_returns_null_when_empty()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1 WHERE 0 = 1";
                connection.Open();

                Assert.Null(command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_long_when_integer()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                Assert.Equal(1L, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_double_when_float()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 3.14";
                connection.Open();

                Assert.Equal(3.14, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_string_when_text()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 'test'";
                connection.Open();

                Assert.Equal("test", command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_byte_array_when_blob()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT x'7e57'";
                connection.Open();

                Assert.Equal(new byte[] { 0x7e, 0x57 }, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_returns_DBNull_when_null()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT NULL";
                connection.Open();

                Assert.Equal(DBNull.Value, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_binds_parameters()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Parameter";
                command.Parameters.AddWithValue("@Parameter", 1);
                connection.Open();

                var result = command.ExecuteScalar();

                Assert.True(command.Parameters.Bound);
                Assert.Equal(1L, result);
            }
        }

        [Fact]
        public void ExecuteScalar_unbinds_parameters()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Parameter";
                var parameter = command.Parameters.AddWithValue("@Parameter", 1);
                connection.Open();
                command.ExecuteNonQuery();
                command.Parameters.Remove(parameter);

                Assert.Equal(DBNull.Value, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_can_be_called_more_than_once()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                Assert.Equal(1L, command.ExecuteScalar());
                Assert.Equal(1L, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteScalar_can_be_called_when_parameter_unset()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Parameter";
                connection.Open();

                Assert.Equal(DBNull.Value, command.ExecuteScalar());
            }
        }

        [Fact]
        public void ExecuteNonQuery_throws_when_open_reader()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                using (command.ExecuteReader())
                {
                    var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteNonQuery());
                    Assert.Equal(Strings.OpenReaderExists, ex.Message);
                }
            }
        }

        [Fact]
        public void ExecuteNonQuery_throws_when_no_connection()
        {
            using (var command = new SQLiteCommand())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteNonQuery());

                Assert.Equal(Strings.FormatCallRequiresOpenConnection("ExecuteNonQuery"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteNonQuery_throws_when_connection_closed()
        {
            using (var connection = new SQLiteConnection())
            using (var command = connection.CreateCommand())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteNonQuery());

                Assert.Equal(Strings.FormatCallRequiresOpenConnection("ExecuteNonQuery"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteNonQuery_throws_when_no_command_text()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteNonQuery());

                Assert.Equal(Strings.FormatCallRequiresSetCommandText("ExecuteNonQuery"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteNonQuery_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Parameter";
                command.Parameters.AddWithValue("@Parameter", 1);
                connection.Open();

                var result = command.ExecuteNonQuery();

                Assert.True(command.Parameters.Bound);
                Assert.Equal(0, result);
            }
        }

        [Fact]
        public void ExecuteNonQuery_can_be_called_more_than_once()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                command.ExecuteNonQuery();
                command.ExecuteNonQuery();
            }
        }

        [Fact]
        public void ExecuteNonQuery_can_be_called_more_than_once_when_text_changed()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                command.ExecuteNonQuery();
                command.CommandText = "SELECT 0";
                command.ExecuteNonQuery();
            }
        }

        [Fact]
        public void ExecuteNonQuery_can_be_called_more_than_once_when_connection_changed()
        {
            using (var command = new SQLiteCommand("SELECT 1"))
            {
                using (var connection = new SQLiteConnection("Filename=:memory:"))
                {
                    command.Connection = connection;
                    connection.Open();

                    command.ExecuteNonQuery();
                }

                using (var connection = new SQLiteConnection("Filename=:memory:"))
                {
                    command.Connection = connection;
                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        [Fact]
        public void ExecuteReader_throws_when_open_reader()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                using (command.ExecuteReader())
                {
                    var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteReader());
                    Assert.Equal(Strings.OpenReaderExists, ex.Message);
                }
            }
        }

        [Fact]
        public void ExecuteReader_throws_when_no_connection()
        {
            using (var command = new SQLiteCommand())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteReader());

                Assert.Equal(Strings.FormatCallRequiresOpenConnection("ExecuteReader"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteReader_throws_when_connection_closed()
        {
            using (var connection = new SQLiteConnection())
            using (var command = connection.CreateCommand())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteReader());

                Assert.Equal(Strings.FormatCallRequiresOpenConnection("ExecuteReader"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteReader_throws_when_no_command_text()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteReader());

                Assert.Equal(Strings.FormatCallRequiresSetCommandText("ExecuteReader"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteReader_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Parameter";
                command.Parameters.AddWithValue("@Parameter", 1);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    Assert.True(command.Parameters.Bound);
                    Assert.NotNull(command.OpenReader);
                    Assert.NotNull(reader);
                }
            }
        }

        [Fact]
        public void Cancel_not_supported()
        {
            using (var command = new SQLiteCommand())
            {
                Assert.Throws<NotSupportedException>(() => command.Cancel());
            }
        }

        [Fact]
        public void Dispose_closes_open_reader()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT @Parameter";
                command.Parameters.AddWithValue("@Parameter", 1);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    command.Dispose();

                    Assert.True(reader.IsClosed);
                }
            }
        }
    }
}
