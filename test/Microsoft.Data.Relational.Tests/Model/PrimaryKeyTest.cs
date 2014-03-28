// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.Relational.Tests.Model
{
    public class PrimaryKeyTest
    {
        [Fact]
        public void Create_and_initialize_primary_key()
        {
            var table = new Table("dbo.MyTable");
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");
            table.AddColumn(column0);
            table.AddColumn(column1);
            var primaryKey = new PrimaryKey(
                "MyPrimaryKey", new[] { column0, column1, }, isClustered: true);

            Assert.Equal("MyPrimaryKey", primaryKey.Name);
            Assert.IsAssignableFrom<IReadOnlyList<Column>>(table.Columns);
            Assert.Equal(2, primaryKey.Columns.Count);
            Assert.Same(column0, primaryKey.Columns[0]);
            Assert.Same(column1, primaryKey.Columns[1]);
            Assert.Same(table, primaryKey.Table);
            Assert.True(primaryKey.IsClustered);
        }
    }
}
