// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Framework.ConfigurationModel;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class ConnectionStringResolverTest
    {
        [Fact]
        public void Connection_string_with_multiple_keys_is_returned_unchanged_and_does_not_require_configuration()
        {
            const string connectionString = "Name=Lobsang;Database=DatumEarth";

            Assert.Equal(connectionString, new ConnectionStringResolver(null).Resolve(connectionString));
        }

        [Fact]
        public void Connection_string_with_single_non_name_key_is_returned_unchanged_and_does_not_require_configuration()
        {
            const string connectionString = "Database=DatumEarth";

            Assert.Equal(connectionString, new ConnectionStringResolver(null).Resolve(connectionString));
        }


        [Fact]
        public void Just_name_is_used_as_path_to_configuration_entry()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource(
                        new Dictionary<string, string>
                            {
                                { "Lobsang", "Database=DatumEarth" },
                                { "Data:Lobsang:ConnectionString", "Database=Rectangles" }
                            })
                };

            Assert.Equal("Database=DatumEarth", new ConnectionStringResolver(configuration).Resolve("Lobsang"));
        }

        [Fact]
        public void Just_name_is_used_as_name_in_convention_path_to_entry()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource(
                        new Dictionary<string, string>
                            {
                                { "Data:Lobsang:ConnectionString", "Database=Rectangles" }
                            })
                };

            Assert.Equal("Database=Rectangles", new ConnectionStringResolver(configuration).Resolve("Lobsang"));
        }

        [Fact]
        public void Name_equals_syntax_is_used_as_path_to_configuration_entry()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource(
                        new Dictionary<string, string>
                            {
                                { "Lobsang", "Database=DatumEarth" },
                                { "Data:Lobsang:ConnectionString", "Database=Rectangles" }
                            })
                };

            Assert.Equal("Database=DatumEarth", new ConnectionStringResolver(configuration).Resolve("name=Lobsang"));
        }

        [Fact]
        public void Name_equals_syntax_is_used_as_name_in_convention_path_to_entry()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource(
                        new Dictionary<string, string>
                            {
                                { "Data:Lobsang:ConnectionString", "Database=Rectangles" }
                            })
                };

            Assert.Equal("Database=Rectangles", new ConnectionStringResolver(configuration).Resolve("name=Lobsang"));
        }

        [Fact]
        public void Throws_if_name_is_used_but_there_is_no_configuration_available()
        {
            Assert.Equal(
                Strings.FormatNoConfigForConnection("Lobsang"),
                Assert.Throws<InvalidOperationException>(() => new ConnectionStringResolver(null).Resolve("name=Lobsang")).Message);
        }

        [Fact]
        public void Throws_if_name_cannot_be_found()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource(
                        new Dictionary<string, string>
                            {
                                { "Data:Joshua:ConnectionString", "Database=Rectangles" }
                            })
                };

            Assert.Equal(
                Strings.FormatConnectionNotFound("Lobsang"),
                Assert.Throws<InvalidOperationException>(() => new ConnectionStringResolver(configuration).Resolve("name=Lobsang")).Message);
        }
    }
}
