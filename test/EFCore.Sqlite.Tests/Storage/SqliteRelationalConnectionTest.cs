// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.Storage;

public class SqliteRelationalConnectionTest
{
    [Fact]
    public void Sets_DefaultTimeout_when_connectionString()
    {
        var services = SqliteTestHelpers.Instance.CreateContextServices(
            new DbContextOptionsBuilder()
                .UseSqlite("Data Source=:memory:", x => x.CommandTimeout(42))
                .Options);

        var connection = (SqliteConnection)services.GetRequiredService<IRelationalConnection>().DbConnection;

        Assert.Equal(42, connection.DefaultTimeout);
    }

    [Fact]
    public void Sets_DefaultTimeout_when_connection()
    {
        var originalConnection = new SqliteConnection("Data Source=:memory:") { DefaultTimeout = 21 };
        var services = SqliteTestHelpers.Instance.CreateContextServices(
            new DbContextOptionsBuilder()
                .UseSqlite(originalConnection, x => x.CommandTimeout(42))
                .Options);

        var connection = (SqliteConnection)services.GetRequiredService<IRelationalConnection>().DbConnection;

        Assert.Same(originalConnection, connection);
        Assert.Equal(42, originalConnection.DefaultTimeout);
    }

    [Fact]
    public void Registers_builtin_functions_as_deterministic()
    {
        var services = SqliteTestHelpers.Instance.CreateContextServices(
            new DbContextOptionsBuilder()
                .UseSqlite("Data Source=:memory:")
                .Options);

        var relationalConnection = services.GetRequiredService<IRelationalConnection>();
        var connection = (SqliteConnection)relationalConnection.DbConnection;
        relationalConnection.Open();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE Data (Value TEXT); CREATE INDEX IX_Data ON Data (Value) WHERE Value REGEXP '^a';";

            Assert.Equal(0, command.ExecuteNonQuery());
        }
        finally
        {
            relationalConnection.Close();
        }
    }
}
