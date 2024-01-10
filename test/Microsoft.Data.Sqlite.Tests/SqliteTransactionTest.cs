// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite.Properties;
using Xunit;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite;

public class SqliteTransactionTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SqliteTransaction_Dispose_does_not_leave_orphaned_transaction(bool async) // Issue #25119
    {
        using var connection = new FakeConnection("Data Source=:memory:");

        if (async)
        {
            await connection.OpenAsync();
        }
        else
        {
            connection.Open();
        }

#if NET5_0_OR_GREATER
        using var transaction = async ? await connection.BeginTransactionAsync() : connection.BeginTransaction();
#else
        using var transaction = connection.BeginTransaction();
#endif

        await AddNewTable("Table1");

        connection.SimulateFailureOnRollback = true;

        try
        {
#if NET5_0_OR_GREATER
            if (async)
            {
                await transaction.DisposeAsync();
            }
            else
            {
                transaction.Dispose();
            }
#else
            transaction.Dispose();
#endif

            Assert.Fail();
        }
        catch (Exception)
        {
            // Expected to throw.
        }

        Assert.Null(connection.Transaction);

        connection.SimulateFailureOnRollback = false;

#if NET5_0_OR_GREATER
        using var transaction2 = async ? await connection.BeginTransactionAsync() : connection.BeginTransaction();
#else
        using var transaction2 = connection.BeginTransaction();
#endif

        await AddNewTable("Table2");

#if NET5_0_OR_GREATER
            if (async)
            {
                await transaction2.DisposeAsync();
            }
            else
            {
                transaction2.Dispose();
            }
#else
        transaction2.Dispose();
#endif

        Assert.Null(connection.Transaction);

        async Task AddNewTable(string tableName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"CREATE TABLE {tableName} (ID INT PRIMARY KEY NOT NULL);";
            _ = async ? await command.ExecuteNonQueryAsync() : command.ExecuteNonQuery();
        }
    }

    private class FakeCommand : SqliteCommand
    {
        private readonly FakeConnection _connection;
        private readonly SqliteCommand _realCommand;

        public FakeCommand(FakeConnection connection, SqliteCommand realCommand)
        {
            _connection = connection;
            _realCommand = realCommand;
        }

        public override int ExecuteNonQuery()
        {
            var result = _realCommand.ExecuteNonQuery();

            if (_connection.SimulateFailureOnRollback && CommandText.Contains("ROLLBACK"))
            {
                throw new SqliteException("Simulated failure", 1);
            }

            return result;
        }

        [AllowNull]
        public override string CommandText { get => _realCommand.CommandText; set => _realCommand.CommandText = value; }
        public override int CommandTimeout { get => _realCommand.CommandTimeout; set => _realCommand.CommandTimeout = value; }
        public override CommandType CommandType { get => _realCommand.CommandType; set => _realCommand.CommandType = value; }
        public override bool DesignTimeVisible { get => _realCommand.DesignTimeVisible; set => _realCommand.DesignTimeVisible = value; }

        public override UpdateRowSource UpdatedRowSource
        {
            get => _realCommand.UpdatedRowSource;
            set => _realCommand.UpdatedRowSource = value;
        }

        public override void Cancel()
            => _realCommand.Cancel();

        public override object? ExecuteScalar()
            => _realCommand.ExecuteScalar();

        public override void Prepare()
            => _realCommand.Prepare();
    }

    private class FakeConnection(string connectionString) : SqliteConnection(connectionString)
    {
        public bool SimulateFailureOnRollback { get; set; }

        public override SqliteCommand CreateCommand()
            => new FakeCommand(this, base.CreateCommand());

        public new SqliteTransaction? Transaction => base.Transaction;
    }

    [Fact]
    public void Ctor_sets_read_uncommitted()
    {
        using var connection = new SqliteConnection("Data Source=:memory:;Cache=Shared");
        connection.Open();

        using (connection.BeginTransaction(IsolationLevel.ReadUncommitted))
        {
            Assert.Equal(1L, connection.ExecuteScalar<long>("PRAGMA read_uncommitted;"));
        }
    }

    [Fact]
    public void Ctor_unsets_read_uncommitted_when_serializable()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using (connection.BeginTransaction(IsolationLevel.Serializable))
        {
            Assert.Equal(0L, connection.ExecuteScalar<long>("PRAGMA read_uncommitted;"));
        }
    }

    [Theory]
    [InlineData(IsolationLevel.Chaos)]
    [InlineData(IsolationLevel.Snapshot)]
    public void Ctor_throws_when_invalid_isolation_level(IsolationLevel isolationLevel)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var ex = Assert.Throws<ArgumentException>(() => connection.BeginTransaction(isolationLevel));

        Assert.Equal(Resources.InvalidIsolationLevel(isolationLevel), ex.Message);
    }

    [Fact]
    public void ReadUncommitted_allows_dirty_reads()
    {
        const string connectionString = "Data Source=read-uncommitted;Mode=Memory;Cache=Shared";

        using var connection1 = new SqliteConnection(connectionString);
        using var connection2 = new SqliteConnection(connectionString);
        connection1.Open();
        connection2.Open();

        connection1.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (0);");

        using (connection1.BeginTransaction())
        {
            using (connection2.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                connection1.ExecuteNonQuery("UPDATE Data SET Value = 1;");

                var value = connection2.ExecuteScalar<long>("SELECT * FROM Data;");

                Assert.Equal(1, value);
            }

            connection2.DefaultTimeout = 1;

            var ex = Assert.Throws<SqliteException>(
                () => connection2.ExecuteScalar<long>("SELECT * FROM Data;"));

            Assert.Equal(SQLITE_LOCKED, ex.SqliteErrorCode);
            Assert.Equal(SQLITE_LOCKED_SHAREDCACHE, ex.SqliteExtendedErrorCode);
        }
    }

    [Fact]
    public void Serialized_disallows_dirty_reads()
    {
        const string connectionString = "Data Source=serialized;Mode=Memory;Cache=Shared";

        using var connection1 = new SqliteConnection(connectionString);
        using var connection2 = new SqliteConnection(connectionString);
        connection1.Open();
        connection2.Open();

        connection1.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (0);");

        using (connection1.BeginTransaction())
        {
            connection1.ExecuteNonQuery("UPDATE Data SET Value = 1;");

            connection2.DefaultTimeout = 1;

            var ex = Assert.Throws<SqliteException>(
                () =>
                {
                    using (connection2.BeginTransaction(IsolationLevel.Serializable))
                    {
                        connection2.ExecuteScalar<long>("SELECT * FROM Data;");
                    }
                });

            Assert.Equal(SQLITE_LOCKED, ex.SqliteErrorCode);
            Assert.Equal(SQLITE_LOCKED_SHAREDCACHE, ex.SqliteExtendedErrorCode);
        }
    }

    [Fact]
    public void Deferred_allows_parallel_reads()
    {
        const string connectionString = "Data Source=deferred;Mode=Memory;Cache=Shared";

        using var connection1 = new SqliteConnection(connectionString);
        using var connection2 = new SqliteConnection(connectionString);
        connection1.Open();
        connection2.Open();

        connection1.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (42);");

        using (connection1.BeginTransaction(deferred: true))
        {
            var value1 = connection1.ExecuteScalar<long>("SELECT * FROM Data;");
            Assert.Equal(42L, value1);

            using (connection2.BeginTransaction(deferred: true))
            {
                var value2 = connection2.ExecuteScalar<long>("SELECT * FROM Data;");
                Assert.Equal(42L, value2);
            }
        }
    }

    [Fact]
    public void IsolationLevel_is_serializable_when_unspecified()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var transaction = connection.BeginTransaction();
        Assert.Equal(IsolationLevel.Serializable, transaction.IsolationLevel);
    }

    [Theory]
    [InlineData(IsolationLevel.ReadUncommitted)]
    [InlineData(IsolationLevel.ReadCommitted)]
    [InlineData(IsolationLevel.RepeatableRead)]
    public void IsolationLevel_is_increased_when_unsupported(IsolationLevel isolationLevel)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var transaction = connection.BeginTransaction(isolationLevel);
        Assert.Equal(IsolationLevel.Serializable, transaction.IsolationLevel);
    }

    [Fact]
    public void Commit_throws_when_completed()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var transaction = connection.BeginTransaction();
        transaction.Dispose();

        var ex = Assert.Throws<InvalidOperationException>(() => transaction.Commit());

        Assert.Equal(Resources.TransactionCompleted, ex.Message);
    }

    [Fact]
    public void Commit_throws_when_completed_externally()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var transaction = connection.BeginTransaction();
        connection.ExecuteNonQuery("ROLLBACK;");

        var ex = Assert.Throws<InvalidOperationException>(() => transaction.Commit());

        Assert.Equal(Resources.TransactionCompleted, ex.Message);
    }

    [Fact]
    public void Commit_throws_when_connection_closed()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var transaction = connection.BeginTransaction();
        connection.Close();

        var ex = Assert.Throws<InvalidOperationException>(() => transaction.Commit());

        Assert.Equal(Resources.TransactionCompleted, ex.Message);
    }

    [Fact]
    public void Commit_works()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        CreateTestTable(connection);

        using (var transaction = connection.BeginTransaction())
        {
            connection.ExecuteNonQuery("INSERT INTO TestTable VALUES (1);");

            transaction.Commit();

            Assert.Null(connection.Transaction);
            Assert.Null(transaction.Connection);
        }

        Assert.Equal(1L, connection.ExecuteScalar<long>("SELECT COUNT(*) FROM TestTable;"));
    }

    [Fact]
    public void Rollback_noops_once_when_completed_externally()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var transaction = connection.BeginTransaction();
        connection.ExecuteNonQuery("ROLLBACK;");

        transaction.Rollback();
        var ex = Assert.Throws<InvalidOperationException>(() => transaction.Rollback());

        Assert.Equal(Resources.TransactionCompleted, ex.Message);
    }

    [Fact]
    public void Rollback_throws_when_completed()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var transaction = connection.BeginTransaction();
        transaction.Dispose();

        var ex = Assert.Throws<InvalidOperationException>(() => transaction.Rollback());

        Assert.Equal(Resources.TransactionCompleted, ex.Message);
    }

    [Fact]
    public void Rollback_works()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        CreateTestTable(connection);

        using (var transaction = connection.BeginTransaction())
        {
            connection.ExecuteNonQuery("INSERT INTO TestTable VALUES (1);");

            transaction.Rollback();

            Assert.Null(connection.Transaction);
            Assert.Null(transaction.Connection);
        }

        Assert.Equal(0L, connection.ExecuteScalar<long>("SELECT COUNT(*) FROM TestTable;"));
    }

    [Fact]
    public void Dispose_works()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        CreateTestTable(connection);

        using (var transaction = connection.BeginTransaction())
        {
            connection.ExecuteNonQuery("INSERT INTO TestTable VALUES (1);");

            transaction.Dispose();

            Assert.Null(connection.Transaction);
            Assert.Null(transaction.Connection);
        }

        Assert.Equal(0L, connection.ExecuteScalar<long>("SELECT COUNT(*) FROM TestTable;"));
    }

    [Fact]
    public void Dispose_can_be_called_more_than_once()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var transaction = connection.BeginTransaction();

        transaction.Dispose();
        transaction.Dispose();
    }

    [Fact]
    public void Savepoint()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        CreateTestTable(connection);

        var transaction = connection.BeginTransaction();
        transaction.Save("MySavepointName");

        connection.ExecuteNonQuery("INSERT INTO TestTable (TestColumn) VALUES (8)");
        Assert.Equal(1L, connection.ExecuteScalar<long>("SELECT COUNT(*) FROM TestTable;"));

        transaction.Rollback("MySavepointName");
        Assert.Equal(0L, connection.ExecuteScalar<long>("SELECT COUNT(*) FROM TestTable;"));

        transaction.Release("MySavepointName");
        Assert.Throws<SqliteException>(() => transaction.Rollback("MySavepointName"));
    }

    private static void CreateTestTable(SqliteConnection connection)
        => connection.ExecuteNonQuery("CREATE TABLE TestTable (TestColumn INTEGER)");
}
