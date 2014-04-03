// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class CreateDatabaseOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var createDatabaseOperation = new CreateDatabaseOperation("MyDatabase");

            Assert.Equal("MyDatabase", createDatabaseOperation.DatabaseName);
            Assert.False(createDatabaseOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var createDatabaseOperation = new CreateDatabaseOperation("MyDatabase");
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>();
            var builder = new Mock<IndentedStringBuilder>();
            createDatabaseOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(createDatabaseOperation, builder.Object, false), Times.Once());
        }
    }
}
