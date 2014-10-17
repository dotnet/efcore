// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class DatabaseBuilderTest
    {
        [Fact]
        public void Build_creates_database()
        {
            var database = new TestDatabaseBuilder().GetDatabase(CreateModel());

            Assert.NotNull(database);
            Assert.Equal(2, database.Tables.Count);

            var table0 = database.Tables[0];
            var table1 = database.Tables[1];

            Assert.Equal("dbo.MyTable0", table0.Name);
            Assert.Equal(1, table0.Columns.Count);
            Assert.Equal("Id", table0.Columns[0].Name);
            Assert.Equal("int", table0.Columns[0].DataType);
            Assert.Equal(ValueGeneration.None, table0.Columns[0].ValueGenerationStrategy);

            Assert.NotNull(table1.PrimaryKey.Name);
            Assert.Equal("MyPK0", table0.PrimaryKey.Name);
            Assert.Same(table0.Columns[0], table0.PrimaryKey.Columns[0]);
            Assert.Equal(1, table0.ForeignKeys.Count);

            Assert.Equal("dbo.MyTable1", table1.Name);
            Assert.Equal(2, table1.Columns.Count);
            Assert.Equal("Id", table1.Columns[0].Name);
            Assert.Equal("int", table1.Columns[0].DataType);
            Assert.Equal(ValueGeneration.OnAdd, table1.Columns[0].ValueGenerationStrategy);
            Assert.Null(table1.Columns[0].MaxLength);

            Assert.NotNull(table1.PrimaryKey.Name);
            Assert.Equal("MyPK1", table1.PrimaryKey.Name);
            Assert.Same(table1.Columns[0], table1.PrimaryKey.Columns[0]);
            Assert.Equal(0, table1.ForeignKeys.Count);

            Assert.Equal("Name", table1.Columns[1].Name);
            Assert.Null(table1.Columns[1].DataType);
            Assert.Equal(ValueGeneration.None, table1.Columns[1].ValueGenerationStrategy);
            Assert.Equal(256, table1.Columns[1].MaxLength);

            var foreignKey = table0.ForeignKeys[0];

            Assert.Equal("MyFK", foreignKey.Name);
            Assert.Same(table0, foreignKey.Table);
            Assert.Same(table1, foreignKey.ReferencedTable);
            Assert.Same(table0.Columns[0], foreignKey.Columns[0]);
            Assert.Same(table1.Columns[0], foreignKey.ReferencedColumns[0]);
            // TODO: Cascading behaviors not supported. Issue #333
            //Assert.True(foreignKey.CascadeDelete);

            var index = table0.Indexes[0];

            Assert.Equal("MyIndex", index.Name);
            Assert.Same(table0, index.Table);
            Assert.Same(table0.Columns[0], index.Columns[0]);
            Assert.True(index.IsUnique);
        }

        [Fact]
        public void Build_fills_in_names_if_StorageName_not_specified()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder.Entity<Blog>(b =>
                {
                    b.Key(k => k.BlogId);
                    b.Property(e => e.BlogId);
                });

            modelBuilder.Entity<Post>(b =>
                {
                    b.Key(k => k.PostId);
                    b.Property(e => e.PostId);
                    b.Property(e => e.BelongsToBlogId);
                    b.ForeignKey<Blog>(p => p.BelongsToBlogId);
                    b.Index(ix => ix.PostId);
                });

            var database = new TestDatabaseBuilder().GetDatabase(modelBuilder.Model);

            Assert.True(database.Tables.Any(t => t.Name == "Blog"));
            Assert.True(database.Tables.Any(t => t.Name == "Post"));

            Assert.Equal("BlogId", database.GetTable("Blog").Columns.Single().Name);
            Assert.Equal("PostId", database.GetTable("Post").Columns[0].Name);
            Assert.Equal("BelongsToBlogId", database.GetTable("Post").Columns[1].Name);

            Assert.Equal("PK_Blog", database.GetTable("Blog").PrimaryKey.Name);
            Assert.Equal("PK_Post", database.GetTable("Post").PrimaryKey.Name);

            Assert.Equal("FK_Post_Blog_BelongsToBlogId", database.GetTable("Post").ForeignKeys.Single().Name);

            Assert.Equal("IX_Post_PostId", database.GetTable("Post").Indexes.Single().Name);
        }

        [Fact]
        public void Build_creates_unique_constraints()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("A",
                b =>
                    {
                        var id = b.Property<int>("Id").Metadata;
                        var p1 = b.Property<long>("P1").ForRelational(rb => rb.Column("C1")).Metadata;
                        var p2 = b.Property<string>("P2").Metadata;
                        b.Key("Id").ForRelational().Name("PK");
                        b.Metadata.AddKey(new[] { id, p1 }).Relational().Name = "UC1";
                        b.Metadata.AddKey(new[] { p2 }).Relational().Name = "UC2";
                    });

            var database = new TestDatabaseBuilder().GetDatabase(modelBuilder.Model);

            Assert.Equal(1, database.Tables.Count);

            var table = database.Tables[0];

            Assert.Equal(2, table.UniqueConstraints.Count);
            Assert.Equal("UC1", table.UniqueConstraints[0].Name);
            Assert.Equal(new[] { "Id", "C1" }, table.UniqueConstraints[0].Columns.Select(c => c.Name));
            Assert.Equal("UC2", table.UniqueConstraints[1].Name);
            Assert.Equal(new[] { "P2" }, table.UniqueConstraints[1].Columns.Select(c => c.Name));
            Assert.NotNull(table.PrimaryKey);
            Assert.Equal("PK", table.PrimaryKey.Name);
        }

        [Fact]
        public void Build_fills_in_unique_constraint_name_if_not_specified()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("A",
                b =>
                    {
                        var id = b.Property<int>("Id").Metadata;
                        var p1 = b.Property<long>("P1").ForRelational(rb => rb.Column("C1")).Metadata;
                        var p2 = b.Property<string>("P2").Metadata;
                        b.Key("Id").ForRelational().Name("PK");
                        b.Metadata.AddKey(new[] { id, p1 });
                        b.Metadata.AddKey(new[] { p2 });
                    });

            var database = new TestDatabaseBuilder().GetDatabase(modelBuilder.Model);
            var table = database.Tables[0];

            Assert.Equal(2, table.UniqueConstraints.Count);
            Assert.Equal("UC_A_Id_C1", table.UniqueConstraints[0].Name);
            Assert.Equal("UC_A_P2", table.UniqueConstraints[1].Name);

            modelBuilder.Entity("A").ForRelational().Table("T", "dbo");
            database = new TestDatabaseBuilder().GetDatabase(modelBuilder.Model);
            table = database.Tables[0];

            Assert.Equal(2, table.UniqueConstraints.Count);
            Assert.Equal("UC_dbo.T_Id_C1", table.UniqueConstraints[0].Name);
            Assert.Equal("UC_dbo.T_P2", table.UniqueConstraints[1].Name);
        }

        private class Blog
        {
            public int BlogId { get; set; }
        }

        private class Post
        {
            public int PostId { get; set; }
            public int BelongsToBlogId { get; set; }
        }

        [Fact]
        public void Name_for_multi_column_FKs()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder.Entity<Principal>()
                .Key(k => new { k.Id0, k.Id1 });

            modelBuilder.Entity<Dependent>(b =>
                {
                    b.Key(k => k.Id);
                    b.ForeignKey<Principal>(p => new { p.FkAAA, p.FkZZZ });
                });

            var builder = new TestDatabaseBuilder();
            var name = builder.GetDatabase(modelBuilder.Model).GetTable("Dependent").ForeignKeys.Single().Name;

            Assert.Equal("FK_Dependent_Principal_FkAAA_FkZZZ", name);
        }

        private class Principal
        {
            public int Id0 { get; set; }
            public int Id1 { get; set; }
        }

        private class Dependent
        {
            public int Id { get; set; }
            public int FkAAA { get; set; }
            public int FkZZZ { get; set; }
        }

        [Fact]
        public void Name_for_multi_column_Indexes()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder.Entity<Dependent>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.Id);
                    b.Property(e => e.FkAAA).ForRelational().Column("ColumnAaa");
                    b.Property(e => e.FkZZZ).ForRelational().Column("ColumnZzz");
                    b.ForRelational().Table("MyTable");
                    b.Index(e => new { e.FkAAA, e.FkZZZ });
                });

            var builder = new TestDatabaseBuilder();
            var name = builder.GetDatabase(modelBuilder.Model).GetTable("MyTable").Indexes.Single().Name;

            Assert.Equal("IX_MyTable_ColumnAaa_ColumnZzz", name);
        }

        [Fact]
        public void Columns_are_ordered_by_name_with_pk_columns_first_and_fk_columns_last()
        {
            var modelBuider = new BasicModelBuilder();
            modelBuider.Entity("A",
                b =>
                    {
                        b.Property<int>("Px");
                        b.Property<int>("Py");
                        b.Key("Px", "Py");
                    });
            modelBuider.Entity("B",
                b =>
                    {
                        b.Property<int>("P6");
                        b.Property<int>("P5");
                        b.Property<int>("P4");
                        b.Property<int>("P3");
                        b.Property<int>("P2");
                        b.Property<int>("P1");
                        b.Key("P5", "P2");
                        b.ForeignKey("A", "P6", "P4");
                        b.ForeignKey("A", "P4", "P5");
                    });

            var databaseModel = new TestDatabaseBuilder().GetDatabase(modelBuider.Model);

            Assert.Equal(2, databaseModel.Tables.Count);
            Assert.Equal(new[] { "Px", "Py" }, databaseModel.Tables[0].Columns.Select(c => c.Name));
            Assert.Equal(new[] { "P5", "P2", "P1", "P3", "P6", "P4" }, databaseModel.Tables[1].Columns.Select(c => c.Name));
        }

        private static IModel CreateModel()
        {
            var model = new Entity.Metadata.Model { StorageName = "MyDatabase" };

            var dependentEntityType = model.AddEntityType("Dependent");
            dependentEntityType.Relational().Schema = "dbo";
            dependentEntityType.Relational().Table = "MyTable0";

            var principalEntityType = model.AddEntityType("Principal");
            principalEntityType.Relational().Schema = "dbo";
            principalEntityType.Relational().Table = "MyTable1";

            var dependentProperty = dependentEntityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            var principalProperty = principalEntityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            principalProperty.ValueGeneration = ValueGeneration.OnAdd;

            principalProperty.Relational().ColumnType = "int";
            dependentProperty.Relational().ColumnType = "int";

            dependentEntityType.GetOrSetPrimaryKey(dependentProperty);
            principalEntityType.GetOrSetPrimaryKey(principalProperty);
            dependentEntityType.GetPrimaryKey().Relational().Name = "MyPK0";
            principalEntityType.GetPrimaryKey().Relational().Name = "MyPK1";

            var foreignKey = dependentEntityType.GetOrAddForeignKey(dependentProperty, principalEntityType.GetPrimaryKey());
            foreignKey.Relational().Name = "MyFK";
            // TODO: Cascading behaviors not supported. Issue #333
            //foreignKey.Annotations.Add(new Annotation(
            //    MetadataExtensions.Annotations.CascadeDelete, "True"));

            var index = dependentEntityType.GetOrAddIndex(dependentProperty);
            index.Relational().Name = "MyIndex";
            index.IsUnique = true;

            var stringProperty = principalEntityType.GetOrAddProperty("Name", typeof(string), shadowProperty: true);
            stringProperty.MaxLength = 256;

            return model;
        }

        private class TestDatabaseBuilder : DatabaseBuilder
        {
            public TestDatabaseBuilder()
                : base(new RelationalTypeMapper())
            {
            }

            protected override Sequence BuildSequence(IProperty property)
            {
                return null;
            }
        }
    }
}
