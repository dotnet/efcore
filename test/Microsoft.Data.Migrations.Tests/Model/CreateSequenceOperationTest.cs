// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class CreateSequenceOperationTest
    {
        [Fact]
        public void CreateAndInitializeOperation()
        {
            var sequence = new Sequence("foo.bar");

            var createSequenceOperation = new CreateSequenceOperation(sequence);

            Assert.Same(sequence, createSequenceOperation.Target);
        }

        [Fact]
        public void ObtainInverse()
        {
            var sequence = new Sequence("foo.bar");
            var createSequenceOperation = new CreateSequenceOperation(sequence);

            var inverse = createSequenceOperation.Inverse;

            Assert.Same(sequence, inverse.Target);
        }

        [Fact]
        public void DispatchesSqlGeneration()
        {
            var sequence = new Sequence("foo.bar");
            var createSequenceOperation = new CreateSequenceOperation(sequence);
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            createSequenceOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(createSequenceOperation, stringBuilder, true), Times.Once());
        }
    }
}
