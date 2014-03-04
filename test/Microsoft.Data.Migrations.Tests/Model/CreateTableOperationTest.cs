// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class CreateTableOperationTest
    {
        [Fact]
        public void CreateAndInitializeOperation()
        {
            var table = new Table("foo.bar");

            var createTableOperation = new CreateTableOperation(table);

            Assert.Same(table, createTableOperation.Target);
        }

        [Fact]
        public void ObtainInverse()
        {
            var table = new Table("foo.bar");
            var createTableOperation = new CreateTableOperation(table);

            var inverse = createTableOperation.Inverse;

            Assert.Same(table, inverse.Target);
        }

        [Fact]
        public void DispatchesSqlGeneration()
        {
            var createTableOperation = new CreateTableOperation(new Table("foo.bar"));
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            createTableOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(createTableOperation, stringBuilder, true), Times.Once());
        }
    }
}
