// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class AlterColumnOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var newColumn = new Column("Foo", typeof(int)) { IsNullable = true };
            var alterColumnOperation = new AlterColumnOperation(
                "dbo.MyTable", newColumn, isDestructiveChange: true);

            Assert.Equal("dbo.MyTable", alterColumnOperation.TableName);
            Assert.Same(newColumn, alterColumnOperation.NewColumn);
            Assert.True(alterColumnOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var newColumn = new Column("Foo", typeof(int)) { IsNullable = true };
            var alterColumnOperation = new AlterColumnOperation(
                "dbo.MyTable", newColumn, isDestructiveChange: true);
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            alterColumnOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(alterColumnOperation, builder.Object), Times.Once());
        }
    }
}
