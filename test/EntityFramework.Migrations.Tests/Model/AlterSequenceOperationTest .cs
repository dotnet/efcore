// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
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
            var builder = new IndentedStringBuilder();
            operation.GenerateSql(sqlGeneratorMock.Object, builder);

            sqlGeneratorMock.Verify(g => g.Generate(operation, builder), Times.Once());

            var codeGeneratorMock = new Mock<MigrationCodeGenerator>(new Mock<ModelCodeGenerator>().Object);
            builder = new IndentedStringBuilder();
            operation.GenerateCode(codeGeneratorMock.Object, builder);

            codeGeneratorMock.Verify(g => g.Generate(operation, builder), Times.Once());

            var visitorMock = new Mock<MigrationOperationVisitor<object>>();
            var context = new object();
            operation.Accept(visitorMock.Object, context);

            visitorMock.Verify(v => v.Visit(operation, context), Times.Once());
        }
    }
}
