// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class DropPrimaryKeyOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation("dbo.MyTable", "MyPK");

            Assert.Equal("dbo.MyTable", dropPrimaryKeyOperation.TableName);
            Assert.Equal("MyPK", dropPrimaryKeyOperation.PrimaryKeyName);
            Assert.True(dropPrimaryKeyOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation("dbo.MyTable", "MyPK");
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            dropPrimaryKeyOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(dropPrimaryKeyOperation, builder.Object), Times.Once());
        }
    }
}
