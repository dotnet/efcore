// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class DropDefaultConstraintOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropDefaultConstraintOperation = new DropDefaultConstraintOperation("dbo.MyTable", "Foo");

            Assert.Equal("dbo.MyTable", dropDefaultConstraintOperation.TableName);
            Assert.Equal("Foo", dropDefaultConstraintOperation.ColumnName);
            Assert.False(dropDefaultConstraintOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropDefaultConstraintOperation = new DropDefaultConstraintOperation("dbo.MyTable", "Foo");
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            dropDefaultConstraintOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(dropDefaultConstraintOperation), Times.Once());
        }
    }
}
