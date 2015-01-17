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
            IDbContextOptions options = new DbContextOptions();
            options.RawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "MaxBatchSize", "1" } };
            options.AddExtension(new SqlServerOptionsExtension());

            var batch = factory.Create(options);

            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator(), p => p.SqlServer())));
            Assert.False(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator(), p => p.SqlServer())));
        }

        [Fact]
        public void MaxBatchSize_is_optional()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator());
            IDbContextOptions options = new DbContextOptions();
            options.RawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            options.AddExtension(new SqlServerOptionsExtension());

            var batch = factory.Create(options);

            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator(), p => p.SqlServer())));
            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator(), p => p.SqlServer())));
        }

        [Fact]
        public void SqlServerOptionsExtension_is_optional()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator());
            IDbContextOptions options = new DbContextOptions();
            options.RawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var batch = factory.Create(options);

            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator(), p => p.SqlServer())));
            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator(), p => p.SqlServer())));
        }

        [Fact]
        public void MaxBatchSize_is_used_only_if_sqlServerOptionsExtension_is_registered()
        {
            var factory = new SqlServerModificationCommandBatchFactory(new SqlServerSqlGenerator());
            IDbContextOptions options = new DbContextOptions();
            options.RawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "MaxBatchSize", "1" } };

            var batch = factory.Create(options);

            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator(), p => p.SqlServer())));
            Assert.True(factory.AddCommand(batch, new ModificationCommand(new SchemaQualifiedName("T1"), new ParameterNameGenerator(), p => p.SqlServer())));
        }
    }
}
