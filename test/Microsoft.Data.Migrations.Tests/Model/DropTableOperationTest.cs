// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class DropTableOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var table = new Table("foo.bar");

            var dropTableOperation = new DropTableOperation(table);

            Assert.Same(table, dropTableOperation.Table);
        }

        [Fact]
        public void Is_destructive_change()
        {
            var dropTableOperation = new DropTableOperation(new Table("foo.bar"));

            Assert.True(dropTableOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_sql_generation()
        {
            var dropTableOperation = new DropTableOperation(new Table("foo.bar"));
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            dropTableOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(dropTableOperation, stringBuilder, true), Times.Once());
        }
    }
}
