// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Update
{
    public class SqlServerModificationCommandBatchFactoryTest
    {
        [Fact]
        public void Uses_MaxBatchSize_specified_in_SqlServerOptionsExtension()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerUpdateSqlGenerator());

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie").MaxBatchSize(1);

            var batch = factory.Create(optionsBuilder.Options, new SqlServerMetadataExtensionProvider());

            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new UntypedValueBufferFactoryFactory())));
            Assert.False(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new UntypedValueBufferFactoryFactory())));
        }

        [Fact]
        public void MaxBatchSize_is_optional()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerUpdateSqlGenerator());

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie");

            var batch = factory.Create(optionsBuilder.Options, new SqlServerMetadataExtensionProvider());

            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new UntypedValueBufferFactoryFactory())));
            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new UntypedValueBufferFactoryFactory())));
        }

        [Fact]
        public void SqlServerOptionsExtension_is_optional()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerUpdateSqlGenerator());

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie");

            var batch = factory.Create(optionsBuilder.Options, new SqlServerMetadataExtensionProvider());

            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new UntypedValueBufferFactoryFactory())));
            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new UntypedValueBufferFactoryFactory())));
        }

        private class TestRelationalOptionsExtension : RelationalOptionsExtension
        {
            public override void ApplyServices(EntityFrameworkServicesBuilder builder)
            {
                throw new NotImplementedException();
            }
        }
    }
}
