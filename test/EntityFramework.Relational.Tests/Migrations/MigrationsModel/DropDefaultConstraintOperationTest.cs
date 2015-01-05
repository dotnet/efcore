// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class DropDefaultConstraintOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropDefaultConstraintOperation = new DropDefaultConstraintOperation("dbo.MyTable", "Foo");

            Assert.Equal("dbo.MyTable", dropDefaultConstraintOperation.TableName);
            Assert.Equal("Foo", dropDefaultConstraintOperation.ColumnName);
            Assert.False(dropDefaultConstraintOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropDefaultConstraintOperation = new DropDefaultConstraintOperation("dbo.MyTable", "Foo");
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            dropDefaultConstraintOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(dropDefaultConstraintOperation, builder.Object), Times.Once());
        }
    }
}
