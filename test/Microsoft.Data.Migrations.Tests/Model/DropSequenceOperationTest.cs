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
        public void CreateAndInitializeOperation()
        {
            var sequence = new Sequence("foo.bar");

            var dropSequenceOperation = new DropSequenceOperation(sequence);

            Assert.Same(sequence, dropSequenceOperation.Target);
        }

        [Fact]
        public void ObtainInverse()
        {
            var sequence = new Sequence("foo.bar");
            var dropSequenceOperation = new DropSequenceOperation(sequence);

            var inverse = dropSequenceOperation.Inverse;

            Assert.Same(sequence, inverse.Target);
        }

        [Fact]
        public void DispatchesSqlGeneration()
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
