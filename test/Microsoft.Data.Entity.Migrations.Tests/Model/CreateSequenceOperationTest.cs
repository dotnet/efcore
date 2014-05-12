// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
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
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());
            var builder = new Mock<IndentedStringBuilder>();
            createSequenceOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(createSequenceOperation, builder.Object, false), Times.Once());
        }
    }
}
