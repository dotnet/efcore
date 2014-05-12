// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
{
    public class RenameColumnOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var renameColumnOperation = new RenameColumnOperation("dbo.MyTable", "Foo", "Foo2");

            Assert.Equal("dbo.MyTable", renameColumnOperation.TableName);
            Assert.Equal("Foo", renameColumnOperation.ColumnName);
            Assert.Equal("Foo2", renameColumnOperation.NewColumnName);
            Assert.False(renameColumnOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var renameColumnOperation = new RenameColumnOperation("dbo.MyTable", "Foo", "Foo2");
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());
            var builder = new Mock<IndentedStringBuilder>();
            renameColumnOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(renameColumnOperation, builder.Object, false), Times.Once());
        }
    }
}
