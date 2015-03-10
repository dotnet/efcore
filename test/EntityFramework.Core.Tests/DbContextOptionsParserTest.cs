// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Framework.ConfigurationModel;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class DbContextOptionsParserTest
    {
        [Fact]
        public void Connection_string_is_found_using_context_name_generic()
        {
            var config = new Configuration
                (
                    new MemoryConfigurationSource
                        {
                            { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "MyConnectionString" }
                        }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);
            ;

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
        }

        [Fact]
        public void Connection_string_is_found_using_context_full_name_generic()
        {
            var config = new Configuration
                (
                    new MemoryConfigurationSource
                        {
                            { "EntityFramework:" + typeof(MyContext).FullName + ":ConnectionString", "MyConnectionString" }
                        }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
        }

        private class MyContext : DbContext
        {
        }

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
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
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
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
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
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
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
            Assert.Equal("Name=Data:DefaultConnection:ConnectionString;Key=Value", rawOptions["ConnectionString"]);
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
            Assert.Equal("Data Source=Data:DefaultConnection:ConnectionString", rawOptions["ConnectionString"]);
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
            Assert.Equal("=Data:DefaultConnection:ConnectionString", rawOptions["ConnectionString"]);
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
            Assert.Equal("=", rawOptions["ConnectionString"]);
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

            Assert.Equal(Strings.ConnectionStringNotFound("MyConnection"),
                Assert.Throws<InvalidOperationException>(() =>
                    new DbContextOptionsParser().ReadRawOptions<MyContext>(config)).Message);
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

            Assert.Equal(Strings.ConnectionStringNotFound(""),
                Assert.Throws<InvalidOperationException>(() =>
                    new DbContextOptionsParser().ReadRawOptions<MyContext>(config)).Message);
        }

        [Fact]
        public void Key_searching_is_case_insensitive()
        {
            var config = new Configuration
                (
                    new MemoryConfigurationSource
                        {
                            { "entityFramework:" + typeof(MyContext).Name + ":connectionString", "MyConnectionString" }
                        }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
        }

        [Fact]
        public void Nested_keys_are_read_using_context_name()
        {
            var config = new Configuration
                (
                    new MemoryConfigurationSource
                        {
                            { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "MyConnectionString" },
                            { "EntityFramework:" + typeof(MyContext).Name + ":SqlServer:MaxBatchSize", "1" },
                            { "EntityFramework:" + typeof(MyContext).Name + ":SqlServer:AnotherSqlServerOption", "SqlServerOptionValue" },
                            { "EntityFramework:" + typeof(MyContext).Name + ":SomeProvider:ProviderSpecificOption", "OptionValue" },
                            { "EntityFramework:" + typeof(MyContext).Name + ":Level1:Level2:Level3", "NestedLevelValue" }
                        }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(5, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
            Assert.Equal("1", rawOptions["SqlServer:MaxBatchSize"]);
            Assert.Equal("SqlServerOptionValue", rawOptions["SqlServer:AnotherSqlServerOption"]);
            Assert.Equal("OptionValue", rawOptions["SomeProvider:ProviderSpecificOption"]);
            Assert.Equal("NestedLevelValue", rawOptions["Level1:Level2:Level3"]);
        }

        [Fact]
        public void Nested_keys_are_read_using_context_full_name()
        {
            var config = new Configuration
                (
                    new MemoryConfigurationSource
                        {
                            { "EntityFramework:" + typeof(MyContext).FullName + ":ConnectionString", "MyConnectionString" },
                            { "EntityFramework:" + typeof(MyContext).FullName + ":SqlServer:MaxBatchSize", "1" },
                            { "EntityFramework:" + typeof(MyContext).FullName + ":SqlServer:AnotherSqlServerOption", "SqlServerOptionValue" },
                            { "EntityFramework:" + typeof(MyContext).FullName + ":SomeProvider:ProviderSpecificOption", "OptionValue" },
                            { "EntityFramework:" + typeof(MyContext).FullName + ":Level1:Level2:Level3", "NestedLevelValue" }
                        }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(5, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
            Assert.Equal("1", rawOptions["SqlServer:MaxBatchSize"]);
            Assert.Equal("SqlServerOptionValue", rawOptions["SqlServer:AnotherSqlServerOption"]);
            Assert.Equal("OptionValue", rawOptions["SomeProvider:ProviderSpecificOption"]);
            Assert.Equal("NestedLevelValue", rawOptions["Level1:Level2:Level3"]);
        }

        [Fact]
        public void Nested_keys_are_read_using_context_name_generic()
        {
            var config = new Configuration
                (
                    new MemoryConfigurationSource
                        {
                            { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "MyConnectionString" },
                            { "EntityFramework:" + typeof(MyContext).Name + ":SqlServer:MaxBatchSize", "1" },
                            { "EntityFramework:" + typeof(MyContext).Name + ":SqlServer:AnotherSqlServerOption", "SqlServerOptionValue" },
                            { "EntityFramework:" + typeof(MyContext).Name + ":SomeProvider:ProviderSpecificOption", "OptionValue" },
                            { "EntityFramework:" + typeof(MyContext).Name + ":Level1:Level2:Level3", "NestedLevelValue" }
                        }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(5, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
            Assert.Equal("1", rawOptions["SqlServer:MaxBatchSize"]);
            Assert.Equal("SqlServerOptionValue", rawOptions["SqlServer:AnotherSqlServerOption"]);
            Assert.Equal("OptionValue", rawOptions["SomeProvider:ProviderSpecificOption"]);
            Assert.Equal("NestedLevelValue", rawOptions["Level1:Level2:Level3"]);
        }

        [Fact]
        public void Nested_keys_are_read_using_context_full_name_generic()
        {
            var config = new Configuration
                (
                    new MemoryConfigurationSource
                        {
                            { "EntityFramework:" + typeof(MyContext).FullName + ":ConnectionString", "MyConnectionString" },
                            { "EntityFramework:" + typeof(MyContext).FullName + ":SqlServer:MaxBatchSize", "1" },
                            { "EntityFramework:" + typeof(MyContext).FullName + ":SqlServer:AnotherSqlServerOption", "SqlServerOptionValue" },
                            { "EntityFramework:" + typeof(MyContext).FullName + ":SomeProvider:ProviderSpecificOption", "OptionValue" },
                            { "EntityFramework:" + typeof(MyContext).FullName + ":Level1:Level2:Level3", "NestedLevelValue" }
                        }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(5, rawOptions.Count);
            Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
            Assert.Equal("1", rawOptions["SqlServer:MaxBatchSize"]);
            Assert.Equal("SqlServerOptionValue", rawOptions["SqlServer:AnotherSqlServerOption"]);
            Assert.Equal("OptionValue", rawOptions["SomeProvider:ProviderSpecificOption"]);
            Assert.Equal("NestedLevelValue", rawOptions["Level1:Level2:Level3"]);
        }

        [Fact]
        public void Uses_connection_string_from_last_loaded_configuration()
        {
            var iniConfigFileContent =
                @"[EntityFramework]
" + typeof(MyContext).Name + ":ConnectionString =IniConnectionString";
            var iniConfigFilePath = Path.GetTempFileName();
            File.WriteAllText(iniConfigFilePath, iniConfigFileContent);

            var memConfig = new MemoryConfigurationSource
                {
                    { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "MemoryConnectionString" }
                };

            var config = new Configuration();
            config.Add(memConfig);
            config.AddIniFile(iniConfigFilePath);

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("IniConnectionString", rawOptions["ConnectionString"]);
        }

        [Fact]
        public void For_same_key_context_full_name_takes_precedence_over_context_name()
        {
            var config = new Configuration
                (
                    new MemoryConfigurationSource
                        {
                            { "EntityFramework:" + typeof(MyContext).Name + ":ConnectionString", "ContextNameConnectionString" },
                            { "EntityFramework:" + typeof(MyContext).FullName + ":ConnectionString", "ContextFullNameConnectionString" }
                        }
                );

            var rawOptions = new DbContextOptionsParser().ReadRawOptions<MyContext>(config);

            Assert.Equal(1, rawOptions.Count);
            Assert.Equal("ContextFullNameConnectionString", rawOptions["ConnectionString"]);
        }
    }
}
