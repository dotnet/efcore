// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class DropPrimaryKeyOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var table = new Table("foo.bar");
            var column = new Column("C", "int");
            table.AddColumn(column);
            var primaryKey = table.PrimaryKey = new PrimaryKey("PK", new[] { column });

            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation(primaryKey);

            Assert.Same(primaryKey, dropPrimaryKeyOperation.PrimaryKey);
        }

        [Fact]
        public void Is_destructive_change()
        {
            var table = new Table("foo.bar");
            var column = new Column("C", "int");
            table.AddColumn(column);
            var primaryKey = table.PrimaryKey = new PrimaryKey("PK", new[] { column });

            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation(primaryKey);

            Assert.True(dropPrimaryKeyOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_sql_generation()
        {
            var table = new Table("foo.bar");
            var column = new Column("C", "int");
            table.AddColumn(column);
            var primaryKey = table.PrimaryKey = new PrimaryKey("PK", new[] { column });

            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation(primaryKey);
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            dropPrimaryKeyOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(dropPrimaryKeyOperation, stringBuilder, true), Times.Once());
        }
    }
}
