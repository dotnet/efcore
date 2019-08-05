// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqliteMigrationBuilderTest
    {
        [Fact]
        public void IsSqlite_when_using_Sqlite()
        {
            var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.Sqlite");
            Assert.True(migrationBuilder.IsSqlite());
        }

        [Fact]
        public void Not_IsSqlite_when_using_different_provider()
        {
            var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.InMemory");
            Assert.False(migrationBuilder.IsSqlite());
        }

    }
}
