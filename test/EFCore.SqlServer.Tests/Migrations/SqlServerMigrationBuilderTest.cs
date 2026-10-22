// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

public class SqlServerMigrationBuilderTest
{
    [ConditionalFact]
    public void IsSqlServer_when_using_SqlServer()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        Assert.True(migrationBuilder.IsSqlServer());
    }

    [ConditionalFact]
    public void Not_IsSqlServer_when_using_different_provider()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.InMemory");
        Assert.False(migrationBuilder.IsSqlServer());
    }
}
