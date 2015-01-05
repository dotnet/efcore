// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class SqlOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var sqlOperation = new SqlOperation("MySql");

            Assert.Equal("MySql", sqlOperation.Sql);
            Assert.True(sqlOperation.IsDestructiveChange);
        }

        [Fact]
        public void Get_set_suppress_transaction()
        {
            var sqlOperation = new SqlOperation("MySql");

            Assert.False(sqlOperation.SuppressTransaction);

            sqlOperation.SuppressTransaction = true;

            Assert.True(sqlOperation.SuppressTransaction);

            sqlOperation.SuppressTransaction = false;

            Assert.False(sqlOperation.SuppressTransaction);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var sqlOperation = new SqlOperation("MySql");
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            sqlOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(sqlOperation, builder.Object), Times.Once());
        }
    }
}
