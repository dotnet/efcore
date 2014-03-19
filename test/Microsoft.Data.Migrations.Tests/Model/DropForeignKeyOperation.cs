// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class DropForeignKeyOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropForeignKeyOperation = new DropForeignKeyOperation("dbo.MyTable", "MyFK");

            Assert.Equal("dbo.MyTable", dropForeignKeyOperation.DependentTableName);
            Assert.Equal("MyFK", dropForeignKeyOperation.ForeignKeyName);
            Assert.True(dropForeignKeyOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropForeignKeyOperation = new DropForeignKeyOperation("dbo.MyTable", "MyFK");
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            dropForeignKeyOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(dropForeignKeyOperation), Times.Once());
        }
    }
}
