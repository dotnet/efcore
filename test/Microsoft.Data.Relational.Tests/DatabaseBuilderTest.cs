// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;
using Microsoft.Data.Relational.Model;
using Metadata = Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Relational.Tests
{
    public class DatabaseBuilderTest
    {
        [Fact]
        public void Build_creates_database()
        {
            var database = new DatabaseBuilder().Build(CreateModel());

            Assert.NotNull(database);
            Assert.Equal(2, database.Tables.Count);

            var table0 = database.Tables[0];
            var table1 = database.Tables[1];

            Assert.Equal("dbo.MyTable0", table0.Name);
            Assert.Equal(1, table0.Columns.Count);
            Assert.Equal("Id", table0.Columns[0].Name);
            Assert.Equal("int", table0.Columns[0].DataType);
            Assert.Equal(StoreValueGenerationStrategy.None, table0.Columns[0].GenerationStrategy);
            Assert.NotNull(table1.PrimaryKey.Name);
            Assert.Equal("MyPK0", table0.PrimaryKey.Name);
            Assert.Same(table0.Columns[0], table0.PrimaryKey.Columns[0]);
            Assert.Equal(1, table0.ForeignKeys.Count);

            Assert.Equal("dbo.MyTable1", table1.Name);
            Assert.Equal(1, table1.Columns.Count);
            Assert.Equal("Id", table1.Columns[0].Name);
            Assert.Equal("int", table1.Columns[0].DataType);
            Assert.Equal(StoreValueGenerationStrategy.Identity, table1.Columns[0].GenerationStrategy);
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

        [Fact]
        public void Build_fills_in_names_if_StorageName_not_specified()
        {
            // TODO: Add and Index when supported by DatabaseBuilder.
            
            // Using Moq because real types will fill in StorageName in some places
            var blogEntity = new Mock<IEntityType>();
            var blogProperty = new Mock<IProperty>();
            var blogKey = new Mock<IKey>();

            blogEntity.Setup(e => e.Name).Returns("Blog");
            blogEntity.Setup(e => e.GetKey()).Returns(blogKey.Object);
            blogEntity.Setup(e => e.Properties).Returns(new List<IProperty> { blogProperty.Object });
            blogEntity.Setup(e => e.ForeignKeys).Returns(new List<IForeignKey>());
            
            blogProperty.Setup(e => e.Name).Returns("BlogId");
            blogProperty.Setup(e => e.PropertyType).Returns(typeof(int));

            blogKey.Setup(k => k.Properties).Returns(new List<IProperty> { blogProperty.Object });
            blogKey.Setup(k => k.EntityType).Returns(blogEntity.Object);

            var postEntity = new Mock<IEntityType>();
            var postKeyProperty = new Mock<IProperty>();
            var postKey = new Mock<IKey>();
            var postForeignKeyProperty = new Mock<IProperty>();
            var postForeignKey = new Mock<IForeignKey>();

            postEntity.Setup(e => e.Name).Returns("Post");
            postEntity.Setup(e => e.GetKey()).Returns(postKey.Object);
            postEntity.Setup(e => e.Properties).Returns(new List<IProperty> { postKeyProperty.Object, postForeignKeyProperty.Object });
            postEntity.Setup(e => e.ForeignKeys).Returns(new List<IForeignKey> { postForeignKey.Object });

            postKeyProperty.Setup(e => e.Name).Returns("PostId");
            postKeyProperty.Setup(e => e.PropertyType).Returns(typeof(int));

            postForeignKeyProperty.Setup(e => e.Name).Returns("BelongsToBlogId");
            postForeignKeyProperty.Setup(e => e.PropertyType).Returns(typeof(int));

            postKey.Setup(k => k.Properties).Returns(new List<IProperty> { postKeyProperty.Object });
            postKey.Setup(k => k.EntityType).Returns(postEntity.Object);

            postForeignKey.Setup(f => f.EntityType).Returns(postEntity.Object);
            postForeignKey.Setup(f => f.Properties).Returns(new List<IProperty> { postForeignKeyProperty.Object });
            postForeignKey.Setup(f => f.ReferencedEntityType).Returns(blogEntity.Object);
            postForeignKey.Setup(f => f.ReferencedProperties).Returns(new List<IProperty> { blogProperty.Object });

            var model = new Mock<IModel>();
            model.Setup(m => m.EntityTypes).Returns(new List<IEntityType> { blogEntity.Object, postEntity.Object });

            // Ensure we have a valid test
            Assert.Null(blogEntity.Object.StorageName);
            Assert.Null(blogEntity.Object.GetKey().StorageName);
            Assert.Null(blogEntity.Object.Properties.Single().StorageName);
            Assert.Null(postEntity.Object.StorageName);
            Assert.Null(postEntity.Object.GetKey().StorageName);
            Assert.False(postEntity.Object.Properties.Any(p => p.StorageName != null));
            Assert.Null(postEntity.Object.ForeignKeys.Single().StorageName);

            var builder = new DatabaseBuilder();
            var database = builder.Build(model.Object);

            Assert.True(database.Tables.Any(t => t.Name == "Blog"));
            Assert.True(database.Tables.Any(t => t.Name == "Post"));

            Assert.Equal("BlogId", database.GetTable("Blog").Columns.Single().Name);
            Assert.Equal("PostId", database.GetTable("Post").Columns[0].Name);
            Assert.Equal("BelongsToBlogId", database.GetTable("Post").Columns[1].Name);

            Assert.Equal("PK_Blog", database.GetTable("Blog").PrimaryKey.Name);
            Assert.Equal("PK_Post", database.GetTable("Post").PrimaryKey.Name);

            Assert.Equal("FK_Post_Blog_BelongsToBlogId", database.GetTable("Post").ForeignKeys.Single().Name);
        }

        [Fact]
        public void Name_for_multi_column_FKs()
        {
            var principalEntity = new Mock<IEntityType>();
            var dependentEntity = new Mock<IEntityType>();
            var fkPropertyOne = new Mock<IProperty>();
            var fkPropertyTwo = new Mock<IProperty>();
            var foreignKey = new Mock<IForeignKey>();

            principalEntity.Setup(e => e.Name).Returns("Principal");

            dependentEntity.Setup(e => e.Name).Returns("Dependent");

            fkPropertyOne.Setup(e => e.Name).Returns("FkZZZ");
            fkPropertyOne.Setup(e => e.PropertyType).Returns(typeof(int));

            fkPropertyTwo.Setup(e => e.Name).Returns("FkAAA");
            fkPropertyTwo.Setup(e => e.PropertyType).Returns(typeof(int));

            foreignKey.Setup(f => f.EntityType).Returns(dependentEntity.Object);
            foreignKey.Setup(f => f.Properties).Returns(new List<IProperty> { fkPropertyOne.Object, fkPropertyTwo.Object });
            foreignKey.Setup(f => f.ReferencedEntityType).Returns(principalEntity.Object);

            var builder = new DatabaseBuilder();
            var name = builder.ForeignKeyName(foreignKey.Object);

            Assert.Equal("FK_Dependent_Principal_FkAAA_FkZZZ", name);
        }

        [Fact]
        public void Name_for_multi_column_Indexes()
        {
            var table = new Table("MyTable");
            var columnOne = new Column("ColumnZzz", "int");
            var columnTwo = new Column("ColumnAaa", "int");
            
            var builder = new DatabaseBuilder();
            var name = builder.IndexName(table, new List<Column>{ columnOne, columnTwo });

            Assert.Equal("IX_MyTable_ColumnAaa_ColumnZzz", name);
        }

        private static IModel CreateModel()
        {
            var model = new Metadata.Model { StorageName = "MyDatabase" };

            var dependentEntityType = new EntityType("Dependent") { StorageName = "dbo.MyTable0" };
            var principalEntityType = new EntityType("Principal") { StorageName = "dbo.MyTable1" };

            var dependentProperty = dependentEntityType.AddProperty("Id", typeof(int));
            var principalProperty = principalEntityType.AddProperty("Id", typeof(int));
            principalProperty.ValueGenerationStrategy = ValueGenerationStrategy.StoreIdentity;

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
