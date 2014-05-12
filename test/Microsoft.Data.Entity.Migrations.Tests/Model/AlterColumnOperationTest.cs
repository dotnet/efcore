// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
{
    public class AlterColumnOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var newColumn = new Column("Foo", "int") { IsNullable = true };
            var alterColumnOperation = new AlterColumnOperation(
                "dbo.MyTable", newColumn, isDestructiveChange: true);

            Assert.Equal("dbo.MyTable", alterColumnOperation.TableName);
            Assert.Same(newColumn, alterColumnOperation.NewColumn);
            Assert.True(alterColumnOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var newColumn = new Column("Foo", "int") { IsNullable = true };
            var alterColumnOperation = new AlterColumnOperation(
                "dbo.MyTable", newColumn, isDestructiveChange: true);
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());
            var builder = new Mock<IndentedStringBuilder>();
            alterColumnOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(alterColumnOperation, builder.Object, false), Times.Once());
        }
    }
}
