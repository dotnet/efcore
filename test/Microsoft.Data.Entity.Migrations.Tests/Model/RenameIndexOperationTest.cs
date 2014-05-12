// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
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
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper());
            var builder = new Mock<IndentedStringBuilder>();
            renameIndexOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(renameIndexOperation, builder.Object, false), Times.Once());
        }
    }
}
