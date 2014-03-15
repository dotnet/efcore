// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class MoveTableOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var table = new Table("foo.bar");

            var moveTableOperation = new MoveTableOperation(table, "foo2");

            Assert.Same(table, moveTableOperation.Table);
            Assert.Equal("foo2", moveTableOperation.Schema);
        }

        [Fact]
        public void Is_not_destructive_change()
        {
            var moveTableOperation = new MoveTableOperation(new Table("foo.bar"), "foo2");

            Assert.False(moveTableOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_sql_generation()
        {
            var moveTableOperation = new MoveTableOperation(new Table("foo.bar"), "foo2");
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            moveTableOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(moveTableOperation, stringBuilder, true), Times.Once());
        }
    }
}
