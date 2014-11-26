// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
{
    public class DropDatabaseOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropDatabaseOperation = new DropDatabaseOperation("MyDatabase");

            Assert.Equal("MyDatabase", dropDatabaseOperation.DatabaseName);
            Assert.True(dropDatabaseOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropDatabaseOperation = new DropDatabaseOperation("MyDatabase");
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            dropDatabaseOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(dropDatabaseOperation, builder.Object), Times.Once());
        }
    }
}
