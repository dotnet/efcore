// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class MigrationCommandListBuilderTest
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void MigrationCommandListBuilder_groups_multiple_statements_into_one_batch(bool suppressTransaction)
        {
            var commandListBuilder = CreateBuilder();
            commandListBuilder.AppendLine("Statement1");
            commandListBuilder.AppendLine("Statement2");
            commandListBuilder.AppendLine("Statement3");
            commandListBuilder.EndCommand(suppressTransaction);

            var commandList = commandListBuilder.GetCommandList();

            Assert.Equal(1, commandList.Count);

            Assert.Equal(suppressTransaction, commandList[0].TransactionSuppressed);
            Assert.Equal(
                @"Statement1
Statement2
Statement3
",
                commandList[0].CommandText,
                ignoreLineEndingDifferences: true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void MigrationCommandListBuilder_correctly_produces_multiple_batches(bool suppressTransaction)
        {
            var commandListBuilder = CreateBuilder();
            commandListBuilder.AppendLine("Statement1");
            commandListBuilder.EndCommand(suppressTransaction);
            commandListBuilder.AppendLine("Statement2");
            commandListBuilder.AppendLine("Statement3");
            commandListBuilder.EndCommand(suppressTransaction);
            commandListBuilder.AppendLine("Statement4");
            commandListBuilder.AppendLine("Statement5");
            commandListBuilder.AppendLine("Statement6");
            commandListBuilder.EndCommand(suppressTransaction);

            var commandList = commandListBuilder.GetCommandList();

            Assert.Equal(3, commandList.Count);

            Assert.Equal(suppressTransaction, commandList[0].TransactionSuppressed);
            Assert.Equal(
                @"Statement1
",
                commandList[0].CommandText,
                ignoreLineEndingDifferences: true);

            Assert.Equal(suppressTransaction, commandList[1].TransactionSuppressed);
            Assert.Equal(
                @"Statement2
Statement3
",
                commandList[1].CommandText,
                ignoreLineEndingDifferences: true);

            Assert.Equal(suppressTransaction, commandList[2].TransactionSuppressed);
            Assert.Equal(
                @"Statement4
Statement5
Statement6
",
                commandList[2].CommandText,
                ignoreLineEndingDifferences: true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void MigrationCommandListBuilder_ignores_empty_batches(bool suppressTransaction)
        {
            var commandListBuilder = CreateBuilder();
            commandListBuilder.AppendLine("Statement1");
            commandListBuilder.EndCommand(suppressTransaction);
            commandListBuilder.EndCommand(suppressTransaction: true);
            commandListBuilder.EndCommand(suppressTransaction: true);
            commandListBuilder.AppendLine("Statement2");
            commandListBuilder.AppendLine("Statement3");
            commandListBuilder.EndCommand(suppressTransaction);
            commandListBuilder.EndCommand();

            var commandList = commandListBuilder.GetCommandList();

            Assert.Equal(2, commandList.Count);

            Assert.Equal(suppressTransaction, commandList[0].TransactionSuppressed);
            Assert.Equal(
                @"Statement1
",
                commandList[0].CommandText,
                ignoreLineEndingDifferences: true);

            Assert.Equal(suppressTransaction, commandList[1].TransactionSuppressed);
            Assert.Equal(
                @"Statement2
Statement3
",
                commandList[1].CommandText,
                ignoreLineEndingDifferences: true);
        }

        private MigrationCommandListBuilder CreateBuilder()
            => new MigrationCommandListBuilder(
                new RelationalCommandBuilderFactory(
                    new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>(),
                    new TestRelationalTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())));
    }
}
