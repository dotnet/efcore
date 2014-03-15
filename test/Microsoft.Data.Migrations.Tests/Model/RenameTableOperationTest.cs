// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class RenameTableOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var table = new Table("foo.bar");

            var renameTableOperation = new RenameTableOperation(table, "bar2");

            Assert.Same(table, renameTableOperation.Table);
            Assert.Equal("bar2", renameTableOperation.TableName);
        }

        [Fact]
        public void Is_not_destructive_change()
        {
            var renameTableOperation = new RenameTableOperation(new Table("foo.bar"), "bar2");

            Assert.False(renameTableOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_sql_generation()
        {
            var renameTableOperation = new RenameTableOperation(new Table("foo.bar"), "bar2");
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            renameTableOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(renameTableOperation, stringBuilder, true), Times.Once());
        }
    }
}
