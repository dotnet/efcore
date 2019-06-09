// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqliteDbContextOptionsBuilderExtensionsTest
    {
        [ConditionalFact]
        public void Can_add_extension_with_max_batch_size()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite("Database=Crunchie", b => b.MaxBatchSize(123));

            var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

            Assert.Equal(123, extension.MaxBatchSize);
        }

        [ConditionalFact]
        public void Can_add_extension_with_command_timeout()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite("Database=Crunchie", b => b.CommandTimeout(30));

            var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

            Assert.Equal(30, extension.CommandTimeout);
        }

        [ConditionalFact]
        public void Can_add_extension_with_connection_string()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite("Database=Crunchie");

            var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

            Assert.Equal("Database=Crunchie", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [ConditionalFact]
        public void Can_add_extension_with_connection_string_using_generic_options()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            optionsBuilder.UseSqlite("Database=Whisper");

            var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

            Assert.Equal("Database=Whisper", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [ConditionalFact]
        public void Can_add_extension_with_connection()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            var connection = new SqliteConnection();

            optionsBuilder.UseSqlite(connection);

            var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [ConditionalFact]
        public void Can_add_extension_with_connection_using_generic_options()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            var connection = new SqliteConnection();

            optionsBuilder.UseSqlite(connection);

            var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }
    }
}
