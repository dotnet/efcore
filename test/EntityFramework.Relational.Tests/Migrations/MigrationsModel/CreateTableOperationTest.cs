// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class CreateTableOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var createTableOperation = new CreateTableOperation("dbo.MyTable");

            Assert.Equal("dbo.MyTable", createTableOperation.TableName);
            Assert.False(createTableOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var createTableOperation = new CreateTableOperation("dbo.MyTable");
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            createTableOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(createTableOperation, builder.Object), Times.Once());
        }
    }
}
