// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class AddPrimaryKeyOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation(
                "dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: true);

            Assert.Equal("dbo.MyTable", addPrimaryKeyOperation.TableName);
            Assert.Equal("MyPK", addPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal(new[] { "Foo", "Bar" }, addPrimaryKeyOperation.ColumnNames);
            Assert.True(addPrimaryKeyOperation.IsClustered);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation(
                "dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: true);
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            addPrimaryKeyOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(addPrimaryKeyOperation, builder.Object), Times.Once());
        }
    }
}
