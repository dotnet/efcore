// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            var builder = TestHelpers.Instance.CreateConventionBuilder(model);

            builder.Entity<Product>();
            builder.Entity<Category>();

            var categoryType = model.GetEntityType(typeof(Category));
            var productType = model.GetEntityType(typeof(Product));

            var categoryFk = productType.GetOrAddForeignKey(productType.GetProperty("CategoryId"), categoryType.GetPrimaryKey());
            var featuredProductFk = categoryType.GetOrAddForeignKey(categoryType.GetProperty("FeaturedProductId"), productType.GetPrimaryKey());
            featuredProductFk.IsUnique = true;

            if (createProducts)
            {
                categoryType.AddNavigation("Products", categoryFk, pointsToPrincipal: false);
            }
            if (createCategory)
            {
                productType.AddNavigation("Category", categoryFk, pointsToPrincipal: true);
            }

            if (createFeaturedProductCategory)
            {
                productType.AddNavigation("FeaturedProductCategory", featuredProductFk, pointsToPrincipal: false);
            }
            if (createFeaturedProduct)
            {
                categoryType.AddNavigation("FeaturedProduct", featuredProductFk, pointsToPrincipal: true);
            }

            return model;
        }

        [Fact]
        public void IsNonNotifyingCollection_returns_false_for_reference_avigations()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<Dependent>(BuildCollectionsModel());

            Assert.False(entry.EntityType.GetNavigation("Principal1").IsNonNotifyingCollection(entry));
        }

        [Fact]
        public void IsNonNotifyingCollection_returns_false_for_collections_typed_for_notifications()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<Principal>(BuildCollectionsModel());

            // TODO: The following assert should be changed to False once INotifyCollectionChanged is supported (Issue #445)
            Assert.True(entry.EntityType.GetNavigation("Dependents2").IsNonNotifyingCollection(entry));
        }

        [Fact]
        public void IsNonNotifyingCollection_returns_false_for_null_collections()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<Principal>(BuildCollectionsModel());

            // TODO: The following assert should be changed to False once INotifyCollectionChanged is supported (Issue #445)
            Assert.True(entry.EntityType.GetNavigation("Dependents1").IsNonNotifyingCollection(entry));
        }

        [Fact]
        public void IsNonNotifyingCollection_returns_false_for_notifying_instances()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                BuildCollectionsModel(),
                EntityState.Detached,
                new Principal { Dependents1 = new ObservableCollection<Dependent>() });

            // TODO: The following assert should be changed to False once INotifyCollectionChanged is supported (Issue #445)
            Assert.True(entry.EntityType.GetNavigation("Dependents1").IsNonNotifyingCollection(entry));
        }

        [Fact]
        public void IsNonNotifyingCollection_returns_true_for_non_notifying_instances()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                BuildCollectionsModel(),
                EntityState.Detached,
                new Principal { Dependents1 = new List<Dependent>() });

            Assert.True(entry.EntityType.GetNavigation("Dependents1").IsNonNotifyingCollection(entry));
        }

        private static IModel BuildCollectionsModel()
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<Principal>().HasMany(e => e.Dependents1).WithOne(e => e.Principal1);
            builder.Entity<Principal>().HasMany(e => e.Dependents2).WithOne(e => e.Principal2);

            return builder.Model;
        }

        private class Principal
        {
            public int Id { get; set; }

            public ICollection<Dependent> Dependents1 { get; set; }
            public ObservableCollection<Dependent> Dependents2 { get; set; }
        }

        private class Dependent
        {
            public int Id { get; set; }

            public Principal Principal1 { get; set; }
            public Principal Principal2 { get; set; }
        }
    }
}
