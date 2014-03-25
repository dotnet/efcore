// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Metadata = Microsoft.Data.Entity.Metadata;
using Xunit;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Relational
{
    public class DatabaseBuilderTest
    {
        [Fact]
        public void Build_creates_database()
        {
            var database = new DatabaseBuilder().Build(CreateModel());

            Assert.NotNull(database);
            Assert.Equal("MyDatabase", database.Name);
            Assert.Equal(2, database.Tables.Count);

            var table0 = database.Tables[0];
            var table1 = database.Tables[1];

            Assert.Equal("dbo.MyTable0", table0.Name);
            Assert.Equal(1, table0.Columns.Count);
            Assert.Equal("Id", table0.Columns[0].Name);
            Assert.Equal("int", table0.Columns[0].DataType);
            Assert.NotNull(table1.PrimaryKey.Name);
            Assert.Equal("MyPK0", table0.PrimaryKey.Name);
            Assert.Same(table0.Columns[0], table0.PrimaryKey.Columns[0]);
            Assert.Equal(1, table0.ForeignKeys.Count);

            Assert.Equal("dbo.MyTable1", table1.Name);
            Assert.Equal(1, table1.Columns.Count);
            Assert.Equal("Id", table1.Columns[0].Name);
            Assert.Equal("int", table1.Columns[0].DataType);
            Assert.NotNull(table1.PrimaryKey.Name);
            Assert.Equal("MyPK1", table1.PrimaryKey.Name);
            Assert.Same(table1.Columns[0], table1.PrimaryKey.Columns[0]);
            Assert.Equal(0, table1.ForeignKeys.Count);

            var foreignKey = table0.ForeignKeys[0];

            Assert.Equal("MyFK", foreignKey.Name);
            Assert.Same(table0, foreignKey.Table);
            Assert.Same(table1, foreignKey.ReferencedTable);
            Assert.Same(table0.Columns[0], foreignKey.Columns[0]);
            Assert.Same(table1.Columns[0], foreignKey.ReferencedColumns[0]);
            Assert.True(foreignKey.CascadeDelete);
        }

        private static Metadata.IModel CreateModel()
        {
            var model = new Metadata.Model() { StorageName = "MyDatabase" };

            var dependentEntityType = new Metadata.EntityType("Dependent") { StorageName = "dbo.MyTable0" };
            var principalEntityType = new Metadata.EntityType("Principal") { StorageName = "dbo.MyTable1" };
            var dependentProperty = dependentEntityType.AddProperty("Id", typeof(int), shadowProperty: false);
            var principalProperty = principalEntityType.AddProperty("Id", typeof(int), shadowProperty: false);

            model.AddEntityType(principalEntityType);
            model.AddEntityType(dependentEntityType);

            principalProperty.AddAnnotation(new Metadata.Annotation(
                ApiExtensions.Annotations.StorageTypeName, "int"));
            dependentProperty.AddAnnotation(new Metadata.Annotation(
                ApiExtensions.Annotations.StorageTypeName, "int"));

            dependentEntityType.SetKey(dependentProperty);
            principalEntityType.SetKey(principalProperty);
            dependentEntityType.GetKey().StorageName = "MyPK0";
            principalEntityType.GetKey().StorageName = "MyPK1";            

            var foreignKey = dependentEntityType.AddForeignKey(principalEntityType.GetKey(), dependentProperty);
            foreignKey.StorageName = "MyFK";
            foreignKey.AddAnnotation(new Metadata.Annotation(
                ApiExtensions.Annotations.CascadeDelete, "True"));

            return model;
        }
    }
}
