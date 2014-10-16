// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerDbContextOptionsExtensionsTest
    {
        [Fact]
        public void Can_add_extension_with_connection_string()
        {
            var options = new DbContextOptions();

            options = options.UseSqlServer("Database=Crunchie");

            var extension = ((IDbContextOptionsExtensions)options).Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Crunchie", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection_string_using_generic_options()
        {
            var options = new DbContextOptions<DbContext>();

            options = options.UseSqlServer("Database=Whisper");

            var extension = ((IDbContextOptionsExtensions)options).Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Whisper", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection()
        {
            var options = new DbContextOptions();
            var connection = new SqlConnection();

            options = options.UseSqlServer(connection);

            var extension = ((IDbContextOptionsExtensions)options).Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [Fact]
        public void Can_add_extension_with_connection_using_generic_options()
        {
            var options = new DbContextOptions<DbContext>();
            var connection = new SqlConnection();

            options = options.UseSqlServer(connection);

            var extension = ((IDbContextOptionsExtensions)options).Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [Fact]
        public void UseSqlServer_throws_if_options_are_locked()
        {
            var options = new DbContextOptions<DbContext>();
            options.Lock();

            Assert.Equal(
                TestHelpers.GetCoreString("FormatEntityConfigurationLocked", "UseSqlServer"),
                Assert.Throws<InvalidOperationException>(() => options.UseSqlServer("Database=DoubleDecker")).Message);

            Assert.Equal(
                TestHelpers.GetCoreString("FormatEntityConfigurationLocked", "UseSqlServer"),
                Assert.Throws<InvalidOperationException>(() => options.UseSqlServer(new SqlConnection())).Message);
        }

        [Fact]
        public void UseSqlServer_uses_connection_string_from_raw_options()
        {
            var options = new DbContextOptions();
            options.RawOptions.Add("ConnectionString", "Database=Crunchie");

            options = options.UseSqlServer();

            var extension = ((IDbContextOptionsExtensions)options).Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Crunchie", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }
    }
}
