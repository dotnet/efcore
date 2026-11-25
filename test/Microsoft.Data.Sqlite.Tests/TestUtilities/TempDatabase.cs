// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Data.Sqlite;

/// <summary>
///     A temporary on-disk database in WAL mode, where connections can have stale read snapshots and
///     where a writer doesn't block readers. Pooling is off so that disposing a connection releases its file handles.
/// </summary>
internal sealed class TempDatabase(string options = "") : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"{nameof(TempDatabase)}-{Guid.NewGuid():N}.db");
    private bool _walEnabled;

    public SqliteConnection Open()
    {
        var connection = new SqliteConnection($"Data Source={_path};Pooling=False;{options}");
        connection.Open();

        if (!_walEnabled)
        {
            Assert.Equal("wal", connection.ExecuteScalar<string>("PRAGMA journal_mode = 'wal';"));
            _walEnabled = true;
        }

        return connection;
    }

    /// <summary>
    ///     Creates a table named Data and gives the connection passed to <paramref name="act" /> a read snapshot that is
    ///     stale because another connection has committed since it was taken, then runs <paramref name="act" />.
    ///     The snapshot is held by the open reader or, if <paramref name="deferred" /> is set, by the transaction.
    ///     Waiting for the default 30 second timeout can't help, since the snapshot stays stale while it is held.
    /// </summary>
    public static async Task WithStaleReadSnapshot(
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

    public void Dispose()
    {
        foreach (var file in Directory.GetFiles(Path.GetTempPath(), Path.GetFileName(_path) + "*"))
        {
            try
            {
                File.Delete(file);
            }
            catch (IOException)
            {
                // Don't let a cleanup failure replace the failure that is being reported
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
