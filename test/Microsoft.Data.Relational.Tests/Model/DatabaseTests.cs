// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Data.Relational.Model.Tests
{
    public class DatabaseTests
    {
        [Fact]
        public void Name_gets_database_name()
        {
            var database = new Database("Database");

            Assert.Equal("Database", database.Name);
        }

        [Fact]
        public void Tables_gets_read_only_list_of_tables()
        {
            var database = new Database("Database");
            var table0 = new Table("Table0");
            var table1 = new Table("Table1");
            var table2 = new Table("Table2");

            database.AddTable(table0);
            database.AddTable(table1);
            database.AddTable(table2);

            Assert.IsAssignableFrom<IReadOnlyList<Table>>(database.Tables);
            Assert.Equal(3, database.Tables.Count);
            Assert.Same(table0, database.Tables[0]);
            Assert.Same(table1, database.Tables[1]);
            Assert.Same(table2, database.Tables[2]);
        }

        [Fact]
        public void AddTable_adds_specified_table()
        {
            var database = new Database("Database");

            Assert.Equal(0, database.Tables.Count);

            var table = new Table("Table");
            database.AddTable(table);

            Assert.Equal(1, database.Tables.Count);
            Assert.Same(database, table.Database);
            Assert.Same(table, database.Tables[0]);
        }
    }
}
