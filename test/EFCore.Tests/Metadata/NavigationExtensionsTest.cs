// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class NavigationExtensionsTest
    {
        [Fact]
        public void Can_get_one_to_many_inverses()
        {
            var model = BuildModel();

            var category = model.FindEntityType(typeof(Product)).GetNavigations().Single(e => e.Name == "Category");
            var products = model.FindEntityType(typeof(Category)).GetNavigations().Single(e => e.Name == "Products");

            Assert.Same(category, products.FindInverse());
            Assert.Same(products, category.FindInverse());
        }

        [Fact]
        public void Can_get_one_to_one_inverses()
        {
            var model = BuildModel();

            var category = model.FindEntityType(typeof(Product)).GetNavigations().Single(e => e.Name == "FeaturedProductCategory");
            var product = model.FindEntityType(typeof(Category)).GetNavigations().Single(e => e.Name == "FeaturedProduct");

            Assert.Same(category, product.FindInverse());
            Assert.Same(product, category.FindInverse());
        }

        [Fact]
        public void Can_get_target_ends()
        {
            var model = BuildModel();

            var productType = model.FindEntityType(typeof(Product));
            var categoryType = model.FindEntityType(typeof(Category));

            var category = productType.GetNavigations().Single(e => e.Name == "Category");
            var products = categoryType.GetNavigations().Single(e => e.Name == "Products");

            Assert.Same(productType, products.GetTargetType());
            Assert.Same(categoryType, category.GetTargetType());
        }

        [Fact]
        public void Returns_null_when_no_inverse()
        {
            var products = BuildModel(createCategory: false).FindEntityType(typeof(Category)).GetNavigations().Single(e => e.Name == "Products");

            Assert.Null(products.FindInverse());

            var category = BuildModel(createProducts: false).FindEntityType(typeof(Product)).GetNavigations().Single(e => e.Name == "Category");

            Assert.Null(category.FindInverse());

            var featuredCategory = BuildModel(createFeaturedProduct: false).FindEntityType(typeof(Product)).GetNavigations().Single(e => e.Name == "FeaturedProductCategory");

            Assert.Null(featuredCategory.FindInverse());

            var featuredProduct = BuildModel(createFeaturedProductCategory: false).FindEntityType(typeof(Category)).GetNavigations().Single(e => e.Name == "FeaturedProduct");

            Assert.Null(featuredProduct.FindInverse());
        }

        private class Category
        {
            public static readonly PropertyInfo ProductsProperty = typeof(Category).GetProperty(nameof(Products));
            public static readonly PropertyInfo FeaturedProductProperty = typeof(Category).GetProperty(nameof(FeaturedProduct));

            public int Id { get; set; }

            public int FeaturedProductId { get; set; }
            public Product FeaturedProduct { get; set; }

            public ICollection<Product> Products { get; set; }
        }

        private class Product
        {
            public static readonly PropertyInfo CategoryProperty = typeof(Product).GetProperty(nameof(Category));
            public static readonly PropertyInfo FeaturedProductCategoryProperty = typeof(Product).GetProperty(nameof(FeaturedProductCategory));

            public int Id { get; set; }

            public Category FeaturedProductCategory { get; set; }

            public int CategoryId { get; set; }
            public Category Category { get; set; }
        }

        private static IModel BuildModel(
            bool createProducts = true, bool createCategory = true,
            bool createFeaturedProductCategory = true, bool createFeaturedProduct = true)
        {
            var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var model = builder.Model;

            builder.Entity<Product>(
                e =>
                {
                    e.Ignore(p => p.Category);
                    e.Ignore(p => p.FeaturedProductCategory);
                });
            builder.Entity<Category>(
                e =>
                {
                    e.Ignore(c => c.Products);
                    e.Ignore(c => c.FeaturedProduct);
                });

            var categoryType = model.FindEntityType(typeof(Category));
            var productType = model.FindEntityType(typeof(Product));

            var categoryFk = productType.GetOrAddForeignKey(productType.FindProperty("CategoryId"), categoryType.FindPrimaryKey(), categoryType);
            var featuredProductFk = categoryType.GetOrAddForeignKey(categoryType.FindProperty("FeaturedProductId"), productType.FindPrimaryKey(), productType);
            featuredProductFk.IsUnique = true;

            if (createProducts)
            {
                categoryFk.HasPrincipalToDependent(Category.ProductsProperty);
            }

            if (createCategory)
            {
                categoryFk.HasDependentToPrincipal(Product.CategoryProperty);
            }

            if (createFeaturedProductCategory)
            {
                featuredProductFk.HasPrincipalToDependent(Product.FeaturedProductCategoryProperty);
            }

            if (createFeaturedProduct)
            {
                featuredProductFk.HasDependentToPrincipal(Category.FeaturedProductProperty);
            }

            return model;
        }
    }
}
