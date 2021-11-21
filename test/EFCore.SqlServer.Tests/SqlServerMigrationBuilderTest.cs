// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests;

public class SqlServerMigrationBuilderTest
{
    [Fact]
    public void IsSqlServer_when_using_SqlServer()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        Assert.True(migrationBuilder.IsSqlServer());
    }

    [Fact]
    public void Not_IsSqlServer_when_using_different_provider()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.InMemory");
        Assert.False(migrationBuilder.IsSqlServer());
    }
}
