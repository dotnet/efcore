// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class RenameColumnOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var renameColumnOperation = new RenameColumnOperation("dbo.MyTable", "Foo", "Foo2");

            Assert.Equal("dbo.MyTable", renameColumnOperation.TableName);
            Assert.Equal("Foo", renameColumnOperation.ColumnName);
            Assert.Equal("Foo2", renameColumnOperation.NewColumnName);
            Assert.False(renameColumnOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var renameColumnOperation = new RenameColumnOperation("dbo.MyTable", "Foo", "Foo2");
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            renameColumnOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(renameColumnOperation), Times.Once());
        }
    }
}
