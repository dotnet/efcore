// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.MigrationsModel
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
            var mockVisitor = MigrationsTestHelpers.MockSqlGenerator();
            var builder = new Mock<SqlBatchBuilder>();
            dropSequenceOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(dropSequenceOperation, builder.Object), Times.Once());
        }
    }
}
