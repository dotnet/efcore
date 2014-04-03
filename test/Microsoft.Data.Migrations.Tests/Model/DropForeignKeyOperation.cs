// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class DropForeignKeyOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropForeignKeyOperation = new DropForeignKeyOperation("dbo.MyTable", "MyFK");

            Assert.Equal("dbo.MyTable", dropForeignKeyOperation.DependentTableName);
            Assert.Equal("MyFK", dropForeignKeyOperation.ForeignKeyName);
            Assert.True(dropForeignKeyOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropForeignKeyOperation = new DropForeignKeyOperation("dbo.MyTable", "MyFK");
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>();
            var builder = new Mock<IndentedStringBuilder>();
            dropForeignKeyOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(dropForeignKeyOperation, builder.Object, false), Times.Once());
        }
    }
}
