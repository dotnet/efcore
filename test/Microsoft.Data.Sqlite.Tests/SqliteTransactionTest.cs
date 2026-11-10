// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite.Properties;
using Xunit;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite;

public class SqliteTransactionTest
{
    [Theory, InlineData(false), InlineData(true)]
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

        using var transaction = async ? await connection.BeginTransactionAsync() : connection.BeginTransaction();

        await AddNewTable("Table1");

        connection.SimulateFailureOnRollback = true;

        try
        {
            if (async)
            {
                await transaction.DisposeAsync();
            }
            else
            {
                transaction.Dispose();
            }

            Assert.Fail();
        }
        catch
        {
            // Expected to throw.
        }

        Assert.Null(connection.Transaction);

        connection.SimulateFailureOnRollback = false;

        using var transaction2 = async ? await connection.BeginTransactionAsync() : connection.BeginTransaction();

        await AddNewTable("Table2");

        if (async)
        {
            await transaction2.DisposeAsync();
        }
        else
        {
            transaction2.Dispose();
        }

        Assert.Null(connection.Transaction);

        async Task AddNewTable(string tableName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"CREATE TABLE {tableName} (ID INT PRIMARY KEY NOT NULL);";
            _ = async ? await command.ExecuteNonQueryAsync() : command.ExecuteNonQuery();
        }
    }

    private class FakeCommand(FakeConnection connection, SqliteCommand realCommand) : SqliteCommand
    {
        public override int ExecuteNonQuery()
        {
            var result = realCommand.ExecuteNonQuery();

            return connection.SimulateFailureOnRollback && CommandText.Contains("ROLLBACK")
                ? throw new SqliteException("Simulated failure", 1)
                : result;
        }

        [AllowNull]
        public override string CommandText { get => realCommand.CommandText; set => realCommand.CommandText = value; }

        public override int CommandTimeout { get => realCommand.CommandTimeout; set => realCommand.CommandTimeout = value; }
        public override CommandType CommandType { get => realCommand.CommandType; set => realCommand.CommandType = value; }
        public override bool DesignTimeVisible { get => realCommand.DesignTimeVisible; set => realCommand.DesignTimeVisible = value; }

        public override UpdateRowSource UpdatedRowSource
        {
            get => realCommand.UpdatedRowSource;
            set => realCommand.UpdatedRowSource = value;
        }

        public override void Cancel()
            => realCommand.Cancel();

        public override object? ExecuteScalar()
            => realCommand.ExecuteScalar();

        public override void Prepare()
            => realCommand.Prepare();
    }

    private class FakeConnection(string connectionString) : SqliteConnection(connectionString)
    {
        public bool SimulateFailureOnRollback { get; set; }

        public override SqliteCommand CreateCommand()
            => new FakeCommand(this, base.CreateCommand());

        public new SqliteTransaction? Transaction
            => base.Transaction;
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

    [Theory, InlineData(IsolationLevel.Chaos), InlineData(IsolationLevel.Snapshot)]
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

            var ex = Assert.Throws<SqliteException>(() => connection2.ExecuteScalar<long>("SELECT * FROM Data;"));

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

            var ex = Assert.Throws<SqliteException>(() =>
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

    [Theory, InlineData(IsolationLevel.ReadUncommitted), InlineData(IsolationLevel.ReadCommitted),
     InlineData(IsolationLevel.RepeatableRead)]
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

        var ex = Assert.Throws<InvalidOperationException>(transaction.Commit);

        Assert.Equal(Resources.TransactionCompleted, ex.Message);
    }

    [Fact]
    public void Commit_throws_when_completed_externally()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var transaction = connection.BeginTransaction();
        connection.ExecuteNonQuery("ROLLBACK;");

        var ex = Assert.Throws<InvalidOperationException>(transaction.Commit);

        Assert.Equal(Resources.TransactionCompleted, ex.Message);
    }

    [Fact]
    public void Commit_throws_when_connection_closed()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var transaction = connection.BeginTransaction();
        connection.Close();

        var ex = Assert.Throws<InvalidOperationException>(transaction.Commit);

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
        var ex = Assert.Throws<InvalidOperationException>(transaction.Rollback);

        Assert.Equal(Resources.TransactionCompleted, ex.Message);
    }

    [Fact]
    public void Rollback_throws_when_completed()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var transaction = connection.BeginTransaction();
        transaction.Dispose();

        var ex = Assert.Throws<InvalidOperationException>(transaction.Rollback);

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

    [Fact]
    public void Handle_can_be_disposed_with_an_open_transaction()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var transaction = connection.BeginTransaction();

        // SQLite rolls the open transaction back inside sqlite3_close_v2 and fires the rollback
        // hook while the handle is being released. Clearing the hook through that same handle threw
        // out of the native callback, which is fatal wherever the close happens to run.
        connection.Handle!.Dispose();

        Assert.True(transaction.ExternalRollback);
    }

    [Fact]
    public void Rollback_works_when_the_handle_is_already_closed()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var transaction = connection.BeginTransaction();

        // Clearing the hook first means closing the handle does not run RollbackExternal, so
        // ExternalRollback stays false and RollbackInternal is the one that reaches for the handle.
        sqlite3_rollback_hook(connection.Handle, null, null);
        connection.Handle!.Dispose();

        Assert.False(transaction.ExternalRollback);

        transaction.Rollback();
    }

    [Fact]
    public Task BeginTransaction_fails_immediately_when_read_snapshot_is_stale()
        => WithStaleReadSnapshot(
            connection =>
            {
                var ex = Assert.Throws<SqliteException>(() => connection.BeginTransaction());

                Assert.Equal(SQLITE_BUSY, ex.SqliteErrorCode);
                Assert.Equal(SQLITE_BUSY_SNAPSHOT, ex.SqliteExtendedErrorCode);
                return Task.CompletedTask;
            });

    [Fact]
    public Task Write_fails_immediately_when_read_snapshot_is_stale()
        => WithStaleReadSnapshot(
            connection =>
            {
                var ex = Assert.Throws<SqliteException>(() => connection.ExecuteNonQuery("INSERT INTO Data VALUES (4)"));

                Assert.Equal(SQLITE_BUSY_SNAPSHOT, ex.SqliteExtendedErrorCode);
                return Task.CompletedTask;
            });

    [Fact]
    public Task Async_write_fails_immediately_when_read_snapshot_is_stale()
        => WithStaleReadSnapshot(
            async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Data VALUES (4)";

                var ex = await Assert.ThrowsAsync<SqliteException>(() => command.ExecuteNonQueryAsync());

                Assert.Equal(SQLITE_BUSY_SNAPSHOT, ex.SqliteExtendedErrorCode);
            });

    // A command timeout of zero means to retry forever, which previously never returned for a stale snapshot.
    [Fact]
    public Task Write_fails_immediately_when_read_snapshot_is_stale_and_timeout_is_zero()
        => WithStaleReadSnapshot(
            connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Data VALUES (4)";
                command.CommandTimeout = 0;

                var ex = Assert.Throws<SqliteException>(() => command.ExecuteNonQuery());

                Assert.Equal(SQLITE_BUSY_SNAPSHOT, ex.SqliteExtendedErrorCode);
                return Task.CompletedTask;
            });

    // The snapshot is held by the transaction, so disposing the reader doesn't help.
    [Fact]
    public Task Write_in_deferred_transaction_fails_immediately_when_read_snapshot_is_stale()
        => WithStaleReadSnapshot(
            connection =>
            {
                var ex = Assert.Throws<SqliteException>(() => connection.ExecuteNonQuery("INSERT INTO Data VALUES (4)"));

                Assert.Equal(SQLITE_BUSY_SNAPSHOT, ex.SqliteExtendedErrorCode);
                return Task.CompletedTask;
            },
            deferred: true,
            disposeReaderFirst: true);

    // Contrast with a stale snapshot: waiting for another writer can succeed, so this must still be retried.
    [Fact]
    public async Task Write_is_retried_when_another_connection_holds_write_lock_in_wal_mode()
    {
        using var database = new TempDatabase();
        using var connection = database.Open();
        connection.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (1);");

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM Data";
        using var reader = command.ExecuteReader();
        Assert.True(reader.Read());

        using var other = database.Open();
        var transaction = other.BeginTransaction();
        other.ExecuteNonQuery("INSERT INTO Data VALUES (2)");
        var release = Task.Run(
            async () =>
            {
                await Task.Delay(1000);
                transaction.Rollback();
            });

        var stopwatch = Stopwatch.StartNew();
        connection.ExecuteNonQuery("INSERT INTO Data VALUES (3)");

        Assert.True(stopwatch.Elapsed >= TimeSpan.FromMilliseconds(500), $"Took {stopwatch.Elapsed}");
        await release;
    }

    [Fact]
    public void Write_is_retried_until_timeout_when_another_connection_holds_write_lock_in_wal_mode()
    {
        using var database = new TempDatabase("Default Timeout=1");
        using var connection = database.Open();
        connection.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (1);");

        using var other = database.Open();
        using var transaction = other.BeginTransaction();
        other.ExecuteNonQuery("INSERT INTO Data VALUES (2)");

        var stopwatch = Stopwatch.StartNew();
        var ex = Assert.Throws<SqliteException>(() => connection.ExecuteNonQuery("INSERT INTO Data VALUES (3)"));

        Assert.Equal(SQLITE_BUSY, ex.SqliteErrorCode);
        Assert.Equal(SQLITE_BUSY, ex.SqliteExtendedErrorCode);
        Assert.True(stopwatch.Elapsed >= TimeSpan.FromMilliseconds(900), $"Took {stopwatch.Elapsed}");
    }

    // Waiting for the default 30 second timeout can't help here, since the snapshot stays stale while it is held.
    private static async Task WithStaleReadSnapshot(
        Func<SqliteConnection, Task> act,
        bool deferred = false,
        bool disposeReaderFirst = false)
    {
        using var database = new TempDatabase();
        using var connection = database.Open();
        connection.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (1), (2);");

        using var transaction = deferred ? connection.BeginTransaction(deferred: true) : null;
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM Data";
        var reader = command.ExecuteReader();
        try
        {
            Assert.True(reader.Read());
            if (disposeReaderFirst)
            {
                reader.Dispose();
            }

            using (var other = database.Open())
            {
                other.ExecuteNonQuery("INSERT INTO Data VALUES (3)");
            }

            var stopwatch = Stopwatch.StartNew();
            await act(connection);
            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10), $"Took {stopwatch.Elapsed}");
        }
        finally
        {
            reader.Dispose();
        }
    }

    private sealed class TempDatabase : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), $"{nameof(SqliteTransactionTest)}-{Guid.NewGuid():N}.db");
        private readonly string _connectionString;
        private bool _walEnabled;

        public TempDatabase(string options = "")
            => _connectionString = $"Data Source={_path};Pooling=False;{options}";

        public SqliteConnection Open()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            if (!_walEnabled)
            {
                Assert.Equal("wal", connection.ExecuteScalar<string>("PRAGMA journal_mode = 'wal';"));
                _walEnabled = true;
            }

            return connection;
        }

        public void Dispose()
        {
            foreach (var file in Directory.GetFiles(Path.GetTempPath(), Path.GetFileName(_path) + "*"))
            {
                File.Delete(file);
            }
        }
    }
}
