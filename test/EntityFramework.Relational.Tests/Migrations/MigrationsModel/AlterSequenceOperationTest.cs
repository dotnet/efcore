// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class AlterSequenceOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var operation = new AlterSequenceOperation("dbo.MySequence", 7);

            Assert.Equal("dbo.MySequence", operation.SequenceName);
            Assert.Equal(7, operation.NewIncrementBy);
            Assert.False(operation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var operation = new AlterSequenceOperation("dbo.MySequence", 7);

            var sqlGeneratorMock = MigrationsTestHelpers.MockSqlGenerator();
            var sqlBuilder = new SqlBatchBuilder();
            operation.GenerateSql(sqlGeneratorMock.Object, sqlBuilder);

            sqlGeneratorMock.Verify(g => g.Generate(operation, sqlBuilder), Times.Once());

            var codeGeneratorMock = new Mock<MigrationCodeGenerator>(new Mock<ModelCodeGenerator>().Object);
            var codeBuilder = new IndentedStringBuilder();
            operation.GenerateCode(codeGeneratorMock.Object, codeBuilder);

            codeGeneratorMock.Verify(g => g.Generate(operation, codeBuilder), Times.Once());

            var visitorMock = new Mock<MigrationOperationVisitor<object>>();
            var context = new object();
            operation.Accept(visitorMock.Object, context);

            visitorMock.Verify(v => v.Visit(operation, context), Times.Once());
        }
    }
}
