// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
{
    public class CreateTableOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var createTableOperation = new CreateTableOperation("dbo.MyTable");

            Assert.Equal("dbo.MyTable", createTableOperation.TableName);
            Assert.False(createTableOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var createTableOperation = new CreateTableOperation("dbo.MyTable");
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>(new RelationalMetadataExtensionProvider(), new RelationalTypeMapper());
            var builder = new Mock<IndentedStringBuilder>();
            createTableOperation.GenerateSql(mockVisitor.Object, builder.Object);

            mockVisitor.Verify(g => g.Generate(createTableOperation, builder.Object), Times.Once());
        }
    }
}
