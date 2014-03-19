// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class AddPrimaryKeyOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var primaryKey = new PrimaryKey("MyPK", new[] { new Column("Foo", "int") });
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation("dbo.MyTable", primaryKey);

            Assert.Equal("dbo.MyTable", addPrimaryKeyOperation.TableName);
            Assert.Same(primaryKey, addPrimaryKeyOperation.PrimaryKey);
            Assert.False(addPrimaryKeyOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var primaryKey = new PrimaryKey("MyPK", new[] { new Column("Foo", "int") });
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation("dbo.MyTable", primaryKey);
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            addPrimaryKeyOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(addPrimaryKeyOperation), Times.Once());
        }
    }
}
