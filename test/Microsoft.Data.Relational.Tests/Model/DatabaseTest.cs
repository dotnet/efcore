// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Relational.Model;
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
        public void Sequences_gets_read_only_list_of_sequences()
        {
            var database = new Database("Database");
            var sequence0 = new Sequence("Sequence0");
            var sequence1 = new Sequence("Sequence1");
            var sequence2 = new Sequence("Sequence2");

            database.AddSequence(sequence0);
            database.AddSequence(sequence1);
            database.AddSequence(sequence2);

            Assert.IsAssignableFrom<IReadOnlyList<Sequence>>(database.Sequences);
            Assert.Equal(3, database.Sequences.Count);
            Assert.Same(sequence0, database.Sequences[0]);
            Assert.Same(sequence1, database.Sequences[1]);
            Assert.Same(sequence2, database.Sequences[2]);
        }

        [Fact]
        public void AddSequence_adds_specified_sequence()
        {
            var database = new Database("Database");

            Assert.Equal(0, database.Sequences.Count);

            var sequence = new Sequence("Sequence");
            database.AddSequence(sequence);

            Assert.Equal(1, database.Sequences.Count);
            Assert.Same(database, sequence.Database);
            Assert.Same(sequence, database.Sequences[0]);
        }

        [Fact]
        public void GetSequence_gets_the_sequence_with_the_specified_name()
        {
            var database = new Database("Database");
            var sequence0 = new Sequence("Sequence0");
            var sequence1 = new Sequence("Sequence1");
            var sequence2 = new Sequence("Sequence2");

            database.AddSequence(sequence0);
            database.AddSequence(sequence1);
            database.AddSequence(sequence2);

            Assert.Same(sequence0, database.GetSequence("Sequence0"));
            Assert.Same(sequence1, database.GetSequence("Sequence1"));
            Assert.Same(sequence2, database.GetSequence("Sequence2"));
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

        [Fact]
        public void GetTable_gets_the_table_with_the_specified_name()
        {
            var database = new Database("Database");
            var table0 = new Table("Table0");
            var table1 = new Table("Table1");
            var table2 = new Table("Table2");

            database.AddTable(table0);
            database.AddTable(table1);
            database.AddTable(table2);

            Assert.Same(table0, database.GetTable("Table0"));
            Assert.Same(table1, database.GetTable("Table1"));
            Assert.Same(table2, database.GetTable("Table2"));
        }
    }
}
