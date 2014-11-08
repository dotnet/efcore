// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
{
    public class CopyDataOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var operation = new CopyDataOperation("dbo.T1", new[] { "A", "B" }, "dbo.T2", new[] { "C", "D" });

            Assert.Equal("dbo.T1", operation.SourceTableName);
            Assert.Equal(new[] { "A", "B" }, operation.SourceColumnNames);
            Assert.Equal("dbo.T2", operation.TargetTableName);
            Assert.Equal(new[] { "C", "D" }, operation.TargetColumnNames);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var operation = new CopyDataOperation("dbo.T1", new[] { "A", "B" }, "dbo.T2", new[] { "C", "D" });

            var sqlGeneratorMock = new Mock<MigrationOperationSqlGenerator>(new RelationalMetadataExtensionProvider(), new RelationalTypeMapper());
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
