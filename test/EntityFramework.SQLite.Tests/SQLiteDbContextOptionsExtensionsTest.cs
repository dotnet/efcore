// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Xunit;

namespace Microsoft.Data.Entity.SQLite.Tests
{
    public class SQLiteDbContextOptionsExtensionsTest
    {
        [Fact]
        public void Can_add_extension_with_connection_string()
        {
            var options = new DbContextOptions();

            options = options.UseSQLite("Database=Crunchie");

            var extension = ((IDbContextOptionsExtensions)options).Extensions.OfType<SQLiteOptionsExtension>().Single();

            Assert.Equal("Database=Crunchie", extension.ConnectionString);
        }

        [Fact]
        public void Can_add_extension_with_connection_string_using_generic_options()
        {
            var options = new DbContextOptions<DbContext>();

            options = options.UseSQLite("Database=Whisper");

            var extension = ((IDbContextOptionsExtensions)options).Extensions.OfType<SQLiteOptionsExtension>().Single();

            Assert.Equal("Database=Whisper", extension.ConnectionString);
        }
    }
}
