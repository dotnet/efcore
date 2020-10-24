// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
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
        public void Sets_DefaultTimeout_when_connection_overrides_connection_string()
        {
            var originalConnection = new SqliteConnection("Data Source=:memory:;Default Timeout=50") { DefaultTimeout = 21 };
            Assert.Equal(21, originalConnection.DefaultTimeout);

            var services = SqliteTestHelpers.Instance.CreateContextServices(
                new DbContextOptionsBuilder()
                    .UseSqlite(originalConnection, x => x.CommandTimeout(42))
                    .Options);

            Assert.Equal(42, originalConnection.DefaultTimeout);
        }

        [Fact]
        public void Sets_DefaultTimeout_when_connection_string()
        {
            var originalConnection = new SqliteConnection("Data Source=:memory:;Default Timeout=50");
            Assert.Equal(50, originalConnection.DefaultTimeout);
        }
    }
}
