// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class CreateDatabaseOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var createDatabaseOperation = new CreateDatabaseOperation("MyDatabase");

            Assert.Equal("MyDatabase", createDatabaseOperation.DatabaseName);
            Assert.False(createDatabaseOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var createDatabaseOperation = new CreateDatabaseOperation("MyDatabase");
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            createDatabaseOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(createDatabaseOperation, builder.Object), Times.Once());
        }
    }
}
