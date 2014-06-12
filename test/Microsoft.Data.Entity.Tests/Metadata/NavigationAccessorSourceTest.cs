// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class NavigationAccessorSourceTest
    {
        [Fact]
        public void Creates_collection_accessor_for_appropriate_IEnumerable_property()
        {
            var model = BuildModel();
            var source = CreateAccessorSource();

            var productsNavigation = model.GetEntityType(typeof(Category)).Navigations.Single(n => n.Name == "Products");
            Assert.IsType<CollectionNavigationAccessor>(source.GetAccessor(productsNavigation));

            var categoriesNavigation = model.GetEntityType(typeof(Product)).Navigations.Single(n => n.Name == "Categories");
            Assert.IsType<CollectionNavigationAccessor>(source.GetAccessor(categoriesNavigation));
        }

        [Fact]
        public void Creates_reference_accessor_for_non_matching_IEnumerable_property()
        {
            var model = BuildModel();
            var source = CreateAccessorSource();

            var productNavigation = model.GetEntityType(typeof(Category)).Navigations.Single(n => n.Name == "Product");
            Assert.IsType<NavigationAccessor>(source.GetAccessor(productNavigation));

            var categoryNavigation = model.GetEntityType(typeof(Product)).Navigations.Single(n => n.Name == "Category");
            Assert.IsType<NavigationAccessor>(source.GetAccessor(categoryNavigation));
        }

        [Fact]
        public void Caches_accessor_for_given_property_on_given_type()
        {
            var model = BuildModel();
            var source = CreateAccessorSource();

            var productNavigation = model.GetEntityType(typeof(Category)).Navigations.Single(n => n.Name == "Product");

            Assert.Same(source.GetAccessor(productNavigation), source.GetAccessor(productNavigation));
        }

        private static NavigationAccessorSource CreateAccessorSource()
        {
            return new NavigationAccessorSource(new ClrPropertyGetterSource(), new ClrPropertySetterSource(), new ClrCollectionAccessorSource());
        }

        private class Category : IEnumerable<Product>
        {
            public int Id { get; set; }

            public IEnumerable<Product> Products { get; set; }

            public int ProductId { get; set; }
            public Product Product { get; set; }

            public IEnumerator<Product> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class Product : IEnumerable<Category>
        {
            public int Id { get; set; }

            public int CategoryId { get; set; }
            public Category Category { get; set; }

            public List<Category> Categories { get; set; }

            public IEnumerator<Category> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private static IModel BuildModel()
        {
            var model = new Model();
            var builder = new ConventionModelBuilder(model);

            builder.Entity<Product>();
            builder.Entity<Category>();

            var categoryType = model.GetEntityType(typeof(Category));
            var productType = model.GetEntityType(typeof(Product));

            var categoryFk = productType.AddForeignKey(categoryType.GetKey(), productType.GetProperty("CategoryId"));
            var productFk = categoryType.AddForeignKey(productType.GetKey(), categoryType.GetProperty("ProductId"));

            categoryType.AddNavigation(new Navigation(categoryFk, "Products", pointsToPrincipal: false));
            productType.AddNavigation(new Navigation(categoryFk, "Category", pointsToPrincipal: true));

            productType.AddNavigation(new Navigation(productFk, "Categories", pointsToPrincipal: false));
            categoryType.AddNavigation(new Navigation(productFk, "Product", pointsToPrincipal: true));

            return model;
        }
    }
}
