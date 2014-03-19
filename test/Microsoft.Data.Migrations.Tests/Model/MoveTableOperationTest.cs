// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class MoveTableOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var moveTableOperation = new MoveTableOperation("dbo.MyTable", "dbo2");

            Assert.Equal("dbo.MyTable", moveTableOperation.TableName);
            Assert.Equal("dbo2", moveTableOperation.NewSchema);
            Assert.False(moveTableOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var moveTableOperation = new MoveTableOperation("dbo.MyTable", "dbo2");
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            moveTableOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(moveTableOperation), Times.Once());
        }
    }
}
