// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.IO;
using Xunit;

namespace Microsoft.Data.SQLite
{
    public class SQLiteConnectionTest
    {
        [Fact]
        public void Ctor_validates_argument()
        {
            var ex = Assert.Throws<ArgumentException>(() => new SQLiteConnection(null));

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("connectionString"), ex.Message);
        }

        [Fact]
        public void Ctor_sets_connection_string()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
            {
                Assert.Equal("Filename=test.db", connection.ConnectionString);
            }
        }

        [Fact]
        public void ConnectionString_setter_validates_argument()
        {
            using (var connection = new SQLiteConnection())
            {
                var ex = Assert.Throws<ArgumentException>(() => connection.ConnectionString = null);

                Assert.Equal(Strings.ArgumentIsNullOrWhitespace("value"), ex.Message);
            }
        }

        [Fact]
        public void ConnectionString_setter_throws_when_open()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
            {
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(
                    () => connection.ConnectionString = "Filename=new.db");

                Assert.Equal(Strings.ConnectionStringRequiresClosedConnection, ex.Message);
            }
        }

        [Fact]
        public void ConnectionString_gets_and_sets_value()
        {
            using (var connection = new SQLiteConnection { ConnectionString = "Filename=test.db" })
            {
                Assert.Equal("Filename=test.db", connection.ConnectionString);
            }
        }

        [Fact]
        public void Database_returns_value()
        {
            using (var connection = new SQLiteConnection())
            {
                Assert.Equal("main", connection.Database);
            }
        }

        [Fact]
        public void DataSource_returns_value()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
            {
                Assert.Equal("test.db", connection.DataSource);
            }
        }

        [Fact]
        public void ServerVersion_returns_value()
        {
            using (var connection = new SQLiteConnection())
            {
                var version = connection.ServerVersion;

                Assert.NotNull(version);
                Assert.True(version.StartsWith("3."));
            }
        }

        [Fact]
        public void State_closed_by_default()
        {
            using (var connection = new SQLiteConnection())
            {
                Assert.Equal(ConnectionState.Closed, connection.State);
            }
        }

        [Fact]
        public void Open_throws_when_disposed()
        {
            var connection = new SQLiteConnection("Filename=test.db");
            connection.Dispose();

            Assert.Throws<ObjectDisposedException>(() => connection.Open());
        }

        [Fact]
        public void Open_throws_when_no_connection_string()
        {
            using (var connection = new SQLiteConnection())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => connection.Open());

                Assert.Equal(Strings.OpenRequiresSetConnectionString, ex.Message);
            }
        }

        [Fact]
        public void Open_throws_when_error()
        {
            using (var connection = new SQLiteConnection("Filename=:*?\"<>|"))
            {
                var ex = Assert.Throws<SQLiteException>(() => connection.Open());

                Assert.Equal(14, ex.ErrorCode);
            }
        }

        [Fact]
        public void Open_works()
        {
            File.Delete("test.db");

            using (var connection = new SQLiteConnection("Filename=test.db"))
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
                    Assert.True(File.Exists("test.db"));
                }
                finally
                {
                    connection.StateChange -= handler;
                }
            }
        }

        [Fact]
        public void Open_can_be_called_more_than_once()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
            {
                connection.Open();
                connection.Open();
            }
        }

        [Fact]
        public void Close_works()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
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
            using (var connection = new SQLiteConnection("Filename=test.db"))
            {
                connection.Close();
            }
        }

        [Fact]
        public void Close_can_be_called_more_than_once()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
            {
                connection.Open();
                connection.Close();
                connection.Close();
            }
        }

        [Fact]
        public void Dispose_closes_connection()
        {
            var connection = new SQLiteConnection("Filename=test.db");
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
                connection.Dispose();

                Assert.True(raised);
                Assert.Equal(ConnectionState.Closed, connection.State);
            }
            finally
            {
                connection.StateChange -= handler;
            }
        }

        [Fact]
        public void Dispose_can_be_called_more_than_once()
        {
            var connection = new SQLiteConnection("Filename=test.db");
            connection.Open();

            connection.Dispose();
            connection.Dispose();
        }

        [Fact]
        public void CreateCommand_returns_command()
        {
            using (var connection = new SQLiteConnection())
            using (var command = connection.CreateCommand())
            {
                Assert.NotNull(command);
                Assert.Same(connection, command.Connection);
            }
        }

        [Fact]
        public void BeginTransaction_validates_argument()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
            {
                connection.Open();

                var ex = Assert.Throws<ArgumentException>(() => connection.BeginTransaction(0));

                Assert.Equal(Strings.InvalidIsolationLevel(0), ex.Message);
            }
        }

        [Fact]
        public void BeginTransaction_throws_when_closed()
        {
            using (var connection = new SQLiteConnection())
            {
                var ex = Assert.Throws<InvalidOperationException>(() => connection.BeginTransaction());

                Assert.Equal(Strings.CallRequiresOpenConnection("BeginTransaction"), ex.Message);
            }
        }

        [Fact]
        public void BeginTransaction_works()
        {
            using (var connection = new SQLiteConnection("Filename=test.db"))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
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
            using (var connection = new SQLiteConnection())
            {
                Assert.Throws<NotSupportedException>(() => connection.ChangeDatabase("new"));
            }
        }
    }
}
