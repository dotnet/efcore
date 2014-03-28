// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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

        private static Entity.Metadata.Model CreateModel()
        {
            var model = new Entity.Metadata.Model() { StorageName = "MyDatabase" };

            var dependentEntityType = new EntityType("Dependent") { StorageName = "dbo.MyTable0" };
            var principalEntityType = new EntityType("Principal") { StorageName = "dbo.MyTable1" };
            var dependentProperty = dependentEntityType.AddProperty("Id", typeof(int), shadowProperty: false);
            var principalProperty = principalEntityType.AddProperty("Id", typeof(int), shadowProperty: false);

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
