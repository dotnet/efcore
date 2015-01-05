// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.SqlServer.Metadata;
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
                        b.Property<int>("Id").ForSqlServer().UseSequence("S", "dbo");
                        b.Key("Id").ForSqlServer().Name("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id").ForSqlServer().UseSequence("S", "OtherSchema");
                        b.Key("Id").ForSqlServer().Name("PK");
                    });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

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
                        b.Property<int>("Id").ForSqlServer().UseSequence("S1", "dbo");
                        b.Key("Id").ForSqlServer().Name("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id").ForSqlServer().UseSequence("S2", "dbo");
                        b.Key("Id").ForSqlServer().Name("PK");
                    });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

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
                        b.Property<int>("Id").ForSqlServer().UseSequence("S1", "dbo");
                        b.Property<short>("P");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id").ForSqlServer().UseSequence("S1", "dbo");
                        b.Property<short>("P").ForSqlServer().UseSequence("S2", "dbo");
                        b.Key("Id");
                    });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);
            Assert.IsType<CreateSequenceOperation>(operations[0]);
            Assert.IsType<AlterColumnOperation>(operations[1]);

            var createSequenceOperation = (CreateSequenceOperation)operations[0];

            Assert.Equal("dbo.S2", createSequenceOperation.SequenceName);
            Assert.Null(sourceModelBuilder.Model.GetEntityType("A").GetProperty("P").GenerateValueOnAdd);
        }

        [Fact]
        public void Diff_finds_dropped_sequence()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id").ForSqlServer().UseSequence("S1", "dbo");
                        b.Property<short>("P").ForSqlServer().UseSequence("S2", "dbo");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id").ForSqlServer().UseSequence("S1", "dbo");
                        b.Property<short>("P");
                        b.Key("Id");
                    });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);
            Assert.IsType<DropSequenceOperation>(operations[0]);
            Assert.IsType<AlterColumnOperation>(operations[1]);

            var dropSequenceOperation = (DropSequenceOperation)operations[0];

            Assert.Equal("dbo.S2", dropSequenceOperation.SequenceName);
            Assert.Equal(true, sourceModelBuilder.Model.GetEntityType("A").GetProperty("P").GenerateValueOnAdd);
        }

        [Fact]
        public void Diff_finds_altered_sequence()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder
                .ForSqlServer(
                    b => { b.Sequence("S", "dbo").IncrementBy(6); })
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id").ForSqlServer().UseSequence("S", "dbo");
                            b.Key("Id");
                        });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder
                .ForSqlServer(
                    b => { b.Sequence("S", "dbo").IncrementBy(7); })
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id").ForSqlServer().UseSequence("S", "dbo");
                            b.Key("Id");
                        });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

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
                        b.Property<int>("Id").ForSqlServer().UseSequence("S0", "dbo");
                        b.Property<int>("P").ForSqlServer().UseSequence("S1", "dbo");
                        b.Key("Id").ForSqlServer().Name("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id").ForSqlServer().UseSequence("S1", "dbo");
                        b.Property<int>("P").ForSqlServer().UseSequence("S0", "dbo");
                        b.Key("Id").ForSqlServer().Name("PK");
                    });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

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
        public void Sequences_are_matched_if_named_on_matching_properties_of_fuzzy_matched_entity_types()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1").ForSqlServer().UseSequence("S1");
                        b.Key("Id").ForSqlServer().Name("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1").ForSqlServer().UseSequence("S2");
                        b.Property<string>("P2");
                        b.Key("Id").ForSqlServer().Name("PK");
                    });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(3, operations.Count);

            Assert.IsType<RenameSequenceOperation>(operations[0]);
            Assert.IsType<RenameTableOperation>(operations[1]);
            Assert.IsType<AddColumnOperation>(operations[2]);

            var renameSequenceOperation = (RenameSequenceOperation)operations[0];

            Assert.Equal("S1", renameSequenceOperation.SequenceName);
            Assert.Equal("S2", renameSequenceOperation.NewSequenceName);
        }

        [Fact]
        public void Sequences_are_matched_if_specified_on_matching_properties_of_fuzzy_matched_entity_types()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder
                .ForSqlServer(
                    b => { b.Sequence("S1").IncrementBy(6); })
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<int>("P1").ForSqlServer().UseSequence("S1");
                            b.Key("Id").ForSqlServer().Name("PK");
                        });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder
                .ForSqlServer(
                    b => { b.Sequence("S2").IncrementBy(7); })
                .Entity("B",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<int>("P1").ForSqlServer().UseSequence("S2");
                            b.Property<string>("P2");
                            b.Key("Id").ForSqlServer().Name("PK");
                        });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

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
        public void Sequences_are_matched_if_named_on_properties_with_same_name_and_different_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P").ForSqlServer().Column("C1").UseSequence("S1");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P").ForSqlServer().Column("C2").UseSequence("S2");
                        b.Key("Id");
                    });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<RenameSequenceOperation>(operations[0]);
            Assert.IsType<RenameColumnOperation>(operations[1]);

            var renameSequenceOperation = (RenameSequenceOperation)operations[0];

            Assert.Equal("S1", renameSequenceOperation.SequenceName);
            Assert.Equal("S2", renameSequenceOperation.NewSequenceName);
        }

        [Fact]
        public void Sequences_are_matched_if_specified_on_properties_with_same_name_and_different_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder
                .ForSqlServer(
                    b => { b.Sequence("S1").IncrementBy(6); })
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<int>("P").ForSqlServer().Column("C1").UseSequence("S1");
                            b.Key("Id");
                        });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder
                .ForSqlServer(
                    b => { b.Sequence("S2").IncrementBy(7); })
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<int>("P").ForSqlServer().Column("C2").UseSequence("S2");
                            b.Key("Id");
                        });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

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
            sourceModelBuilder
                .ForSqlServer(
                    b => { b.Sequence("S1").IncrementBy(6); })
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<int>("P1").ForSqlServer().Column("C").UseSequence("S1");
                            b.Key("Id");
                        });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder
                .ForSqlServer(
                    b => { b.Sequence("S2").IncrementBy(7); })
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<int>("P2").ForSqlServer().Column("C").UseSequence("S2");
                            b.Key("Id");
                        });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

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
        public void Sequences_are_matched_if_named_on_different_properties_with_same_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1").ForSqlServer().Column("C").UseSequence("S1");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P2").ForSqlServer().Column("C").UseSequence("S2");
                        b.Key("Id");
                    });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);

            Assert.IsType<RenameSequenceOperation>(operations[0]);

            var renameSequenceOperation = (RenameSequenceOperation)operations[0];

            Assert.Equal("S1", renameSequenceOperation.SequenceName);
            Assert.Equal("S2", renameSequenceOperation.NewSequenceName);
        }

        [Fact]
        public void Sequences_are_matched_if_specified_on_matching_properties_with_different_clr_types()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P").ForSqlServer().UseSequence("S");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<short>("P").ForSqlServer().UseSequence("S");
                        b.Key("Id");
                    });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);

            Assert.IsType<AlterColumnOperation>(operations[0]);
        }

        [Fact]
        public void Indexes_are_not_matched_if_different_clustered_flag()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id");
                        b.Index("P1").ForSqlServer().Clustered(false);
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id");
                        b.Index("P1").ForSqlServer().Clustered();
                    });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropIndexOperation>(operations[0]);
            Assert.IsType<CreateIndexOperation>(operations[1]);

            var dropIndexOperation = (DropIndexOperation)operations[0];
            var createIndexOperation = (CreateIndexOperation)operations[1];

            Assert.Equal("IX_A_P1", dropIndexOperation.IndexName);
            Assert.Equal("IX_A_P1", createIndexOperation.IndexName);
            Assert.True(createIndexOperation.IsClustered);
        }

        [Fact]
        public void Indexes_are_not_not_clustered_by_default_but_can_be_made_clustered()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id");
                        b.Index("P1").ForSqlServer();
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id");
                        b.Index("P1").ForSqlServer().Clustered();
                    });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropIndexOperation>(operations[0]);
            Assert.IsType<CreateIndexOperation>(operations[1]);

            var dropIndexOperation = (DropIndexOperation)operations[0];
            var createIndexOperation = (CreateIndexOperation)operations[1];

            Assert.Equal("IX_A_P1", dropIndexOperation.IndexName);
            Assert.Equal("IX_A_P1", createIndexOperation.IndexName);
            Assert.True(createIndexOperation.IsClustered);
        }

        [Fact]
        public void Primary_keys_are_not_matched_if_different_clustered_flag()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").ForSqlServer().Clustered();
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").ForSqlServer().Clustered(false);
                    });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[1]);

            var dropPrimaryKeyOperation = (DropPrimaryKeyOperation)operations[0];
            var addPrimaryKeyOperation = (AddPrimaryKeyOperation)operations[1];

            Assert.Equal("PK_A", dropPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal("PK_A", addPrimaryKeyOperation.PrimaryKeyName);
            Assert.False(addPrimaryKeyOperation.IsClustered);
        }

        [Fact]
        public void Primary_keys_clustered_by_default_but_can_be_made_non_clustered()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").ForSqlServer();
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").ForSqlServer().Clustered(false);
                    });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[1]);

            var dropPrimaryKeyOperation = (DropPrimaryKeyOperation)operations[0];
            var addPrimaryKeyOperation = (AddPrimaryKeyOperation)operations[1];

            Assert.Equal("PK_A", dropPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal("PK_A", addPrimaryKeyOperation.PrimaryKeyName);
            Assert.False(addPrimaryKeyOperation.IsClustered);
        }

        #endregion

        [Fact]
        public void Diff_finds_altered_column_if_string_property_added_to_primary_key()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<string>("P");
                            b.Key("Id").ForRelational().Name("PK");
                        });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<string>("P");
                            b.Key("Id", "P").ForRelational().Name("PK");
                        });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(3, operations.Count);
            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
            Assert.IsType<AlterColumnOperation>(operations[1]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[2]);

            var alterColumnOperation = (AlterColumnOperation)operations[1];

            Assert.Equal("P", alterColumnOperation.NewColumn.Name);
            Assert.Null(alterColumnOperation.NewColumn.DataType);
        }

        [Fact]
        public void Diff_finds_altered_column_if_string_property_removed_from_primary_key()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<string>("P");
                            b.Key("Id", "P").ForRelational().Name("PK");
                        });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<string>("P");
                            b.Key("Id").ForRelational().Name("PK");
                        });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(3, operations.Count);
            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
            Assert.IsType<AlterColumnOperation>(operations[1]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[2]);

            var alterColumnOperation = (AlterColumnOperation)operations[1];

            Assert.Equal("P", alterColumnOperation.NewColumn.Name);
            Assert.Null(alterColumnOperation.NewColumn.DataType);
        }

        [Fact]
        public void Diff_finds_altered_column_if_string_property_added_to_unique_constraint()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            var p1 = b.Property<int>("P1").Metadata;
                            b.Property<string>("P2");
                            b.Key("Id");
                            b.Metadata.AddKey(new[] { p1 }).Relational().Name = "UC";
                        });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            var p1 = b.Property<int>("P1").Metadata;
                            var p2 = b.Property<string>("P2").Metadata;
                            b.Key("Id");
                            b.Metadata.AddKey(new[] { p1, p2 }).Relational().Name = "UC";
                        });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(3, operations.Count);
            Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
            Assert.IsType<AlterColumnOperation>(operations[1]);
            Assert.IsType<AddUniqueConstraintOperation>(operations[2]);

            var alterColumnOperation = (AlterColumnOperation)operations[1];

            Assert.Equal("P2", alterColumnOperation.NewColumn.Name);
            Assert.Null(alterColumnOperation.NewColumn.DataType);
        }

        [Fact]
        public void Diff_finds_altered_column_if_string_property_removed_from_unique_constraint()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            var p1 = b.Property<int>("P1").Metadata;
                            var p2 = b.Property<string>("P2").Metadata;
                            b.Key("Id");
                            b.Metadata.AddKey(new[] { p1, p2 }).Relational().Name = "UC";
                        });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            var p1 = b.Property<int>("P1").Metadata;
                            b.Property<string>("P2");
                            b.Key("Id");
                            b.Metadata.AddKey(new[] { p1 }).Relational().Name = "UC";
                        });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(3, operations.Count);
            Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
            Assert.IsType<AlterColumnOperation>(operations[1]);
            Assert.IsType<AddUniqueConstraintOperation>(operations[2]);

            var alterColumnOperation = (AlterColumnOperation)operations[1];

            Assert.Equal("P2", alterColumnOperation.NewColumn.Name);
            Assert.Null(alterColumnOperation.NewColumn.DataType);
        }

        [Fact]
        public void Diff_finds_altered_columns_if_string_property_added_to_foreign_key()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<string>("P1");
                            b.Key("Id");
                        });
            sourceModelBuilder
                .Entity("B",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<string>("P2");
                            b.Key("Id");
                            b.ForeignKey("A", "Id");
                        });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<string>("P1");
                            b.Key("Id", "P1");
                        });
            targetModelBuilder
                .Entity("B",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<string>("P2");
                            b.Key("Id");
                            b.ForeignKey("A", "Id", "P2");
                        });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(6, operations.Count);
            Assert.IsType<DropForeignKeyOperation>(operations[0]);
            Assert.IsType<DropPrimaryKeyOperation>(operations[1]);
            Assert.IsType<AlterColumnOperation>(operations[2]);
            Assert.IsType<AlterColumnOperation>(operations[3]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[4]);
            Assert.IsType<AddForeignKeyOperation>(operations[5]);

            var alterColumnOperation1 = (AlterColumnOperation)operations[2];
            var alterColumnOperation2 = (AlterColumnOperation)operations[3];

            Assert.Equal("P1", alterColumnOperation1.NewColumn.Name);
            Assert.Equal("P2", alterColumnOperation2.NewColumn.Name);
            Assert.Null(alterColumnOperation1.NewColumn.DataType);
            Assert.Null(alterColumnOperation2.NewColumn.DataType);
        }

        [Fact]
        public void Diff_finds_altered_columns_if_string_property_removed_from_foreign_key()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<string>("P1");
                            b.Key("Id", "P1");
                        });
            sourceModelBuilder
                .Entity("B",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<string>("P2");
                            b.Key("Id");
                            b.ForeignKey("A", "Id", "P2");
                        });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder
                .Entity("A",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<string>("P1");
                            b.Key("Id");
                        });
            targetModelBuilder
                .Entity("B",
                    b =>
                        {
                            b.Property<int>("Id");
                            b.Property<string>("P2");
                            b.Key("Id");
                            b.ForeignKey("A", "Id");
                        });

            var operations = Diff(sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(6, operations.Count);
            Assert.IsType<DropForeignKeyOperation>(operations[0]);
            Assert.IsType<DropPrimaryKeyOperation>(operations[1]);
            Assert.IsType<AlterColumnOperation>(operations[2]);
            Assert.IsType<AlterColumnOperation>(operations[3]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[4]);
            Assert.IsType<AddForeignKeyOperation>(operations[5]);

            var alterColumnOperation1 = (AlterColumnOperation)operations[2];
            var alterColumnOperation2 = (AlterColumnOperation)operations[3];

            Assert.Equal("P1", alterColumnOperation1.NewColumn.Name);
            Assert.Equal("P2", alterColumnOperation2.NewColumn.Name);
            Assert.Null(alterColumnOperation1.NewColumn.DataType);
            Assert.Null(alterColumnOperation2.NewColumn.DataType);
        }

        private static IReadOnlyList<MigrationOperation> Diff(IModel sourceModel, IModel targetModel)
        {
            var extensionProvider = new SqlServerMetadataExtensionProvider();
            var typeMapper = new SqlServerTypeMapper();
            var operationFactory = new SqlServerMigrationOperationFactory(extensionProvider);
            var operationProcessor = new SqlServerMigrationOperationProcessor(
                extensionProvider, typeMapper, operationFactory);
            var modelDiffer = new SqlServerModelDiffer(
                extensionProvider, typeMapper, operationFactory, operationProcessor);

            return modelDiffer.Diff(sourceModel, targetModel);
        }
    }
}
