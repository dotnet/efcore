// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.ConfigurationModel;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class DbContextOptionsParserTest
    {
        [Fact]
        public void Connection_string_is_found_using_context_name()
        {
            var config = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "MyConnectionString" }
                        }
                };

            var rawOptions = new DbContextOptionsParser().ReadRawOptions(config, typeof(MyContext), new Dictionary<string, string>());

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
        }

        [Fact]
        public void Connection_string_is_found_using_context_full_name()
        {
            var config = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            { "EntityFramework:" + typeof(MyContext).FullName + ":ConnectionString", "MyConnectionString" }
                        }
                };

            var rawOptions = new DbContextOptionsParser().ReadRawOptions(config, typeof(MyContext), new Dictionary<string, string>());

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
        }

        [Fact]
        public void Connection_string_is_found_using_context_name_generic()
        {
            var config = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "MyConnectionString" }
                        }
                };

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config, new Dictionary<string, string>());

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
        }

        [Fact]
        public void Connection_string_is_found_using_context_full_name_generic()
        {
            var config = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            { "EntityFramework:" + typeof(MyContext).FullName + ":ConnectionString", "MyConnectionString" }
                        }
                };

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config, new Dictionary<string, string>());

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
        }

        private class MyContext : DbContext
        {
        }

        [Fact]
        public void Indirect_connection_string_is_found_using_context_name()
        {
            var config = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            { "Data:DefaultConnection:ConnectionString", "MyConnectionString" },
                            { "EntityFramework:" + typeof(MyContext).FullName + ":ConnectionStringKey", "Data:DefaultConnection:ConnectionString" }
                        }
                };

            var rawOptions = new DbContextOptionsParser().ReadRawOptions(config, typeof(MyContext), new Dictionary<string, string>());

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
        }

        [Fact]
        public void Indirect_connection_string_is_found_using_context_full_name()
        {
            var config = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            { "Data:DefaultConnection:ConnectionString", "MyConnectionString" },
                            { "EntityFramework:" + typeof(MyContext).FullName + ":ConnectionStringKey", "Data:DefaultConnection:ConnectionString" }
                        }
                };

            var rawOptions = new DbContextOptionsParser().ReadRawOptions(config, typeof(MyContext), new Dictionary<string, string>());

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
        }

        [Fact]
        public void Indirect_connection_string_is_found_using_context_name_generic()
        {
            var config = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            { "Data:DefaultConnection:ConnectionString", "MyConnectionString" },
                            { "EntityFramework:" + typeof(MyContext).FullName + ":ConnectionStringKey", "Data:DefaultConnection:ConnectionString" }
                        }
                };

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config, new Dictionary<string, string>());

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
        }

        [Fact]
        public void Indirect_connection_string_is_found_using_context_full_name_generic()
        {
            var config = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            { "Data:DefaultConnection:ConnectionString", "MyConnectionString" },
                            { "EntityFramework:" + typeof(MyContext).FullName + ":ConnectionStringKey", "Data:DefaultConnection:ConnectionString" }
                        }
                };

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config, new Dictionary<string, string>());

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
        }

        [Fact]
        public void Existing_options_are_updated()
        {
            var config = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            { "EntityFramework:" + typeof(MyContext).FullName + ":ConnectionString", "MyConnectionString" }
                        }
                };

            var currentOptions = new Dictionary<string, string> { { "Foo", "Goo" } };

            var rawOptions = new DbContextOptionsParser().ReadRawOptions(config, typeof(MyContext), currentOptions);

            Assert.Equal(2, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
            Assert.Equal("Goo", rawOptions["Foo"]);
        }

        [Fact]
        public void Existing_options_are_updated_generic()
        {
            var config = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            { "EntityFramework:" + typeof(MyContext).FullName + ":ConnectionString", "MyConnectionString" }
                        }
                };

            var currentOptions = new Dictionary<string, string> { { "Foo", "Goo" } };

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config, currentOptions);

            Assert.Equal(2, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
            Assert.Equal("Goo", rawOptions["Foo"]);
        }
    }
}
