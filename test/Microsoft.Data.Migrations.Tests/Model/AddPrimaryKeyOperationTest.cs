// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class AddPrimaryKeyOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation(
                "dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: true);

            Assert.Equal("dbo.MyTable", addPrimaryKeyOperation.TableName);
            Assert.Equal("MyPK", addPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal(new[] { "Foo", "Bar" }, addPrimaryKeyOperation.ColumnNames);
            Assert.True(addPrimaryKeyOperation.IsClustered);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation(
                "dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: true);
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            addPrimaryKeyOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(addPrimaryKeyOperation), Times.Once());
        }
    }
}
