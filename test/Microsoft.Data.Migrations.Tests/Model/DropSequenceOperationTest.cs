// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class DropSequenceOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropSequenceOperation = new DropSequenceOperation("dbo.MySequence");

            Assert.Equal("dbo.MySequence", dropSequenceOperation.SequenceName);
            Assert.True(dropSequenceOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropSequenceOperation = new DropSequenceOperation("dbo.MySequence");
            var mockVisitor = new Mock<MigrationOperationVisitor>();

            dropSequenceOperation.Accept(mockVisitor.Object);

            mockVisitor.Verify(g => g.Visit(dropSequenceOperation), Times.Once());
        }
    }
}
