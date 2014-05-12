// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
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
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());
            var builder = new Mock<IndentedStringBuilder>();
            dropSequenceOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(dropSequenceOperation, builder.Object, false), Times.Once());
        }
    }
}
