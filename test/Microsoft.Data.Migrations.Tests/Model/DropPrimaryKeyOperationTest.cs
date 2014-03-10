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
        public void CreateAndInitializeOperation()
        {
            var table = new Table("foo.bar");
            var column = new Column("C", "int");
            table.AddColumn(column);
            var primaryKey = new PrimaryKey("PK", column);

            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation(primaryKey, table);

            Assert.Same(primaryKey, dropPrimaryKeyOperation.Target);
            Assert.Same(table, dropPrimaryKeyOperation.Table);
        }

        [Fact]
        public void ObtainInverse()
        {
            var table = new Table("foo.bar");
            var column = new Column("C", "int");
            table.AddColumn(column);
            var primaryKey = new PrimaryKey("PK", column);

            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation(primaryKey, table);

            var inverse = dropPrimaryKeyOperation.Inverse;

            Assert.Same(primaryKey, inverse.Target);
            Assert.Same(table, inverse.Table);
        }

        [Fact]
        public void DispatchesSqlGeneration()
        {
            var table = new Table("foo.bar");
            var column = new Column("C", "int");
            table.AddColumn(column);
            var primaryKey = new PrimaryKey("PK", column);

            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation(primaryKey, table);
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            dropPrimaryKeyOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(dropPrimaryKeyOperation, stringBuilder, true), Times.Once());
        }
    }
}
