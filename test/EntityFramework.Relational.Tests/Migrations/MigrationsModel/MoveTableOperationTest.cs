// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class MoveTableOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var moveTableOperation = new MoveTableOperation("dbo.MyTable", "dbo2");

            Assert.Equal("dbo.MyTable", moveTableOperation.TableName);
            Assert.Equal("dbo2", moveTableOperation.NewSchema);
            Assert.False(moveTableOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var moveTableOperation = new MoveTableOperation("dbo.MyTable", "dbo2");
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            moveTableOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(moveTableOperation, builder.Object), Times.Once());
        }
    }
}
