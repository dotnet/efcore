// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.Storage;

public class SqliteRelationalConnectionTest
{
    [ConditionalFact]
    public void Sets_DefaultTimeout_when_connectionString()
    {
        var services = SqliteTestHelpers.Instance.CreateContextServices(
            new DbContextOptionsBuilder()
                .UseSqlite("Data Source=:memory:", x => x.CommandTimeout(42))
                .Options);

        var connection = (SqliteConnection)services.GetRequiredService<IRelationalConnection>().DbConnection;

        Assert.Equal(42, connection.DefaultTimeout);
    }

    [ConditionalFact]
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
}
