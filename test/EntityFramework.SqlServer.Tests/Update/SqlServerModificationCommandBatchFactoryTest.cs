// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Update.Internal;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Update
{
    public class SqlServerModificationCommandBatchFactoryTest
    {
        [Fact]
        public void Uses_MaxBatchSize_specified_in_SqlServerOptionsExtension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie").MaxBatchSize(1);

            var factory = new SqlServerModificationCommandBatchFactory(
                new RelationalCommandBuilderFactory(new SqlServerTypeMapper()),
                new SqlServerSqlGenerator(),
                new SqlServerUpdateSqlGenerator(new SqlServerSqlGenerator()),
                new UntypedRelationalValueBufferFactoryFactory(),
                optionsBuilder.Options);

            var batch = factory.Create();

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer())));
            Assert.False(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer())));
        }

        [Fact]
        public void MaxBatchSize_is_optional()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie");

            var factory = new SqlServerModificationCommandBatchFactory(
                new RelationalCommandBuilderFactory(new SqlServerTypeMapper()),
                new SqlServerSqlGenerator(),
                new SqlServerUpdateSqlGenerator(new SqlServerSqlGenerator()),
                new UntypedRelationalValueBufferFactoryFactory(),
                optionsBuilder.Options);

            var batch = factory.Create();

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer())));
            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer())));
        }
    }
}
