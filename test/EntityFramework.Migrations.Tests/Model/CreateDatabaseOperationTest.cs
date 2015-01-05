// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
{
    public class CreateDatabaseOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var createDatabaseOperation = new CreateDatabaseOperation();

            Assert.False(createDatabaseOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var createDatabaseOperation = new CreateDatabaseOperation();
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            createDatabaseOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(createDatabaseOperation, builder.Object), Times.Once());
        }
    }
}
