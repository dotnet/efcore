// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.Relational.Tests.Model
{
    public class DatabaseTest
    {
        [Fact]
        public void Create_and_initialize_database()
        {
            var database = new Database("MyDatabase");

            Assert.Equal("MyDatabase", database.Name);
        }

        [Fact]
        public void Sequences_gets_read_only_list_of_sequences()
        {
            var database = new Database("MyDatabase");
            var sequence0 = new Sequence("dbo.MySequence0");
            var sequence1 = new Sequence("dbo.MySequence1");

            database.AddSequence(sequence0);
            database.AddSequence(sequence1);

            Assert.IsAssignableFrom<IReadOnlyList<Sequence>>(database.Sequences);
            Assert.Equal(2, database.Sequences.Count);
            Assert.Same(sequence0, database.Sequences[0]);
            Assert.Same(sequence1, database.Sequences[1]);
        }

        [Fact]
        public void AddSequence_adds_specified_sequence()
        {
            var database = new Database("MyDatabase");

            Assert.Equal(0, database.Sequences.Count);

            var sequence = new Sequence("dbo.MySequence");
            database.AddSequence(sequence);

            Assert.Equal(1, database.Sequences.Count);
            Assert.Same(database, sequence.Database);
            Assert.Same(sequence, database.Sequences[0]);
        }

        [Fact]
        public void GetSequence_finds_sequence_by_name()
        {
            var database = new Database("MyDatabase");
            var sequence0 = new Sequence("dbo.MySequence0");
            var sequence1 = new Sequence("dbo.MySequence1");

            database.AddSequence(sequence0);
            database.AddSequence(sequence1);

            Assert.Same(sequence0, database.GetSequence("dbo.MySequence0"));
            Assert.Same(sequence1, database.GetSequence("dbo.MySequence1"));
        }

        [Fact]
        public void Tables_gets_read_only_list_of_tables()
        {
            var database = new Database("MyDatabase");
            var table0 = new Table("dbo.MyTable0");
            var table1 = new Table("dbo.MyTable1");

            database.AddTable(table0);
            database.AddTable(table1);

            Assert.IsAssignableFrom<IReadOnlyList<Table>>(database.Tables);
            Assert.Equal(2, database.Tables.Count);
            Assert.Same(table0, database.Tables[0]);
            Assert.Same(table1, database.Tables[1]);
        }

        [Fact]
        public void AddTable_adds_specified_table()
        {
            var database = new Database("MyDatabase");

            Assert.Equal(0, database.Tables.Count);

            var table = new Table("dbo.MyTable");
            database.AddTable(table);

            Assert.Equal(1, database.Tables.Count);
            Assert.Same(database, table.Database);
            Assert.Same(table, database.Tables[0]);
        }

        [Fact]
        public void GetTable_finds_table_by_name()
        {
            var database = new Database("MyDatabase");
            var table0 = new Table("dbo.MyTable0");
            var table1 = new Table("dbo.MyTable1");

            database.AddTable(table0);
            database.AddTable(table1);

            Assert.Same(table0, database.GetTable("dbo.MyTable0"));
            Assert.Same(table1, database.GetTable("dbo.MyTable1"));
        }
    }
}
