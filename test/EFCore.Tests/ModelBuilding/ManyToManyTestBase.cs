// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        }
    }
}
