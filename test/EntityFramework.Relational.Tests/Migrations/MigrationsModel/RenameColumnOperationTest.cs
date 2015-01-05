// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class RenameColumnOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var renameColumnOperation = new RenameColumnOperation("dbo.MyTable", "Foo", "Foo2");

            Assert.Equal("dbo.MyTable", renameColumnOperation.TableName);
            Assert.Equal("Foo", renameColumnOperation.ColumnName);
            Assert.Equal("Foo2", renameColumnOperation.NewColumnName);
            Assert.False(renameColumnOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var renameColumnOperation = new RenameColumnOperation("dbo.MyTable", "Foo", "Foo2");
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            renameColumnOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(renameColumnOperation, builder.Object), Times.Once());
        }
    }
}
