// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

public class SqliteMigrationBuilderTest
{
    [ConditionalFact]
    public void IsSqlite_when_using_Sqlite()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.Sqlite");
        Assert.True(migrationBuilder.IsSqlite());
    }

    [ConditionalFact]
    public void Not_IsSqlite_when_using_different_provider()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.InMemory");
        Assert.False(migrationBuilder.IsSqlite());
    }
}
