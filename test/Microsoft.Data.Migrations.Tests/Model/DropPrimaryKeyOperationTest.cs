// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class DropPrimaryKeyOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation("dbo.MyTable", "MyPK");

            Assert.Equal("dbo.MyTable", dropPrimaryKeyOperation.TableName);
            Assert.Equal("MyPK", dropPrimaryKeyOperation.PrimaryKeyName);
            Assert.True(dropPrimaryKeyOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation("dbo.MyTable", "MyPK");
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            dropPrimaryKeyOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(dropPrimaryKeyOperation), Times.Once());
        }
    }
}
