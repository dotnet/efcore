// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class DropSequenceOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var sequence = new Sequence("foo.bar");

            var dropSequenceOperation = new DropSequenceOperation(sequence);

            Assert.Same(sequence, dropSequenceOperation.Sequence);
        }

        [Fact]
        public void Is_destructive_change()
        {
            var dropSequenceOperation = new DropSequenceOperation(new Sequence("foo.bar"));

            Assert.True(dropSequenceOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_sql_generation()
        {
            var dropSequenceOperation = new DropSequenceOperation(new Sequence("foo.bar"));
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            dropSequenceOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(dropSequenceOperation, stringBuilder, true), Times.Once());
        }
    }
}
