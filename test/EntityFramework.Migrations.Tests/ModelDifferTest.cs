// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests
{
    public class ModelDifferTest
    {
        [Fact]
        public void CreateSchema_creates_operations()
        {
            var operations = new ModelDiffer(new DatabaseBuilder()).CreateSchema(CreateModel());

            Assert.Equal(4, operations.Count);

            var createTableOperation0 = (CreateTableOperation)operations[0];
            var createTableOperation1 = (CreateTableOperation)operations[1];
            var addForeignKeyOperation = (AddForeignKeyOperation)operations[2];
            var createIndexOperation = (CreateIndexOperation)operations[3];

            Assert.Equal("dbo.MyTable0", createTableOperation0.Table.Name);
            Assert.Equal("dbo.MyTable1", createTableOperation1.Table.Name);
            Assert.Equal("MyFK", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal("MyIndex", createIndexOperation.IndexName);
        }

        [Fact]
        public void DropSchema_creates_operations()
        {
            var operations = new ModelDiffer(new DatabaseBuilder()).DropSchema(CreateModel());

            Assert.Equal(3, operations.Count);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];
            var dropTableOperation0 = (DropTableOperation)operations[1];
            var dropTableOperation1 = (DropTableOperation)operations[2];

            Assert.Equal("MyFK", dropForeignKeyOperation.ForeignKeyName);
            Assert.Equal("dbo.MyTable0", dropTableOperation0.TableName);
            Assert.Equal("dbo.MyTable1", dropTableOperation1.TableName);
        }

        [Fact]
        public void Diff_finds_moved_table()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var dependentEntity = targetModel.GetEntityType("Dependent");
            dependentEntity.SetTableName("MyTable0");
            dependentEntity.SetSchema("newdbo");

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<MoveTableOperation>(operations[0]);

            var moveTableOperation = (MoveTableOperation)operations[0];

            Assert.Equal("dbo.MyTable0", moveTableOperation.TableName);
            Assert.Equal("newdbo", moveTableOperation.NewSchema);
        }

        [Fact]
        public void Diff_finds_renamed_table()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var dependentEntity = targetModel.GetEntityType("Dependent");
            dependentEntity.SetTableName("MyNewTable0");
            dependentEntity.SetSchema("dbo");

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<RenameTableOperation>(operations[0]);

            var renameTableOperation = (RenameTableOperation)operations[0];

            Assert.Equal("dbo.MyTable0", renameTableOperation.TableName);
            Assert.Equal("MyNewTable0", renameTableOperation.NewTableName);
        }

        [Fact]
        public void Diff_finds_created_table()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var principalEntityType = targetModel.GetEntityType("Principal");
            var dependentEntityType = new EntityType("NewDependent");
            dependentEntityType.SetTableName("MyNewTable");
            dependentEntityType.SetSchema("dbo");
            var dependentProperty = dependentEntityType.AddProperty("Id", typeof(int));

            targetModel.AddEntityType(dependentEntityType);

            dependentProperty.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.StorageTypeName, "int"));

            dependentEntityType.SetKey(dependentProperty);
            dependentEntityType.GetKey().SetKeyName("MyNewPK");

            var foreignKey = dependentEntityType.AddForeignKey(principalEntityType.GetKey(), dependentProperty);
            foreignKey.SetKeyName("MyNewFK");
            foreignKey.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.CascadeDelete, "True"));

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(2, operations.Count);
            Assert.IsType<CreateTableOperation>(operations[0]);
            Assert.IsType<AddForeignKeyOperation>(operations[1]);

            var createTableOperation = (CreateTableOperation)operations[0];

            Assert.Equal("dbo.MyNewTable", createTableOperation.Table.Name);
            Assert.Equal(1, createTableOperation.Table.Columns.Count);
            Assert.Equal("Id", createTableOperation.Table.Columns[0].Name);
            Assert.Equal(typeof(int), createTableOperation.Table.Columns[0].ClrType);
            Assert.NotNull(createTableOperation.Table.PrimaryKey);
            Assert.Equal("MyNewPK", createTableOperation.Table.PrimaryKey.Name);
            Assert.Equal(1, createTableOperation.Table.ForeignKeys.Count);
            Assert.Equal("MyNewFK", createTableOperation.Table.ForeignKeys[0].Name);
            Assert.True(createTableOperation.Table.ForeignKeys[0].CascadeDelete);

            var addForeignKeyOperation = (AddForeignKeyOperation)operations[1];

            Assert.Equal("MyNewFK", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal("dbo.MyNewTable", addForeignKeyOperation.TableName);
            Assert.Equal("dbo.MyTable1", addForeignKeyOperation.ReferencedTableName);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation.ColumnNames);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation.ReferencedColumnNames);
            Assert.True(addForeignKeyOperation.CascadeDelete);
        }

        [Fact]
        public void Diff_finds_dropped_table()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var principalEntityType = targetModel.GetEntityType("Principal");
            var dependentEntityType = new EntityType("OldDependent");
            dependentEntityType.SetTableName("MyOldTable");
            dependentEntityType.SetSchema("dbo");
            var dependentProperty = dependentEntityType.AddProperty("Id", typeof(int));

            sourceModel.AddEntityType(dependentEntityType);

            dependentProperty.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.StorageTypeName, "int"));

            dependentEntityType.SetKey(dependentProperty);
            dependentEntityType.GetKey().SetKeyName("MyOldPK");

            var foreignKey = dependentEntityType.AddForeignKey(principalEntityType.GetKey(), dependentProperty);
            foreignKey.SetKeyName("MyOldFK");
            foreignKey.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.CascadeDelete, "True"));

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropTableOperation>(operations[0]);

            var dropTableOperation = (DropTableOperation)operations[0];

            Assert.Equal("dbo.MyOldTable", dropTableOperation.TableName);
        }

        [Fact]
        public void Diff_finds_renamed_column()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            targetModel.GetEntityType("Dependent").GetProperty("Id").SetColumnName("NewId");

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<RenameColumnOperation>(operations[0]);

            var renameColumnOperation = (RenameColumnOperation)operations[0];

            Assert.Equal("dbo.MyTable0", renameColumnOperation.TableName);
            Assert.Equal("Id", renameColumnOperation.ColumnName);
            Assert.Equal("NewId", renameColumnOperation.NewColumnName);
        }

        [Fact]
        public void Diff_finds_added_column()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var property = targetModel.GetEntityType("Dependent").AddProperty("MyNewProperty", typeof(string));
            property.SetColumnName("MyNewColumn");

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AddColumnOperation>(operations[0]);

            var addColumnOperation = (AddColumnOperation)operations[0];

            Assert.Equal("dbo.MyTable0", addColumnOperation.TableName);
            Assert.Equal("MyNewColumn", addColumnOperation.Column.Name);
            Assert.Equal(typeof(string), addColumnOperation.Column.ClrType);
        }

        [Fact]
        public void Diff_finds_dropped_column()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var property = sourceModel.GetEntityType("Dependent").AddProperty("MyOldProperty", typeof(string));
            property.SetColumnName("MyOldColumn");

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropColumnOperation>(operations[0]);

            var dropColumnOperation = (DropColumnOperation)operations[0];

            Assert.Equal("dbo.MyTable0", dropColumnOperation.TableName);
            Assert.Equal("MyOldColumn", dropColumnOperation.ColumnName);
        }

        [Fact]
        public void Diff_finds_altered_column_nullable_flag()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var property = targetModel.GetEntityType("Dependent").GetProperty("MyProperty");
            property.IsNullable = false;

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AlterColumnOperation>(operations[0]);

            var alterColumnOperation = (AlterColumnOperation)operations[0];

            Assert.Equal("dbo.MyTable0", alterColumnOperation.TableName);
            Assert.Equal("MyColumn", alterColumnOperation.NewColumn.Name);
            Assert.False(alterColumnOperation.NewColumn.IsNullable);
        }

        [Fact]
        public void Diff_finds_altered_column_clr_type()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var entityType = targetModel.GetEntityType("Dependent");
            var property = entityType.GetProperty("MyProperty");

            entityType.RemoveProperty(property);
            property = entityType.AddProperty("MyProperty", typeof(double));
            property.SetColumnName("MyColumn");

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AlterColumnOperation>(operations[0]);

            var alterColumnOperation = (AlterColumnOperation)operations[0];

            Assert.Equal("dbo.MyTable0", alterColumnOperation.TableName);
            Assert.Equal("MyColumn", alterColumnOperation.NewColumn.Name);
            Assert.Equal(typeof(double), alterColumnOperation.NewColumn.ClrType);
        }

        [Fact]
        public void Diff_finds_altered_column_data_type()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var property = targetModel.GetEntityType("Dependent").GetProperty("MyProperty");
            property.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.StorageTypeName, "nvarchar(10)"));

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AlterColumnOperation>(operations[0]);

            var alterColumnOperation = (AlterColumnOperation)operations[0];

            Assert.Equal("dbo.MyTable0", alterColumnOperation.TableName);
            Assert.Equal("MyColumn", alterColumnOperation.NewColumn.Name);
            Assert.Equal("nvarchar(10)", alterColumnOperation.NewColumn.DataType);
        }

        [Fact]
        public void Diff_finds_added_default_constraint_with_value()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var property = targetModel.GetEntityType("Dependent").GetProperty("MyProperty");
            property.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.ColumnDefaultValue, "MyDefaultValue"));

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AddDefaultConstraintOperation>(operations[0]);

            var addDefaultConstraintOperation = (AddDefaultConstraintOperation)operations[0];

            Assert.Equal("dbo.MyTable0", addDefaultConstraintOperation.TableName);
            Assert.Equal("MyColumn", addDefaultConstraintOperation.ColumnName);
            Assert.Equal("MyDefaultValue", addDefaultConstraintOperation.DefaultValue);
            Assert.Null(addDefaultConstraintOperation.DefaultSql);
        }

        [Fact]
        public void Diff_finds_added_default_constraint_with_sql()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var property = targetModel.GetEntityType("Dependent").GetProperty("MyProperty");
            property.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.ColumnDefaultSql, "MyDefaultSql"));

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AddDefaultConstraintOperation>(operations[0]);

            var addDefaultConstraintOperation = (AddDefaultConstraintOperation)operations[0];

            Assert.Equal("dbo.MyTable0", addDefaultConstraintOperation.TableName);
            Assert.Equal("MyColumn", addDefaultConstraintOperation.ColumnName);
            Assert.Null(addDefaultConstraintOperation.DefaultValue);
            Assert.Equal("MyDefaultSql", addDefaultConstraintOperation.DefaultSql);
        }

        [Fact]
        public void Diff_finds_dropped_default_constraint_with_value()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var property = sourceModel.GetEntityType("Dependent").GetProperty("MyProperty");
            property.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.ColumnDefaultValue, "MyDefaultValue"));

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropDefaultConstraintOperation>(operations[0]);

            var dropDefaultConstraintOperation = (DropDefaultConstraintOperation)operations[0];

            Assert.Equal("dbo.MyTable0", dropDefaultConstraintOperation.TableName);
            Assert.Equal("MyColumn", dropDefaultConstraintOperation.ColumnName);
        }

        [Fact]
        public void Diff_finds_dropped_default_constraint_with_sql()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var property = sourceModel.GetEntityType("Dependent").GetProperty("MyProperty");
            property.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.ColumnDefaultSql, "MyDefaultSql"));

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropDefaultConstraintOperation>(operations[0]);

            var dropDefaultConstraintOperation = (DropDefaultConstraintOperation)operations[0];

            Assert.Equal("dbo.MyTable0", dropDefaultConstraintOperation.TableName);
            Assert.Equal("MyColumn", dropDefaultConstraintOperation.ColumnName);
        }

        [Fact]
        public void Diff_finds_updated_primary_key()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var entityType = targetModel.GetEntityType("Dependent");
            entityType.SetKey(entityType.GetProperty("MyProperty"));
            entityType.GetKey().SetKeyName("MyNewPK");

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(2, operations.Count);
            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[1]);

            var dropPrimaryKeyOperation = (DropPrimaryKeyOperation)operations[0];
            var addPrimaryKeyOperation = (AddPrimaryKeyOperation)operations[1];

            Assert.Equal("dbo.MyTable0", dropPrimaryKeyOperation.TableName);
            Assert.Equal("MyPK0", dropPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal("dbo.MyTable0", addPrimaryKeyOperation.TableName);
            Assert.Equal("MyNewPK", addPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal(new[] { "MyColumn" }, addPrimaryKeyOperation.ColumnNames);
        }

        [Fact]
        public void Diff_finds_added_foreign_key()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var entityType = sourceModel.GetEntityType("Dependent");
            entityType.RemoveForeignKey(entityType.ForeignKeys[0]);

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AddForeignKeyOperation>(operations[0]);

            var addForeignKeyOperation = (AddForeignKeyOperation)operations[0];

            Assert.Equal("dbo.MyTable0", addForeignKeyOperation.TableName);
            Assert.Equal("MyFK", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation.ColumnNames);
            Assert.True(addForeignKeyOperation.CascadeDelete);
        }

        [Fact]
        public void Diff_finds_dropped_foreign_key()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var entityType = targetModel.GetEntityType("Dependent");
            entityType.RemoveForeignKey(entityType.ForeignKeys[0]);

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropForeignKeyOperation>(operations[0]);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];

            Assert.Equal("dbo.MyTable0", dropForeignKeyOperation.TableName);
            Assert.Equal("MyFK", dropForeignKeyOperation.ForeignKeyName);
        }

        [Fact]
        public void Diff_finds_added_index()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var entityType = sourceModel.GetEntityType("Dependent");
            entityType.RemoveIndex(entityType.Indexes[0]);

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<CreateIndexOperation>(operations[0]);

            var createIndexOperation = (CreateIndexOperation)operations[0];

            Assert.Equal("dbo.MyTable0", createIndexOperation.TableName);
            Assert.Equal("MyIndex", createIndexOperation.IndexName);
            Assert.Equal(new[] { "Id" }, createIndexOperation.ColumnNames);
            Assert.True(createIndexOperation.IsUnique);
        }

        [Fact]
        public void Diff_finds_removed_index()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var entityType = targetModel.GetEntityType("Dependent");
            entityType.RemoveIndex(entityType.Indexes[0]);

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropIndexOperation>(operations[0]);

            var dropIndexOperation = (DropIndexOperation)operations[0];

            Assert.Equal("dbo.MyTable0", dropIndexOperation.TableName);
            Assert.Equal("MyIndex", dropIndexOperation.IndexName);
        }

        [Fact]
        public void Diff_finds_renamed_index()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var entityType = targetModel.GetEntityType("Dependent");
            var index = entityType.Indexes[0];
            index.SetIndexName("MyNewIndex");

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<RenameIndexOperation>(operations[0]);

            var renameIndexOperation = (RenameIndexOperation)operations[0];

            Assert.Equal("dbo.MyTable0", renameIndexOperation.TableName);
            Assert.Equal("MyIndex", renameIndexOperation.IndexName);
            Assert.Equal("MyNewIndex", renameIndexOperation.NewIndexName);
        }

        [Fact]
        public void Diff_finds_index_with_unique_updated()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var entityType = targetModel.GetEntityType("Dependent");
            var index = entityType.Indexes[0];
            index.IsUnique = false;

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(2, operations.Count);
            Assert.IsType<DropIndexOperation>(operations[0]);
            Assert.IsType<CreateIndexOperation>(operations[1]);

            var dropIndexOperation = (DropIndexOperation)operations[0];
            var createIndexOperation = (CreateIndexOperation)operations[1];

            Assert.Equal("dbo.MyTable0", dropIndexOperation.TableName);
            Assert.Equal("MyIndex", dropIndexOperation.IndexName);
            Assert.Equal("dbo.MyTable0", createIndexOperation.TableName);
            Assert.Equal("MyIndex", createIndexOperation.IndexName);
            Assert.False(createIndexOperation.IsUnique);
        }

        [Fact]
        public void Diff_handles_transitive_table_renames()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var dependentEntityType = targetModel.GetEntityType("Dependent");
            var principalEntityType = targetModel.GetEntityType("Principal");
            var principalTableName = principalEntityType.TableName();
            principalEntityType.SetTableName(dependentEntityType.TableName());
            dependentEntityType.SetTableName(principalTableName);

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(3, operations.Count);

            Assert.IsType<RenameTableOperation>(operations[0]);
            Assert.IsType<RenameTableOperation>(operations[1]);
            Assert.IsType<RenameTableOperation>(operations[2]);

            var renameTableOperation0 = (RenameTableOperation)operations[0];
            var renameTableOperation1 = (RenameTableOperation)operations[1];
            var renameTableOperation2 = (RenameTableOperation)operations[2];

            Assert.Equal("dbo.MyTable0", renameTableOperation0.TableName);
            Assert.Equal("__mig_tmp__0", renameTableOperation0.NewTableName);
            Assert.Equal("dbo.MyTable1", renameTableOperation1.TableName);
            Assert.Equal("MyTable0", renameTableOperation1.NewTableName);
            Assert.Equal("dbo.__mig_tmp__0", renameTableOperation2.TableName);
            Assert.Equal("MyTable1", renameTableOperation2.NewTableName);
        }

        [Fact]
        public void Diff_handles_transitive_column_renames()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var principalEntityType = sourceModel.GetEntityType("Principal");
            var property = principalEntityType.AddProperty("P1", typeof(string));
            property.SetColumnName("C1");
            property = principalEntityType.AddProperty("P2", typeof(string));
            property.SetColumnName("C2");

            principalEntityType = targetModel.GetEntityType("Principal");
            property = principalEntityType.AddProperty("P1", typeof(string));
            property.SetColumnName("C2");
            property = principalEntityType.AddProperty("P2", typeof(string));
            property.SetColumnName("C1");

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(3, operations.Count);

            Assert.IsType<RenameColumnOperation>(operations[0]);
            Assert.IsType<RenameColumnOperation>(operations[1]);
            Assert.IsType<RenameColumnOperation>(operations[2]);

            var renameColumnOperation0 = (RenameColumnOperation)operations[0];
            var renameColumnOperation1 = (RenameColumnOperation)operations[1];
            var renameColumnOperation2 = (RenameColumnOperation)operations[2];

            Assert.Equal("C1", renameColumnOperation0.ColumnName);
            Assert.Equal("__mig_tmp__0", renameColumnOperation0.NewColumnName);
            Assert.Equal("C2", renameColumnOperation1.ColumnName);
            Assert.Equal("C1", renameColumnOperation1.NewColumnName);
            Assert.Equal("__mig_tmp__0", renameColumnOperation2.ColumnName);
            Assert.Equal("C2", renameColumnOperation2.NewColumnName);
        }

        [Fact]
        public void Diff_handles_transitive_index_renames()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var principalEntityType = sourceModel.GetEntityType("Principal");
            var property = principalEntityType.AddProperty("P1", typeof(string));
            var index = principalEntityType.AddIndex(property);
            index.SetIndexName("IX1");
            property = principalEntityType.AddProperty("P2", typeof(string));
            index = principalEntityType.AddIndex(property);
            index.SetIndexName("IX2");

            principalEntityType = targetModel.GetEntityType("Principal");
            property = principalEntityType.AddProperty("P1", typeof(string));
            index = principalEntityType.AddIndex(property);
            index.SetIndexName("IX2");
            property = principalEntityType.AddProperty("P2", typeof(string));
            index = principalEntityType.AddIndex(property);
            index.SetIndexName("IX1");

            var operations = new ModelDiffer(new DatabaseBuilder()).Diff(sourceModel, targetModel);

            Assert.Equal(3, operations.Count);

            Assert.IsType<RenameIndexOperation>(operations[0]);
            Assert.IsType<RenameIndexOperation>(operations[1]);
            Assert.IsType<RenameIndexOperation>(operations[2]);

            var renameIndexOperation0 = (RenameIndexOperation)operations[0];
            var renameIndexOperation1 = (RenameIndexOperation)operations[1];
            var renameIndexOperation2 = (RenameIndexOperation)operations[2];

            Assert.Equal("IX1", renameIndexOperation0.IndexName);
            Assert.Equal("__mig_tmp__0", renameIndexOperation0.NewIndexName);
            Assert.Equal("IX2", renameIndexOperation1.IndexName);
            Assert.Equal("IX1", renameIndexOperation1.NewIndexName);
            Assert.Equal("__mig_tmp__0", renameIndexOperation2.IndexName);
            Assert.Equal("IX2", renameIndexOperation2.NewIndexName);
        }

        private static Metadata.Model CreateModel()
        {
            var model = new Metadata.Model() { StorageName = "MyDatabase" };

            var dependentEntityType = new EntityType("Dependent");
            dependentEntityType.SetTableName("MyTable0");
            dependentEntityType.SetSchema("dbo");

            var principalEntityType = new EntityType("Principal");
            principalEntityType.SetTableName("MyTable1");
            principalEntityType.SetSchema("dbo");

            var dependentProperty = dependentEntityType.AddProperty("Id", typeof(int));
            var principalProperty = principalEntityType.AddProperty("Id", typeof(int));

            var property = dependentEntityType.AddProperty("MyProperty", typeof(string));
            property.SetColumnName("MyColumn");
            property = principalEntityType.AddProperty("MyProperty", typeof(string));
            property.SetColumnName("MyColumn");

            model.AddEntityType(principalEntityType);
            model.AddEntityType(dependentEntityType);

            principalProperty.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.StorageTypeName, "int"));
            dependentProperty.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.StorageTypeName, "int"));

            dependentEntityType.SetKey(dependentProperty);
            principalEntityType.SetKey(principalProperty);
            dependentEntityType.GetKey().SetKeyName("MyPK0");
            principalEntityType.GetKey().SetKeyName("MyPK1");

            var foreignKey = dependentEntityType.AddForeignKey(principalEntityType.GetKey(), dependentProperty);
            foreignKey.SetKeyName("MyFK");
            foreignKey.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.CascadeDelete, "True"));

            var index = dependentEntityType.AddIndex(dependentProperty);
            index.SetIndexName("MyIndex");
            index.IsUnique = true;

            return model;
        }
    }
}
