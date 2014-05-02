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

using System.Collections.Generic;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.Relational.Tests.Model
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
    }
}
