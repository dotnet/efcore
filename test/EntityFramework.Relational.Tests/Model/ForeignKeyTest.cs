// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Model
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

        public void Clone_replicates_instance_and_adds_column_clones_to_cache()
        {
            var column0 = new Column("C0", typeof(int));
            var column1 = new Column("C1", typeof(int));
            var referencedColumn0 = new Column("RC0", typeof(int));
            var referencedColumn1 = new Column("RC1", typeof(int));
            var foreignKey 
                = new ForeignKey(
                    "FK", 
                    new[] { column0, column1 },
                    new[] { referencedColumn0, referencedColumn1 }, 
                    cascadeDelete: true);

            var cloneContext = new CloneContext();
            var clone = foreignKey.Clone(cloneContext);

            Assert.NotSame(foreignKey, clone);
            Assert.Equal("FK", clone.Name);
            Assert.Equal(2, clone.Columns.Count);
            Assert.NotSame(column0, clone.Columns[0]);
            Assert.NotSame(column1, clone.Columns[1]);
            Assert.Equal("C0", clone.Columns[0].Name);
            Assert.Equal("C1", clone.Columns[1].Name);
            Assert.Equal(2, clone.ReferencedColumns.Count);
            Assert.NotSame(referencedColumn0, clone.ReferencedColumns[0]);
            Assert.NotSame(referencedColumn1, clone.ReferencedColumns[1]);
            Assert.Equal("RC0", clone.ReferencedColumns[0].Name);
            Assert.Equal("RC1", clone.ReferencedColumns[1].Name);
            Assert.True(clone.CascadeDelete);

            Assert.Same(clone.Columns[0], cloneContext.GetOrAdd(column0, () => null));
            Assert.Same(clone.Columns[1], cloneContext.GetOrAdd(column1, () => null));
            Assert.Same(clone.ReferencedColumns[0], cloneContext.GetOrAdd(referencedColumn0, () => null));
            Assert.Same(clone.ReferencedColumns[1], cloneContext.GetOrAdd(referencedColumn1, () => null));
        }

        public void Clone_gets_column_clones_from_cache()
        {
            var column0 = new Column("C0", typeof(int));
            var column1 = new Column("C1", typeof(int));
            var referencedColumn0 = new Column("RC0", typeof(int));
            var referencedColumn1 = new Column("RC1", typeof(int));
            var foreignKey
                = new ForeignKey(
                    "FK",
                    new[] { column0, column1 },
                    new[] { referencedColumn0, referencedColumn1 },
                    cascadeDelete: true);

            var cloneContext = new CloneContext();
            var columnClone0 = column0.Clone(cloneContext);
            var columnClone1 = column1.Clone(cloneContext);
            var referencedColumnClone0 = referencedColumn0.Clone(cloneContext);
            var referencedColumnClone1 = referencedColumn1.Clone(cloneContext);
            var clone = foreignKey.Clone(cloneContext);

            Assert.NotSame(foreignKey, clone);
            Assert.Equal(2, clone.Columns.Count);
            Assert.Same(columnClone0, clone.Columns[0]);
            Assert.Same(columnClone1, clone.Columns[1]);
            Assert.Equal(2, clone.ReferencedColumns.Count);
            Assert.Same(referencedColumnClone0, clone.ReferencedColumns[0]);
            Assert.Same(referencedColumnClone1, clone.ReferencedColumns[1]);
        }
    }
}
