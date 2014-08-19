// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests
{
    public class ModelDifferTest
    {
        #region Basic diffs

        [Fact]
        public void CreateSchema_creates_operations()
        {
            var modelBuider = new BasicModelBuilder();
            modelBuider.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P0").ColumnName("C0");
                        b.Key("Id").KeyName("PK0");
                        b.ToTable("T0", "dbo");
                    });
            modelBuider.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1").ColumnName("C1");
                        b.Key("Id").KeyName("PK1");
                        b.ToTable("T1", "dbo");
                        b.ForeignKey("A", "Id").KeyName("FK").CascadeDelete(true);
                        b.Index("Id").IsUnique().IndexName("IX").IsUnique();
                    });

            var databaseBuilder = new DatabaseBuilder();
            var operations = new ModelDiffer(databaseBuilder).CreateSchema(modelBuider.Model);
            var dbModel = databaseBuilder.GetDatabase(modelBuider.Model);

            Assert.Equal(4, operations.Count);

            var createTableOperation0 = (CreateTableOperation)operations[0];
            var createTableOperation1 = (CreateTableOperation)operations[1];

            Assert.Same(dbModel.Tables[0], createTableOperation0.Table);
            Assert.Same(dbModel.Tables[1], createTableOperation1.Table);

            var addForeignKeyOperation = (AddForeignKeyOperation)operations[2];

            Assert.Equal("FK", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal("dbo.T1", addForeignKeyOperation.TableName);
            Assert.Equal("dbo.T0", addForeignKeyOperation.ReferencedTableName);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation.ColumnNames);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation.ReferencedColumnNames);
            Assert.True(addForeignKeyOperation.CascadeDelete);

            var createIndexOperation = (CreateIndexOperation)operations[3];

            Assert.Equal("IX", createIndexOperation.IndexName);
            Assert.Equal("dbo.T1", createIndexOperation.TableName);
            Assert.Equal(new[] { "Id" }, createIndexOperation.ColumnNames);
            Assert.True(createIndexOperation.IsUnique);
        }

        [Fact]
        public void DropSchema_creates_operations()
        {
            var modelBuider = new BasicModelBuilder();
            modelBuider.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P0").ColumnName("C0");
                        b.Key("Id").KeyName("PK0");
                        b.ToTable("T0", "dbo");
                    });
            modelBuider.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1").ColumnName("C1");
                        b.Key("Id").KeyName("PK1");
                        b.ToTable("T1", "dbo");
                        b.ForeignKey("A", "Id").KeyName("FK");
                        b.Index("Id").IsUnique().IndexName("IX");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).DropSchema(modelBuider.Model);

            Assert.Equal(3, operations.Count);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];
            var dropTableOperation0 = (DropTableOperation)operations[1];
            var dropTableOperation1 = (DropTableOperation)operations[2];

            Assert.Equal("FK", dropForeignKeyOperation.ForeignKeyName);
            Assert.Equal("dbo.T0", dropTableOperation0.TableName);
            Assert.Equal("dbo.T1", dropTableOperation1.TableName);
        }

        [Fact]
        public void Diff_finds_moved_table()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").KeyName("PK");
                        b.ToTable("T", "dbo");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").KeyName("PK");
                        b.ToTable("T", "OtherSchema");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<MoveTableOperation>(operations[0]);

            var moveTableOperation = (MoveTableOperation)operations[0];

            Assert.Equal("dbo.T", moveTableOperation.TableName);
            Assert.Equal("OtherSchema", moveTableOperation.NewSchema);
        }

        [Fact]
        public void Diff_finds_renamed_table()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").KeyName("PK");
                        b.ToTable("T", "dbo");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").KeyName("PK");
                        b.ToTable("RenamedTable", "dbo");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<RenameTableOperation>(operations[0]);

            var renameTableOperation = (RenameTableOperation)operations[0];

            Assert.Equal("dbo.T", renameTableOperation.TableName);
            Assert.Equal("RenamedTable", renameTableOperation.NewTableName);
        }

        [Fact]
        public void Diff_finds_created_table()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").KeyName("PK");
                        b.ForeignKey("A", "Id").KeyName("FK").CascadeDelete(true);
                        b.Index("Id").IndexName("IX").IsUnique();
                    });

            var databaseBuilder = new DatabaseBuilder();
            var operations = new ModelDiffer(databaseBuilder).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);
            var targetDbModel = databaseBuilder.GetDatabase(targetModelBuilder.Model);

            Assert.Equal(3, operations.Count);
            Assert.IsType<CreateTableOperation>(operations[0]);
            Assert.IsType<AddForeignKeyOperation>(operations[1]);
            Assert.IsType<CreateIndexOperation>(operations[2]);

            var createTableOperation = (CreateTableOperation)operations[0];

            Assert.Same(targetDbModel.Tables[1], createTableOperation.Table);

            var addForeignKeyOperation = (AddForeignKeyOperation)operations[1];

            Assert.Equal("FK", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal("B", addForeignKeyOperation.TableName);
            Assert.Equal("A", addForeignKeyOperation.ReferencedTableName);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation.ColumnNames);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation.ReferencedColumnNames);
            Assert.True(addForeignKeyOperation.CascadeDelete);

            var createIndexOperation = (CreateIndexOperation)operations[2];

            Assert.Equal("IX", createIndexOperation.IndexName);
            Assert.Equal("B", createIndexOperation.TableName);
            Assert.Equal(new[] { "Id" }, createIndexOperation.ColumnNames);
            Assert.True(createIndexOperation.IsUnique);
        }

        [Fact]
        public void Diff_finds_dropped_table()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });
            sourceModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropTableOperation>(operations[0]);

            var dropTableOperation = (DropTableOperation)operations[0];

            Assert.Equal("B", dropTableOperation.TableName);
        }

        [Fact]
        public void Diff_finds_renamed_column()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P").ColumnName("C");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P").ColumnName("RenamedColumn");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<RenameColumnOperation>(operations[0]);

            var renameColumnOperation = (RenameColumnOperation)operations[0];

            Assert.Equal("A", renameColumnOperation.TableName);
            Assert.Equal("C", renameColumnOperation.ColumnName);
            Assert.Equal("RenamedColumn", renameColumnOperation.NewColumnName);
        }

        [Fact]
        public void Diff_finds_added_column()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AddColumnOperation>(operations[0]);

            var addColumnOperation = (AddColumnOperation)operations[0];

            Assert.Equal("A", addColumnOperation.TableName);
            Assert.Equal("P", addColumnOperation.Column.Name);
            Assert.Equal(typeof(string), addColumnOperation.Column.ClrType);
        }

        [Fact]
        public void Diff_finds_dropped_column()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropColumnOperation>(operations[0]);

            var dropColumnOperation = (DropColumnOperation)operations[0];

            Assert.Equal("A", dropColumnOperation.TableName);
            Assert.Equal("P", dropColumnOperation.ColumnName);
        }

        [Fact]
        public void Diff_finds_altered_column()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AlterColumnOperation>(operations[0]);

            var alterColumnOperation = (AlterColumnOperation)operations[0];

            Assert.Equal("A", alterColumnOperation.TableName);
            Assert.Equal("P", alterColumnOperation.NewColumn.Name);
            Assert.Equal(typeof(string), alterColumnOperation.NewColumn.ClrType);
        }

        [Fact]
        public void Diff_finds_updated_primary_key()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P");
                        b.Key("Id").KeyName("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P");
                        b.Key("Id", "P").KeyName("PK");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);
            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[1]);

            var dropPrimaryKeyOperation = (DropPrimaryKeyOperation)operations[0];
            var addPrimaryKeyOperation = (AddPrimaryKeyOperation)operations[1];

            Assert.Equal("A", dropPrimaryKeyOperation.TableName);
            Assert.Equal("PK", dropPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal("A", addPrimaryKeyOperation.TableName);
            Assert.Equal("PK", addPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal(new[] { "Id", "P" }, addPrimaryKeyOperation.ColumnNames);
        }

        [Fact]
        public void Diff_finds_added_foreign_key()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });
            sourceModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P");
                        b.Key("Id");
                        b.ForeignKey("A", "Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P");
                        b.Key("Id");
                        b.ForeignKey("A", "Id");
                        b.ForeignKey("A", "P").KeyName("FK").CascadeDelete(true);
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AddForeignKeyOperation>(operations[0]);

            var addForeignKeyOperation = (AddForeignKeyOperation)operations[0];

            Assert.Equal("FK", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal("B", addForeignKeyOperation.TableName);
            Assert.Equal("A", addForeignKeyOperation.ReferencedTableName);
            Assert.Equal(new[] { "P" }, addForeignKeyOperation.ColumnNames);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation.ReferencedColumnNames);
            Assert.True(addForeignKeyOperation.CascadeDelete);
        }

        [Fact]
        public void Diff_finds_dropped_foreign_key()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });
            sourceModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P");
                        b.Key("Id");
                        b.ForeignKey("A", "Id");
                        b.ForeignKey("A", "P").KeyName("FK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P");
                        b.Key("Id");
                        b.ForeignKey("A", "Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropForeignKeyOperation>(operations[0]);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];

            Assert.Equal("B", dropForeignKeyOperation.TableName);
            Assert.Equal("FK", dropForeignKeyOperation.ForeignKeyName);
        }

        [Fact]
        public void Diff_finds_added_index()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                        b.Index("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                        b.Index("Id");
                        b.Index("P").IndexName("IX").IsUnique();
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<CreateIndexOperation>(operations[0]);

            var createIndexOperation = (CreateIndexOperation)operations[0];

            Assert.Equal("A", createIndexOperation.TableName);
            Assert.Equal("IX", createIndexOperation.IndexName);
            Assert.Equal(new[] { "P" }, createIndexOperation.ColumnNames);
            Assert.True(createIndexOperation.IsUnique);
        }

        [Fact]
        public void Diff_finds_removed_index()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                        b.Index("Id");
                        b.Index("P").IndexName("IX");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                        b.Index("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropIndexOperation>(operations[0]);

            var dropIndexOperation = (DropIndexOperation)operations[0];

            Assert.Equal("A", dropIndexOperation.TableName);
            Assert.Equal("IX", dropIndexOperation.IndexName);
        }

        [Fact]
        public void Diff_finds_renamed_index()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                        b.Index("Id").IndexName("IX");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                        b.Index("Id").IndexName("RenamedIndex");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<RenameIndexOperation>(operations[0]);

            var renameIndexOperation = (RenameIndexOperation)operations[0];

            Assert.Equal("A", renameIndexOperation.TableName);
            Assert.Equal("IX", renameIndexOperation.IndexName);
            Assert.Equal("RenamedIndex", renameIndexOperation.NewIndexName);
        }

        #endregion

        #region Transitive renames

        [Fact]
        public void Diff_handles_transitive_table_renames()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").KeyName("PK0");
                        b.ToTable("T0", "dbo");
                    });
            sourceModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").KeyName("PK1");
                        b.ToTable("T1", "dbo");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").KeyName("PK0");
                        b.ToTable("T1", "dbo");
                    });
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").KeyName("PK1");
                        b.ToTable("T0", "dbo");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(3, operations.Count);

            Assert.IsType<RenameTableOperation>(operations[0]);
            Assert.IsType<RenameTableOperation>(operations[1]);
            Assert.IsType<RenameTableOperation>(operations[2]);

            var renameTableOperation0 = (RenameTableOperation)operations[0];
            var renameTableOperation1 = (RenameTableOperation)operations[1];
            var renameTableOperation2 = (RenameTableOperation)operations[2];

            Assert.Equal("dbo.T0", renameTableOperation0.TableName);
            Assert.Equal("__mig_tmp__0", renameTableOperation0.NewTableName);
            Assert.Equal("dbo.T1", renameTableOperation1.TableName);
            Assert.Equal("T0", renameTableOperation1.NewTableName);
            Assert.Equal("dbo.__mig_tmp__0", renameTableOperation2.TableName);
            Assert.Equal("T1", renameTableOperation2.NewTableName);
        }

        [Fact]
        public void Diff_handles_transitive_column_renames()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P0").ColumnName("C0");
                        b.Property<string>("P1").ColumnName("C1");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P0").ColumnName("C1");
                        b.Property<string>("P1").ColumnName("C0");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(3, operations.Count);

            Assert.IsType<RenameColumnOperation>(operations[0]);
            Assert.IsType<RenameColumnOperation>(operations[1]);
            Assert.IsType<RenameColumnOperation>(operations[2]);

            var renameColumnOperation0 = (RenameColumnOperation)operations[0];
            var renameColumnOperation1 = (RenameColumnOperation)operations[1];
            var renameColumnOperation2 = (RenameColumnOperation)operations[2];

            Assert.Equal("C0", renameColumnOperation0.ColumnName);
            Assert.Equal("__mig_tmp__0", renameColumnOperation0.NewColumnName);
            Assert.Equal("C1", renameColumnOperation1.ColumnName);
            Assert.Equal("C0", renameColumnOperation1.NewColumnName);
            Assert.Equal("__mig_tmp__0", renameColumnOperation2.ColumnName);
            Assert.Equal("C1", renameColumnOperation2.NewColumnName);
        }

        [Fact]
        public void Diff_handles_transitive_index_renames()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                        b.Index("Id").IndexName("IX0");
                        b.Index("P").IndexName("IX1");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                        b.Index("Id").IndexName("IX1");
                        b.Index("P").IndexName("IX0");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(3, operations.Count);

            Assert.IsType<RenameIndexOperation>(operations[0]);
            Assert.IsType<RenameIndexOperation>(operations[1]);
            Assert.IsType<RenameIndexOperation>(operations[2]);

            var renameIndexOperation0 = (RenameIndexOperation)operations[0];
            var renameIndexOperation1 = (RenameIndexOperation)operations[1];
            var renameIndexOperation2 = (RenameIndexOperation)operations[2];

            Assert.Equal("IX0", renameIndexOperation0.IndexName);
            Assert.Equal("__mig_tmp__0", renameIndexOperation0.NewIndexName);
            Assert.Equal("IX1", renameIndexOperation1.IndexName);
            Assert.Equal("IX0", renameIndexOperation1.NewIndexName);
            Assert.Equal("__mig_tmp__0", renameIndexOperation2.IndexName);
            Assert.Equal("IX1", renameIndexOperation2.NewIndexName);
        }

        #endregion

        #region EntityType matching

        [Fact]
        public void Entity_types_are_fuzzy_matched_if_80_percent_or_more_of_properties_match_accross_both_entity_types()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id").KeyName("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Property<string>("P2");
                        b.Key("Id").KeyName("PK");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<RenameTableOperation>(operations[0]);
            Assert.IsType<AddColumnOperation>(operations[1]);

            var renameTableOperation = (RenameTableOperation)operations[0];
            var addColumnOperation = (AddColumnOperation)operations[1];

            Assert.Equal("A", renameTableOperation.TableName);
            Assert.Equal("B", renameTableOperation.NewTableName);
            Assert.Equal("P2", addColumnOperation.Column.Name);
        }

        [Fact]
        public void Entity_types_are_not_fuzzy_matched_if_less_than_80_percent_of_properties_match_accross_both_entity_types()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Property<string>("P2");
                        b.Key("Id").KeyName("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Property<string>("P3");
                        b.Key("Id").KeyName("PK");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropTableOperation>(operations[0]);
            Assert.IsType<CreateTableOperation>(operations[1]);

            var dropTableOperation = (DropTableOperation)operations[0];
            var createTableOperation = (CreateTableOperation)operations[1];

            Assert.Equal("A", dropTableOperation.TableName);
            Assert.Equal("B", createTableOperation.Table.Name);
            Assert.Equal(new[] { "Id", "P1", "P3" }, createTableOperation.Table.Columns.Select(c => c.Name));
        }

        #endregion

        #region Property matching

        [Fact]
        public void Properties_are_matched_if_different_but_have_same_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1").ColumnName("C");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P2").ColumnName("C");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);

            Assert.IsType<AlterColumnOperation>(operations[0]);

            var alterColumnOperation = (AlterColumnOperation)operations[0];

            Assert.Equal("C", alterColumnOperation.NewColumn.Name);
            Assert.Same(typeof(int), alterColumnOperation.NewColumn.ClrType);
        }

        [Fact]
        public void Properties_are_not_matched_if_different_names_and_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P2");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropColumnOperation>(operations[0]);
            Assert.IsType<AddColumnOperation>(operations[1]);

            var dropColumnOperation = (DropColumnOperation)operations[0];
            var addColumnOperation = (AddColumnOperation)operations[1];

            Assert.Equal("P1", dropColumnOperation.ColumnName);
            Assert.Equal("P2", addColumnOperation.Column.Name);
        }

        [Fact]
        public void Properties_are_matched_before_columns()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1").ColumnName("C2");
                        b.Property<int>("P2").ColumnName("C1");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1").ColumnName("C1");
                        b.Property<string>("P4").ColumnName("C2");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(3, operations.Count);

            Assert.IsType<DropColumnOperation>(operations[0]);
            Assert.IsType<RenameColumnOperation>(operations[1]);
            Assert.IsType<AddColumnOperation>(operations[2]);

            var dropColumnOperation = (DropColumnOperation)operations[0];
            var renameColumnOperation = (RenameColumnOperation)operations[1];
            var addColumnOperation = (AddColumnOperation)operations[2];

            Assert.Equal("C1", dropColumnOperation.ColumnName);
            Assert.Equal("C2", renameColumnOperation.ColumnName);
            Assert.Equal("C1", renameColumnOperation.NewColumnName);
            Assert.Equal("C2", addColumnOperation.Column.Name);
            Assert.Equal(typeof(string), addColumnOperation.Column.ClrType);
        }

        #endregion

        #region Column matching

        [Fact]
        public void Columns_are_matched_if_different_but_same_property_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P").ColumnName("C1");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P").ColumnName("C2");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<RenameColumnOperation>(operations[0]);
            Assert.IsType<AlterColumnOperation>(operations[1]);

            var renameColumnOperation = (RenameColumnOperation)operations[0];
            var alterColumnOperation = (AlterColumnOperation)operations[1];

            Assert.Equal("C1", renameColumnOperation.ColumnName);
            Assert.Equal("C2", renameColumnOperation.NewColumnName);
            Assert.Equal("C2", alterColumnOperation.NewColumn.Name);
        }

        [Fact]
        public void Columns_are_not_matched_if_different_clr_type()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<string>("Id");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AlterColumnOperation>(operations[0]);

            var alterColumnOperation = (AlterColumnOperation)operations[0];

            Assert.Equal(typeof(string), alterColumnOperation.NewColumn.ClrType);
        }

        [Fact]
        public void Columns_are_not_matched_if_different_data_type()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id").ColumnType("int");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<string>("Id").ColumnType("smallint");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AlterColumnOperation>(operations[0]);

            var alterColumnOperation = (AlterColumnOperation)operations[0];

            Assert.Equal("smallint", alterColumnOperation.NewColumn.DataType);
        }

        [Fact]
        public void Columns_are_not_matched_if_different_default_value()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id").Annotation(MetadataExtensions.Annotations.ColumnDefaultValue, "V0");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<string>("Id").Annotation(MetadataExtensions.Annotations.ColumnDefaultValue, "V1");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AlterColumnOperation>(operations[0]);

            var alterColumnOperation = (AlterColumnOperation)operations[0];

            Assert.Equal("V1", alterColumnOperation.NewColumn.DefaultValue);
        }

        [Fact]
        public void Columns_are_not_matched_if_different_default_sql()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id").ColumnDefaultSql("Sql0");
                        b.Key("Id");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id").ColumnDefaultSql("Sql1");
                        b.Key("Id");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AlterColumnOperation>(operations[0]);

            var alterColumnOperation = (AlterColumnOperation)operations[0];

            Assert.Equal("Sql1", alterColumnOperation.NewColumn.DefaultSql);
        }

        // TODO: Add the rest of the test cases when annotations/extension methods are available for all properties of Column.

        #endregion

        #region PrimaryKey matching

        [Fact]
        public void Primary_keys_are_matched_if_different_columns_but_same_property_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P").ColumnName("C1");
                        b.Key("Id", "P");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P").ColumnName("C2");
                        b.Key("Id", "P");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<RenameColumnOperation>(operations[0]);
            Assert.IsType<AlterColumnOperation>(operations[1]);
        }

        [Fact]
        public void Primary_keys_are_matched_if_different_properties_but_same_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1").ColumnName("C");
                        b.Key("Id", "P1").KeyName("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P2").ColumnName("C");
                        b.Key("Id", "P2").KeyName("PK");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);

            Assert.IsType<AlterColumnOperation>(operations[0]);
        }

        [Fact]
        public void Primary_keys_are_not_matched_if_different_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").KeyName("PK1");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").KeyName("PK2");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[1]);

            var dropPrimaryKeyOperation = (DropPrimaryKeyOperation)operations[0];
            var addPrimaryKeyOperation = (AddPrimaryKeyOperation)operations[1];

            Assert.Equal("PK1", dropPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal("PK2", addPrimaryKeyOperation.PrimaryKeyName);
        }

        [Fact]
        public void Primary_keys_are_not_matched_if_different_clustered_flag()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").Annotation(MetadataExtensions.Annotations.IsClustered, "true");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id").Annotation(MetadataExtensions.Annotations.IsClustered, "false");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

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
        public void Primary_keys_are_not_matched_if_different_property_count()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id", "P1").KeyName("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id").KeyName("PK");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[1]);

            var dropPrimaryKeyOperation = (DropPrimaryKeyOperation)operations[0];
            var addPrimaryKeyOperation = (AddPrimaryKeyOperation)operations[1];

            Assert.Equal("PK", dropPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal("PK", addPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal(new[] { "Id" }, addPrimaryKeyOperation.ColumnNames);
        }

        [Fact]
        public void Primary_keys_are_not_matched_if_different_property_and_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Property<string>("P2");
                        b.Key("Id", "P1").KeyName("PK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Property<string>("P2");
                        b.Key("Id", "P2").KeyName("PK");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[1]);

            var dropPrimaryKeyOperation = (DropPrimaryKeyOperation)operations[0];
            var addPrimaryKeyOperation = (AddPrimaryKeyOperation)operations[1];

            Assert.Equal("PK", dropPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal("PK", addPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal(new[] { "Id", "P2" }, addPrimaryKeyOperation.ColumnNames);
        }

        #endregion

        #region ForeignKey matching

        [Fact]
        public void Foreign_keys_are_matched_if_different_columns_but_same_property_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id").ColumnName("C1");
                        b.Key("Id");
                    });
            sourceModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P").ColumnName("C2");
                        b.Key("Id");
                        b.ForeignKey("A", "P").KeyName("FK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<string>("Id").ColumnName("C2");
                        b.Key("Id");
                    });
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P").ColumnName("C3");
                        b.Key("Id");
                        b.ForeignKey("A", "P").KeyName("FK");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(5, operations.Count);

            Assert.IsType<RenameColumnOperation>(operations[0]);
            Assert.IsType<RenameColumnOperation>(operations[1]);
            Assert.IsType<RenameColumnOperation>(operations[2]);
            Assert.IsType<AlterColumnOperation>(operations[3]);
            Assert.IsType<AlterColumnOperation>(operations[4]);
        }

        [Fact]
        public void Foreign_keys_are_matched_if_different_properties_but_same_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id1").ColumnName("C1");
                        b.Key("Id1");
                    });
            sourceModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1").ColumnName("C2");
                        b.Key("Id");
                        b.ForeignKey("A", "P1").KeyName("FK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<string>("Id2").ColumnName("C1");
                        b.Key("Id2");
                    });
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P2").ColumnName("C2");
                        b.Key("Id");
                        b.ForeignKey("A", "P2").KeyName("FK");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<AlterColumnOperation>(operations[0]);
            Assert.IsType<AlterColumnOperation>(operations[1]);
        }

        [Fact]
        public void Foreign_keys_are_not_matched_if_different_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });
            sourceModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                        b.ForeignKey("A", "Id").KeyName("FK1");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                        b.ForeignKey("A", "Id").KeyName("FK2");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropForeignKeyOperation>(operations[0]);
            Assert.IsType<AddForeignKeyOperation>(operations[1]);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];
            var addForeignKeyOperation = (AddForeignKeyOperation)operations[1];

            Assert.Equal("FK1", dropForeignKeyOperation.ForeignKeyName);
            Assert.Equal("FK2", addForeignKeyOperation.ForeignKeyName);
        }

        [Fact]
        public void Foreign_keys_are_not_matched_if_different_cascade_delete_flag()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });
            sourceModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                        b.ForeignKey("A", "Id").CascadeDelete(true);
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                        b.ForeignKey("A", "Id").CascadeDelete(false);
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropForeignKeyOperation>(operations[0]);
            Assert.IsType<AddForeignKeyOperation>(operations[1]);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];
            var addForeignKeyOperation = (AddForeignKeyOperation)operations[1];

            Assert.Equal("FK_B_A_Id", dropForeignKeyOperation.ForeignKeyName);
            Assert.Equal("FK_B_A_Id", addForeignKeyOperation.ForeignKeyName);
            Assert.False(addForeignKeyOperation.CascadeDelete);
        }

        [Fact]
        public void Foreign_keys_are_not_matched_if_different_property_count()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P");
                        b.Key("Id", "P");
                    });
            sourceModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P");
                        b.Key("Id");
                        b.ForeignKey("A", "Id").KeyName("FK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P");
                        b.Key("Id", "P");
                    });
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P");
                        b.Key("Id");
                        b.ForeignKey("A", "Id", "P").KeyName("FK");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropForeignKeyOperation>(operations[0]);
            Assert.IsType<AddForeignKeyOperation>(operations[1]);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];
            var addForeignKeyOperation = (AddForeignKeyOperation)operations[1];

            Assert.Equal("FK", dropForeignKeyOperation.ForeignKeyName);
            Assert.Equal("FK", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal(new[] { "Id", "P" }, addForeignKeyOperation.ColumnNames);
        }

        [Fact]
        public void Foreign_keys_are_not_matched_if_different_property_and_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1");
                        b.Property<int>("P2");
                        b.Key("Id", "P1");
                    });
            sourceModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1");
                        b.Property<int>("P2");
                        b.Key("Id");
                        b.ForeignKey("A", "Id", "P1").KeyName("FK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1");
                        b.Property<int>("P2");
                        b.Key("Id", "P1");
                    });
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1");
                        b.Property<int>("P2");
                        b.Key("Id");
                        b.ForeignKey("A", "Id", "P2").KeyName("FK");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropForeignKeyOperation>(operations[0]);
            Assert.IsType<AddForeignKeyOperation>(operations[1]);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];
            var addForeignKeyOperation = (AddForeignKeyOperation)operations[1];

            Assert.Equal("FK", dropForeignKeyOperation.ForeignKeyName);
            Assert.Equal("FK", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal(new[] { "Id", "P2" }, addForeignKeyOperation.ColumnNames);
        }

        [Fact]
        public void Foreign_keys_are_not_matched_if_different_referenced_property_and_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1");
                        b.Property<int>("P2");
                        b.Key("Id", "P1").KeyName("PK");
                    });
            sourceModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1");
                        b.Property<int>("P2");
                        b.Key("Id");
                        b.ForeignKey("A", "Id", "P1").KeyName("FK");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1");
                        b.Property<int>("P2");
                        b.Key("Id", "P2").KeyName("PK");
                    });
            targetModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1");
                        b.Property<int>("P2");
                        b.Key("Id");
                        b.ForeignKey("A", "Id", "P1").KeyName("FK");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(4, operations.Count);

            Assert.IsType<DropForeignKeyOperation>(operations[0]);
            Assert.IsType<DropPrimaryKeyOperation>(operations[1]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[2]);
            Assert.IsType<AddForeignKeyOperation>(operations[3]);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];
            var addForeignKeyOperation = (AddForeignKeyOperation)operations[3];

            Assert.Equal("FK", dropForeignKeyOperation.ForeignKeyName);
            Assert.Equal("FK", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal(new[] { "Id", "P2" }, addForeignKeyOperation.ReferencedColumnNames);
        }

        #endregion

        #region Index matching

        [Fact]
        public void Indexes_are_matched_if_different_columns_but_same_property_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P").ColumnName("C1");
                        b.Key("Id");
                        b.Index("Id", "P").IndexName("IX");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P").ColumnName("C2");
                        b.Key("Id");
                        b.Index("Id", "P").IndexName("IX");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<RenameColumnOperation>(operations[0]);
            Assert.IsType<AlterColumnOperation>(operations[1]);
        }

        [Fact]
        public void Indexes_are_matched_if_different_properties_but_same_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("P1").ColumnName("C");
                        b.Key("Id");
                        b.Index("Id", "P1").IndexName("IX");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P2").ColumnName("C");
                        b.Key("Id");
                        b.Index("Id", "P2").IndexName("IX");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);

            Assert.IsType<AlterColumnOperation>(operations[0]);
        }

        [Fact]
        public void Indexes_are_matched_if_different_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id");
                        b.Index("P1").IndexName("IX1");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id");
                        b.Index("P1").IndexName("IX2");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(1, operations.Count);

            Assert.IsType<RenameIndexOperation>(operations[0]);

            var renameIndexOperation = (RenameIndexOperation)operations[0];

            Assert.Equal("IX1", renameIndexOperation.IndexName);
            Assert.Equal("IX2", renameIndexOperation.NewIndexName);
        }

        [Fact]
        public void Indexes_are_not_matched_if_different_unique_flag()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id");
                        b.Index("P1");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id");
                        b.Index("P1").IsUnique();
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropIndexOperation>(operations[0]);
            Assert.IsType<CreateIndexOperation>(operations[1]);

            var dropIndexOperation = (DropIndexOperation)operations[0];
            var createIndexOperation = (CreateIndexOperation)operations[1];

            Assert.Equal("IX_A_P1", dropIndexOperation.IndexName);
            Assert.Equal("IX_A_P1", createIndexOperation.IndexName);
            Assert.True(createIndexOperation.IsUnique);
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
                        b.Index("P1").IsClustered(false);
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id");
                        b.Index("P1").IsClustered(true);
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

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
        public void Indexes_are_not_matched_if_different_property_count()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id");
                        b.Index("Id").IndexName("IX");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Key("Id");
                        b.Index("Id", "P1").IndexName("IX");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropIndexOperation>(operations[0]);
            Assert.IsType<CreateIndexOperation>(operations[1]);

            var dropIndexOperation = (DropIndexOperation)operations[0];
            var createIndexOperation = (CreateIndexOperation)operations[1];

            Assert.Equal("IX", dropIndexOperation.IndexName);
            Assert.Equal("IX", createIndexOperation.IndexName);
        }

        [Fact]
        public void Indexes_are_not_matched_if_different_property_and_column_names()
        {
            var sourceModelBuilder = new BasicModelBuilder();
            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Property<string>("P2");
                        b.Key("Id");
                        b.Index("P1").IndexName("IX");
                    });

            var targetModelBuilder = new BasicModelBuilder();
            targetModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P1");
                        b.Property<string>("P2");
                        b.Key("Id");
                        b.Index("P2").IndexName("IX");
                    });

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(
                sourceModelBuilder.Model, targetModelBuilder.Model);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropIndexOperation>(operations[0]);
            Assert.IsType<CreateIndexOperation>(operations[1]);

            var dropIndexOperation = (DropIndexOperation)operations[0];
            var createIndexOperation = (CreateIndexOperation)operations[1];

            Assert.Equal("IX", dropIndexOperation.IndexName);
            Assert.Equal("IX", createIndexOperation.IndexName);
        }

        #endregion
    }
}
