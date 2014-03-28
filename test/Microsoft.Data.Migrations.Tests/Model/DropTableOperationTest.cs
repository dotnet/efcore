// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class DropTableOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropTableOperation = new DropTableOperation("dbo.MyTable");

            Assert.Equal("dbo.MyTable", dropTableOperation.TableName);
            Assert.True(dropTableOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropTableOperation = new DropTableOperation("dbo.MyTable");
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            dropTableOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(dropTableOperation), Times.Once());
        }
    }
}
