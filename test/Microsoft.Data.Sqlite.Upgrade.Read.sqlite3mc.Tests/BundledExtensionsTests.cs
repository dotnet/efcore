// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Microsoft.Data.Sqlite.Upgrade.Tests;

// Same probes as the seed project, but executed against SQLite3MC.PCLRaw.bundle.
// Comparing pass/fail with the seed project documents which extensions become
// available after upgrading from e_sqlite3 to SQLite3MC.
public class BundledExtensionsTests
{
    static BundledExtensionsTests()
    {
        SQLitePCL.Batteries_V2.Init();
    }

    private static SqliteConnection OpenInMemory()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
    }

    [Fact]
    public void Sha1_function()
    {
        using var connection = OpenInMemory();
        var hash = connection.ExecuteScalar<string>("SELECT sha1('abc');");
        Assert.Equal("a9993e364706816aba3e25717850c26c9cd0d89d", hash);
    }

    [Fact]
    public void Sha3_function()
    {
        using var connection = OpenInMemory();
        var hash = connection.ExecuteScalar<string>("SELECT lower(hex(sha3('abc')));");
        Assert.Equal("3a985da74fe225b2045c172d6bd390bd855f086e3e9d525b46bfe24511431532", hash);
    }

    [Fact]
    public void Tointeger_function()
    {
        using var connection = OpenInMemory();
        Assert.Equal(123L, connection.ExecuteScalar<long>("SELECT tointeger('123');"));

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT tointeger('not-a-number');";
        Assert.Null(command.ExecuteScalar());
    }

    [Fact]
    public void Toreal_function()
    {
        using var connection = OpenInMemory();
        Assert.Equal(1.5, connection.ExecuteScalar<double>("SELECT toreal('1.5');"));

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT toreal('not-a-number');";
        Assert.Null(command.ExecuteScalar());
    }

    [Fact]
    public void Percentile_function()
    {
        using var connection = OpenInMemory();
        connection.ExecuteNonQuery(
            """
            CREATE TABLE T (V REAL NOT NULL);
            INSERT INTO T (V) VALUES (1), (2), (3), (4), (5);
            """);

        var median = connection.ExecuteScalar<double>("SELECT percentile(V, 50) FROM T;");
        Assert.Equal(3.0, median);
    }

    [Fact]
    public void Compress_function_roundtrips()
    {
        using var connection = OpenInMemory();

        var input = new string('a', 1024);
        connection.ExecuteNonQuery("CREATE TABLE T (V BLOB NOT NULL);");
        connection.ExecuteNonQuery(
            "INSERT INTO T (V) VALUES (compress($v));",
            new SqliteParameter("$v", input));

        var compressedLength = connection.ExecuteScalar<long>("SELECT length(V) FROM T;");
        Assert.True(
            compressedLength < input.Length,
            $"Expected compressed length < {input.Length} but was {compressedLength}.");

        var roundTripped = connection.ExecuteScalar<string>("SELECT CAST(uncompress(V) AS TEXT) FROM T;");
        Assert.Equal(input, roundTripped);
    }

    [Fact]
    public void Zipfile_vtable_module_is_registered()
    {
        using var connection = OpenInMemory();
        var count = connection.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM pragma_module_list WHERE name = 'zipfile';");
        Assert.Equal(1, count);
    }

    [Fact]
    public void Vtshim_is_compiled_in()
    {
        using var connection = OpenInMemory();
        var count = connection.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM pragma_compile_options WHERE compile_options LIKE '%VTSHIM%';");
        Assert.True(count >= 1, "Expected at least one compile option matching '%VTSHIM%'.");
    }

    [Fact]
    public void User_defined_function_works()
    {
        using var connection = OpenInMemory();
        connection.CreateFunction<long, long, long>("add_one", (a, b) => a + b + 1);

        var result = connection.ExecuteScalar<long>("SELECT add_one(2, 3);");
        Assert.Equal(6, result);
    }
}
