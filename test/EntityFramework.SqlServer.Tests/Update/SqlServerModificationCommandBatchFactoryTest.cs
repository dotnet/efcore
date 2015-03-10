// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
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
            optionsBuilder.UseSqlServer().MaxBatchSize(1);

            var batch = factory.Create(optionsBuilder.Options);

            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer())));
            Assert.False(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer())));
        }

        [Fact]
        public void MaxBatchSize_is_optional()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator());

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer();

            var batch = factory.Create(optionsBuilder.Options);

            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer())));
            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer())));
        }

        [Fact]
        public void SqlServerOptionsExtension_is_optional()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator());

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer();

            var batch = factory.Create(optionsBuilder.Options);

            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer())));
            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer())));
        }

        [Fact]
        public void MaxBatchSize_is_used_only_if_sqlServerOptionsExtension_is_registered()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator());

            var rawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "MaxBatchSize", "1" } };
            var optionsExtension = new TestRelationalOptionsExtension(
                new DbContextOptions<DbContext>(rawOptions, new Dictionary<Type, IDbContextOptionsExtension>()));

            var optionsBuilder = new DbContextOptionsBuilder();
            ((IOptionsBuilderExtender)optionsBuilder).AddOrUpdateExtension(optionsExtension);

            var batch = factory.Create(optionsBuilder.Options);

            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer())));
            Assert.True(factory.AddCommand(batch, new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer())));
        }

        private class TestRelationalOptionsExtension : RelationalOptionsExtension
        {
            public TestRelationalOptionsExtension(IDbContextOptions options)
                : base(options)
            {
            }

            public override void ApplyServices(EntityFrameworkServicesBuilder builder)
            {
                throw new NotImplementedException();
            }
        }
    }
}
