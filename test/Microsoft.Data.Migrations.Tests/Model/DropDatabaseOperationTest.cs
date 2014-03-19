// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class DropDatabaseOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropDatabaseOperation = new DropDatabaseOperation("MyDatabase");

            Assert.Equal("MyDatabase", dropDatabaseOperation.DatabaseName);
            Assert.True(dropDatabaseOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropDatabaseOperation = new DropDatabaseOperation("MyDatabase");
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            dropDatabaseOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(dropDatabaseOperation), Times.Once());
        }
    }
}
