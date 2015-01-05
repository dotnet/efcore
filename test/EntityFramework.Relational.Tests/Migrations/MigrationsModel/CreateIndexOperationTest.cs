// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class CreateIndexOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var createIndexOperation = new CreateIndexOperation(
                "dbo.MyTable", "MyIndex", new[] { "Foo", "Bar" },
                isUnique: true, isClustered: true);

            Assert.Equal("dbo.MyTable", createIndexOperation.TableName);
            Assert.Equal("MyIndex", createIndexOperation.IndexName);
            Assert.Equal(new[] { "Foo", "Bar" }, createIndexOperation.ColumnNames);
            Assert.True(createIndexOperation.IsUnique);
            Assert.True(createIndexOperation.IsClustered);
            Assert.False(createIndexOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var createIndexOperation = new CreateIndexOperation(
                "dbo.MyTable", "MyIndex", new[] { "Foo", "Bar" },
                isUnique: true, isClustered: true);
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            createIndexOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(createIndexOperation, builder.Object), Times.Once());
        }
    }
}
