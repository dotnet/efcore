// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class AddColumnOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var table = new Table("foo.bar");
            var column = new Column("C", "int");

            var addColumnOperation = new AddColumnOperation(column, table);

            Assert.Same(column, addColumnOperation.Column);
            Assert.Same(table, addColumnOperation.Table);
        }

        [Fact]
        public void Is_not_destructive_change()
        {
            var addColumnOperation = new AddColumnOperation(new Column("C", "int"), new Table("foo.bar"));

            Assert.False(addColumnOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_sql_generation()
        {
            var addColumnOperation = new AddColumnOperation(new Column("C", "int"), new Table("foo.bar"));
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            addColumnOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(addColumnOperation, stringBuilder, true), Times.Once());
        }
    }
}
