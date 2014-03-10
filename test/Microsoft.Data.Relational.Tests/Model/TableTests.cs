// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.Relational.Tests.Model
{
    public class TableTests
    {
        [Fact]
        public void Database_gets_parent_database()
        {
            var database = new Database("Database");
            var table = new Table("Table");

            Assert.Null(table.Database);

            database.AddTable(table);

            Assert.Same(database, table.Database);
        }

        [Fact]
        public void Columns_gets_read_only_list_of_columns()
        {
            var table = new Table("Table");
            var column0 = new Column("Column0", "int");
            var column1 = new Column("Column1", "int");
            var column2 = new Column("Column2", "int");

            table.AddColumn(column0);
            table.AddColumn(column1);
            table.AddColumn(column2);

            Assert.IsAssignableFrom<IReadOnlyList<Column>>(table.Columns);
            Assert.Equal(3, table.Columns.Count);
            Assert.Same(column0, table.Columns[0]);
            Assert.Same(column1, table.Columns[1]);
            Assert.Same(column2, table.Columns[2]);
        }

        [Fact]
        public void AddColumn_adds_specified_column()
        {
            var table = new Table("Table");

            Assert.Equal(0, table.Columns.Count);

            var column = new Column("Column", "int");
            table.AddColumn(column);

            Assert.Equal(1, table.Columns.Count);
            Assert.Same(table, column.Table);
            Assert.Same(column, table.Columns[0]);
        }

        [Fact]
        public void RemoveColumn_removes_specified_column()
        {
            var table = new Table("Table");
            var column = new Column("Column", "int");
            table.AddColumn(column);

            Assert.Equal(1, table.Columns.Count);

            Assert.True(table.RemoveColumn(column));

            Assert.Equal(0, table.Columns.Count);
            Assert.Null(column.Table);
        }

        [Fact]
        public void RemoveColumn_removes_specified_column_from_primary_key_columns()
        {
            var table = new Table("Table");
            var column0 = new Column("Column0", "int");
            var column1 = new Column("Column1", "int");
            table.AddColumn(column0);
            table.AddColumn(column1);
            var primaryKey = new PrimaryKey("PrimaryKey", column0);
            primaryKey.AddColumn(column1);
            table.PrimaryKey = primaryKey;

            Assert.Equal(2, table.Columns.Count);
            Assert.Equal(2, table.PrimaryKey.Columns.Count);

            Assert.True(table.RemoveColumn(column0));

            Assert.Equal(1, table.Columns.Count);
            Assert.Equal(1, table.PrimaryKey.Columns.Count);
            Assert.Same(column1, table.Columns[0]);
            Assert.Same(column1, table.PrimaryKey.Columns[0]);
            Assert.Null(column0.Table);
        }

        [Fact]
        public void RemoveColumn_does_not_remove_single_primary_key_column()
        {
            var table = new Table("Table");
            var column = new Column("Column", "int");
            table.AddColumn(column);
            var primaryKey = new PrimaryKey("PrimaryKey", column);
            table.PrimaryKey = primaryKey;

            Assert.Equal(1, table.Columns.Count);
            Assert.Equal(1, table.PrimaryKey.Columns.Count);

            Assert.False(table.RemoveColumn(column));

            Assert.Equal(1, table.Columns.Count);
            Assert.Equal(1, table.PrimaryKey.Columns.Count);
            Assert.Same(column, table.Columns[0]);
            Assert.Same(column, table.PrimaryKey.Columns[0]);
            Assert.Same(table, column.Table);
        }
    }
}
