// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Extensions
{
    public class SqliteEntityOptionsBuilderExtensionsTest
    {
        [Fact]
        public void Can_add_extension_with_connection_string()
        {
            var optionsBuilder = new EntityOptionsBuilder();
            optionsBuilder.UseSqlite("Database=Crunchie");

            var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

            Assert.Equal("Database=Crunchie", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection_string_using_generic_options()
        {
            var optionsBuilder = new EntityOptionsBuilder<DbContext>();
            optionsBuilder.UseSqlite("Database=Whisper");

            var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

            Assert.Equal("Database=Whisper", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection()
        {
            var optionsBuilder = new EntityOptionsBuilder();
            var connection = new SqliteConnection();

            optionsBuilder.UseSqlite(connection);

            var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [Fact]
        public void Can_add_extension_with_connection_using_generic_options()
        {
            var optionsBuilder = new EntityOptionsBuilder<DbContext>();
            var connection = new SqliteConnection();

            optionsBuilder.UseSqlite(connection);

            var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }
    }
}
