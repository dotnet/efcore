// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
{
    public class DropPrimaryKeyOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation("dbo.MyTable", "MyPK");

            Assert.Equal("dbo.MyTable", dropPrimaryKeyOperation.TableName);
            Assert.Equal("MyPK", dropPrimaryKeyOperation.PrimaryKeyName);
            Assert.True(dropPrimaryKeyOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation("dbo.MyTable", "MyPK");
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());
            var builder = new Mock<IndentedStringBuilder>();
            dropPrimaryKeyOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(dropPrimaryKeyOperation, builder.Object, false), Times.Once());
        }
    }
}
