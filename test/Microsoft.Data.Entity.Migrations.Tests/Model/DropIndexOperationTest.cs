// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
{
    public class DropIndexOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropIndexOperation = new DropIndexOperation("dbo.MyTable", "MyIndex");

            Assert.Equal("dbo.MyTable", dropIndexOperation.TableName);
            Assert.Equal("MyIndex", dropIndexOperation.IndexName);
            Assert.False(dropIndexOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropIndexOperation = new DropIndexOperation("dbo.MyTable", "MyIndex");
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());
            var builder = new Mock<IndentedStringBuilder>();
            dropIndexOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(dropIndexOperation, builder.Object, false), Times.Once());
        }
    }
}
