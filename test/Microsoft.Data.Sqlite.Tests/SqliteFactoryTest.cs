// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.Data.Sqlite;

public class SqliteFactoryTest
{
    [Fact]
    public void CreateConnection_works()
        => Assert.IsType<SqliteConnection>(SqliteFactory.Instance.CreateConnection());

    [Fact]
    public void CreateConnectionStringBuilder_works()
        => Assert.IsType<SqliteConnectionStringBuilder>(SqliteFactory.Instance.CreateConnectionStringBuilder());

    [Fact]
    public void CreateCommand_works()
        => Assert.IsType<SqliteCommand>(SqliteFactory.Instance.CreateCommand());

    [Fact]
    public void CreateParameter_works()
        => Assert.IsType<SqliteParameter>(SqliteFactory.Instance.CreateParameter());
}
