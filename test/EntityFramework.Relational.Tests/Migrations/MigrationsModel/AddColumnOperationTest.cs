// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class AddColumnOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var column = new Column("Foo", typeof(int));
            var addColumnOperation = new AddColumnOperation("dbo.MyTable", column);

            Assert.Equal("dbo.MyTable", addColumnOperation.TableName);
            Assert.Same(column, addColumnOperation.Column);
            Assert.False(addColumnOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var addColumnOperation = new AddColumnOperation("dbo.MyTable", new Column("Foo", typeof(int)));
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            addColumnOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(addColumnOperation, builder.Object), Times.Once());
        }
    }
}
