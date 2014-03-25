// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class AlterColumnOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var alterColumnOperation = new AlterColumnOperation(
                "dbo.MyTable", "Foo", "int", isNullable: true, isDestructiveChange: true);

            Assert.Equal("dbo.MyTable", alterColumnOperation.TableName);
            Assert.Equal("Foo", alterColumnOperation.ColumnName);
            Assert.Equal("int", alterColumnOperation.DataType);
            Assert.True(alterColumnOperation.IsNullable);
            Assert.True(alterColumnOperation.IsDestructiveChange);

        }

        [Fact]
        public void Dispatches_visitor()
        {
            var alterColumnOperation = new AlterColumnOperation(
                "dbo.MyTable", "Foo", "int", isNullable: true, isDestructiveChange: true);
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            alterColumnOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(alterColumnOperation), Times.Once());
        }
    }
}
