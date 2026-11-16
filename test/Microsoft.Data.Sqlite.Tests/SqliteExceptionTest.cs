// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using Microsoft.Data.Sqlite.Properties;
using Xunit;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite;

public class SqliteExceptionTest
{
    [Fact]
    public void Ctor_sets_message_and_errorCode()
    {
        var ex = new SqliteException("test", 1);

        Assert.Equal("test", ex.Message);
        Assert.Equal(1, ex.SqliteErrorCode);
        Assert.Equal(1, ex.SqliteExtendedErrorCode);
    }

    [Fact]
    public void Ctor_sets_extendedErrorCode()
    {
        var ex = new SqliteException("test", 1, 2);

        Assert.Equal("test", ex.Message);
        Assert.Equal(1, ex.SqliteErrorCode);
        Assert.Equal(2, ex.SqliteExtendedErrorCode);
    }

    [Theory, InlineData(SQLITE_OK), InlineData(SQLITE_ROW), InlineData(SQLITE_DONE)]
    public void ThrowExceptionForRC_does_nothing_when_non_error(int rc)
        => SqliteException.ThrowExceptionForRC(rc, null);

    [Fact]
    public void Message_has_no_extra_text_when_extended_code_matches_and_no_guidance()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var ex = Assert.Throws<SqliteException>(() => connection.ExecuteNonQuery("SELECT * FROM Missing"));

        Assert.Equal(SQLITE_ERROR, ex.SqliteExtendedErrorCode);
        Assert.Equal(Resources.SqliteNativeError(SQLITE_ERROR, "no such table: Missing"), ex.Message);
    }

    [Fact]
    public void Message_includes_extended_code_name()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        connection.ExecuteNonQuery("CREATE TABLE Data (Id INTEGER PRIMARY KEY, Value TEXT UNIQUE)");
        connection.ExecuteNonQuery("INSERT INTO Data (Value) VALUES ('A')");

        var ex = Assert.Throws<SqliteException>(() => connection.ExecuteNonQuery("INSERT INTO Data (Value) VALUES ('A')"));

        Assert.Equal(SQLITE_CONSTRAINT, ex.SqliteErrorCode);
        Assert.Equal(SQLITE_CONSTRAINT_UNIQUE, ex.SqliteExtendedErrorCode);
        Assert.Contains($"{SQLITE_CONSTRAINT_UNIQUE} (SQLITE_CONSTRAINT_UNIQUE)", ex.Message);
        Assert.StartsWith(Resources.SqliteNativeError(SQLITE_CONSTRAINT, "UNIQUE constraint failed: Data.Value"), ex.Message);
    }

    [Fact]
    public void Message_includes_guidance_when_write_follows_other_connections_commit_while_reading()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{nameof(SqliteExceptionTest)}-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={path};Pooling=False;Default Timeout=1";
        try
        {
            using var reading = new SqliteConnection(connectionString);
            reading.Open();
            reading.ExecuteNonQuery("PRAGMA journal_mode = 'wal'; CREATE TABLE Data (Value); INSERT INTO Data VALUES (1), (2);");

            using var command = reading.CreateCommand();
            command.CommandText = "SELECT Value FROM Data";
            using var reader = command.ExecuteReader();
            Assert.True(reader.Read());

            using (var writing = new SqliteConnection(connectionString))
            {
                writing.Open();
                writing.ExecuteNonQuery("INSERT INTO Data VALUES (3)");
            }

            var ex = Assert.Throws<SqliteException>(() => reading.BeginTransaction());

            Assert.Equal(SQLITE_BUSY, ex.SqliteErrorCode);
            Assert.Equal(SQLITE_BUSY_SNAPSHOT, ex.SqliteExtendedErrorCode);
            Assert.Contains("SQLITE_BUSY_SNAPSHOT", ex.Message);
            Assert.Contains(Resources.SqliteBusySnapshotGuidance, ex.Message);
        }
        finally
        {
            DeleteDatabase(path);
        }
    }

    // Disposing the reader doesn't help here: the snapshot is held until the transaction ends, so the guidance has to say to retry it.
    [Fact]
    public void Message_includes_guidance_when_deferred_transaction_writes_after_other_connections_commit()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{nameof(SqliteExceptionTest)}-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={path};Pooling=False;Default Timeout=1";
        try
        {
            using var reading = new SqliteConnection(connectionString);
            reading.Open();
            reading.ExecuteNonQuery("PRAGMA journal_mode = 'wal'; CREATE TABLE Data (Value); INSERT INTO Data VALUES (1), (2);");

            using var transaction = reading.BeginTransaction(deferred: true);
            Assert.Equal(2L, reading.ExecuteScalar<long>("SELECT COUNT(*) FROM Data"));

            using (var writing = new SqliteConnection(connectionString))
            {
                writing.Open();
                writing.ExecuteNonQuery("INSERT INTO Data VALUES (3)");
            }

            var ex = Assert.Throws<SqliteException>(() => reading.ExecuteNonQuery("INSERT INTO Data VALUES (4)"));

            Assert.Equal(SQLITE_BUSY, ex.SqliteErrorCode);
            Assert.Equal(SQLITE_BUSY_SNAPSHOT, ex.SqliteExtendedErrorCode);
            Assert.Contains("SQLITE_BUSY_SNAPSHOT", ex.Message);
            Assert.Contains(Resources.SqliteBusySnapshotGuidance, ex.Message);
        }
        finally
        {
            DeleteDatabase(path);
        }
    }

    // Plain SQLITE_BUSY has many causes (another writer, backup, ...) so no guidance is given for it.
    [Fact]
    public void Message_has_no_guidance_when_database_stays_busy()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{nameof(SqliteExceptionTest)}-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={path};Pooling=False;Default Timeout=1";
        try
        {
            using var first = new SqliteConnection(connectionString);
            first.Open();
            first.ExecuteNonQuery("CREATE TABLE Data (Value)");
            using var transaction = first.BeginTransaction();

            using var second = new SqliteConnection(connectionString);
            second.Open();

            var ex = Assert.Throws<SqliteException>(() => second.BeginTransaction());

            Assert.Equal(SQLITE_BUSY, ex.SqliteErrorCode);
            Assert.Equal(SQLITE_BUSY, ex.SqliteExtendedErrorCode);
            Assert.Equal(Resources.SqliteNativeError(SQLITE_BUSY, "database is locked"), ex.Message);
        }
        finally
        {
            DeleteDatabase(path);
        }
    }

    [Fact]
    public void ThrowExceptionForRC_uses_default_message_without_a_database()
    {
        var ex = Assert.Throws<SqliteException>(() => SqliteException.ThrowExceptionForRC(SQLITE_BUSY, null));

        Assert.Equal(SQLITE_BUSY, ex.SqliteErrorCode);
        Assert.Equal(SQLITE_BUSY, ex.SqliteExtendedErrorCode);
        Assert.Equal(
            Resources.SqliteNativeError(SQLITE_BUSY, sqlite3_errstr(SQLITE_BUSY).utf8_to_string() + " " + Resources.DefaultNativeError),
            ex.Message);
    }

    // SQLitePCL.raw doesn't define this code, so it is declared locally.
    [Fact]
    public void Message_includes_name_of_constraint_datatype_violation_in_strict_table()
    {
        Assert.SkipUnless(sqlite3_libversion_number() >= 3037000, "STRICT tables require SQLite 3.37.");

        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        connection.ExecuteNonQuery("CREATE TABLE Data (Value INTEGER) STRICT");

        var ex = Assert.Throws<SqliteException>(() => connection.ExecuteNonQuery("INSERT INTO Data VALUES ('a')"));

        Assert.Equal(SQLITE_CONSTRAINT, ex.SqliteErrorCode);
        Assert.Contains("(SQLITE_CONSTRAINT_DATATYPE)", ex.Message);
    }

    [Fact]
    public void Message_includes_extended_code_name_when_extended_result_codes_are_enabled()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        connection.ExecuteNonQuery("CREATE TABLE Data (Id INTEGER PRIMARY KEY, Value TEXT UNIQUE)");
        connection.ExecuteNonQuery("INSERT INTO Data (Value) VALUES ('A')");
        Assert.Equal(SQLITE_OK, sqlite3_extended_result_codes(connection.Handle, 1));

        var ex = Assert.Throws<SqliteException>(() => connection.ExecuteNonQuery("INSERT INTO Data (Value) VALUES ('A')"));

        Assert.Equal(SQLITE_CONSTRAINT_UNIQUE, ex.SqliteErrorCode);
        Assert.Equal(SQLITE_CONSTRAINT_UNIQUE, ex.SqliteExtendedErrorCode);
        Assert.Equal(
            Resources.SqliteNativeError(SQLITE_CONSTRAINT_UNIQUE, "UNIQUE constraint failed: Data.Value")
            + " " + Resources.SqliteExtendedErrorCode($"{SQLITE_CONSTRAINT_UNIQUE} (SQLITE_CONSTRAINT_UNIQUE)"),
            ex.Message);
    }

    [Fact]
    public void FormatMessage_adds_nothing_when_extended_code_matches()
        => Assert.Equal(
            Resources.SqliteNativeError(SQLITE_ERROR, "Test"),
            SqliteException.FormatMessage(SQLITE_ERROR, SQLITE_ERROR, "Test"));

    [Fact]
    public void FormatMessage_adds_only_the_number_for_unnamed_extended_code()
        => Assert.Equal(
            Resources.SqliteNativeError(SQLITE_ERROR, "Test") + " " + Resources.SqliteExtendedErrorCode(12345),
            SqliteException.FormatMessage(SQLITE_ERROR, 12345, "Test"));

    [Theory]
    [InlineData(SQLITE_BUSY, SQLITE_BUSY_SNAPSHOT, "SQLITE_BUSY_SNAPSHOT")]
    [InlineData(SQLITE_BUSY, SQLITE_BUSY_RECOVERY, "SQLITE_BUSY_RECOVERY")]
    [InlineData(SQLITE_LOCKED, SQLITE_LOCKED_SHAREDCACHE, "SQLITE_LOCKED_SHAREDCACHE")]
    [InlineData(SQLITE_CONSTRAINT, SQLITE_CONSTRAINT_UNIQUE, "SQLITE_CONSTRAINT_UNIQUE")]
    [InlineData(SQLITE_CONSTRAINT, SQLITE_CONSTRAINT_PRIMARYKEY, "SQLITE_CONSTRAINT_PRIMARYKEY")]
    [InlineData(SQLITE_CONSTRAINT, SQLITE_CONSTRAINT_TRIGGER, "SQLITE_CONSTRAINT_TRIGGER")]
    [InlineData(SQLITE_ERROR, SqliteException.SQLITE_ERROR_MISSING_COLLSEQ, "SQLITE_ERROR_MISSING_COLLSEQ")]
    [InlineData(SQLITE_READONLY, SQLITE_READONLY_DBMOVED, "SQLITE_READONLY_DBMOVED")]
    [InlineData(SQLITE_CANTOPEN, SQLITE_CANTOPEN_ISDIR, "SQLITE_CANTOPEN_ISDIR")]
    [InlineData(SQLITE_IOERR, SQLITE_IOERR_SHORT_READ, "SQLITE_IOERR_SHORT_READ")]
    [InlineData(SQLITE_CONSTRAINT, 3091, "SQLITE_CONSTRAINT_DATATYPE")]
    [InlineData(SQLITE_BUSY, 773, "SQLITE_BUSY_TIMEOUT")]
    [InlineData(SQLITE_ABORT, SQLITE_ABORT_ROLLBACK, "SQLITE_ABORT_ROLLBACK")]
    [InlineData(SQLITE_IOERR, SQLITE_IOERR_SHMOPEN, "SQLITE_IOERR_SHMOPEN")]
    [InlineData(SQLITE_CONSTRAINT_UNIQUE, SQLITE_CONSTRAINT_UNIQUE, "SQLITE_CONSTRAINT_UNIQUE")] // Extended result codes enabled
    public void FormatMessage_names_common_extended_codes(int errorCode, int extendedErrorCode, string name)
        => Assert.Contains(
            Resources.SqliteExtendedErrorCode($"{extendedErrorCode} ({name})"),
            SqliteException.FormatMessage(errorCode, extendedErrorCode, "Test"));

    [Theory]
    [InlineData(SQLITE_BUSY, SQLITE_BUSY_SNAPSHOT, true)]
    [InlineData(SQLITE_BUSY_SNAPSHOT, SQLITE_BUSY_SNAPSHOT, true)] // Extended result codes enabled
    [InlineData(SQLITE_BUSY, SQLITE_BUSY, false)]
    [InlineData(SQLITE_BUSY, SQLITE_BUSY_RECOVERY, false)]
    [InlineData(SQLITE_LOCKED, SQLITE_LOCKED, false)]
    [InlineData(SQLITE_CONSTRAINT, SQLITE_CONSTRAINT_UNIQUE, false)]
    public void FormatMessage_adds_guidance_only_where_the_cause_is_known(int errorCode, int extendedErrorCode, bool expected)
        => Assert.Equal(
            expected,
            SqliteException.FormatMessage(errorCode, extendedErrorCode, "Test").EndsWith(Resources.SqliteBusySnapshotGuidance));

    private static void DeleteDatabase(string path)
    {
        foreach (var file in Directory.GetFiles(Path.GetTempPath(), Path.GetFileName(path) + "*"))
        {
            File.Delete(file);
        }
    }
}
