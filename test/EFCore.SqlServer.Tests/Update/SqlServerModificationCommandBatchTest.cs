// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Update
{
    public class SqlServerModificationCommandBatchTest
    {
        [ConditionalFact]
        public void AddCommand_returns_false_when_max_batch_size_is_reached()
        {
            var typeMapper = new SqlServerTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

            var logger = new FakeRelationalCommandDiagnosticsLogger();

            var batch = new SqlServerModificationCommandBatch(
                new ModificationCommandBatchFactoryDependencies(
                    new RelationalCommandBuilderFactory(
                        new RelationalCommandBuilderDependencies(
                            typeMapper)),
                    new SqlServerSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies()),
                    new SqlServerUpdateSqlGenerator(
                        new UpdateSqlGeneratorDependencies(
                            new SqlServerSqlGenerationHelper(
                                new RelationalSqlGenerationHelperDependencies()),
                            typeMapper)),
                    new TypedRelationalValueBufferFactoryFactory(
                        new RelationalValueBufferFactoryDependencies(
                            typeMapper, new CoreSingletonOptions())),
                    new CurrentDbContext(new FakeDbContext()),
                    logger),
                1);

            Assert.True(
                batch.AddCommand(
                    new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null, null)));
            Assert.False(
                batch.AddCommand(
                    new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null, null)));
        }

        private class FakeDbContext : DbContext
        {
        }
    }
}
