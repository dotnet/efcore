// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class DropColumnOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropColumnOperation = new DropColumnOperation("dbo.MyTable", "Foo");

            Assert.Equal("dbo.MyTable", dropColumnOperation.TableName);
            Assert.Equal("Foo", dropColumnOperation.ColumnName);
            Assert.True(dropColumnOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropColumnOperation = new DropColumnOperation("dbo.MyTable", "Foo");
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            dropColumnOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(dropColumnOperation, builder.Object), Times.Once());
        }
    }
}
