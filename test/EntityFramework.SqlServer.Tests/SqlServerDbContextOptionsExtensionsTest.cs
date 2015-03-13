// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Framework.ConfigurationModel;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerDbContextOptionsExtensionsTest
    {
        [Fact]
        public void Can_add_extension_with_max_batch_size()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer().MaxBatchSize(123);

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal(123, extension.MaxBatchSize);
        }

        [Fact]
        public void Can_add_extension_with_connection_string()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie");

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Crunchie", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection_string_using_generic_options()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            optionsBuilder.UseSqlServer("Database=Whisper");

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Whisper", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_redirected_connection_string()
        {
            var config = new Configuration
                (
                new MemoryConfigurationSource
                    {
                        { "Foo:Bar:Aero", "Database=Whisper" }
                    }
                );

            var optionsBuilder = new DbContextOptionsBuilder(
                new DbContextOptions<DbContext>(new Dictionary<string, string>(), new Dictionary<Type, IDbContextOptionsExtension>(), config));

            optionsBuilder.UseSqlServer("name=Foo:Bar:Aero");

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Whisper", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection_string_read_from_config_by_convention()
        {
            var config = new Configuration
                (
                new MemoryConfigurationSource
                    {
                        { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "Database=Whisper" }
                    }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            var optionsBuilder = new DbContextOptionsBuilder(
                new DbContextOptions<DbContext>(rawOptions, new Dictionary<Type, IDbContextOptionsExtension>(), config));

            optionsBuilder.UseSqlServer();

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Whisper", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_redirected_connection_string_read_from_config_by_convention()
        {
            var config = new Configuration
                (
                new MemoryConfigurationSource
                    {
                        { "Foo:Bar:Aero", "Database=Whisper" },
                        { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "name=Foo:Bar:Aero" }
                    }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            var optionsBuilder = new DbContextOptionsBuilder(
                new DbContextOptions<DbContext>(rawOptions, new Dictionary<Type, IDbContextOptionsExtension>(), config));

            optionsBuilder.UseSqlServer();

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Whisper", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            var connection = new SqlConnection();

            optionsBuilder.UseSqlServer(connection);

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [Fact]
        public void Can_add_extension_with_connection_using_generic_options()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            var connection = new SqlConnection();

            optionsBuilder.UseSqlServer(connection);

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [Fact]
        public void UseSqlServer_uses_connection_string_from_raw_options()
        {
            var optionsBuilder = new DbContextOptionsBuilder(
                new DbContextOptions<DbContext>(
                    new Dictionary<string, string> { { "ConnectionString", "Database=Crunchie" } },
                    new Dictionary<Type, IDbContextOptionsExtension>(),
                    null));

            optionsBuilder.UseSqlServer();

            var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal("Database=Crunchie", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        private class MyContext : DbContext
        {
        }
    }
}
