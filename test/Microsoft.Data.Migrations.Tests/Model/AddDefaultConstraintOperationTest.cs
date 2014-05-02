// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
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
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>();
            var builder = new Mock<IndentedStringBuilder>();
            addDefaultConstraintOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(addDefaultConstraintOperation, builder.Object, false), Times.Once());
        }
    }
}
