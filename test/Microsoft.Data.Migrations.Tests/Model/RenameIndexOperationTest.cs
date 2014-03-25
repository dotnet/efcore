// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class RenameIndexOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var renameIndexOperation = new RenameIndexOperation("dbo.MyTable", "MyIndex", "MyIndex2");

            Assert.Equal("dbo.MyTable", renameIndexOperation.TableName);
            Assert.Equal("MyIndex", renameIndexOperation.IndexName);
            Assert.Equal("MyIndex2", renameIndexOperation.NewIndexName);
            Assert.False(renameIndexOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var renameIndexOperation = new RenameIndexOperation("dbo.MyTable", "MyIndex", "MyIndex2");
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            renameIndexOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(renameIndexOperation), Times.Once());
        }
    }
}
