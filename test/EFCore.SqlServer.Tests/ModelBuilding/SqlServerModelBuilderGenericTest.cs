// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class SqlServerModelBuilderGenericTest : ModelBuilderGenericTest
    {
        public class SqlServerGenericNonRelationship : GenericNonRelationship
        {
            [ConditionalFact]
            public virtual void Index_has_a_filter_if_nonclustered_unique_with_nullable_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var entityTypeBuilder = modelBuilder
                    .Entity<Customer>();
                var indexBuilder = entityTypeBuilder
                    .HasIndex(ix => ix.Name)
                    .IsUnique();

                var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
                var index = entityType.GetIndexes().Single();
                Assert.Equal("[Name] IS NOT NULL", index.GetFilter());

                indexBuilder.IsUnique(false);

                Assert.Null(index.GetFilter());

                indexBuilder.IsUnique();

                Assert.Equal("[Name] IS NOT NULL", index.GetFilter());

                indexBuilder.IsClustered();

                Assert.Null(index.GetFilter());

                indexBuilder.IsClustered(false);

                Assert.Equal("[Name] IS NOT NULL", index.GetFilter());

                entityTypeBuilder.Property(e => e.Name).IsRequired();

                Assert.Null(index.GetFilter());

                entityTypeBuilder.Property(e => e.Name).IsRequired(false);

                Assert.Equal("[Name] IS NOT NULL", index.GetFilter());

                entityTypeBuilder.Property(e => e.Name).HasColumnName("RelationalName");

                Assert.Equal("[RelationalName] IS NOT NULL", index.GetFilter());

                entityTypeBuilder.Property(e => e.Name).HasColumnName("SqlServerName");

                Assert.Equal("[SqlServerName] IS NOT NULL", index.GetFilter());

                entityTypeBuilder.Property(e => e.Name).HasColumnName(null);

                Assert.Equal("[Name] IS NOT NULL", index.GetFilter());

                indexBuilder.HasFilter("Foo");

                Assert.Equal("Foo", index.GetFilter());

                indexBuilder.HasFilter(null);

                Assert.Null(index.GetFilter());
            }

            [ConditionalFact]
            public virtual void Can_set_store_type_for_property_type()
            {
                var modelBuilder = CreateModelBuilder(c =>
                {
                    c.Properties<int>().HaveColumnType("smallint");
                    c.Properties<string>().HaveColumnType("nchar(max)");
                });

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property<int>("Charm");
                        b.Property<string>("Strange");
                        b.Property<int>("Top");
                        b.Property<string>("Bottom");
                    });

                var model = modelBuilder.FinalizeModel();
                var entityType = model.FindEntityType(typeof(Quarks));

                Assert.Equal("smallint", entityType.FindProperty(Customer.IdProperty.Name).GetColumnType());
                Assert.Equal("smallint", entityType.FindProperty("Up").GetColumnType());
                Assert.Equal("nchar(max)", entityType.FindProperty("Down").GetColumnType());
                Assert.Equal("smallint", entityType.FindProperty("Charm").GetColumnType());
                Assert.Equal("nchar(max)", entityType.FindProperty("Strange").GetColumnType());
                Assert.Equal("smallint", entityType.FindProperty("Top").GetColumnType());
                Assert.Equal("nchar(max)", entityType.FindProperty("Bottom").GetColumnType());
            }

            [ConditionalFact]
            public virtual void Can_set_fixed_length_for_property_type()
            {
                var modelBuilder = CreateModelBuilder(c =>
                {
                    c.Properties<int>().AreFixedLength(false);
                    c.Properties<string>().AreFixedLength();
                });

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property<int>("Charm");
                        b.Property<string>("Strange");
                        b.Property<int>("Top");
                        b.Property<string>("Bottom");
                    });

                var model = modelBuilder.FinalizeModel();
                var entityType = model.FindEntityType(typeof(Quarks));

                Assert.False(entityType.FindProperty(Customer.IdProperty.Name).IsFixedLength());
                Assert.False(entityType.FindProperty("Up").IsFixedLength());
                Assert.True(entityType.FindProperty("Down").IsFixedLength());
                Assert.False(entityType.FindProperty("Charm").IsFixedLength());
                Assert.True(entityType.FindProperty("Strange").IsFixedLength());
                Assert.False(entityType.FindProperty("Top").IsFixedLength());
                Assert.True(entityType.FindProperty("Bottom").IsFixedLength());
            }

            [ConditionalFact]
            public virtual void Can_set_collation_for_property_type()
            {
                var modelBuilder = CreateModelBuilder(c =>
                {
                    c.Properties<int>().UseCollation("Latin1_General_CS_AS_KS_WS");
                    c.Properties<string>().UseCollation("Latin1_General_BIN");
                });

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property<int>("Charm");
                        b.Property<string>("Strange");
                        b.Property<int>("Top");
                        b.Property<string>("Bottom");
                    });

                var model = modelBuilder.FinalizeModel();
                var entityType = model.FindEntityType(typeof(Quarks));

                Assert.Equal("Latin1_General_CS_AS_KS_WS", entityType.FindProperty(Customer.IdProperty.Name).GetCollation());
                Assert.Equal("Latin1_General_CS_AS_KS_WS", entityType.FindProperty("Up").GetCollation());
                Assert.Equal("Latin1_General_BIN", entityType.FindProperty("Down").GetCollation());
                Assert.Equal("Latin1_General_CS_AS_KS_WS", entityType.FindProperty("Charm").GetCollation());
                Assert.Equal("Latin1_General_BIN", entityType.FindProperty("Strange").GetCollation());
                Assert.Equal("Latin1_General_CS_AS_KS_WS", entityType.FindProperty("Top").GetCollation());
                Assert.Equal("Latin1_General_BIN", entityType.FindProperty("Bottom").GetCollation());
            }

            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance, configure);
        }

        public class SqlServerGenericInheritance : GenericInheritance
        {
            [ConditionalFact] // #7240
            public void Can_use_shadow_FK_that_collides_with_convention_shadow_FK_on_other_derived_type()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Child>();
                modelBuilder.Entity<Parent>()
                    .HasOne(p => p.A)
                    .WithOne()
                    .HasForeignKey<DisjointChildSubclass1>("ParentId");

                var model = modelBuilder.FinalizeModel();

                var property1 = model.FindEntityType(typeof(DisjointChildSubclass1)).FindProperty("ParentId");
                Assert.True(property1.IsForeignKey());
                Assert.Equal("ParentId", property1.GetColumnBaseName());
                var property2 = model.FindEntityType(typeof(DisjointChildSubclass2)).FindProperty("ParentId");
                Assert.True(property2.IsForeignKey());
                Assert.Equal("DisjointChildSubclass2_ParentId", property2.GetColumnBaseName());
            }

            [ConditionalFact]
            public void Inherited_clr_properties_are_mapped_to_the_same_column()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<ChildBase>();
                modelBuilder.Ignore<Child>();
                modelBuilder.Entity<DisjointChildSubclass1>();
                modelBuilder.Entity<DisjointChildSubclass2>();

                var model = modelBuilder.FinalizeModel();

                var property1 = model.FindEntityType(typeof(DisjointChildSubclass1)).FindProperty(nameof(Child.Name));
                Assert.Equal(nameof(Child.Name), property1.GetColumnBaseName());
                var property2 = model.FindEntityType(typeof(DisjointChildSubclass2)).FindProperty(nameof(Child.Name));
                Assert.Equal(nameof(Child.Name), property2.GetColumnBaseName());
            }

            [ConditionalFact] //Issue#10659
            public void Index_convention_run_for_fk_when_derived_type_discovered_before_base_type()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<Order>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Entity<DetailsBase>();

                var index = modelBuilder.Model.FindEntityType(typeof(CustomerDetails)).GetIndexes().Single();

                Assert.Equal("[CustomerId] IS NOT NULL", index.GetFilter());
            }

            [ConditionalFact]
            public void Index_convention_sets_filter_for_unique_index_when_base_type_changed()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<Customer>();
                modelBuilder.Entity<CustomerDetails>()
                    .HasIndex(e => e.CustomerId)
                    .IsUnique();

                modelBuilder.Entity<DetailsBase>();

                var index = modelBuilder.Model.FindEntityType(typeof(CustomerDetails)).GetIndexes().Single();

                Assert.Equal("[CustomerId] IS NOT NULL", index.GetFilter());

                modelBuilder.Ignore<DetailsBase>();

                Assert.Null(index.GetFilter());
            }

            [ConditionalFact]
            public virtual void TPT_identifying_FK_are_created_only_on_declaring_type()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<BigMak>()
                    .Ignore(b => b.Bun)
                    .Ignore(b => b.Pickles);
                modelBuilder.Entity<Ingredient>(b =>
                {
                    b.ToTable("Ingredients");
                    b.Ignore(i => i.BigMak);
                });
                modelBuilder.Entity<Bun>(b =>
                {
                    b.ToTable("Buns");
                    b.HasOne(i => i.BigMak).WithOne().HasForeignKey<Bun>(i => i.Id);
                });
                modelBuilder.Entity<SesameBun>(b =>
                {
                    b.ToTable("SesameBuns");
                });

                var model = modelBuilder.FinalizeModel();

                var principalType = model.FindEntityType(typeof(BigMak));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Empty(principalType.GetIndexes());

                var ingredientType = model.FindEntityType(typeof(Ingredient));

                var bunType = model.FindEntityType(typeof(Bun));
                Assert.Empty(bunType.GetIndexes());
                var bunFk = bunType.GetDeclaredForeignKeys().Single(fk => !fk.IsBaseLinking());
                Assert.Equal("FK_Buns_BigMak_Id", bunFk.GetConstraintName());
                Assert.Equal("FK_Buns_BigMak_Id", bunFk.GetConstraintName(
                    StoreObjectIdentifier.Create(bunType, StoreObjectType.Table).Value,
                    StoreObjectIdentifier.Create(principalType, StoreObjectType.Table).Value));
                Assert.Single(bunFk.GetMappedConstraints());

                var bunLinkingFk = bunType.GetDeclaredForeignKeys().Single(fk => fk.IsBaseLinking());
                Assert.Equal("FK_Buns_Ingredients_Id", bunLinkingFk.GetConstraintName());
                Assert.Equal("FK_Buns_Ingredients_Id", bunLinkingFk.GetConstraintName(
                    StoreObjectIdentifier.Create(bunType, StoreObjectType.Table).Value,
                    StoreObjectIdentifier.Create(ingredientType, StoreObjectType.Table).Value));
                Assert.Single(bunLinkingFk.GetMappedConstraints());

                var sesameBunType = model.FindEntityType(typeof(SesameBun));
                Assert.Empty(sesameBunType.GetIndexes());
                var sesameBunFk = sesameBunType.GetDeclaredForeignKeys().Single();
                Assert.True(sesameBunFk.IsBaseLinking());
                Assert.Equal("FK_SesameBuns_Buns_Id", sesameBunFk.GetConstraintName());
                Assert.Equal("FK_SesameBuns_Buns_Id", sesameBunFk.GetConstraintName(
                    StoreObjectIdentifier.Create(sesameBunType, StoreObjectType.Table).Value,
                    StoreObjectIdentifier.Create(bunType, StoreObjectType.Table).Value));
                Assert.Single(sesameBunFk.GetMappedConstraints());
            }

            [ConditionalFact]
            public virtual void TPT_index_can_use_inherited_properties()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<BigMak>()
                    .Ignore(b => b.Bun)
                    .Ignore(b => b.Pickles);
                modelBuilder.Entity<Ingredient>(b =>
                {
                    b.ToTable("Ingredients");
                    b.Property<int?>("NullableProp");
                    b.Ignore(i => i.BigMak);
                });
                modelBuilder.Entity<Bun>(b =>
                {
                    b.ToTable("Buns");
                    b.HasIndex(bun => bun.BurgerId);
                    b.HasIndex("NullableProp");
                    b.HasOne(i => i.BigMak).WithOne().HasForeignKey<Bun>(i => i.Id);
                });

                var model = modelBuilder.FinalizeModel();

                var bunType = model.FindEntityType(typeof(Bun));
                Assert.All(bunType.GetIndexes(), i => Assert.Null(i.GetFilter()));
            }

            [ConditionalFact]
            public void Can_add_check_constraints()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Child>()
                    .HasBaseType(null)
                    .HasCheckConstraint("LargeId", "Id > 1000", c => c.HasName("CK_LargeId"));
                modelBuilder.Entity<ChildBase>()
                    .HasCheckConstraint("PositiveId", "Id > 0")
                    .HasCheckConstraint("LargeId", "Id > 1000");
                modelBuilder.Entity<Child>()
                    .HasBaseType<ChildBase>();
                modelBuilder.Entity<DisjointChildSubclass1>();

                var model = modelBuilder.FinalizeModel();

                var @base = model.FindEntityType(typeof(ChildBase));
                Assert.Equal(2, @base.GetCheckConstraints().Count());

                var firstCheckConstraint = @base.FindCheckConstraint("PositiveId");
                Assert.Equal("PositiveId", firstCheckConstraint.ModelName);
                Assert.Equal("Id > 0", firstCheckConstraint.Sql);
                Assert.Equal("CK_ChildBase_PositiveId", firstCheckConstraint.Name);

                var secondCheckConstraint = @base.FindCheckConstraint("LargeId");
                Assert.Equal("LargeId", secondCheckConstraint.ModelName);
                Assert.Equal("Id > 1000", secondCheckConstraint.Sql);
                Assert.Equal("CK_LargeId", secondCheckConstraint.Name);

                var child = model.FindEntityType(typeof(Child));
                Assert.Equal(@base.GetCheckConstraints(), child.GetCheckConstraints());
                Assert.Empty(child.GetDeclaredCheckConstraints());
            }

            [ConditionalFact]
            public void Adding_conflicting_check_constraint_to_derived_type_throws()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<ChildBase>()
                    .HasCheckConstraint("LargeId", "Id > 100", c => c.HasName("CK_LargeId"));

                Assert.Equal(
                    RelationalStrings.DuplicateCheckConstraint("LargeId", nameof(Child), nameof(ChildBase)),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<Child>().HasCheckConstraint("LargeId", "Id > 1000")).Message);
            }

            [ConditionalFact]
            public void Adding_conflicting_check_constraint_to_derived_type_before_base_throws()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Child>()
                    .HasBaseType(null)
                    .HasCheckConstraint("LargeId", "Id > 1000");
                modelBuilder.Entity<ChildBase>()
                    .HasCheckConstraint("LargeId", "Id > 100", c => c.HasName("CK_LargeId"));

                Assert.Equal(
                    RelationalStrings.DuplicateCheckConstraint("LargeId", nameof(Child), nameof(ChildBase)),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<Child>().HasBaseType<ChildBase>()).Message);
            }

            public class Parent
            {
                public int Id { get; set; }
                public DisjointChildSubclass1 A { get; set; }
                public IList<DisjointChildSubclass2> B { get; set; }
            }

            public abstract class ChildBase
            {
                public int Id { get; set; }
            }

            public abstract class Child : ChildBase
            {
                public string Name { get; set; }
            }

            public class DisjointChildSubclass1 : Child
            {
            }

            public class DisjointChildSubclass2 : Child
            {
            }

            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance, configure);
        }

        public class SqlServerGenericOneToMany : GenericOneToMany
        {
            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance, configure);
        }

        public class SqlServerGenericManyToOne : GenericManyToOne
        {
            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance, configure);
        }

        public class SqlServerGenericOneToOne : GenericOneToOne
        {
            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance, configure);
        }

        public class SqlServerGenericManyToMany : GenericManyToMany
        {
            [ConditionalFact]
            public virtual void Join_entity_type_uses_same_schema()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Category>().ToTable("Category", "mySchema").Ignore(c => c.ProductCategories);
                modelBuilder.Entity<Product>().ToTable("Product", "mySchema");
                modelBuilder.Entity<CategoryBase>();

                var model = modelBuilder.FinalizeModel();

                var productType = model.FindEntityType(typeof(Product));
                var categoryType = model.FindEntityType(typeof(Category));

                var categoriesNavigation = productType.GetSkipNavigations().Single();
                var productsNavigation = categoryType.GetSkipNavigations().Single();

                var categoriesFk = categoriesNavigation.ForeignKey;
                var productsFk = productsNavigation.ForeignKey;
                var productCategoryType = categoriesFk.DeclaringEntityType;

                Assert.Equal(typeof(Dictionary<string, object>), productCategoryType.ClrType);
                Assert.Equal("mySchema", productCategoryType.GetSchema());
                Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
                Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
                Assert.Equal(2, productCategoryType.GetForeignKeys().Count());
            }

            [ConditionalFact]
            public virtual void Join_entity_type_uses_default_schema_if_related_are_different()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Category>().ToTable("Category").Ignore(c => c.ProductCategories);
                modelBuilder.Entity<Product>().ToTable("Product", "dbo");
                modelBuilder.Entity<CategoryBase>();

                var model = modelBuilder.FinalizeModel();

                var productType = model.FindEntityType(typeof(Product));
                var categoryType = model.FindEntityType(typeof(Category));

                var categoriesNavigation = productType.GetSkipNavigations().Single();
                var productsNavigation = categoryType.GetSkipNavigations().Single();

                var categoriesFk = categoriesNavigation.ForeignKey;
                var productsFk = productsNavigation.ForeignKey;
                var productCategoryType = categoriesFk.DeclaringEntityType;

                Assert.Equal(typeof(Dictionary<string, object>), productCategoryType.ClrType);
                Assert.Null(productCategoryType.GetSchema());
                Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
                Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
                Assert.Equal(2, productCategoryType.GetForeignKeys().Count());
            }

            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance, configure);
        }

        public class SqlServerGenericOwnedTypes : GenericOwnedTypes
        {
            [ConditionalFact]
            public virtual void Owned_types_use_table_splitting_by_default()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel)
                    .Ignore(l => l.Book)
                    .OwnsOne(l => l.AnotherBookLabel)
                    .Ignore(l => l.Book)
                    .OwnsOne(s => s.SpecialBookLabel)
                    .Ignore(l => l.Book)
                    .Ignore(l => l.BookLabel);

                modelBuilder.Entity<Book>().OwnsOne(b => b.Label)
                    .Ignore(l => l.Book)
                    .OwnsOne(l => l.SpecialBookLabel)
                    .Ignore(l => l.Book)
                    .OwnsOne(a => a.AnotherBookLabel)
                    .Ignore(l => l.Book);

                modelBuilder.Entity<Book>().OwnsOne(b => b.Label)
                    .OwnsOne(l => l.AnotherBookLabel)
                    .Ignore(l => l.Book)
                    .OwnsOne(a => a.SpecialBookLabel)
                    .Ignore(l => l.Book)
                    .Ignore(l => l.BookLabel);

                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel)
                    .OwnsOne(l => l.SpecialBookLabel)
                    .Ignore(l => l.Book)
                    .OwnsOne(s => s.AnotherBookLabel)
                    .Ignore(l => l.Book);

                var book = model.FindEntityType(typeof(Book));
                var bookOwnership1 = book.FindNavigation(nameof(Book.Label)).ForeignKey;
                var bookOwnership2 = book.FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;
                var bookLabel1Ownership1 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel1Ownership2 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel2Ownership1 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel2Ownership2 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;

                Assert.Equal(book.GetTableName(), bookOwnership1.DeclaringEntityType.GetTableName());
                Assert.Equal(book.GetTableName(), bookOwnership2.DeclaringEntityType.GetTableName());
                Assert.Equal(book.GetTableName(), bookLabel1Ownership1.DeclaringEntityType.GetTableName());
                Assert.Equal(book.GetTableName(), bookLabel1Ownership2.DeclaringEntityType.GetTableName());
                Assert.Equal(book.GetTableName(), bookLabel2Ownership1.DeclaringEntityType.GetTableName());
                Assert.Equal(book.GetTableName(), bookLabel2Ownership2.DeclaringEntityType.GetTableName());

                Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
                Assert.Single(bookOwnership1.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookOwnership1.DeclaringEntityType.GetForeignKeys());

                Assert.NotSame(bookLabel1Ownership1.DeclaringEntityType, bookLabel2Ownership1.DeclaringEntityType);
                Assert.NotSame(bookLabel1Ownership2.DeclaringEntityType, bookLabel2Ownership2.DeclaringEntityType);
                Assert.Single(bookLabel1Ownership1.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel1Ownership2.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel2Ownership1.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel2Ownership2.DeclaringEntityType.GetForeignKeys());

                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(SpecialBookLabel)));

                Assert.Null(
                    bookOwnership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id))
                        .GetColumnName(StoreObjectIdentifier.Table("Label", null)));
                Assert.Null(
                    bookLabel2Ownership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id))
                        .GetColumnName(StoreObjectIdentifier.Table("AlternateLabel", null)));

                modelBuilder.Entity<Book>().OwnsOne(b => b.Label).ToTable("Label");
                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel).ToTable("AlternateLabel");

                modelBuilder.FinalizeModel();

                Assert.Equal(
                    nameof(BookLabel.Id),
                    bookOwnership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id))
                        .GetColumnName(StoreObjectIdentifier.Table("Label", null)));
                Assert.Equal(
                    nameof(BookLabel.AnotherBookLabel) + "_" + nameof(BookLabel.Id),
                    bookLabel2Ownership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id))
                        .GetColumnName(StoreObjectIdentifier.Table("AlternateLabel", null)));
            }

            [ConditionalFact]
            public virtual void Owned_types_can_be_mapped_to_different_tables()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Book>(
                    bb =>
                    {
                        bb.ToTable("BT", "BS", t => t.ExcludeFromMigrations());
                        bb.OwnsOne(
                            b => b.AlternateLabel, tb =>
                            {
                                tb.Ignore(l => l.Book);
                                tb.WithOwner()
                                    .HasConstraintName("AlternateLabelFK");
                                tb.ToTable("TT", "TS");
                                tb.IsMemoryOptimized();
                                tb.OwnsOne(
                                    l => l.AnotherBookLabel, ab =>
                                    {
                                        ab.Ignore(l => l.Book);
                                        ab.ToTable("AT1", "AS1", excludedFromMigrations: false);
                                        ab.OwnsOne(s => s.SpecialBookLabel)
                                            .ToTable("ST11", "SS11")
                                            .Ignore(l => l.Book)
                                            .Ignore(l => l.BookLabel);

                                        ab.OwnedEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel))
                                            .AddAnnotation("Foo", "Bar");
                                    });
                                tb.OwnsOne(
                                    l => l.SpecialBookLabel, sb =>
                                    {
                                        sb.Ignore(l => l.Book);
                                        sb.ToTable("ST2", "SS2");
                                        sb.OwnsOne(s => s.AnotherBookLabel)
                                            .ToTable("AT21", "AS21")
                                            .Ignore(l => l.Book);
                                    });
                            });
                        bb.OwnsOne(
                            b => b.Label, lb =>
                            {
                                lb.Ignore(l => l.Book);
                                lb.ToTable("LT", "LS");
                                lb.OwnsOne(
                                    l => l.SpecialBookLabel, sb =>
                                    {
                                        sb.Ignore(l => l.Book);
                                        sb.ToTable("ST1", "SS1");
                                        sb.OwnsOne(a => a.AnotherBookLabel)
                                            .ToTable("AT11", "AS11")
                                            .Ignore(l => l.Book);
                                    });
                                lb.OwnsOne(
                                    l => l.AnotherBookLabel, ab =>
                                    {
                                        ab.Ignore(l => l.Book);
                                        ab.ToTable("AT2", "AS2");
                                        ab.OwnsOne(a => a.SpecialBookLabel)
                                            .ToTable("ST21", "SS21")
                                            .Ignore(l => l.BookLabel)
                                            .Ignore(l => l.Book);
                                    });
                            });
                    });

                modelBuilder.FinalizeModel();

                var book = model.FindEntityType(typeof(Book));
                var bookOwnership1 = book.FindNavigation(nameof(Book.Label)).ForeignKey;
                var bookOwnership2 = book.FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;
                var bookLabel1Ownership1 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel1Ownership2 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel2Ownership1 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel2Ownership2 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel1Ownership11 = bookLabel1Ownership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel))
                    .ForeignKey;
                var bookLabel1Ownership21 = bookLabel1Ownership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel))
                    .ForeignKey;
                var bookLabel2Ownership11 = bookLabel2Ownership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel))
                    .ForeignKey;
                var bookLabel2Ownership21 = bookLabel2Ownership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel))
                    .ForeignKey;

                Assert.Equal("AlternateLabelFK", bookOwnership2.GetConstraintName());

                Assert.Equal("BS", book.GetSchema());
                Assert.Equal("BT", book.GetTableName());
                Assert.True(book.IsTableExcludedFromMigrations());
                Assert.Equal("LS", bookOwnership1.DeclaringEntityType.GetSchema());
                Assert.Equal("LT", bookOwnership1.DeclaringEntityType.GetTableName());
                Assert.False(bookOwnership1.DeclaringEntityType.IsMemoryOptimized());
                Assert.True(bookOwnership1.DeclaringEntityType.IsTableExcludedFromMigrations());
                Assert.Equal("TS", bookOwnership2.DeclaringEntityType.GetSchema());
                Assert.Equal("TT", bookOwnership2.DeclaringEntityType.GetTableName());
                Assert.True(bookOwnership2.DeclaringEntityType.IsMemoryOptimized());
                Assert.True(bookOwnership2.DeclaringEntityType.IsTableExcludedFromMigrations());
                Assert.Equal("AS2", bookLabel1Ownership1.DeclaringEntityType.GetSchema());
                Assert.Equal("AT2", bookLabel1Ownership1.DeclaringEntityType.GetTableName());
                Assert.Equal("SS1", bookLabel1Ownership2.DeclaringEntityType.GetSchema());
                Assert.Equal("ST1", bookLabel1Ownership2.DeclaringEntityType.GetTableName());
                Assert.Equal("AS1", bookLabel2Ownership1.DeclaringEntityType.GetSchema());
                Assert.Equal("AT1", bookLabel2Ownership1.DeclaringEntityType.GetTableName());
                Assert.False(bookLabel2Ownership1.DeclaringEntityType.IsTableExcludedFromMigrations());
                Assert.Equal("SS2", bookLabel2Ownership2.DeclaringEntityType.GetSchema());
                Assert.Equal("ST2", bookLabel2Ownership2.DeclaringEntityType.GetTableName());
                Assert.Equal("SS21", bookLabel1Ownership11.DeclaringEntityType.GetSchema());
                Assert.Equal("ST21", bookLabel1Ownership11.DeclaringEntityType.GetTableName());
                Assert.Equal("AS11", bookLabel1Ownership21.DeclaringEntityType.GetSchema());
                Assert.Equal("AT11", bookLabel1Ownership21.DeclaringEntityType.GetTableName());
                Assert.Equal("SS11", bookLabel2Ownership11.DeclaringEntityType.GetSchema());
                Assert.Equal("ST11", bookLabel2Ownership11.DeclaringEntityType.GetTableName());
                Assert.Equal("AS21", bookLabel2Ownership21.DeclaringEntityType.GetSchema());
                Assert.Equal("AT21", bookLabel2Ownership21.DeclaringEntityType.GetTableName());

                Assert.Equal("Bar", bookLabel2Ownership11.PrincipalToDependent["Foo"]);

                Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
                Assert.Single(bookOwnership1.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookOwnership2.DeclaringEntityType.GetForeignKeys());

                Assert.NotSame(bookLabel1Ownership1.DeclaringEntityType, bookLabel2Ownership1.DeclaringEntityType);
                Assert.NotSame(bookLabel1Ownership2.DeclaringEntityType, bookLabel2Ownership2.DeclaringEntityType);
                Assert.Single(bookLabel1Ownership1.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel1Ownership2.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel2Ownership1.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel2Ownership2.DeclaringEntityType.GetForeignKeys());

                Assert.NotSame(bookLabel1Ownership11.DeclaringEntityType, bookLabel2Ownership11.DeclaringEntityType);
                Assert.NotSame(bookLabel1Ownership21.DeclaringEntityType, bookLabel2Ownership21.DeclaringEntityType);
                Assert.Single(bookLabel1Ownership11.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel1Ownership21.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel2Ownership11.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel2Ownership21.DeclaringEntityType.GetForeignKeys());

                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(SpecialBookLabel)));

                Assert.Equal(ValueGenerated.Never, bookOwnership1.DeclaringEntityType.FindPrimaryKey().Properties.Single().ValueGenerated);
                Assert.Equal(ValueGenerated.Never, bookOwnership2.DeclaringEntityType.FindPrimaryKey().Properties.Single().ValueGenerated);

                Assert.Equal(
                    ValueGenerated.Never, bookLabel1Ownership1.DeclaringEntityType.FindPrimaryKey().Properties.Single().ValueGenerated);
                Assert.Equal(
                    ValueGenerated.Never, bookLabel1Ownership2.DeclaringEntityType.FindPrimaryKey().Properties.Single().ValueGenerated);
                Assert.Equal(
                    ValueGenerated.Never, bookLabel2Ownership1.DeclaringEntityType.FindPrimaryKey().Properties.Single().ValueGenerated);
                Assert.Equal(
                    ValueGenerated.Never, bookLabel2Ownership2.DeclaringEntityType.FindPrimaryKey().Properties.Single().ValueGenerated);

                Assert.Equal(
                    ValueGenerated.Never, bookLabel1Ownership11.DeclaringEntityType.FindPrimaryKey().Properties.Single().ValueGenerated);
                Assert.Equal(
                    ValueGenerated.Never, bookLabel1Ownership21.DeclaringEntityType.FindPrimaryKey().Properties.Single().ValueGenerated);
                Assert.Equal(
                    ValueGenerated.Never, bookLabel2Ownership11.DeclaringEntityType.FindPrimaryKey().Properties.Single().ValueGenerated);
                Assert.Equal(
                    ValueGenerated.Never, bookLabel2Ownership21.DeclaringEntityType.FindPrimaryKey().Properties.Single().ValueGenerated);
            }

            [ConditionalFact]
            public virtual void Owned_type_collections_can_be_mapped_to_different_tables()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().OwnsMany(
                    c => c.Orders,
                    r =>
                    {
                        r.HasKey(o => o.OrderId);
                        r.IsMemoryOptimized();
                        r.Ignore(o => o.OrderCombination);
                        r.Ignore(o => o.Details);
                    });

                var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
                var owned = ownership.DeclaringEntityType;
                Assert.True(ownership.IsOwnership);
                Assert.Equal(nameof(Order.Customer), ownership.DependentToPrincipal.Name);
                Assert.Equal("FK_Order_Customer_CustomerId", ownership.GetConstraintName());

                Assert.Single(owned.GetForeignKeys());
                Assert.Single(owned.GetIndexes());
                Assert.Equal(
                    new[] { nameof(Order.OrderId), nameof(Order.AnotherCustomerId), nameof(Order.CustomerId) },
                    owned.GetProperties().Select(p => p.GetColumnBaseName()));
                Assert.Equal(nameof(Order), owned.GetTableName());
                Assert.Null(owned.GetSchema());
                Assert.True(owned.IsMemoryOptimized());

                modelBuilder.Entity<Customer>().OwnsMany(
                    c => c.Orders,
                    r =>
                    {
                        r.WithOwner(o => o.Customer).HasConstraintName("Owned");
                        r.ToTable("bar", "foo");
                    });

                Assert.Equal("bar", owned.GetTableName());
                Assert.Equal("foo", owned.GetSchema());
                Assert.Equal("Owned", ownership.GetConstraintName());

                modelBuilder.Entity<Customer>().OwnsMany(
                    c => c.Orders,
                    r => r.ToTable("blah"));

                modelBuilder.FinalizeModel();

                Assert.Equal("blah", owned.GetTableName());
                Assert.Null(owned.GetSchema());
            }

            [ConditionalFact]
            public override void Can_configure_owned_type()
            {
                var modelBuilder = CreateModelBuilder();

                var ownedBuilder = modelBuilder.Entity<Customer>().OwnsOne(c => c.Details)
                    .ToTable("CustomerDetails")
                    .HasCheckConstraint("CK_CustomerDetails_T", "AlternateKey <> 0", c => c.HasName("CK_Guid"));
                ownedBuilder.Property(d => d.CustomerId);
                ownedBuilder.HasIndex(d => d.CustomerId);
                ownedBuilder.WithOwner(d => d.Customer)
                    .HasPrincipalKey(c => c.AlternateKey);

                var model = modelBuilder.FinalizeModel();

                var owner = model.FindEntityType(typeof(Customer));
                Assert.Equal(typeof(Customer).FullName, owner.Name);
                var ownership = owner.FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.True(ownership.IsOwnership);
                Assert.Equal(nameof(Customer.Details), ownership.PrincipalToDependent.Name);
                Assert.Equal("CustomerAlternateKey", ownership.Properties.Single().Name);
                Assert.Equal(nameof(Customer.AlternateKey), ownership.PrincipalKey.Properties.Single().Name);
                var owned = ownership.DeclaringEntityType;
                Assert.Same(ownedBuilder.OwnedEntityType, owned);
                Assert.Equal("CustomerDetails", owned.GetTableName());
                var checkConstraint = owned.GetCheckConstraints().Single();
                Assert.Equal("CK_CustomerDetails_T", checkConstraint.ModelName);
                Assert.Equal("AlternateKey <> 0", checkConstraint.Sql);
                Assert.Equal("CK_Guid", checkConstraint.Name);
                Assert.Single(owned.GetForeignKeys());
                Assert.Equal(nameof(CustomerDetails.CustomerId), owned.GetIndexes().Single().Properties.Single().Name);
                Assert.Equal(
                    new[] { "CustomerAlternateKey", nameof(CustomerDetails.CustomerId), nameof(CustomerDetails.Id) },
                    owned.GetProperties().Select(p => p.Name));
                Assert.NotNull(model.FindEntityType(typeof(CustomerDetails)));
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
            }

            [ConditionalFact]
            public override void Can_configure_owned_type_key()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details)
                    .ToTable("Details")
                    .HasKey(c => c.Id);

                modelBuilder.FinalizeModel();

                var owner = model.FindEntityType(typeof(Customer));
                var owned = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
                Assert.Equal(
                    new[] { nameof(CustomerDetails.Id), nameof(CustomerDetails.CustomerId) },
                    owned.GetProperties().Select(p => p.Name).ToArray());
                Assert.Equal(nameof(CustomerDetails.Id), owned.FindPrimaryKey().Properties.Single().Name);
            }


            [ConditionalFact]
            public virtual void Temporal_table_default_settings()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().ToTable(tb => tb.IsTemporal());
                modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));
                Assert.True(entity.IsTemporal());
                Assert.Equal("CustomerHistory", entity.GetTemporalHistoryTableName());
                Assert.Null(entity.GetTemporalHistoryTableSchema());

                var periodStart = entity.GetProperty(entity.GetTemporalPeriodStartPropertyName());
                var periodEnd = entity.GetProperty(entity.GetTemporalPeriodEndPropertyName());

                Assert.Equal("PeriodStart", periodStart.Name);
                Assert.True(periodStart.IsShadowProperty());
                Assert.Equal(typeof(DateTime), periodStart.ClrType);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, periodStart.ValueGenerated);

                Assert.Equal("PeriodEnd", periodEnd.Name);
                Assert.True(periodEnd.IsShadowProperty());
                Assert.Equal(typeof(DateTime), periodEnd.ClrType);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, periodEnd.ValueGenerated);
            }

            [ConditionalFact]
            public virtual void Temporal_table_with_history_table_configuration()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().ToTable(tb => tb.IsTemporal(ttb =>
                {
                    ttb.WithHistoryTable("HistoryTable", "historySchema");
                    ttb.HasPeriodStart("MyPeriodStart").HasColumnName("PeriodStartColumn");
                    ttb.HasPeriodEnd("MyPeriodEnd").HasColumnName("PeriodEndColumn");
                }));

                modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));
                Assert.True(entity.IsTemporal());
                Assert.Equal(5, entity.GetProperties().Count());

                Assert.Equal("HistoryTable", entity.GetTemporalHistoryTableName());
                Assert.Equal("historySchema", entity.GetTemporalHistoryTableSchema());

                var periodStart = entity.GetProperty(entity.GetTemporalPeriodStartPropertyName());
                var periodEnd = entity.GetProperty(entity.GetTemporalPeriodEndPropertyName());

                Assert.Equal("MyPeriodStart", periodStart.Name);
                Assert.Equal("PeriodStartColumn", periodStart[RelationalAnnotationNames.ColumnName]);
                Assert.True(periodStart.IsShadowProperty());
                Assert.Equal(typeof(DateTime), periodStart.ClrType);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, periodStart.ValueGenerated);

                Assert.Equal("MyPeriodEnd", periodEnd.Name);
                Assert.Equal("PeriodEndColumn", periodEnd[RelationalAnnotationNames.ColumnName]);
                Assert.True(periodEnd.IsShadowProperty());
                Assert.Equal(typeof(DateTime), periodEnd.ClrType);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, periodEnd.ValueGenerated);
            }

            [ConditionalFact]
            public virtual void Temporal_table_with_changed_configuration()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().ToTable(tb => tb.IsTemporal(ttb =>
                {
                    ttb.WithHistoryTable("HistoryTable", "historySchema");
                    ttb.HasPeriodStart("MyPeriodStart").HasColumnName("PeriodStartColumn");
                    ttb.HasPeriodEnd("MyPeriodEnd").HasColumnName("PeriodEndColumn");
                }));

                modelBuilder.Entity<Customer>().ToTable(tb => tb.IsTemporal(ttb =>
                {
                    ttb.WithHistoryTable("ChangedHistoryTable", "changedHistorySchema");
                    ttb.HasPeriodStart("ChangedMyPeriodStart").HasColumnName("ChangedPeriodStartColumn");
                    ttb.HasPeriodEnd("ChangedMyPeriodEnd").HasColumnName("ChangedPeriodEndColumn");
                }));

                modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));
                Assert.True(entity.IsTemporal());
                Assert.Equal(5, entity.GetProperties().Count());

                Assert.Equal("ChangedHistoryTable", entity.GetTemporalHistoryTableName());
                Assert.Equal("changedHistorySchema", entity.GetTemporalHistoryTableSchema());

                var periodStart = entity.GetProperty(entity.GetTemporalPeriodStartPropertyName());
                var periodEnd = entity.GetProperty(entity.GetTemporalPeriodEndPropertyName());

                Assert.Equal("ChangedMyPeriodStart", periodStart.Name);
                Assert.Equal("ChangedPeriodStartColumn", periodStart[RelationalAnnotationNames.ColumnName]);
                Assert.True(periodStart.IsShadowProperty());
                Assert.Equal(typeof(DateTime), periodStart.ClrType);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, periodStart.ValueGenerated);

                Assert.Equal("ChangedMyPeriodEnd", periodEnd.Name);
                Assert.Equal("ChangedPeriodEndColumn", periodEnd[RelationalAnnotationNames.ColumnName]);
                Assert.True(periodEnd.IsShadowProperty());
                Assert.Equal(typeof(DateTime), periodEnd.ClrType);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, periodEnd.ValueGenerated);
            }

            [ConditionalFact]
            public virtual void Temporal_table_with_explicit_properties_mapped_to_the_period_columns()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().ToTable(tb => tb.IsTemporal(ttb =>
                {
                    ttb.WithHistoryTable("HistoryTable", schema: null);
                    ttb.HasPeriodStart("Start").HasColumnName("PeriodStartColumn");
                    ttb.HasPeriodEnd("End").HasColumnName("PeriodEndColumn");
                }));

                modelBuilder.Entity<Customer>()
                    .Property<DateTime>("MappedStart")
                    .HasColumnName("PeriodStartColumn")
                    .ValueGeneratedOnAddOrUpdate();

                modelBuilder.Entity<Customer>()
                    .Property<DateTime>("MappedEnd")
                    .HasColumnName("PeriodEndColumn")
                    .ValueGeneratedOnAddOrUpdate();

                modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));
                Assert.True(entity.IsTemporal());
                Assert.Equal(7, entity.GetProperties().Count());

                Assert.Equal("HistoryTable", entity.GetTemporalHistoryTableName());

                var periodStart = entity.GetProperty(entity.GetTemporalPeriodStartPropertyName());
                var periodEnd = entity.GetProperty(entity.GetTemporalPeriodEndPropertyName());

                Assert.Equal("Start", periodStart.Name);
                Assert.Equal("PeriodStartColumn", periodStart[RelationalAnnotationNames.ColumnName]);
                Assert.True(periodStart.IsShadowProperty());
                Assert.Equal(typeof(DateTime), periodStart.ClrType);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, periodStart.ValueGenerated);

                Assert.Equal("End", periodEnd.Name);
                Assert.Equal("PeriodEndColumn", periodEnd[RelationalAnnotationNames.ColumnName]);
                Assert.True(periodEnd.IsShadowProperty());
                Assert.Equal(typeof(DateTime), periodEnd.ClrType);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, periodEnd.ValueGenerated);

                var propertyMappedToStart = entity.GetProperty("MappedStart");
                Assert.Equal("PeriodStartColumn", propertyMappedToStart[RelationalAnnotationNames.ColumnName]);

                var propertyMappedToEnd = entity.GetProperty("MappedEnd");
                Assert.Equal("PeriodEndColumn", propertyMappedToEnd[RelationalAnnotationNames.ColumnName]);
            }

            [ConditionalFact]
            public virtual void Temporal_table_with_explicit_properties_with_same_name_as_default_periods_but_different_periods_defined_explicity_as_well()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>()
                    .Property<DateTime>("PeriodStart")
                    .HasColumnName("PeriodStartColumn");

                modelBuilder.Entity<Customer>()
                    .Property<DateTime>("PeriodEnd")
                    .HasColumnName("PeriodEndColumn");

                modelBuilder.Entity<Customer>().ToTable(tb => tb.IsTemporal(ttb =>
                {
                    ttb.WithHistoryTable("HistoryTable", schema: null);
                    ttb.HasPeriodStart("Start");
                    ttb.HasPeriodEnd("End");
                }));

                modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));
                Assert.True(entity.IsTemporal());
                Assert.Equal(7, entity.GetProperties().Count());

                Assert.Equal("HistoryTable", entity.GetTemporalHistoryTableName());

                var periodStart = entity.GetProperty(entity.GetTemporalPeriodStartPropertyName());
                var periodEnd = entity.GetProperty(entity.GetTemporalPeriodEndPropertyName());

                Assert.Equal("Start", periodStart.Name);
                Assert.Equal("Start", periodStart[RelationalAnnotationNames.ColumnName]);
                Assert.True(periodStart.IsShadowProperty());
                Assert.Equal(typeof(DateTime), periodStart.ClrType);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, periodStart.ValueGenerated);

                Assert.Equal("End", periodEnd.Name);
                Assert.Equal("End", periodEnd[RelationalAnnotationNames.ColumnName]);
                Assert.True(periodEnd.IsShadowProperty());
                Assert.Equal(typeof(DateTime), periodEnd.ClrType);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, periodEnd.ValueGenerated);

                var propertyMappedToStart = entity.GetProperty("PeriodStart");
                Assert.Equal("PeriodStartColumn", propertyMappedToStart[RelationalAnnotationNames.ColumnName]);
                Assert.Equal(ValueGenerated.Never, propertyMappedToStart.ValueGenerated);

                var propertyMappedToEnd = entity.GetProperty("PeriodEnd");
                Assert.Equal("PeriodEndColumn", propertyMappedToEnd[RelationalAnnotationNames.ColumnName]);
                Assert.Equal(ValueGenerated.Never, propertyMappedToEnd.ValueGenerated);
            }

            [ConditionalFact]
            public virtual void Switching_from_temporal_to_non_temporal_default_settings()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().ToTable(tb => tb.IsTemporal());
                modelBuilder.Entity<Customer>().ToTable(tb => tb.IsTemporal(false));

                modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));
                Assert.False(entity.IsTemporal());
                Assert.Null(entity.GetTemporalPeriodStartPropertyName());
                Assert.Null(entity.GetTemporalPeriodEndPropertyName());
                Assert.Equal(3, entity.GetProperties().Count());
            }

            [ConditionalFact]
            public virtual void Implicit_many_to_many_converted_from_non_temporal_to_temporal()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<ImplicitManyToManyA>();
                modelBuilder.Entity<ImplicitManyToManyB>();

                modelBuilder.Entity<ImplicitManyToManyA>().ToTable(tb => tb.IsTemporal());
                modelBuilder.Entity<ImplicitManyToManyB>().ToTable(tb => tb.IsTemporal());

                modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(ImplicitManyToManyA));
                var joinEntity = entity.GetSkipNavigations().Single().JoinEntityType;

                Assert.True(joinEntity.IsTemporal());
            }

            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance, configure);
        }

        public abstract class TestTemporalTableBuilder<TEntity>
            where TEntity : class
        {
            public abstract TestTemporalTableBuilder<TEntity> WithHistoryTable(string name, string schema);

            public abstract TestTemporalPeriodPropertyBuilder HasPeriodStart(string propertyName);
            public abstract TestTemporalPeriodPropertyBuilder HasPeriodEnd(string propertyName);
        }

        public class GenericTestTemporalTableBuilder<TEntity> : TestTemporalTableBuilder<TEntity>, IInfrastructure<TemporalTableBuilder<TEntity>>
            where TEntity : class
        {
            public GenericTestTemporalTableBuilder(TemporalTableBuilder<TEntity> temporalTableBuilder)
            {
                TemporalTableBuilder = temporalTableBuilder;
            }

            protected TemporalTableBuilder<TEntity> TemporalTableBuilder { get; }

            public TemporalTableBuilder<TEntity> Instance => TemporalTableBuilder;

            protected virtual TestTemporalTableBuilder<TEntity> Wrap(TemporalTableBuilder<TEntity> tableBuilder)
                => new GenericTestTemporalTableBuilder<TEntity>(tableBuilder);

            public override TestTemporalTableBuilder<TEntity> WithHistoryTable(string name, string schema)
                => Wrap(TemporalTableBuilder.WithHistoryTable(name, schema));

            public override TestTemporalPeriodPropertyBuilder HasPeriodStart(string propertyName)
                => new TestTemporalPeriodPropertyBuilder(TemporalTableBuilder.HasPeriodStart(propertyName));

            public override TestTemporalPeriodPropertyBuilder HasPeriodEnd(string propertyName)
                => new TestTemporalPeriodPropertyBuilder(TemporalTableBuilder.HasPeriodEnd(propertyName));
        }

        public class NonGenericTestTemporalTableBuilder<TEntity> : TestTemporalTableBuilder<TEntity>, IInfrastructure<TemporalTableBuilder>
            where TEntity : class
        {
            public NonGenericTestTemporalTableBuilder(TemporalTableBuilder temporalTableBuilder)
            {
                TemporalTableBuilder = temporalTableBuilder;
            }

            protected TemporalTableBuilder TemporalTableBuilder { get; }

            public TemporalTableBuilder Instance => TemporalTableBuilder;

            protected virtual TestTemporalTableBuilder<TEntity> Wrap(TemporalTableBuilder temporalTableBuilder)
                => new NonGenericTestTemporalTableBuilder<TEntity>(temporalTableBuilder);

            public override TestTemporalTableBuilder<TEntity> WithHistoryTable(string name, string schema)
                => Wrap(TemporalTableBuilder.WithHistoryTable(name, schema));

            public override TestTemporalPeriodPropertyBuilder HasPeriodStart(string propertyName)
                => new TestTemporalPeriodPropertyBuilder(TemporalTableBuilder.HasPeriodStart(propertyName));

            public override TestTemporalPeriodPropertyBuilder HasPeriodEnd(string propertyName)
                => new TestTemporalPeriodPropertyBuilder(TemporalTableBuilder.HasPeriodEnd(propertyName));
        }

        public class TestTemporalPeriodPropertyBuilder
        {
            public TestTemporalPeriodPropertyBuilder(TemporalPeriodPropertyBuilder temporalPeriodPropertyBuilder)
            {
                TemporalPeriodPropertyBuilder = temporalPeriodPropertyBuilder;
            }

            protected TemporalPeriodPropertyBuilder TemporalPeriodPropertyBuilder { get; }

            public TestTemporalPeriodPropertyBuilder HasColumnName(string name)
                => new TestTemporalPeriodPropertyBuilder(TemporalPeriodPropertyBuilder.HasColumnName(name));
        }
    }
}
