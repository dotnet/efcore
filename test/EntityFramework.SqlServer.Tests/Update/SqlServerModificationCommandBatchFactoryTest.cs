// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Query;
using Microsoft.Data.Entity.SqlServer.Update;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Update
{
    public class SqlServerModificationCommandBatchFactoryTest
    {
        [Fact]
        public void Uses_MaxBatchSize_specified_in_SqlServerOptionsExtension()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator());

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie").MaxBatchSize(1);

            var batch = factory.Create(optionsBuilder.Options);

            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new BoxedValueReaderSource(), new SqlServerValueReaderFactoryFactory())));
            Assert.False(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new BoxedValueReaderSource(), new SqlServerValueReaderFactoryFactory())));
        }

        [Fact]
        public void MaxBatchSize_is_optional()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator());

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie");

            var batch = factory.Create(optionsBuilder.Options);

            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new BoxedValueReaderSource(), new SqlServerValueReaderFactoryFactory())));
            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new BoxedValueReaderSource(), new SqlServerValueReaderFactoryFactory())));
        }

        [Fact]
        public void SqlServerOptionsExtension_is_optional()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator());

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie");

            var batch = factory.Create(optionsBuilder.Options);

            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new BoxedValueReaderSource(), new SqlServerValueReaderFactoryFactory())));
            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new BoxedValueReaderSource(), new SqlServerValueReaderFactoryFactory())));
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
