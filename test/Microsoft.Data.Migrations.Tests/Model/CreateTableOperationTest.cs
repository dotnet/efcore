// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class CreateTableOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var table = new Table("dbo.MyTable");
            var createTableOperation = new CreateTableOperation(table);

            Assert.Same(table, createTableOperation.Table);
            Assert.False(createTableOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var createTableOperation = new CreateTableOperation(new Table("dbo.MyTable"));
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            createTableOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(createTableOperation), Times.Once());
        }
    }
}
