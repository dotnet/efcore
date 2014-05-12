// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
{
    public class DropForeignKeyOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropForeignKeyOperation = new DropForeignKeyOperation("dbo.MyTable", "MyFK");

            Assert.Equal("dbo.MyTable", dropForeignKeyOperation.TableName);
            Assert.Equal("MyFK", dropForeignKeyOperation.ForeignKeyName);
            Assert.True(dropForeignKeyOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropForeignKeyOperation = new DropForeignKeyOperation("dbo.MyTable", "MyFK");
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());
            var builder = new Mock<IndentedStringBuilder>();
            dropForeignKeyOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(dropForeignKeyOperation, builder.Object, false), Times.Once());
        }
    }
}
