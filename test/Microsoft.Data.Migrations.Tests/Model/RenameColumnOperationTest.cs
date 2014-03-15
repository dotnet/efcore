// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class RenameColumnOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var table = new Table("foo.bar");
            var column = new Column("C", "int");
            table.AddColumn(column);

            var renameColumnOperation = new RenameColumnOperation(column, "D");

            Assert.Same(column, renameColumnOperation.Column);
            Assert.Equal("D", renameColumnOperation.ColumnName);
        }

        [Fact]
        public void Is_not_destructive_change()
        {
            var table = new Table("foo.bar");
            var column = new Column("C", "int");
            table.AddColumn(column);

            var renameColumnOperation = new RenameColumnOperation(column, "D");

            Assert.False(renameColumnOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_sql_generation()
        {
            var table = new Table("foo.bar");
            var column = new Column("C", "int");
            table.AddColumn(column);

            var renameColumnOperation = new RenameColumnOperation(column, "D");
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            renameColumnOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(renameColumnOperation, stringBuilder, true), Times.Once());
        }
    }
}
