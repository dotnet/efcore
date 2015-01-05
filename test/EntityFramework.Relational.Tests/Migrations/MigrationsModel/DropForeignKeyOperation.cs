// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
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
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            dropForeignKeyOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(dropForeignKeyOperation, builder.Object), Times.Once());
        }
    }
}
