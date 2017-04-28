// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests.Update
{
    public class SqlServerModificationCommandBatchFactoryTest
    {
        [Fact]
        public void Uses_MaxBatchSize_specified_in_SqlServerOptionsExtension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie", b => b.MaxBatchSize(1));

            var factory = new SqlServerModificationCommandBatchFactory(
                new RelationalCommandBuilderFactory(
                    new DiagnosticsLogger<LoggerCategory.Database.Sql>(
                        new FakeInterceptingLogger<LoggerCategory.Database.Sql>(),
                        new DiagnosticListener("Fake")),
                    new DiagnosticsLogger<LoggerCategory.Database.DataReader>(
                        new FakeInterceptingLogger<LoggerCategory.Database.DataReader>(),
                        new DiagnosticListener("Fake")),
                    new SqlServerTypeMapper(
                        new RelationalTypeMapperDependencies())),
                new SqlServerSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                new SqlServerUpdateSqlGenerator(
                    new UpdateSqlGeneratorDependencies(
                        new SqlServerSqlGenerationHelper(
                            new RelationalSqlGenerationHelperDependencies())),
                    new SqlServerTypeMapper(
                        new RelationalTypeMapperDependencies())),
                new UntypedRelationalValueBufferFactoryFactory(
                    new RelationalValueBufferFactoryDependencies()),
                optionsBuilder.Options);

            var batch = factory.Create();

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, new SqlServerAnnotationProvider(), false, null)));
            Assert.False(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, new SqlServerAnnotationProvider(), false, null)));
        }

        [Fact]
        public void MaxBatchSize_is_optional()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer("Database=Crunchie");

            var factory = new SqlServerModificationCommandBatchFactory(
                new RelationalCommandBuilderFactory(
                    new DiagnosticsLogger<LoggerCategory.Database.Sql>(
                        new FakeInterceptingLogger<LoggerCategory.Database.Sql>(),
                        new DiagnosticListener("Fake")),
                    new DiagnosticsLogger<LoggerCategory.Database.DataReader>(
                        new FakeInterceptingLogger<LoggerCategory.Database.DataReader>(),
                        new DiagnosticListener("Fake")),
                    new SqlServerTypeMapper(
                        new RelationalTypeMapperDependencies())),
                new SqlServerSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                new SqlServerUpdateSqlGenerator(
                    new UpdateSqlGeneratorDependencies(
                        new SqlServerSqlGenerationHelper(
                            new RelationalSqlGenerationHelperDependencies())),
                    new SqlServerTypeMapper(new RelationalTypeMapperDependencies())),
                new UntypedRelationalValueBufferFactoryFactory(
                    new RelationalValueBufferFactoryDependencies()),
                optionsBuilder.Options);

            var batch = factory.Create();

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, new SqlServerAnnotationProvider(), false, null)));
            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, new SqlServerAnnotationProvider(), false, null)));
        }
    }
}
