// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Framework.ConfigurationModel;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityFrameworkConfigurationExtensionsTest
    {
        [Fact]
        public void Indirect_connection_string_can_be_specified_with_name()
        {
            var config = new Configuration
                (
                new MemoryConfigurationSource
                    {
                        { "Data:DefaultConnection:ConnectionString", "MyConnectionString" },
                        { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "Name=Data:DefaultConnection:ConnectionString" }
                    }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", config.ResolveConnectionString(config.ResolveConnectionString(rawOptions["ConnectionString"])));
        }

        [Fact]
        public void Indirect_connection_string_is_read_case_insensitively()
        {
            var config = new Configuration
                (
                new MemoryConfigurationSource
                    {
                        { "Data:DefaultConnection:ConnectionString", "MyConnectionString" },
                        { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "name=Data:DefaultConnection:ConnectionString" }
                    }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", config.ResolveConnectionString(config.ResolveConnectionString(rawOptions["ConnectionString"])));
        }

        [Fact]
        public void Indirect_connection_string_is_read_removing_whitespaces()
        {
            var config = new Configuration
                (
                new MemoryConfigurationSource
                    {
                        { "Data:DefaultConnection:ConnectionString", "MyConnectionString" },
                        { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "name = Data:DefaultConnection:ConnectionString" }
                    }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", config.ResolveConnectionString(rawOptions["ConnectionString"]));
        }

        [Fact]
        public void Indirect_connection_string_must_have_only_one_equal_sign()
        {
            var config = new Configuration
                (
                new MemoryConfigurationSource
                    {
                        { "Data:DefaultConnection:ConnectionString", "MyConnectionString" },
                        { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "Name=Data:DefaultConnection:ConnectionString;Key=Value" }
                    }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("Name=Data:DefaultConnection:ConnectionString;Key=Value", config.ResolveConnectionString(rawOptions["ConnectionString"]));
        }

        [Fact]
        public void Single_key_value_pair_does_not_cause_redirection_if_key_is_not_name()
        {
            var config = new Configuration
                (
                new MemoryConfigurationSource
                    {
                        { "Data:DefaultConnection:ConnectionString", "MyConnectionString" },
                        { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "Data Source=Data:DefaultConnection:ConnectionString" }
                    }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("Data Source=Data:DefaultConnection:ConnectionString", config.ResolveConnectionString(rawOptions["ConnectionString"]));
        }

        [Fact]
        public void Connection_string_starting_with_equal_sign_does_not_cause_redirection()
        {
            var config = new Configuration
                (
                new MemoryConfigurationSource
                    {
                        { "Data:DefaultConnection:ConnectionString", "MyConnectionString" },
                        { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "=Data:DefaultConnection:ConnectionString" }
                    }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("=Data:DefaultConnection:ConnectionString", config.ResolveConnectionString(rawOptions["ConnectionString"]));
        }

        [Fact]
        public void Equal_sign_as_connection_string_does_not_cause_redirection()
        {
            var config = new Configuration
                (
                new MemoryConfigurationSource
                    {
                        { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "=" }
                    }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("=", config.ResolveConnectionString(rawOptions["ConnectionString"]));
        }

        [Fact]
        public void Throws_when_indirect_connection_string_is_not_found()
        {
            var config = new Configuration
                (
                new MemoryConfigurationSource
                    {
                        { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "Name=MyConnection" }
                    }
                );

            var connectionString = new DbContextOptionsParser().ReadRawOptions<MyContext>(config)["ConnectionString"];

            Assert.Equal(Strings.ConnectionStringNotFound("MyConnection"),
                Assert.Throws<InvalidOperationException>(() =>
                    config.ResolveConnectionString(connectionString)).Message);
        }

        [Fact]
        public void Throws_when_indirect_connection_string_is_empty()
        {
            var config = new Configuration
                (
                new MemoryConfigurationSource
                    {
                        { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "Name=" }
                    }
                );

            var connectionString = new DbContextOptionsParser().ReadRawOptions<MyContext>(config)["ConnectionString"];

            Assert.Equal(Strings.ConnectionStringNotFound(""),
                Assert.Throws<InvalidOperationException>(() =>
                    config.ResolveConnectionString(connectionString)).Message);
        }

        [Fact]
        public void Throws_when_indirect_connection_string_is_used_without_configuration()
        {
            Assert.Equal(Strings.ConnectionStringNotFound("Aero"),
                Assert.Throws<InvalidOperationException>(() =>
                    EntityFrameworkConfigurationExtensions.ResolveConnectionString(null, "name=Aero")).Message);
        }

        private class MyContext : DbContext
        {
        }
    }
}
