// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class RenameIndexOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var renameIndexOperation = new RenameIndexOperation("dbo.MyTable", "MyIndex", "MyIndex2");

            Assert.Equal("dbo.MyTable", renameIndexOperation.TableName);
            Assert.Equal("MyIndex", renameIndexOperation.IndexName);
            Assert.Equal("MyIndex2", renameIndexOperation.NewIndexName);
            Assert.False(renameIndexOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var renameIndexOperation = new RenameIndexOperation("dbo.MyTable", "MyIndex", "MyIndex2");
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            renameIndexOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(renameIndexOperation, builder.Object), Times.Once());
        }
    }
}
