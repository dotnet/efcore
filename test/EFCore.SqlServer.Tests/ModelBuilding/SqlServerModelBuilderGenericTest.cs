// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
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

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
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

                modelBuilder.FinalizeModel();

                var property1 = modelBuilder.Model.FindEntityType(typeof(DisjointChildSubclass1)).FindProperty("ParentId");
                Assert.True(property1.IsForeignKey());
                Assert.Equal("ParentId", property1.GetColumnBaseName());
                var property2 = modelBuilder.Model.FindEntityType(typeof(DisjointChildSubclass2)).FindProperty("ParentId");
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

                modelBuilder.FinalizeModel();

                var property1 = modelBuilder.Model.FindEntityType(typeof(DisjointChildSubclass1)).FindProperty(nameof(Child.Name));
                Assert.Equal(nameof(Child.Name), property1.GetColumnBaseName());
                var property2 = modelBuilder.Model.FindEntityType(typeof(DisjointChildSubclass2)).FindProperty(nameof(Child.Name));
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

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
        }

        public class SqlServerGenericOneToMany : GenericOneToMany
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
        }

        public class SqlServerGenericManyToOne : GenericManyToOne
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
        }

        public class SqlServerGenericOneToOne : GenericOneToOne
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
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

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
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
                        bb.ToTable("BT", "BS");
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
                                        ab.ToTable("AT1", "AS1");
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
                Assert.Equal("LS", bookOwnership1.DeclaringEntityType.GetSchema());
                Assert.Equal("LT", bookOwnership1.DeclaringEntityType.GetTableName());
                Assert.False(bookOwnership1.DeclaringEntityType.IsMemoryOptimized());
                Assert.Equal("TS", bookOwnership2.DeclaringEntityType.GetSchema());
                Assert.Equal("TT", bookOwnership2.DeclaringEntityType.GetTableName());
                Assert.True(bookOwnership2.DeclaringEntityType.IsMemoryOptimized());
                Assert.Equal("AS2", bookLabel1Ownership1.DeclaringEntityType.GetSchema());
                Assert.Equal("AT2", bookLabel1Ownership1.DeclaringEntityType.GetTableName());
                Assert.Equal("SS1", bookLabel1Ownership2.DeclaringEntityType.GetSchema());
                Assert.Equal("ST1", bookLabel1Ownership2.DeclaringEntityType.GetTableName());
                Assert.Equal("AS1", bookLabel2Ownership1.DeclaringEntityType.GetSchema());
                Assert.Equal("AT1", bookLabel2Ownership1.DeclaringEntityType.GetTableName());
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
                var model = modelBuilder.Model;

                var entityBuilder = modelBuilder.Entity<Customer>().OwnsOne(c => c.Details)
                    .ToTable("CustomerDetails");
                entityBuilder.Property(d => d.CustomerId);
                entityBuilder.HasIndex(d => d.CustomerId);
                entityBuilder.WithOwner(d => d.Customer)
                    .HasPrincipalKey(c => c.AlternateKey);

                modelBuilder.FinalizeModel();

                var owner = model.FindEntityType(typeof(Customer));
                Assert.Equal(typeof(Customer).FullName, owner.Name);
                var ownership = owner.FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.True(ownership.IsOwnership);
                Assert.Equal(nameof(Customer.Details), ownership.PrincipalToDependent.Name);
                Assert.Equal("CustomerAlternateKey", ownership.Properties.Single().Name);
                Assert.Equal(nameof(Customer.AlternateKey), ownership.PrincipalKey.Properties.Single().Name);
                var owned = ownership.DeclaringEntityType;
                Assert.Same(entityBuilder.OwnedEntityType, owned);
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

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
        }

        public class SqlServerGenericKeylessEntities : GenericKeylessEntities
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
        }
    }
}
