// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class CreateSequenceOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var sequence = new Sequence("dbo.MySequence");
            var createSequenceOperation = new CreateSequenceOperation(sequence);

            Assert.Same(sequence, createSequenceOperation.Sequence);
            Assert.False(createSequenceOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var createSequenceOperation = new CreateSequenceOperation(new Sequence("dbo.MySequence"));
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            createSequenceOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(createSequenceOperation), Times.Once());
        }
    }
}
