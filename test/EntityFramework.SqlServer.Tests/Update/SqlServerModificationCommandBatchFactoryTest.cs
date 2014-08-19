// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Framework.ConfigurationModel;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Update
{
    public class SqlServerModificationCommandBatchFactoryTest
    {
        [Fact]
        public void Uses_MaxBatchSize_specified_in_configuration()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource(
                        new Dictionary<string, string>
                            {
                                { "Data:SqlServer:MaxBatchSize", "1" }
                            })
                };

            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator(), new[] { configuration });
            
            var batch = factory.Create();
            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator())));
            Assert.False(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator())));
        }

        [Fact]
        public void MaxBatchSize_configuration_is_optional()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource()
                };

            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator(), new[] { configuration });

            var batch = factory.Create();
            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator())));
            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator())));
        }

        [Fact]
        public void Configuration_can_be_empty()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator(), new IConfiguration[0]);

            var batch = factory.Create();
            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator())));
            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator())));
        }

        [Fact]
        public void Configuration_can_be_null()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator(), null);

            var batch = factory.Create();
            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator())));
            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator())));
        }

        [Fact]
        public void Throws_on_invalid_MaxBatchSize_specified_in_configuration()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource(
                        new Dictionary<string, string>
                            {
                                { "Data:SqlServer:MaxBatchSize", "one" }
                            })
                };

            Assert.Equal(Strings.FormatIntegerConfigurationValueFormatError("Data:SqlServer:MaxBatchSize", "one"),
                Assert.Throws<InvalidOperationException>(() => new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator(), new[] { configuration })).Message);
        }
    }
}
