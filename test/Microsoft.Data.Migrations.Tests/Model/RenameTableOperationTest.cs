// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class RenameTableOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var renameTableOperation = new RenameTableOperation("dbo.MyTable", "MyTable2");

            Assert.Equal("dbo.MyTable", renameTableOperation.TableName);
            Assert.Equal("MyTable2", renameTableOperation.NewTableName);
            Assert.False(renameTableOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var renameTableOperation = new RenameTableOperation("dbo.MyTable", "MyTable2");
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            renameTableOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(renameTableOperation), Times.Once());
        }
    }
}
