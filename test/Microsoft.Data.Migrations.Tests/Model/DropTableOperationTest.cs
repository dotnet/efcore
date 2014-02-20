// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Model
{
    public class DropTableOperationTest
    {
        [Fact]
        public void CreateAndInitializeOperation()
        {
            var table = new Table("foo.bar");

            var dropTableOperation = new DropTableOperation(table);

            Assert.Same(table, dropTableOperation.Target);
        }

        [Fact]
        public void ObtainInverse()
        {
            var table = new Table("foo.bar");
            var dropTableOperation = new DropTableOperation(table);

            var inverse = dropTableOperation.Inverse;

            Assert.Same(table, inverse.Target);
        }

        [Fact]
        public void DispatchesSqlGeneration()
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
