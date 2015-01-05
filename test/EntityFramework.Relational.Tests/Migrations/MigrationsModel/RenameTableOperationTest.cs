// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class RenameTableOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var renameTableOperation = new RenameTableOperation("dbo.MyTable", "MyTable2");

            Assert.Equal("dbo.MyTable", renameTableOperation.TableName);
            Assert.Equal("MyTable2", renameTableOperation.NewTableName);
            Assert.False(renameTableOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var renameTableOperation = new RenameTableOperation("dbo.MyTable", "MyTable2");
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            renameTableOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(renameTableOperation, builder.Object), Times.Once());
        }
    }
}
