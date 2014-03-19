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
            var column = new Column("Foo", "int");
            var alterColumnOperation = new AlterColumnOperation("dbo.MyTable", column, isDestructiveChange: true);

            Assert.Equal("dbo.MyTable", alterColumnOperation.TableName);
            Assert.Same(column, alterColumnOperation.Column);
            Assert.True(alterColumnOperation.IsDestructiveChange);

        }

        [Fact]
        public void Dispatches_visitor()
        {
            var alterColumnOperation = new AlterColumnOperation(
                "dbo.MyTable", new Column("Foo", "int"), isDestructiveChange: true);
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            alterColumnOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(alterColumnOperation), Times.Once());
        }
    }
}
