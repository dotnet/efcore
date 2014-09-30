// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Model;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerModelDifferTest
    {
        #region Basic diffs

        [Fact]
        public void Diff_finds_moved_sequence()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id")
                            .GenerateValuesUsingSequence("dbo.S", 6);
                        b.Key("Id").KeyName("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id")
                            .GenerateValuesUsingSequence("OtherSchema.S", 6);
                        b.Key("Id").KeyName("PK");
                    });

            var operations = new SqlServerModelDiffer(new SqlServerDatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<MoveSequenceOperation>(operations[0]);

            var moveSequenceOperation = (MoveSequenceOperation)operations[0];

            Assert.Equal("dbo.S", moveSequenceOperation.SequenceName);
            Assert.Equal("OtherSchema", moveSequenceOperation.NewSchema);
        }

        [Fact]
        public void Diff_finds_renamed_sequence()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id")
                            .GenerateValuesUsingSequence("dbo.S1", 6);
                        b.Key("Id").KeyName("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id")
                            .GenerateValuesUsingSequence("dbo.S2", 6);
                        b.Key("Id").KeyName("PK");
                    });

            var operations = new SqlServerModelDiffer(new SqlServerDatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<RenameSequenceOperation>(operations[0]);

            var renameSequenceOperation = (RenameSequenceOperation)operations[0];

            Assert.Equal("dbo.S1", renameSequenceOperation.SequenceName);
            Assert.Equal("S2", renameSequenceOperation.NewSequenceName);
        }

        [Fact]
        public void Diff_finds_created_sequence()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id")
                            .GenerateValuesUsingSequence("dbo.S1", 6);
                        b.Property<short>("P");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id")
                            .GenerateValuesUsingSequence("dbo.S1", 6);
                        b.Property<short>("P")
                            .GenerateValuesUsingSequence("dbo.S2", 7);
                        b.Key("Id");
                    });

            var databaseBuilder = new SqlServerDatabaseBuilder();
            var operations = new SqlServerModelDiffer(databaseBuilder).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);
            var sourceDbModel = databaseBuilder.GetDatabase(sourceModelBuilder.Model);
            var targetDbModel = databaseBuilder.GetDatabase(targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);
            Assert.IsType<CreateSequenceOperation>(operations[0]);
            Assert.IsType<AlterColumnOperation>(operations[1]);

            var createSequenceOperation = (CreateSequenceOperation)operations[0];

            Assert.Same(targetDbModel.Sequences[1], createSequenceOperation.Sequence);

            var alterColumnOperation = (AlterColumnOperation)operations[1];

            Assert.Equal(ValueGeneration.None, sourceDbModel.GetTable(alterColumnOperation.TableName).GetColumn(alterColumnOperation.NewColumn.Name).ValueGenerationStrategy);
            Assert.Equal(ValueGeneration.OnAdd, alterColumnOperation.NewColumn.ValueGenerationStrategy);            
        }

        [Fact]
        public void Diff_finds_dropped_sequence()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id")
                            .GenerateValuesUsingSequence("dbo.S1", 6);
                        b.Property<short>("P")
                            .GenerateValuesUsingSequence("dbo.S2", 7);
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id")
                            .GenerateValuesUsingSequence("dbo.S1", 6);
                        b.Property<short>("P");
                        b.Key("Id");
                    });

            var databaseBuilder = new SqlServerDatabaseBuilder();
            var operations = new SqlServerModelDiffer(databaseBuilder).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);
            var sourceDbModel = databaseBuilder.GetDatabase(sourceModelBuilder.Model);
            var targetDbModel = databaseBuilder.GetDatabase(targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);
            Assert.IsType<DropSequenceOperation>(operations[0]);
            Assert.IsType<AlterColumnOperation>(operations[1]);

            var dropSequenceOperation = (DropSequenceOperation)operations[0];

            Assert.Equal("dbo.S2", dropSequenceOperation.SequenceName);

            var alterColumnOperation = (AlterColumnOperation)operations[1];

            Assert.Equal(ValueGeneration.OnAdd, sourceDbModel.GetTable(alterColumnOperation.TableName).GetColumn(alterColumnOperation.NewColumn.Name).ValueGenerationStrategy);
            Assert.Equal(ValueGeneration.None, targetDbModel.GetTable(alterColumnOperation.TableName).GetColumn(alterColumnOperation.NewColumn.Name).ValueGenerationStrategy);
        }

        [Fact]
        public void Diff_finds_altered_sequence()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                {
                    b.Property<int>("Id")
                        .GenerateValuesUsingSequence("dbo.S", 6);
                    b.Key("Id");
                });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                {
                    b.Property<int>("Id")
                        .GenerateValuesUsingSequence("dbo.S", 7);
                    b.Key("Id");
                });

            var operations = new SqlServerModelDiffer(new SqlServerDatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AlterSequenceOperation>(operations[0]);

            var alterSequenceOperation = (AlterSequenceOperation)operations[0];

            Assert.Equal("dbo.S", alterSequenceOperation.SequenceName);
            Assert.Equal(7, alterSequenceOperation.NewIncrementBy);
        }

        #endregion

        #region Transitive renames

        [Fact]
        public void Diff_handles_transitive_sequence_renames()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id")
                            .GenerateValuesUsingSequence("dbo.S0", 6);
                        b.Property<int>("P")
                            .GenerateValuesUsingSequence("dbo.S1", 6);
                        b.Key("Id").KeyName("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id")
                            .GenerateValuesUsingSequence("dbo.S1", 6);
                        b.Property<int>("P")
                            .GenerateValuesUsingSequence("dbo.S0", 6);
                        b.Key("Id").KeyName("PK");
                    });

            var operations = new SqlServerModelDiffer(new SqlServerDatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(3, operations.Count);

            Assert.IsType<RenameSequenceOperation>(operations[0]);
            Assert.IsType<RenameSequenceOperation>(operations[1]);
            Assert.IsType<RenameSequenceOperation>(operations[2]);

            var renameSequenceOperation0 = (RenameSequenceOperation)operations[0];
            var renameSequenceOperation1 = (RenameSequenceOperation)operations[1];
            var renameSequenceOperation2 = (RenameSequenceOperation)operations[2];

            Assert.Equal("dbo.S0", renameSequenceOperation0.SequenceName);
            Assert.Equal("__mig_tmp__0", renameSequenceOperation0.NewSequenceName);
            Assert.Equal("dbo.S1", renameSequenceOperation1.SequenceName);
            Assert.Equal("S0", renameSequenceOperation1.NewSequenceName);
            Assert.Equal("dbo.__mig_tmp__0", renameSequenceOperation2.SequenceName);
            Assert.Equal("S1", renameSequenceOperation2.NewSequenceName);
        }

        #endregion

        #region Sequence matching

        [Fact]
        public void Sequences_are_matched_if_specified_on_matching_properties_of_fuzzy_matched_entity_types()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1")
                            .GenerateValuesUsingSequence("S1", 6);
                        b.Key("Id").KeyName("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1")
                            .GenerateValuesUsingSequence("S2", 7);
                        b.Property<string>("P2");
                        b.Key("Id").KeyName("PK");
                    });

            var operations = new SqlServerModelDiffer(new SqlServerDatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(4, operations.Count);

            Assert.IsType<RenameSequenceOperation>(operations[0]);
            Assert.IsType<AlterSequenceOperation>(operations[1]);
            Assert.IsType<RenameTableOperation>(operations[2]);
            Assert.IsType<AddColumnOperation>(operations[3]);

            var renameSequenceOperation = (RenameSequenceOperation)operations[0];
            var alterSequenceOperation = (AlterSequenceOperation)operations[1];

            Assert.Equal("S1", renameSequenceOperation.SequenceName);
            Assert.Equal("S2", renameSequenceOperation.NewSequenceName);
            Assert.Equal("S2", alterSequenceOperation.SequenceName);
            Assert.Equal(7, alterSequenceOperation.NewIncrementBy);
        }

        [Fact]
        public void Sequences_are_matched_if_specified_on_properties_with_same_name_and_different_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P")
                            .ColumnName("C1")
                            .GenerateValuesUsingSequence("S1", 6);
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P")
                            .ColumnName("C2")
                            .GenerateValuesUsingSequence("S2", 7);
                        b.Key("Id");
                    });

            var operations = new SqlServerModelDiffer(new SqlServerDatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(3, operations.Count);

            Assert.IsType<RenameSequenceOperation>(operations[0]);
            Assert.IsType<AlterSequenceOperation>(operations[1]);
            Assert.IsType<RenameColumnOperation>(operations[2]);

            var renameSequenceOperation = (RenameSequenceOperation)operations[0];
            var alterSequenceOperation = (AlterSequenceOperation)operations[1];

            Assert.Equal("S1", renameSequenceOperation.SequenceName);
            Assert.Equal("S2", renameSequenceOperation.NewSequenceName);
            Assert.Equal("S2", alterSequenceOperation.SequenceName);
            Assert.Equal(7, alterSequenceOperation.NewIncrementBy);
        }

        [Fact]
        public void Sequences_are_matched_if_specified_on_different_properties_with_same_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1")
                            .ColumnName("C")
                            .GenerateValuesUsingSequence("S1", 6);
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P2")
                            .ColumnName("C")
                            .GenerateValuesUsingSequence("S2", 7);
                        b.Key("Id");
                    });

            var operations = new SqlServerModelDiffer(new SqlServerDatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<RenameSequenceOperation>(operations[0]);
            Assert.IsType<AlterSequenceOperation>(operations[1]);

            var renameSequenceOperation = (RenameSequenceOperation)operations[0];
            var alterSequenceOperation = (AlterSequenceOperation)operations[1];

            Assert.Equal("S1", renameSequenceOperation.SequenceName);
            Assert.Equal("S2", renameSequenceOperation.NewSequenceName);
            Assert.Equal("S2", alterSequenceOperation.SequenceName);
            Assert.Equal(7, alterSequenceOperation.NewIncrementBy);
        }

        [Fact]
        public void Sequences_are_matched_if_specified_on_matching_properties_with_different_clr_types()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                {
                    b.Property<int>("Id");
                    b.Property<int>("P")
                        .GenerateValuesUsingSequence("S", 6);
                    b.Key("Id");
                });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                {
                    b.Property<int>("Id");
                    b.Property<short>("P")
                        .GenerateValuesUsingSequence("S", 6);
                    b.Key("Id");
                });

            var operations = new SqlServerModelDiffer(new SqlServerDatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);

            Assert.IsType<AlterColumnOperation>(operations[0]);
        }

        #endregion
    }
}
