// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
{
    public class AddDefaultConstraintOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var addDefaultConstraintOperation = new AddDefaultConstraintOperation(
                "dbo.MyTable", "Foo", "MyDefault", null);

            Assert.Equal("dbo.MyTable", addDefaultConstraintOperation.TableName);
            Assert.Same("Foo", addDefaultConstraintOperation.ColumnName);
            Assert.Equal("MyDefault", addDefaultConstraintOperation.DefaultValue);
            Assert.Null(addDefaultConstraintOperation.DefaultSql);
            Assert.False(addDefaultConstraintOperation.IsDestructiveChange);

            var addDefaultConstraintOperation2 = new AddDefaultConstraintOperation(
                "dbo.MyTable", "Foo", null, "GETDATE()");

            Assert.Equal("dbo.MyTable", addDefaultConstraintOperation2.TableName);
            Assert.Same("Foo", addDefaultConstraintOperation2.ColumnName);
            Assert.Null(addDefaultConstraintOperation2.DefaultValue);
            Assert.Equal("GETDATE()", addDefaultConstraintOperation2.DefaultSql);
            Assert.False(addDefaultConstraintOperation2.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var addDefaultConstraintOperation = new AddDefaultConstraintOperation(
                "dbo.MyTable", "Foo", "MyDefault", null);
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());
            var builder = new Mock<IndentedStringBuilder>();
            addDefaultConstraintOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(addDefaultConstraintOperation, builder.Object, false), Times.Once());
        }
    }
}
