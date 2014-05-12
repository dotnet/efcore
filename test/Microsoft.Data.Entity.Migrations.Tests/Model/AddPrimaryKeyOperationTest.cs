// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
{
    public class AddPrimaryKeyOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation(
                "dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: true);

            Assert.Equal("dbo.MyTable", addPrimaryKeyOperation.TableName);
            Assert.Equal("MyPK", addPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal(new[] { "Foo", "Bar" }, addPrimaryKeyOperation.ColumnNames);
            Assert.True(addPrimaryKeyOperation.IsClustered);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation(
                "dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: true);
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());
            var builder = new Mock<IndentedStringBuilder>();
            addPrimaryKeyOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(addPrimaryKeyOperation, builder.Object, false), Times.Once());
        }
    }
}
