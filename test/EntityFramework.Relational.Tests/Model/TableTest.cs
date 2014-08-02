// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Tests.Model
{
    public class TableTest
    {
        [Fact]
        public void Create_and_initialize_table()
        {
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");
            var table = new Table("dbo.MyTable", new[] { column0, column1 });

            Assert.Equal("dbo.MyTable", table.Name);
            Assert.IsAssignableFrom<IReadOnlyList<Column>>(table.Columns);
            Assert.Equal(2, table.Columns.Count);
            Assert.Same(column0, table.Columns[0]);
            Assert.Same(column1, table.Columns[1]);
            Assert.Same(table, column0.Table);
            Assert.Same(table, column1.Table);
        }

        [Fact]
        public void Can_set_name()
        {
            var table = new Table("dbo.Table");

            Assert.Equal("dbo.Table", table.Name);

            table.Name = "dbo.RenamedTable";

            Assert.Equal("dbo.RenamedTable", table.Name);
        }

        [Fact]
        public void Can_set_schema()
        {
            var table = new Table("dbo.Table");

            Assert.Equal("dbo.Table", table.Name);

            table.Name = "renamedSchema.Table";

            Assert.Equal("renamedSchema.Table", table.Name);
        }

        [Fact]
        public void Columns_gets_read_only_list_of_columns()
        {
            var table = new Table("dbo.MyTable");
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");

            table.AddColumn(column0);
            table.AddColumn(column1);

            Assert.IsAssignableFrom<IReadOnlyList<Column>>(table.Columns);
            Assert.Equal(2, table.Columns.Count);
            Assert.Same(column0, table.Columns[0]);
            Assert.Same(column1, table.Columns[1]);
        }

        [Fact]
        public void AddColumn_adds_specified_column()
        {
            var table = new Table("dbo.MyTable");

            Assert.Equal(0, table.Columns.Count);

            var column = new Column("Foo", "int");
            table.AddColumn(column);

            Assert.Equal(1, table.Columns.Count);
            Assert.Same(table, column.Table);
            Assert.Same(column, table.Columns[0]);
        }

        [Fact]
        public void RemoveColumn_removes_specified_column()
        {
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");
            var table = new Table("dbo.MyTable", new[] { column0, column1 });

            Assert.Equal(2, table.Columns.Count);

            table.RemoveColumn("Bar");

            Assert.Equal(1, table.Columns.Count);
            Assert.Same(column0, table.Columns[0]);
            Assert.Null(column1.Table);
        }

        [Fact]
        public void GetColumn_finds_column_by_name()
        {
            var table = new Table("dbo.MyTable");
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");

            table.AddColumn(column0);
            table.AddColumn(column1);

            Assert.Same(column0, table.GetColumn("Foo"));
            Assert.Same(column1, table.GetColumn("Bar"));
        }

        [Fact]
        public void Get_set_primary_key()
        {
            var columns = new[] { new Column("Foo", "int") };
            var table = new Table("dbo.MyTable", columns);
            var primaryKey = new PrimaryKey("PK", columns);

            Assert.Null(table.PrimaryKey);

            table.PrimaryKey = primaryKey;

            Assert.Same(primaryKey, table.PrimaryKey);

            table.PrimaryKey = null;

            Assert.Null(table.PrimaryKey);
        }

        [Fact]
        public void ForeignKeys_gets_read_only_list_of_foreign_keys()
        {
            var column00 = new Column("C00", "int");
            var column01 = new Column("C01", "int");
            var table0 = new Table("dbo.T0", new[] { column00, column01 });
            var column10 = new Column("C10", "int");
            var column11 = new Column("C11", "int");
            var table1 = new Table("dbo.T0", new[] { column10, column11 });
            var foreignKey0 = new ForeignKey("FK0", new[] { column00 }, new[] { column10 });
            var foreignKey1 = new ForeignKey("FK1", new[] { column01 }, new[] { column11 });

            table0.AddForeignKey(foreignKey0);
            table0.AddForeignKey(foreignKey1);

            Assert.IsAssignableFrom<IReadOnlyList<ForeignKey>>(table0.ForeignKeys);
            Assert.Equal(2, table0.ForeignKeys.Count);
            Assert.Same(foreignKey0, table0.ForeignKeys[0]);
            Assert.Same(foreignKey1, table0.ForeignKeys[1]);
        }

        [Fact]
        public void AddForeignKey_adds_specified_foreign_key()
        {
            var column00 = new Column("C00", "int");
            var column01 = new Column("C01", "int");
            var table0 = new Table("dbo.T0", new[] { column00, column01 });
            var column10 = new Column("C10", "int");
            var column11 = new Column("C11", "int");
            var table1 = new Table("dbo.T0", new[] { column10, column11 });
            var foreignKey0 = new ForeignKey("FK0", new[] { column00 }, new[] { column10 });
            var foreignKey1 = new ForeignKey("FK1", new[] { column01 }, new[] { column11 });

            Assert.Equal(0, table0.ForeignKeys.Count);

            table0.AddForeignKey(foreignKey0);

            Assert.Equal(1, table0.ForeignKeys.Count);
            Assert.Same(foreignKey0, table0.ForeignKeys[0]);

            table0.AddForeignKey(foreignKey1);

            Assert.Equal(2, table0.ForeignKeys.Count);
            Assert.Same(foreignKey0, table0.ForeignKeys[0]);
            Assert.Same(foreignKey1, table0.ForeignKeys[1]);
        }

        [Fact]
        public void RemoveForeignKey_removes_specified_foreign_key()
        {
            var column00 = new Column("C00", "int");
            var column01 = new Column("C01", "int");
            var table0 = new Table("dbo.T0", new[] { column00, column01 });
            var column10 = new Column("C10", "int");
            var column11 = new Column("C11", "int");
            var table1 = new Table("dbo.T0", new[] { column10, column11 });
            var foreignKey0 = new ForeignKey("FK0", new[] { column00 }, new[] { column10 });
            var foreignKey1 = new ForeignKey("FK1", new[] { column01 }, new[] { column11 });

            table0.AddForeignKey(foreignKey0);
            table0.AddForeignKey(foreignKey1);

            Assert.Equal(2, table0.ForeignKeys.Count);

            table0.RemoveForeignKey("FK1");

            Assert.Equal(1, table0.ForeignKeys.Count);
            Assert.Same(foreignKey0, table0.ForeignKeys[0]);
        }

        [Fact]
        public void GetForeignKey_finds_foreign_key_by_name()
        {
            var column00 = new Column("C00", "int");
            var column01 = new Column("C01", "int");
            var table0 = new Table("dbo.T0", new[] { column00, column01 });
            var column10 = new Column("C10", "int");
            var column11 = new Column("C11", "int");
            var table1 = new Table("dbo.T0", new[] { column10, column11 });
            var foreignKey0 = new ForeignKey("FK0", new[] { column00 }, new[] { column10 });
            var foreignKey1 = new ForeignKey("FK1", new[] { column01 }, new[] { column11 });

            table0.AddForeignKey(foreignKey0);
            table0.AddForeignKey(foreignKey1);

            Assert.Same(foreignKey0, table0.GetForeignKey("FK0"));
            Assert.Same(foreignKey1, table0.GetForeignKey("FK1"));
        }

        [Fact]
        public void Indexes_gets_read_only_list_of_indexes()
        {
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");
            var table = new Table("dbo.MyTable", new[] { column0, column1 });
            var index0 = new Index("MyIndex0", new[] { column0 });
            var index1 = new Index("MyIndex1", new[] { column1 });

            table.AddIndex(index0);
            table.AddIndex(index1);

            Assert.IsAssignableFrom<IReadOnlyList<Index>>(table.Indexes);
            Assert.Equal(2, table.Indexes.Count);
            Assert.Same(index0, table.Indexes[0]);
            Assert.Same(index1, table.Indexes[1]);
        }

        [Fact]
        public void AddIndex_adds_specified_column()
        {
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");
            var table = new Table("dbo.MyTable", new[] { column0, column1 });

            Assert.Equal(0, table.Indexes.Count);

            var index = new Index("MyIndex", new[] { column1 });
            table.AddIndex(index);

            Assert.Equal(1, table.Indexes.Count);
            Assert.Same(column1, index.Columns[0]);
            Assert.Same(table, index.Table);
        }

        [Fact]
        public void RemoveIndex_removes_specified_index()
        {
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");
            var table = new Table("dbo.MyTable", new[] { column0, column1 });
            var index0 = new Index("MyIndex0", new[] { column0 });
            var index1 = new Index("MyIndex1", new[] { column1 });

            table.AddIndex(index0);
            table.AddIndex(index1);

            Assert.Equal(2, table.Indexes.Count);

            table.RemoveIndex("MyIndex1");

            Assert.Equal(1, table.Indexes.Count);
            Assert.Same(index0, table.Indexes[0]);
        }

        [Fact]
        public void GetIndex_finds_index_by_name()
        {
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");
            var table = new Table("dbo.MyTable", new[] { column0, column1 });
            var index0 = new Index("MyIndex0", new[] { column0 });
            var index1 = new Index("MyIndex1", new[] { column1 });

            table.AddIndex(index0);
            table.AddIndex(index1);

            Assert.Same(index0, table.GetIndex("MyIndex0"));
            Assert.Same(index1, table.GetIndex("MyIndex1"));
        }

        [Fact]
        public void Clone_replicates_instance_and_uses_cache()
        {
            var column00 = new Column("C00", typeof(int));
            var column01 = new Column("C01", typeof(string));
            var column10 = new Column("C10", typeof(int));
            var column11 = new Column("C11", typeof(string));
            var table0 = new Table("dbo.T0", new[] { column00, column01 });
            var table1 = new Table("dbo.T1", new[] { column10, column11 });
            var primaryKey0 = new PrimaryKey("PK0", new[] { column00 });
            var primaryKey1 = new PrimaryKey("PK1", new[] { column10 });
            var foreignKey0 = new ForeignKey("FK0", new[] { column01 }, new[] { column10 });
            var foreignKey1 = new ForeignKey("FK1", new[] { column11 }, new[] { column00 });
            var index0 = new Index("IX0", new[] { column00 });
            var index1 = new Index("IX1", new[] { column10 });

            table0.PrimaryKey = primaryKey0;
            table1.PrimaryKey = primaryKey1;
            table0.AddForeignKey(foreignKey0);
            table1.AddForeignKey(foreignKey1);
            table0.AddIndex(index0);
            table1.AddIndex(index1);

            var cloneContext = new CloneContext();
            var clone0 = table0.Clone(cloneContext);

            Assert.NotSame(table0, clone0);
            Assert.Equal("dbo.T0", clone0.Name);

            Assert.Equal(2, clone0.Columns.Count);
            Assert.NotSame(column00, clone0.Columns[0]);
            Assert.NotSame(column01, clone0.Columns[1]);
            Assert.Equal("C00", clone0.Columns[0].Name);
            Assert.Equal("C01", clone0.Columns[1].Name);
            Assert.Equal(typeof(int), clone0.Columns[0].ClrType);
            Assert.Equal(typeof(string), clone0.Columns[1].ClrType);

            Assert.NotNull(clone0.PrimaryKey);
            Assert.NotSame(primaryKey0, clone0.PrimaryKey);
            Assert.Equal("PK0", clone0.PrimaryKey.Name);
            Assert.Equal(1, clone0.PrimaryKey.Columns.Count);
            Assert.Same(clone0.Columns[0], clone0.PrimaryKey.Columns[0]);

            Assert.Equal(1, clone0.ForeignKeys.Count);
            Assert.NotSame(foreignKey0, clone0.ForeignKeys[0]);
            Assert.Equal("FK0", clone0.ForeignKeys[0].Name);
            Assert.Equal(1, clone0.ForeignKeys[0].Columns.Count);
            Assert.Equal(1, clone0.ForeignKeys[0].ReferencedColumns.Count);
            Assert.Same(clone0.Columns[1], clone0.ForeignKeys[0].Columns[0]);
            Assert.Same(cloneContext.GetOrAdd(column10, () => null), clone0.ForeignKeys[0].ReferencedColumns[0]);

            Assert.Equal(1, clone0.Indexes.Count);
            Assert.NotSame(index0, clone0.Indexes[0]);
            Assert.Equal("IX0", clone0.Indexes[0].Name);
            Assert.Equal(1, clone0.Indexes[0].Columns.Count);
            Assert.Same(clone0.Columns[0], clone0.Indexes[0].Columns[0]);

            Assert.Equal(3, cloneContext.ItemCount);
            Assert.Same(clone0.Columns[0], cloneContext.GetOrAdd(column00, () => null));
            Assert.Same(clone0.Columns[1], cloneContext.GetOrAdd(column01, () => null));
            Assert.NotNull(cloneContext.GetOrAdd(column10, () => null));

            var clone1 = table1.Clone(cloneContext);

            Assert.NotSame(table1, clone1);
            Assert.Equal("dbo.T1", clone1.Name);

            Assert.Equal(2, clone1.Columns.Count);
            Assert.NotSame(column10, clone1.Columns[0]);
            Assert.NotSame(column11, clone1.Columns[1]);
            Assert.Equal("C10", clone1.Columns[0].Name);
            Assert.Equal("C11", clone1.Columns[1].Name);
            Assert.Equal(typeof(int), clone1.Columns[0].ClrType);
            Assert.Equal(typeof(string), clone1.Columns[1].ClrType);

            Assert.NotNull(clone1.PrimaryKey);
            Assert.NotSame(primaryKey1, clone1.PrimaryKey);
            Assert.Equal("PK1", clone1.PrimaryKey.Name);
            Assert.Equal(1, clone1.PrimaryKey.Columns.Count);
            Assert.Same(clone1.Columns[0], clone1.PrimaryKey.Columns[0]);

            Assert.Equal(1, clone1.ForeignKeys.Count);
            Assert.NotSame(foreignKey1, clone1.ForeignKeys[0]);
            Assert.Equal("FK1", clone1.ForeignKeys[0].Name);
            Assert.Equal(1, clone1.ForeignKeys[0].Columns.Count);
            Assert.Equal(1, clone1.ForeignKeys[0].ReferencedColumns.Count);
            Assert.Same(clone1.Columns[1], clone1.ForeignKeys[0].Columns[0]);
            Assert.Same(clone0.Columns[0], clone1.ForeignKeys[0].ReferencedColumns[0]);
            Assert.Same(clone1.Columns[0], clone0.ForeignKeys[0].ReferencedColumns[0]);

            Assert.Equal(1, clone1.Indexes.Count);
            Assert.NotSame(index1, clone1.Indexes[0]);
            Assert.Equal("IX1", clone1.Indexes[0].Name);
            Assert.Equal(1, clone1.Indexes[0].Columns.Count);
            Assert.Same(clone1.Columns[0], clone1.Indexes[0].Columns[0]);

            Assert.Equal(4, cloneContext.ItemCount);
            Assert.Same(clone0.Columns[0], cloneContext.GetOrAdd(column00, () => null));
            Assert.Same(clone0.Columns[1], cloneContext.GetOrAdd(column01, () => null));
            Assert.Same(clone1.Columns[0], cloneContext.GetOrAdd(column10, () => null));
            Assert.Same(clone1.Columns[1], cloneContext.GetOrAdd(column11, () => null));
        }
    }
}
