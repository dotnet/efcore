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
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("commandText"), ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => new SQLiteCommand("SELECT 1", null));
            Assert.Equal("connection", ex.ParamName);

            using (var connection = new SQLiteConnection())
            {
                ex = Assert.Throws<ArgumentNullException>(() => new SQLiteCommand("SELECT 1", connection, null));
                Assert.Equal("transaction", ex.ParamName);
            }
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

                Assert.Equal(Strings.InvalidCommandType(0), ex.Message);
            }
        }

        [Fact]
        public void CommandText_validates_value()
        {
            using (var command = new SQLiteCommand())
            {
                var ex = Assert.Throws<ArgumentException>(() => command.CommandText = null);

                Assert.Equal(Strings.ArgumentIsNullOrWhitespace("value"), ex.Message);
            }
        }

        [Fact]
        public void Connection_validates_value()
        {
            using (var command = new SQLiteCommand())
            {
                var ex = Assert.Throws<ArgumentNullException>(() => command.Connection = null);

                Assert.Equal("value", ex.ParamName);
            }
        }

        [Fact]
        public void Prepare_throws_when_disposed()
        {
            var command = new SQLiteCommand();
            command.Dispose();

            Assert.Throws<ObjectDisposedException>(() => command.Prepare());
        }

        [Fact]
        public void Prepare_throws_when_no_connection()
        {
            using (var command = new SQLiteCommand())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => command.Prepare());

                Assert.Equal(Strings.CallRequiresOpenConnection("Prepare"), ex.Message);
            }
        }

        [Fact]
        public void Prepare_throws_when_connection_closed()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
            using (var command = connection.CreateCommand())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => command.Prepare());

                Assert.Equal(Strings.CallRequiresOpenConnection("Prepare"), ex.Message);
            }
        }

        [Fact]
        public void Prepare_throws_when_no_command_text()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
            using (var command = connection.CreateCommand())
            {
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => command.Prepare());

                Assert.Equal(Strings.CallRequiresSetCommandText("Prepare"), ex.Message);
            }
        }

        [Fact]
        public void Prepare_throws_on_error()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INVALID";
                connection.Open();

                var ex = Assert.Throws<SQLiteException>(() => command.Prepare());

                Assert.Equal(1, ex.ErrorCode);
            }
        }

        [Fact]
        public void ExecuteNonQuery_throws_when_disposed()
        {
            var command = new SQLiteCommand();
            command.Dispose();

            Assert.Throws<ObjectDisposedException>(() => command.ExecuteNonQuery());
        }

        [Fact]
        public void ExecuteNonQuery_throws_when_no_connection()
        {
            using (var command = new SQLiteCommand())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteNonQuery());

                Assert.Equal(Strings.CallRequiresOpenConnection("ExecuteNonQuery"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteNonQuery_throws_when_connection_closed()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
            using (var command = connection.CreateCommand())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteNonQuery());

                Assert.Equal(Strings.CallRequiresOpenConnection("ExecuteNonQuery"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteNonQuery_throws_when_no_command_text()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
            using (var command = connection.CreateCommand())
            {
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteNonQuery());

                Assert.Equal(Strings.CallRequiresSetCommandText("ExecuteNonQuery"), ex.Message);
            }
        }

        [Fact]
        public void ExecuteNonQuery_works()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                var result = command.ExecuteNonQuery();

                Assert.Equal(0, result);
            }
        }

        [Fact]
        public void ExecuteNonQuery_can_be_called_more_than_once()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
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
            using (var connection = new SQLiteConnection("Filename=test.db"))
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
                using (var connection = new SQLiteConnection("Filename=test.db"))
                {
                    command.Connection = connection;
                    connection.Open();

                    command.ExecuteNonQuery();
                }

                using (var connection = new SQLiteConnection("Filename=new.db"))
                {
                    command.Connection = connection;
                    connection.Open();

                    command.ExecuteNonQuery();
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
    }
}
