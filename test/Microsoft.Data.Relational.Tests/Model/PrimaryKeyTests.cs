// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.Relational.Tests.Model
{
    public class PrimaryKeyTests
    {
        [Fact]
        public void Constructor_checks_arguments()
        {
            Assert.Equal(
                "column",
                Assert.Throws<ArgumentNullException>(() => new PrimaryKey("PrimaryKey", null)).ParamName);
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
            var primaryKey = new PrimaryKey("PrimaryKey", column0);
            primaryKey.AddColumn(column1);
            primaryKey.AddColumn(column2);

            Assert.IsAssignableFrom<IReadOnlyList<Column>>(primaryKey.Columns);
            Assert.Equal(3, primaryKey.Columns.Count);
            Assert.Same(column0, primaryKey.Columns[0]);
            Assert.Same(column1, primaryKey.Columns[1]);
            Assert.Same(column2, primaryKey.Columns[2]);
        }

        [Fact]
        public void AddColumn_adds_specified_column()
        {
            var table = new Table("Table");
            var column0 = new Column("Column0", "int");
            var column1 = new Column("Column1", "int");
            table.AddColumn(column0);
            table.AddColumn(column1);
            var primaryKey = new PrimaryKey("PrimaryKey", column0);

            Assert.Equal(1, primaryKey.Columns.Count);

            primaryKey.AddColumn(column1);

            Assert.Equal(2, primaryKey.Columns.Count);
            Assert.Same(column0, primaryKey.Columns[0]);
            Assert.Same(column1, primaryKey.Columns[1]);
        }

        [Fact]
        public void RemoveColumn_removes_specified_column()
        {
            var table = new Table("Table");
            var column0 = new Column("Column0", "int");
            var column1 = new Column("Column1", "int");
            table.AddColumn(column0);
            table.AddColumn(column1);
            var primaryKey = new PrimaryKey("PrimaryKey", column0);
            primaryKey.AddColumn(column1);

            Assert.Equal(2, primaryKey.Columns.Count);

            Assert.True(primaryKey.RemoveColumn(column0));

            Assert.Equal(1, primaryKey.Columns.Count);
            Assert.Same(column1, primaryKey.Columns[0]);
        }

        [Fact]
        public void RemoveColumn_does_not_remove_single_primary_key_column()
        {
            var table = new Table("Table");
            var column = new Column("Column", "int");
            table.AddColumn(column);
            var primaryKey = new PrimaryKey("PrimaryKey", column);

            Assert.Equal(1, primaryKey.Columns.Count);

            Assert.False(primaryKey.RemoveColumn(column));

            Assert.Equal(1, primaryKey.Columns.Count);
            Assert.Same(column, primaryKey.Columns[0]);
        }
    }
}
