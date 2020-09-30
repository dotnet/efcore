// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class ManyToManyTestBase : ModelBuilderTestBase
        {
            [ConditionalFact]
            public virtual void Discovers_navigations()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Category>().Ignore(c => c.ProductCategories);
                modelBuilder.Entity<Product>();
                modelBuilder.Entity<CategoryBase>();

                var sharedTypeName = nameof(Category) + nameof(Product);

                modelBuilder.SharedTypeEntity<Dictionary<string, object>>(sharedTypeName);

                var model = modelBuilder.FinalizeModel();

                var productType = model.FindEntityType(typeof(Product));
                var categoryType = model.FindEntityType(typeof(Category));

                var categoriesNavigation = productType.GetSkipNavigations().Single();
                var productsNavigation = categoryType.GetSkipNavigations().Single();

                var categoriesFk = categoriesNavigation.ForeignKey;
                var productsFk = productsNavigation.ForeignKey;
                var productCategoryType = categoriesFk.DeclaringEntityType;

                Assert.Equal(typeof(Dictionary<string, object>), productCategoryType.ClrType);
                Assert.Equal(sharedTypeName, productCategoryType.Name);
                Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
                Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
                Assert.Equal(2, productCategoryType.GetForeignKeys().Count());
            }

            [ConditionalFact]
            public virtual void Finds_existing_navigations_and_uses_associated_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = (IModel)modelBuilder.Model;

                modelBuilder.Entity<Category>().Ignore(c => c.Products);
                modelBuilder.Entity<Product>().Ignore(p => p.Categories);

                modelBuilder.Entity<Category>()
                    .HasMany(o => o.Products).WithMany(c => c.Categories)
                    .UsingEntity<ProductCategory>(
                        pcb => pcb.HasOne(pc => pc.Product).WithMany(),
                        pcb => pcb.HasOne(pc => pc.Category).WithMany(c => c.ProductCategories))
                    .HasKey(pc => new { pc.ProductId, pc.CategoryId });

                var productType = model.FindEntityType(typeof(Product));
                var categoryType = model.FindEntityType(typeof(Category));
                var productCategoryType = model.FindEntityType(typeof(ProductCategory));

                var categoriesNavigation = productType.GetSkipNavigations().Single();
                var productsNavigation = categoryType.GetSkipNavigations().Single();

                var categoriesFk = categoriesNavigation.ForeignKey;
                var productsFk = productsNavigation.ForeignKey;

                Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
                Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
                Assert.Equal(2, productCategoryType.GetForeignKeys().Count());

                modelBuilder.Entity<Category>()
                    .HasMany(o => o.Products).WithMany(c => c.Categories)
                    .UsingEntity<ProductCategory>(
                        pcb => pcb.HasOne(pc => pc.Product).WithMany(),
                        pcb => pcb.HasOne(pc => pc.Category).WithMany(c => c.ProductCategories));

                model = modelBuilder.FinalizeModel();

                Assert.Same(categoriesNavigation, productType.GetSkipNavigations().Single());
                Assert.Same(productsNavigation, categoryType.GetSkipNavigations().Single());
                Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
                Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
                Assert.Equal(2, productCategoryType.GetForeignKeys().Count());
            }

            [ConditionalFact]
            public virtual void Finds_existing_navigations_and_uses_associated_FK_with_fields()
            {
                var modelBuilder = CreateModelBuilder();
                var model = (IModel)modelBuilder.Model;

                modelBuilder.Entity<ManyToManyPrincipalWithField>()
                    .HasMany(p => p.Dependents)
                    .WithMany(d => d.ManyToManyPrincipals)
                    .UsingEntity<ManyToManyJoinWithFields>(
                        jwf => jwf.HasOne(j => j.DependentWithField)
                            .WithMany(),
                        jwf => jwf.HasOne(j => j.ManyToManyPrincipalWithField)
                            .WithMany())
                    .HasKey(j => new { j.DependentWithFieldId, j.ManyToManyPrincipalWithFieldId });

                modelBuilder.Entity<ManyToManyPrincipalWithField>(
                    e =>
                    {
                        e.Property(p => p.Id);
                        e.Property(p => p.Name);
                        e.HasKey(p => p.Id);
                    });
                modelBuilder.Entity<DependentWithField>(
                    e =>
                    {
                        e.Property(d => d.DependentWithFieldId);
                        e.Property(d => d.AnotherOneToManyPrincipalId);
                        e.Ignore(d => d.OneToManyPrincipal);
                        e.Ignore(d => d.OneToOnePrincipal);
                        e.HasKey(d => d.DependentWithFieldId);
                    });

                var principalEntityType = model.FindEntityType(typeof(ManyToManyPrincipalWithField));
                var dependentEntityType = model.FindEntityType(typeof(DependentWithField));
                var joinEntityType = model.FindEntityType(typeof(ManyToManyJoinWithFields));

                var principalToJoinNav = principalEntityType.GetSkipNavigations().Single();
                var dependentToJoinNav = dependentEntityType.GetSkipNavigations().Single();

                var principalToDependentFk = principalToJoinNav.ForeignKey;
                var dependentToPrincipalFk = dependentToJoinNav.ForeignKey;

                Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
                Assert.Same(principalToDependentFk, joinEntityType.GetForeignKeys().Last());
                Assert.Same(dependentToPrincipalFk, joinEntityType.GetForeignKeys().First());

                modelBuilder.Entity<ManyToManyPrincipalWithField>()
                    .HasMany(p => p.Dependents)
                    .WithMany(d => d.ManyToManyPrincipals)
                    .UsingEntity<ManyToManyJoinWithFields>(
                        jwf => jwf.HasOne(j => j.DependentWithField)
                            .WithMany(),
                        jwf => jwf.HasOne(j => j.ManyToManyPrincipalWithField)
                            .WithMany());

                model = modelBuilder.FinalizeModel();

                Assert.Same(principalToJoinNav, principalEntityType.GetSkipNavigations().Single());
                Assert.Same(dependentToJoinNav, dependentEntityType.GetSkipNavigations().Single());
                Assert.Single(principalEntityType.GetDeclaredKeys());
                Assert.Single(dependentEntityType.GetDeclaredKeys());
                Assert.Single(joinEntityType.GetDeclaredKeys());
                Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
                Assert.Same(principalToDependentFk, joinEntityType.GetForeignKeys().Last());
                Assert.Same(dependentToPrincipalFk, joinEntityType.GetForeignKeys().First());
            }

            [ConditionalFact]
            public virtual void Join_type_is_automatically_configured_by_convention()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<ImplicitManyToManyA>();

                var model = modelBuilder.FinalizeModel();

                var manyToManyA = model.FindEntityType(typeof(ImplicitManyToManyA));
                var manyToManyB = model.FindEntityType(typeof(ImplicitManyToManyB));
                var joinEntityType = model.GetEntityTypes()
                    .Where(et => ((EntityType)et).IsImplicitlyCreatedJoinEntityType)
                    .Single();
                Assert.Equal("ImplicitManyToManyAImplicitManyToManyB", joinEntityType.Name);

                var navigationOnManyToManyA = manyToManyA.GetSkipNavigations().Single();
                var navigationOnManyToManyB = manyToManyB.GetSkipNavigations().Single();
                Assert.Equal("Bs", navigationOnManyToManyA.Name);
                Assert.Equal("As", navigationOnManyToManyB.Name);
                Assert.Same(navigationOnManyToManyA.Inverse, navigationOnManyToManyB);
                Assert.Same(navigationOnManyToManyB.Inverse, navigationOnManyToManyA);

                var manyToManyAForeignKey = navigationOnManyToManyA.ForeignKey;
                var manyToManyBForeignKey = navigationOnManyToManyB.ForeignKey;
                Assert.NotNull(manyToManyAForeignKey);
                Assert.NotNull(manyToManyBForeignKey);
                Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
                Assert.Equal(manyToManyAForeignKey.DeclaringEntityType, joinEntityType);
                Assert.Equal(manyToManyBForeignKey.DeclaringEntityType, joinEntityType);

                var key = joinEntityType.FindPrimaryKey();
                Assert.Equal(
                    new[]
                    {
                        nameof(ImplicitManyToManyB.As) + nameof(ImplicitManyToManyA.Id),
                        nameof(ImplicitManyToManyA.Bs) + nameof(ImplicitManyToManyB.Id)
                    },
                    key.Properties.Select(p => p.Name));

                Assert.DoesNotContain(joinEntityType.GetProperties(), p => !p.IsIndexerProperty());
            }

            [ConditionalFact]
            public virtual void Join_type_is_not_automatically_configured_when_navigations_are_ambiguous()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Hob>();

                var hob = model.FindEntityType(typeof(Hob));
                var nob = model.FindEntityType(typeof(Nob));
                Assert.Empty(
                    model.GetEntityTypes()
                        .Where(et => ((EntityType)et).IsImplicitlyCreatedJoinEntityType));

                Assert.Empty(hob.GetSkipNavigations());
                Assert.Empty(nob.GetSkipNavigations());
            }

            [ConditionalFact]
            public virtual void Can_configure_join_type_using_fluent_api()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Category>().Ignore(c => c.Products);
                modelBuilder.Entity<Product>().Ignore(p => p.Categories);

                var manyToMany = modelBuilder.Entity<Category>()
                    .HasMany(o => o.Products).WithMany(c => c.Categories)
                    .UsingEntity<ProductCategory>(
                        pcb => pcb.HasOne(pc => pc.Product).WithMany(),
                        pcb => pcb.HasOne(pc => pc.Category).WithMany(c => c.ProductCategories),
                        pcb => pcb.HasKey(pc => new { pc.ProductId, pc.CategoryId }));

                var model = modelBuilder.FinalizeModel();

                Assert.Equal(typeof(Category), manyToMany.Metadata.ClrType);

                var productType = model.FindEntityType(typeof(Product));
                var categoryType = model.FindEntityType(typeof(Category));
                var productCategoryType = model.FindEntityType(typeof(ProductCategory));

                var categoriesNavigation = productType.GetSkipNavigations().Single();
                var productsNavigation = categoryType.GetSkipNavigations().Single();

                var categoriesFk = categoriesNavigation.ForeignKey;
                var productsFk = productsNavigation.ForeignKey;

                Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
                Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
                Assert.Equal(2, productCategoryType.GetForeignKeys().Count());

                var key = productCategoryType.FindPrimaryKey();
                Assert.Equal(
                    new[] { nameof(ProductCategory.ProductId), nameof(ProductCategory.CategoryId) },
                    key.Properties.Select(p => p.Name));
            }

            [ConditionalFact]
            public virtual void Can_ignore_existing_navigations()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Category>()
                    .HasMany(p => p.Products).WithMany(c => c.Categories);

                modelBuilder.Entity<Category>().Ignore(c => c.Products);
                modelBuilder.Entity<Product>().Ignore(p => p.Categories);

                // Issue #19550
                modelBuilder.Ignore<ProductCategory>();

                var model = modelBuilder.FinalizeModel();

                var productType = model.FindEntityType(typeof(Product));
                var categoryType = model.FindEntityType(typeof(Category));

                Assert.Empty(productType.GetSkipNavigations());
                Assert.Empty(categoryType.GetSkipNavigations());
            }

            [ConditionalFact]
            public virtual void Throws_for_conflicting_many_to_one_on_left()
            {
                var modelBuilder = CreateModelBuilder();

                // make sure we do not set up the automatic many-to-many relationship
                modelBuilder.Entity<Category>().Ignore(e => e.Products);

                modelBuilder.Entity<Category>()
                    .HasMany(o => o.Products).WithOne();

                Assert.Equal(
                    CoreStrings.ConflictingRelationshipNavigation(
                        nameof(Category) + "." + nameof(Category.Products),
                        nameof(Product) + "." + nameof(Product.Categories),
                        nameof(Category) + "." + nameof(Category.Products),
                        nameof(Product)),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<Category>()
                            .HasMany(o => o.Products).WithMany(c => c.Categories)).Message);
            }

            [ConditionalFact]
            public virtual void Throws_for_conflicting_many_to_one_on_right()
            {
                var modelBuilder = CreateModelBuilder();

                // make sure we do not set up the automatic many-to-many relationship
                modelBuilder.Entity<Category>().Ignore(e => e.Products);

                modelBuilder.Entity<Category>()
                    .HasMany(o => o.Products).WithOne();

                Assert.Equal(
                    CoreStrings.ConflictingRelationshipNavigation(
                        nameof(Product) + "." + nameof(Product.Categories),
                        nameof(Category) + "." + nameof(Category.Products),
                        nameof(Category) + "." + nameof(Category.Products),
                        nameof(Product)),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<Product>()
                            .HasMany(o => o.Categories).WithMany(c => c.Products)).Message);
            }

            [ConditionalFact]
            public virtual void Throws_for_many_to_many_with_only_one_navigation_configured()
            {
                var modelBuilder = CreateModelBuilder();

                Assert.Equal(
                    CoreStrings.MissingInverseManyToManyNavigation(
                        nameof(ManyToManyNavPrincipal),
                        nameof(NavDependent)),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<ManyToManyNavPrincipal>()
                            .HasMany<NavDependent>( /* leaving empty causes the exception */)
                            .WithMany(d => d.ManyToManyPrincipals)).Message);
            }

            [ConditionalFact]
            public virtual void Throws_for_ForeignKeyAttribute_on_navigation()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<CategoryWithAttribute>();

                Assert.Equal(
                    CoreStrings.FkAttributeOnSkipNavigation(
                        nameof(ProductWithAttribute), nameof(Product.Categories)),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.FinalizeModel()).Message);
            }

            [ConditionalFact]
            public virtual void Overrides_ForeignKeyAttribute()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<CategoryWithAttribute>()
                    .HasMany(e => e.Products)
                    .WithMany(e => e.Categories)
                    .UsingEntity<Dictionary<string, object>>(
                        "ProductCategory",
                        e => e.HasOne<ProductWithAttribute>().WithMany().HasForeignKey("ProductKey"),
                        e => e.HasOne<CategoryWithAttribute>().WithMany().HasForeignKey("CategoryKey"));

                var model = modelBuilder.FinalizeModel();

                var category = model.FindEntityType(typeof(CategoryWithAttribute));
                var productsNavigation = category.GetSkipNavigations().Single();
                var categoryFk = productsNavigation.ForeignKey;
                Assert.Equal("CategoryKey", categoryFk.Properties.Single().Name);
            }

            protected class ProductWithAttribute
            {
                public int Id { get; set; }

                [ForeignKey("ProductId")]
                public virtual ICollection<CategoryWithAttribute> Categories { get; set; }
            }

            protected class CategoryWithAttribute
            {
                public int Id { get; set; }
                public virtual ICollection<ProductWithAttribute> Products { get; set; }
            }

            [ConditionalFact]
            public virtual void Navigation_properties_can_set_access_mode()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<ManyToManyNavPrincipal>()
                    .HasMany(e => e.Dependents)
                    .WithMany(e => e.ManyToManyPrincipals);

                modelBuilder.Entity<ManyToManyNavPrincipal>()
                    .Navigation(e => e.Dependents)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

                modelBuilder.Entity<NavDependent>()
                    .Navigation(e => e.ManyToManyPrincipals)
                    .UsePropertyAccessMode(PropertyAccessMode.Property);

                modelBuilder.Entity<NavDependent>()
                    .Ignore(n => n.OneToOnePrincipal);

                var model = modelBuilder.FinalizeModel();

                var principal = model.FindEntityType(typeof(ManyToManyNavPrincipal));
                var dependent = model.FindEntityType(typeof(NavDependent));

                Assert.Equal(PropertyAccessMode.Field, principal.FindSkipNavigation("Dependents").GetPropertyAccessMode());
                Assert.Equal(PropertyAccessMode.Property, dependent.FindSkipNavigation("ManyToManyPrincipals").GetPropertyAccessMode());
            }

            [ConditionalFact]
            public virtual void Skip_navigation_field_can_be_set_via_attribute()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<OneToManyNavPrincipal>();
                modelBuilder.Ignore<OneToOneNavPrincipal>();
                modelBuilder.Entity<ManyToManyNavPrincipal>()
                    .HasMany(e => e.Dependents)
                    .WithMany(e => e.ManyToManyPrincipals);

                var model = modelBuilder.FinalizeModel();

                Assert.Equal(
                    "_randomField", model.FindEntityType(typeof(ManyToManyNavPrincipal)).FindSkipNavigation("Dependents").GetFieldName());
            }

            [ConditionalFact]
            public virtual void IsRequired_throws()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<ManyToManyNavPrincipal>()
                    .HasMany(n => n.Dependents)
                    .WithMany(n => n.ManyToManyPrincipals);

                Assert.Equal(
                    CoreStrings.RequiredSkipNavigation(nameof(ManyToManyNavPrincipal), nameof(ManyToManyNavPrincipal.Dependents)),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<ManyToManyNavPrincipal>()
                            .Navigation(p => p.Dependents)
                            .IsRequired()).Message);
            }

            [ConditionalFact]
            public virtual void Can_use_shared_Type_as_join_entity()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<OneToManyNavPrincipal>();
                modelBuilder.Ignore<OneToOneNavPrincipal>();

                modelBuilder.Entity<ManyToManyNavPrincipal>()
                    .HasMany(e => e.Dependents)
                    .WithMany(e => e.ManyToManyPrincipals)
                    .UsingEntity<Dictionary<string, object>>(
                        "Shared1",
                        e => e.HasOne<NavDependent>().WithMany(),
                        e => e.HasOne<ManyToManyNavPrincipal>().WithMany());

                modelBuilder.Entity<ManyToManyPrincipalWithField>()
                    .HasMany(e => e.Dependents)
                    .WithMany(e => e.ManyToManyPrincipals)
                    .UsingEntity<Dictionary<string, object>>(
                        "Shared2",
                        e => e.HasOne<DependentWithField>().WithMany(),
                        e => e.HasOne<ManyToManyPrincipalWithField>().WithMany(),
                        e => e.IndexerProperty<int>("Payload"));

                modelBuilder.Entity<ManyToManyPrincipalWithField>().HasKey(d => d.Id);
                modelBuilder.Entity<OneToManyPrincipalWithField>().HasKey(d => d.Id);
                modelBuilder.Entity<OneToOnePrincipalWithField>().HasKey(d => d.Id);
                modelBuilder.Entity<DependentWithField>().HasKey(d => d.DependentWithFieldId);

                var model = modelBuilder.Model;

                var shared1 = model.FindEntityType("Shared1");
                Assert.NotNull(shared1);
                Assert.Equal(2, shared1.GetForeignKeys().Count());
                Assert.Equal(new[]
                    {
                        nameof(ManyToManyNavPrincipal.Dependents) + nameof(NavDependent.Id),
                        nameof(NavDependent.ManyToManyPrincipals) + nameof(ManyToManyNavPrincipal.Id)
                    },
                    shared1.GetProperties().Select(p => p.Name));
                Assert.True(shared1.HasSharedClrType);
                Assert.Equal(typeof(Dictionary<string, object>), shared1.ClrType);

                var shared2 = model.FindEntityType("Shared2");
                Assert.NotNull(shared2);
                Assert.Equal(2, shared2.GetForeignKeys().Count());
                Assert.Equal(new[]
                    {
                        nameof(ManyToManyPrincipalWithField.Dependents) + nameof(DependentWithField.DependentWithFieldId),
                        nameof(DependentWithField.ManyToManyPrincipals) + nameof(ManyToManyPrincipalWithField.Id),
                        "Payload"
                    },
                    shared2.GetProperties().Select(p => p.Name));
                Assert.True(shared2.HasSharedClrType);
                Assert.Equal(typeof(Dictionary<string, object>), shared2.ClrType);

                Assert.Equal(
                    CoreStrings.ClashingSharedType(typeof(Dictionary<string, object>).DisplayName()),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<Dictionary<string, object>>()).Message);

                modelBuilder.FinalizeModel();
            }

            [ConditionalFact]
            public virtual void UsingEntity_with_shared_type_fails_when_not_marked()
            {
                var modelBuilder = CreateModelBuilder();

                Assert.Equal(
                    CoreStrings.TypeNotMarkedAsShared(typeof(ManyToManyJoinWithFields).DisplayName()),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<ManyToManyPrincipalWithField>()
                            .HasMany(e => e.Dependents)
                            .WithMany(e => e.ManyToManyPrincipals)
                            .UsingEntity<ManyToManyJoinWithFields>(
                                "Shared",
                                r => r.HasOne<DependentWithField>().WithMany(),
                                l => l.HasOne<ManyToManyPrincipalWithField>().WithMany())).Message);
            }

            [ConditionalFact]
            public virtual void UsingEntity_with_shared_type_passed_when_marked_as_shared_type()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.SharedTypeEntity<ManyToManyJoinWithFields>("Shared");

                var joinEntityType = modelBuilder.Entity<ManyToManyPrincipalWithField>()
                    .HasMany(e => e.Dependents)
                    .WithMany(e => e.ManyToManyPrincipals)
                    .UsingEntity<ManyToManyJoinWithFields>(
                        "Shared",
                        r => r.HasOne<DependentWithField>().WithMany(),
                        l => l.HasOne<ManyToManyPrincipalWithField>().WithMany()).Metadata;

                modelBuilder.Entity<ManyToManyPrincipalWithField>().HasKey(d => d.Id);
                modelBuilder.Entity<OneToManyPrincipalWithField>().HasKey(d => d.Id);
                modelBuilder.Entity<OneToOnePrincipalWithField>().HasKey(d => d.Id);
                modelBuilder.Entity<DependentWithField>().HasKey(d => d.DependentWithFieldId);

                var model = modelBuilder.FinalizeModel();

                Assert.True(joinEntityType.HasSharedClrType);
                Assert.Equal("Shared", joinEntityType.Name);
                Assert.Equal(typeof(ManyToManyJoinWithFields), joinEntityType.ClrType);
            }

            [ConditionalFact]
            public virtual void UsingEntity_with_shared_type_passes_when_configured_as_shared()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.SharedTypeEntity<ManyToManyJoinWithFields>("Shared");

                var joinEntityType = modelBuilder.Entity<ManyToManyPrincipalWithField>()
                    .HasMany(e => e.Dependents)
                    .WithMany(e => e.ManyToManyPrincipals)
                    .UsingEntity<ManyToManyJoinWithFields>(
                        "Shared",
                        r => r.HasOne<DependentWithField>().WithMany(),
                        l => l.HasOne<ManyToManyPrincipalWithField>().WithMany()).Metadata;

                modelBuilder.Entity<ManyToManyPrincipalWithField>().HasKey(d => d.Id);
                modelBuilder.Entity<OneToManyPrincipalWithField>().HasKey(d => d.Id);
                modelBuilder.Entity<OneToOnePrincipalWithField>().HasKey(d => d.Id);
                modelBuilder.Entity<DependentWithField>().HasKey(d => d.DependentWithFieldId);

                var model = modelBuilder.FinalizeModel();

                Assert.True(joinEntityType.HasSharedClrType);
                Assert.Equal("Shared", joinEntityType.Name);
                Assert.Equal(typeof(ManyToManyJoinWithFields), joinEntityType.ClrType);
            }

            [ConditionalFact]
            public virtual void Unconfigured_many_to_many_navigations_throw()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<AmbiguousManyToManyImplicitLeft>();

                Assert.Equal(
                    CoreStrings.NavigationNotAdded(
                        typeof(AmbiguousManyToManyImplicitLeft).DisplayName(fullName: false), "Navigation1",
                        typeof(List<AmbiguousManyToManyImplicitRight>).DisplayName(fullName: false)),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
            }
        }
    }
}
