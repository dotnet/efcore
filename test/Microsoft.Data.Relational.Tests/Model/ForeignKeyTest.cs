// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.Relational.Tests.Model
{
    public class ForeignKeyTest
    {
        [Fact]
        public void Create_and_initialize_foreign_key()
        {
            var table = new Table("dbo.MyTable");
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");

            table.AddColumn(column0);
            table.AddColumn(column1);

            var referencedTable = new Table("dbo.MyReferencedTable");
            var referencedColumn0 = new Column("ReferencedFoo", "int");
            var referencedColumn1 = new Column("ReferencedBar", "int");

            referencedTable.AddColumn(referencedColumn0);
            referencedTable.AddColumn(referencedColumn1);

            var foreignKey = new ForeignKey(
                "MyForeignKey", 
                new[] { column0, column1 },
                new[] { referencedColumn0, referencedColumn1 },
                cascadeDelete: true);

            Assert.Equal("MyForeignKey", foreignKey.Name);
            Assert.IsAssignableFrom<IReadOnlyList<Column>>(foreignKey.Columns);
            Assert.Equal(2, foreignKey.Columns.Count);
            Assert.Same(column0, foreignKey.Columns[0]);
            Assert.Same(column1, foreignKey.Columns[1]);
            Assert.Same(table, foreignKey.Table);
            Assert.IsAssignableFrom<IReadOnlyList<Column>>(foreignKey.ReferencedColumns);
            Assert.Equal(2, foreignKey.ReferencedColumns.Count);
            Assert.Same(referencedColumn0, foreignKey.ReferencedColumns[0]);
            Assert.Same(referencedColumn1, foreignKey.ReferencedColumns[1]);
            Assert.Same(referencedTable, foreignKey.ReferencedTable);
            Assert.True(foreignKey.CascadeDelete);
        }
    }
}
