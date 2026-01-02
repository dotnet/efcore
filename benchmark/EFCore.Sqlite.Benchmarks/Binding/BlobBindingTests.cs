// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Data.Sqlite;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Microsoft.EntityFrameworkCore.Benchmarks.Binding;

[DisplayName(nameof(BlobBindingTests))]
[MemoryDiagnoser]
public class BlobBindingTests
{
    private SqliteConnection _connection;

    [Params(100_000, 1_000_000, 10_000_000, 100_000_000)]
    public int BlobSize { get; set; }

    [GlobalSetup]
    public void OpenConnection()
    {
        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        using var command = _connection.CreateCommand();
        command.CommandText = "CREATE TABLE Files (FileId INTEGER PRIMARY KEY, Data BLOB NOT NULL)";
        command.ExecuteNonQuery();
    }

    [GlobalCleanup]
    public void CloseConnection()
    {
        _connection.Dispose();
    }

    [IterationSetup]
    public void ClearTable()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "DELETE FROM Files";
        command.ExecuteNonQuery();
    }

    [Benchmark]
    public async Task OversizedByteArray()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "INSERT INTO Files (Data) VALUES (@Data)";

        var buffer = new byte[BlobSize + 1]; // add one to ensure the array size is different from the parameter size

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@Data";
        parameter.Size = BlobSize;
        parameter.Value = buffer;
        command.Parameters.Add(parameter);

        await command.ExecuteNonQueryAsync();
    }
}
