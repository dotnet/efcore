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
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>();
            var builder = new Mock<IndentedStringBuilder>();
            addPrimaryKeyOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(addPrimaryKeyOperation, builder.Object, false), Times.Once());
        }
    }
}
