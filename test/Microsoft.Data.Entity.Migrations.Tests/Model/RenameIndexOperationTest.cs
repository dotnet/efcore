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

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
{
    public class RenameIndexOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var renameIndexOperation = new RenameIndexOperation("dbo.MyTable", "MyIndex", "MyIndex2");

            Assert.Equal("dbo.MyTable", renameIndexOperation.TableName);
            Assert.Equal("MyIndex", renameIndexOperation.IndexName);
            Assert.Equal("MyIndex2", renameIndexOperation.NewIndexName);
            Assert.False(renameIndexOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var renameIndexOperation = new RenameIndexOperation("dbo.MyTable", "MyIndex", "MyIndex2");
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>();
            var builder = new Mock<IndentedStringBuilder>();
            renameIndexOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(renameIndexOperation, builder.Object, false), Times.Once());
        }
    }
}
