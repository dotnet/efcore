// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class ManyToManyTestBase : ModelBuilderTestBase
        {
            [ConditionalFact]
            public virtual void Finds_existing_navigations_and_uses_associated_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

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

                modelBuilder.FinalizeModel();

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
                var model = modelBuilder.Model;

                modelBuilder.Entity<ManyToManyPrincipalWithField>(e =>
                {
                    e.Property(p => p.Id);
                    e.Property(p => p.Name);
                    e.HasKey(p => p.Id);
                });
                modelBuilder.Entity<DependentWithField>(e =>
                {
                    e.Property(d => d.DependentWithFieldId);
                    e.Property(d => d.AnotherOneToManyPrincipalId);
                    e.Ignore(d => d.OneToManyPrincipal);
                    e.Ignore(d => d.OneToOnePrincipal);
                    e.HasKey(d => d.DependentWithFieldId);
                });

                modelBuilder.Entity<ManyToManyPrincipalWithField>()
                    .HasMany(p => p.Dependents)
                    .WithMany(d => d.ManyToManyPrincipals)
                    .UsingEntity<ManyToManyJoinWithFields>(
                        jwf => jwf.HasOne<DependentWithField>(j => j.DependentWithField)
                            .WithMany(),
                        jwf => jwf.HasOne<ManyToManyPrincipalWithField>(j => j.ManyToManyPrincipalWithField)
                            .WithMany())
                    .HasKey(j => new { j.DependentWithFieldId, j.ManyToManyPrincipalWithFieldId });

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
                        jwf => jwf.HasOne<DependentWithField>(j => j.DependentWithField)
                            .WithMany(),
                        jwf => jwf.HasOne<ManyToManyPrincipalWithField>(j => j.ManyToManyPrincipalWithField)
                            .WithMany());

                modelBuilder.FinalizeModel();

                Assert.Same(principalToJoinNav, principalEntityType.GetSkipNavigations().Single());
                Assert.Same(dependentToJoinNav, dependentEntityType.GetSkipNavigations().Single());
                Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
                Assert.Same(principalToDependentFk, joinEntityType.GetForeignKeys().Last());
                Assert.Same(dependentToPrincipalFk, joinEntityType.GetForeignKeys().First());
            }

            [ConditionalFact]
            public virtual void Configures_association_type()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Category>().Ignore(c => c.Products);
                modelBuilder.Entity<Product>().Ignore(p => p.Categories);

                modelBuilder.Entity<Category>()
                    .HasMany(o => o.Products).WithMany(c => c.Categories)
                    .UsingEntity<ProductCategory>(
                        pcb => pcb.HasOne(pc => pc.Product).WithMany(),
                        pcb => pcb.HasOne(pc => pc.Category).WithMany(c => c.ProductCategories),
                        pcb => pcb.HasKey(pc => new { pc.ProductId, pc.CategoryId }));

                modelBuilder.FinalizeModel();

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
                var model = modelBuilder.Model;
                modelBuilder.Entity<Category>()
                    .HasMany(p => p.Products).WithMany(c => c.Categories);

                modelBuilder.Entity<Category>().Ignore(c => c.Products);
                modelBuilder.Entity<Product>().Ignore(p => p.Categories);

                // Issue #19550
                modelBuilder.Ignore<ProductCategory>();

                var productType = model.FindEntityType(typeof(Product));
                var categoryType = model.FindEntityType(typeof(Category));

                Assert.Empty(productType.GetSkipNavigations());
                Assert.Empty(categoryType.GetSkipNavigations());

                modelBuilder.FinalizeModel();
            }

            [ConditionalFact]
            public virtual void Throws_for_conflicting_many_to_one_on_left()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

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
                var model = modelBuilder.Model;

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
                var model = modelBuilder.Model;

                Assert.Equal(
                    CoreStrings.MissingInverseManyToManyNavigation(
                        nameof(ManyToManyNavPrincipal),
                        nameof(NavDependent)),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<ManyToManyNavPrincipal>()
                                .HasMany<NavDependent>(/* leaving empty causes the exception */)
                                .WithMany(d => d.ManyToManyPrincipals)).Message);
            }

            [ConditionalFact]
            public virtual void Navigation_properties_can_set_access_mode_using_expressions()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<ManyToManyNavPrincipal>()
                    .HasMany(e => e.Dependents)
                    .WithMany(e => e.ManyToManyPrincipals);

                modelBuilder.Entity<ManyToManyNavPrincipal>()
                    .Navigation(e => e.Dependents)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

                modelBuilder.Entity<NavDependent>()
                    .Navigation(e => e.ManyToManyPrincipals)
                    .UsePropertyAccessMode(PropertyAccessMode.Property);

                var principal = (IEntityType)model.FindEntityType(typeof(ManyToManyNavPrincipal));
                var dependent = (IEntityType)model.FindEntityType(typeof(NavDependent));

                Assert.Equal(PropertyAccessMode.Field, principal.FindSkipNavigation("Dependents").GetPropertyAccessMode());
                Assert.Equal(PropertyAccessMode.Property, dependent.FindSkipNavigation("ManyToManyPrincipals").GetPropertyAccessMode());
            }

            [ConditionalFact]
            public virtual void Navigation_properties_can_set_access_mode_using_navigation_names()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<ManyToManyNavPrincipal>()
                    .HasMany<NavDependent>("Dependents")
                    .WithMany("ManyToManyPrincipals");

                modelBuilder.Entity<ManyToManyNavPrincipal>()
                    .Navigation("Dependents")
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

                modelBuilder.Entity<NavDependent>()
                    .Navigation("ManyToManyPrincipals")
                    .UsePropertyAccessMode(PropertyAccessMode.Property);

                var principal = (IEntityType)model.FindEntityType(typeof(ManyToManyNavPrincipal));
                var dependent = (IEntityType)model.FindEntityType(typeof(NavDependent));

                Assert.Equal(PropertyAccessMode.Field, principal.FindSkipNavigation("Dependents").GetPropertyAccessMode());
                Assert.Equal(PropertyAccessMode.Property, dependent.FindSkipNavigation("ManyToManyPrincipals").GetPropertyAccessMode());
            }
        }
    }
}
