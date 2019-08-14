// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests
{
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
}
