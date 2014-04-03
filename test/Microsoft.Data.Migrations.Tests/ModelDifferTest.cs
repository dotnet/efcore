// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Migrations.Model;
using Xunit;

namespace Microsoft.Data.Migrations.Tests
{
    public class ModelDifferTest
    {
        [Fact]
        public void DiffSource_creates_operations()
        {
            var operations = new ModelDiffer().DiffSource(CreateModel());

            Assert.Equal(3, operations.Count);

            var createTableOperation0 = (CreateTableOperation)operations[0];
            var createTableOperation1 = (CreateTableOperation)operations[1];
            var addForeignKeyOperation = (AddForeignKeyOperation)operations[2];

            Assert.Equal("dbo.MyTable0", createTableOperation0.Table.Name);
            Assert.Equal("dbo.MyTable1", createTableOperation1.Table.Name);
            Assert.Equal("MyFK", addForeignKeyOperation.ForeignKeyName);
        }

        [Fact]
        public void DiffTarget_creates_operations()
        {
            var operations = new ModelDiffer().DiffTarget(CreateModel());

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

            targetModel.GetEntityType("Dependent").StorageName = "newdbo.MyTable0";

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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

            targetModel.GetEntityType("Dependent").StorageName = "dbo.MyNewTable0";

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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
            var dependentEntityType = new EntityType("NewDependent") { StorageName = "dbo.MyNewTable" };
            var dependentProperty = dependentEntityType.AddProperty("Id", typeof(int));

            targetModel.AddEntityType(dependentEntityType);

            dependentProperty.AddAnnotation(new Annotation(
                MetadataExtensions.Annotations.StorageTypeName, "int"));

            dependentEntityType.SetKey(dependentProperty);
            dependentEntityType.GetKey().StorageName = "MyNewPK";

            var foreignKey = dependentEntityType.AddForeignKey(principalEntityType.GetKey(), dependentProperty);
            foreignKey.StorageName = "MyNewFK";
            foreignKey.AddAnnotation(new Annotation(
                MetadataExtensions.Annotations.CascadeDelete, "True"));

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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
            var dependentEntityType = new EntityType("OldDependent") { StorageName = "dbo.MyOldTable" };
            var dependentProperty = dependentEntityType.AddProperty("Id", typeof(int));

            sourceModel.AddEntityType(dependentEntityType);

            dependentProperty.AddAnnotation(new Annotation(
                MetadataExtensions.Annotations.StorageTypeName, "int"));

            dependentEntityType.SetKey(dependentProperty);
            dependentEntityType.GetKey().StorageName = "MyOldPK";

            var foreignKey = dependentEntityType.AddForeignKey(principalEntityType.GetKey(), dependentProperty);
            foreignKey.StorageName = "MyOldFK";
            foreignKey.AddAnnotation(new Annotation(
                MetadataExtensions.Annotations.CascadeDelete, "True"));

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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

            targetModel.GetEntityType("Dependent").GetProperty("Id").StorageName = "NewId";

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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
            property.StorageName = "MyNewColumn";

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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
            property.StorageName = "MyOldColumn";

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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
            property.StorageName = "MyColumn";

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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
            property.AddAnnotation(new Annotation(
                MetadataExtensions.Annotations.StorageTypeName, "nvarchar(10)"));

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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
            property.AddAnnotation(new Annotation(
                MetadataExtensions.Annotations.ColumnDefaultValue, "MyDefaultValue"));

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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
            property.AddAnnotation(new Annotation(
                MetadataExtensions.Annotations.ColumnDefaultSql, "MyDefaultSql"));

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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
            property.AddAnnotation(new Annotation(
                MetadataExtensions.Annotations.ColumnDefaultValue, "MyDefaultValue"));

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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
            property.AddAnnotation(new Annotation(
                MetadataExtensions.Annotations.ColumnDefaultSql, "MyDefaultSql"));

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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
            entityType.GetKey().StorageName = "MyNewPK";

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

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

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropForeignKeyOperation>(operations[0]);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];

            Assert.Equal("dbo.MyTable0", dropForeignKeyOperation.TableName);
            Assert.Equal("MyFK", dropForeignKeyOperation.ForeignKeyName);
        }

        [Fact]
        public void Diff_handles_table_rename_conflicts()
        {
            var sourceModel = CreateModel();
            var targetModel = CreateModel();

            var dependentEntityType = targetModel.GetEntityType("Dependent");
            var principalEntityType = targetModel.GetEntityType("Principal");
            targetModel.RemoveEntityType(dependentEntityType);
            principalEntityType.StorageName = dependentEntityType.StorageName;

            var operations = new ModelDiffer().Diff(sourceModel, targetModel);

            Assert.Equal(3, operations.Count);

            Assert.IsType<RenameTableOperation>(operations[0]);
            Assert.IsType<RenameTableOperation>(operations[1]);
            Assert.IsType<DropTableOperation>(operations[2]);

            var renameTableOperation0 = (RenameTableOperation)operations[0];
            var renameTableOperation1 = (RenameTableOperation)operations[1];
            var dropTableOperation = (DropTableOperation)operations[2];

            Assert.Equal("dbo.MyTable0", renameTableOperation0.TableName);
            Assert.Equal("__mig_tmp__0", renameTableOperation0.NewTableName);
            Assert.Equal("dbo.MyTable1", renameTableOperation1.TableName);
            Assert.Equal("MyTable0", renameTableOperation1.NewTableName);
            Assert.Equal("dbo.__mig_tmp__0", dropTableOperation.TableName);
        }

        private static Entity.Metadata.Model CreateModel()
        {
            var model = new Entity.Metadata.Model() { StorageName = "MyDatabase" };

            var dependentEntityType = new EntityType("Dependent") { StorageName = "dbo.MyTable0" };
            var principalEntityType = new EntityType("Principal") { StorageName = "dbo.MyTable1" };
            var dependentProperty = dependentEntityType.AddProperty("Id", typeof(int));
            var principalProperty = principalEntityType.AddProperty("Id", typeof(int));

            var property = dependentEntityType.AddProperty("MyProperty", typeof(string));
            property.StorageName = "MyColumn";
            property = principalEntityType.AddProperty("MyProperty", typeof(string));
            property.StorageName = "MyColumn";

            model.AddEntityType(principalEntityType);
            model.AddEntityType(dependentEntityType);

            principalProperty.AddAnnotation(new Annotation(
                MetadataExtensions.Annotations.StorageTypeName, "int"));
            dependentProperty.AddAnnotation(new Annotation(
                MetadataExtensions.Annotations.StorageTypeName, "int"));

            dependentEntityType.SetKey(dependentProperty);
            principalEntityType.SetKey(principalProperty);
            dependentEntityType.GetKey().StorageName = "MyPK0";
            principalEntityType.GetKey().StorageName = "MyPK1";

            var foreignKey = dependentEntityType.AddForeignKey(principalEntityType.GetKey(), dependentProperty);
            foreignKey.StorageName = "MyFK";
            foreignKey.AddAnnotation(new Annotation(
                MetadataExtensions.Annotations.CascadeDelete, "True"));

            return model;
        }
    }
}
