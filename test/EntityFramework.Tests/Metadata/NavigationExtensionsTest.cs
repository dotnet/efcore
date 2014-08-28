// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class NavigationExtensionsTest
    {
        [Fact]
        public void Can_get_one_to_many_inverses()
        {
            var model = BuildModel();

            var category = model.GetEntityType(typeof(Product)).Navigations.Single(e => e.Name == "Category");
            var products = model.GetEntityType(typeof(Category)).Navigations.Single(e => e.Name == "Products");

            Assert.Same(category, products.TryGetInverse());
            Assert.Same(products, category.TryGetInverse());
        }

        [Fact]
        public void Can_get_one_to_one_inverses()
        {
            var model = BuildModel();

            var category = model.GetEntityType(typeof(Product)).Navigations.Single(e => e.Name == "FeaturedProductCategory");
            var product = model.GetEntityType(typeof(Category)).Navigations.Single(e => e.Name == "FeaturedProduct");

            Assert.Same(category, product.TryGetInverse());
            Assert.Same(product, category.TryGetInverse());
        }

        [Fact]
        public void Can_get_target_ends()
        {
            var model = BuildModel();

            var productType = model.GetEntityType(typeof(Product));
            var categoryType = model.GetEntityType(typeof(Category));

            var category = productType.Navigations.Single(e => e.Name == "Category");
            var products = categoryType.Navigations.Single(e => e.Name == "Products");

            Assert.Same(productType, products.GetTargetType());
            Assert.Same(categoryType, category.GetTargetType());
        }

        [Fact]
        public void Returns_null_when_no_inverse()
        {
            var products = BuildModel(createCategory: false).GetEntityType(typeof(Category)).Navigations.Single(e => e.Name == "Products");

            Assert.Null(products.TryGetInverse());

            var category = BuildModel(createProducts: false).GetEntityType(typeof(Product)).Navigations.Single(e => e.Name == "Category");

            Assert.Null(category.TryGetInverse());

            var featuredCategory = BuildModel(createFeaturedProduct: false).GetEntityType(typeof(Product)).Navigations.Single(e => e.Name == "FeaturedProductCategory");

            Assert.Null(featuredCategory.TryGetInverse());

            var featuredProduct = BuildModel(createFeaturedProductCategory: false).GetEntityType(typeof(Category)).Navigations.Single(e => e.Name == "FeaturedProduct");

            Assert.Null(featuredProduct.TryGetInverse());
        }

        private class Category
        {
            public int Id { get; set; }

            public int FeaturedProductId { get; set; }
            public Product FeaturedProduct { get; set; }

            public ICollection<Product> Products { get; set; }
        }

        private class Product
        {
            public int Id { get; set; }

            public Category FeaturedProductCategory { get; set; }

            public int CategoryId { get; set; }
            public Category Category { get; set; }
        }

        private static IModel BuildModel(
            bool createProducts = true, bool createCategory = true,
            bool createFeaturedProductCategory = true, bool createFeaturedProduct = true)
        {
            var model = new Model();
            var builder = new ModelBuilder(model);

            builder.Entity<Product>();
            builder.Entity<Category>();

            var categoryType = model.GetEntityType(typeof(Category));
            var productType = model.GetEntityType(typeof(Product));

            var categoryFk = productType.GetOrAddForeignKey(categoryType.GetPrimaryKey(), productType.GetProperty("CategoryId"));
            var featuredProductFk = categoryType.GetOrAddForeignKey(productType.GetPrimaryKey(), categoryType.GetProperty("FeaturedProductId"));
            featuredProductFk.IsUnique = true;

            if (createProducts)
            {
                categoryType.AddNavigation(new Navigation(categoryFk, "Products", pointsToPrincipal: false));
            }
            if (createCategory)
            {
                productType.AddNavigation(new Navigation(categoryFk, "Category", pointsToPrincipal: true));
            }

            if (createFeaturedProductCategory)
            {
                productType.AddNavigation(new Navigation(featuredProductFk, "FeaturedProductCategory", pointsToPrincipal: false));
            }
            if (createFeaturedProduct)
            {
                categoryType.AddNavigation(new Navigation(featuredProductFk, "FeaturedProduct", pointsToPrincipal: true));
            }

            return model;
        }
    }
}
