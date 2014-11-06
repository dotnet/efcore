// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Relational.Model;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Model
{
    public class DatabaseModelTest
    {
        [Fact]
        public void Create_and_initialize_database()
        {
            var database = new DatabaseModel();

            Assert.NotNull(database.Tables);
        }

        [Fact]
        public void Sequences_gets_read_only_list_of_sequences()
        {
            var database = new DatabaseModel();
            var sequence0 = new Sequence("dbo.MySequence0", typeof(long), 0, 1);
            var sequence1 = new Sequence("dbo.MySequence1", typeof(long), 0, 1);

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
            var database = new DatabaseModel();

            Assert.Equal(0, database.Sequences.Count);

            var sequence = new Sequence("dbo.MySequence", typeof(long), 0, 1);
            database.AddSequence(sequence);

            Assert.Equal(1, database.Sequences.Count);
            Assert.Same(sequence, database.Sequences[0]);
        }

        [Fact]
        public void RemoveSequence_removes_specified_sequence()
        {
            var database = new DatabaseModel();
            var sequence0 = new Sequence("dbo.MySequence0", typeof(long), 0, 1);
            var sequence1 = new Sequence("dbo.MySequence1", typeof(long), 0, 1);

            database.AddSequence(sequence0);
            database.AddSequence(sequence1);

            Assert.Equal(2, database.Sequences.Count);

            database.RemoveSequence("dbo.MySequence1");

            Assert.Equal(1, database.Sequences.Count);
            Assert.Same(sequence0, database.Sequences[0]);
        }

        [Fact]
        public void GetSequence_finds_sequence_by_name()
        {
            var database = new DatabaseModel();
            var sequence0 = new Sequence("dbo.MySequence0", typeof(long), 0, 1);
            var sequence1 = new Sequence("dbo.MySequence1", typeof(long), 0, 1);

            database.AddSequence(sequence0);
            database.AddSequence(sequence1);

            Assert.Same(sequence0, database.GetSequence("dbo.MySequence0"));
            Assert.Same(sequence1, database.GetSequence("dbo.MySequence1"));
        }

        [Fact]
        public void Tables_gets_read_only_list_of_tables()
        {
            var database = new DatabaseModel();
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
            var database = new DatabaseModel();

            Assert.Equal(0, database.Tables.Count);

            var table = new Table("dbo.MyTable");
            database.AddTable(table);

            Assert.Equal(1, database.Tables.Count);
            Assert.Same(table, database.Tables[0]);
        }

        [Fact]
        public void RemoveTable_removes_specified_table()
        {
            var database = new DatabaseModel();
            var table0 = new Table("dbo.MyTable0");
            var table1 = new Table("dbo.MyTable1");

            database.AddTable(table0);
            database.AddTable(table1);

            Assert.Equal(2, database.Tables.Count);

            database.RemoveTable("dbo.MyTable1");

            Assert.Equal(1, database.Tables.Count);
            Assert.Same(table0, database.Tables[0]);
        }

        [Fact]
        public void GetTable_finds_table_by_name()
        {
            var database = new DatabaseModel();
            var table0 = new Table("dbo.MyTable0");
            var table1 = new Table("dbo.MyTable1");

            database.AddTable(table0);
            database.AddTable(table1);

            Assert.Same(table0, database.GetTable("dbo.MyTable0"));
            Assert.Same(table1, database.GetTable("dbo.MyTable1"));
        }

        [Fact]
        public void TryGetTable_finds_table_by_name()
        {
            var database = new DatabaseModel();
            var table0 = new Table("dbo.MyTable0");
            var table1 = new Table("dbo.MyTable1");

            database.AddTable(table0);
            database.AddTable(table1);

            Assert.Same(table0, database.GetTable("dbo.MyTable0"));
            Assert.Same(table1, database.GetTable("dbo.MyTable1"));
        }

        [Fact]
        public void TryGetTable_returns_null_if_table_not_found()
        {
            var database = new DatabaseModel();
            var table0 = new Table("dbo.MyTable0");
            var table1 = new Table("dbo.MyTable1");

            database.AddTable(table0);
            database.AddTable(table1);

            Assert.Null(database.TryGetTable("dbo.MyTable2"));
        }

        [Fact]
        public void Clone_replicates_tables()
        {
            var databaseModel = new DatabaseModel();
            var sequence0 = new Sequence("dbo.S0", typeof(long), 0, 1);
            var sequence1 = new Sequence("dbo.S1", typeof(long), 0, 1);
            var table0 = new Table("dbo.T0");
            var table1 = new Table("dbo.T1");

            databaseModel.AddSequence(sequence0);
            databaseModel.AddSequence(sequence1);
            databaseModel.AddTable(table0);
            databaseModel.AddTable(table1);

            var clone = databaseModel.Clone();

            Assert.NotSame(databaseModel, clone);
            Assert.Equal(2, clone.Sequences.Count);
            Assert.NotSame(sequence0, clone.Sequences[0]);
            Assert.NotSame(sequence1, clone.Sequences[1]);
            Assert.Equal("dbo.S0", clone.Sequences[0].Name);
            Assert.Equal("dbo.S1", clone.Sequences[1].Name);
            Assert.Equal(2, clone.Tables.Count);
            Assert.NotSame(table0, clone.Tables[0]);
            Assert.NotSame(table1, clone.Tables[1]);
            Assert.Equal("dbo.T0", clone.Tables[0].Name);
            Assert.Equal("dbo.T1", clone.Tables[1].Name);
        }
    }
}
