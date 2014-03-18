// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class DropColumnOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var table = new Table("foo.bar");
            var column = new Column("C", "int");
            table.AddColumn(column);

            var dropColumnOperation = new DropColumnOperation(column);

            Assert.Same(column, dropColumnOperation.Column);
        }

        [Fact]
        public void Is_destructive_change()
        {
            var table = new Table("foo.bar");
            var column = new Column("C", "int");
            table.AddColumn(column);

            var dropColumnOperation = new DropColumnOperation(column);

            Assert.True(dropColumnOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_sql_generation()
        {
            var table = new Table("foo.bar");
            var column = new Column("C", "int");
            table.AddColumn(column);

            var dropColumnOperation = new DropColumnOperation(column);
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            dropColumnOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(dropColumnOperation, stringBuilder, true), Times.Once());
        }
    }
}
