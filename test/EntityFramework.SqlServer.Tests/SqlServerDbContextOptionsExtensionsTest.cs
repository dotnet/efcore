// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerDbContextOptionsExtensionsTest
    {
        [Fact]
        public void Can_add_extension_with_connection_string()
        {
            var options = new DbContextOptions();
            options.UseSqlServer("Database=Crunchie");

            var extension = ((IDbContextOptions)options).Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Crunchie", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection_string_using_generic_options()
        {
            var options = new DbContextOptions<DbContext>();
            options.UseSqlServer("Database=Whisper");

            var extension = ((IDbContextOptions)options).Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Whisper", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection()
        {
            var options = new DbContextOptions();
            var connection = new SqlConnection();

            options.UseSqlServer(connection);

            var extension = ((IDbContextOptions)options).Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [Fact]
        public void Can_add_extension_with_connection_using_generic_options()
        {
            var options = new DbContextOptions<DbContext>();
            var connection = new SqlConnection();

            options.UseSqlServer(connection);

            var extension = ((IDbContextOptions)options).Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [Fact]
        public void UseSqlServer_uses_connection_string_from_raw_options()
        {
            var options = new DbContextOptions();
            ((IDbContextOptions)options).RawOptions = new Dictionary<string, string> { { "ConnectionString", "Database=Crunchie" } };

            options.UseSqlServer();

            var extension = ((IDbContextOptions)options).Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Crunchie", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }
    }
}
