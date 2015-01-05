// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
{
    public class CreateSequenceOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var createSequenceOperation = new CreateSequenceOperation(
                "dbo.MySequence", 13, 7, 3, 103, typeof(int));

            Assert.Equal("dbo.MySequence", createSequenceOperation.SequenceName);
            Assert.Equal(13, createSequenceOperation.StartValue);
            Assert.Equal(7, createSequenceOperation.IncrementBy);
            Assert.Equal(3, createSequenceOperation.MinValue);
            Assert.Equal(103, createSequenceOperation.MaxValue);
            Assert.Equal(typeof(int), createSequenceOperation.Type);
            Assert.False(createSequenceOperation.IsDestructiveChange);
        }

        [Fact]
        public void Create_and_initialize_operation_with_defaults()
        {
            var createSequenceOperation = new CreateSequenceOperation("dbo.MySequence");

            Assert.Equal("dbo.MySequence", createSequenceOperation.SequenceName);
            Assert.Equal(Sequence.DefaultStartValue, createSequenceOperation.StartValue);
            Assert.Equal(Sequence.DefaultIncrement, createSequenceOperation.IncrementBy);
            Assert.False(createSequenceOperation.MinValue.HasValue);
            Assert.False(createSequenceOperation.MaxValue.HasValue);
            Assert.Equal(typeof(long), createSequenceOperation.Type);
            Assert.False(createSequenceOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var createSequenceOperation = new CreateSequenceOperation("dbo.MySequence");
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            createSequenceOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(createSequenceOperation, builder.Object), Times.Once());
        }
    }
}
