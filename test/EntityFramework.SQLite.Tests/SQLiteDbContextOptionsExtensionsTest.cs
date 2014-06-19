// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.SQLite;
using Xunit;

namespace Microsoft.Data.Entity.Tests
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

        [Fact]
        public void UseSQLite_throws_if_options_are_locked()
        {
            var options = new DbContextOptions<DbContext>();
            options.Lock();

            Assert.Equal(
                GetString("FormatEntityConfigurationLocked", "UseSQLite"),
                Assert.Throws<InvalidOperationException>(() => options.UseSQLite("Database=DoubleDecker")).Message);
        }

        private static string GetString(string stringName, params object[] parameters)
        {
            var strings = typeof(DbContext).GetTypeInfo().Assembly.GetType(typeof(DbContext).Namespace + ".Strings");
            return (string)strings.GetTypeInfo().GetDeclaredMethods(stringName).Single().Invoke(null, parameters);
        }
    }
}
