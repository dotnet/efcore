// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata
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
            var builder = TestHelpers.Instance.CreateConventionBuilder();
            var model = builder.Model;

            builder.Entity<Product>(e =>
                {
                    e.Ignore(p => p.Category);
                    e.Ignore(p => p.FeaturedProductCategory);
                });
            builder.Entity<Category>(e =>
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
                categoryFk.HasPrincipalToDependent(nameof(Category.Products));
            }
            if (createCategory)
            {
                categoryFk.HasDependentToPrincipal(nameof(Product.Category));
            }

            if (createFeaturedProductCategory)
            {
                featuredProductFk.HasPrincipalToDependent(nameof(Product.FeaturedProductCategory));
            }
            if (createFeaturedProduct)
            {
                featuredProductFk.HasDependentToPrincipal(nameof(Category.FeaturedProduct));
            }

            return model;
        }

        [Fact]
        public void IsNonNotifyingCollection_returns_false_for_reference_avigations()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<Dependent>(BuildCollectionsModel());

            Assert.False(entry.EntityType.FindNavigation("Principal1").IsNonNotifyingCollection(entry));
        }

        [Fact]
        public void IsNonNotifyingCollection_returns_false_for_collections_typed_for_notifications()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<Principal>(BuildCollectionsModel());

            // TODO: The following assert should be changed to False once INotifyCollectionChanged is supported (Issue #445)
            Assert.True(entry.EntityType.FindNavigation("Dependents2").IsNonNotifyingCollection(entry));
        }

        [Fact]
        public void IsNonNotifyingCollection_returns_false_for_null_collections()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<Principal>(BuildCollectionsModel());

            // TODO: The following assert should be changed to False once INotifyCollectionChanged is supported (Issue #445)
            Assert.True(entry.EntityType.FindNavigation("Dependents1").IsNonNotifyingCollection(entry));
        }

        [Fact]
        public void IsNonNotifyingCollection_returns_false_for_notifying_instances()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                BuildCollectionsModel(),
                EntityState.Detached,
                new Principal { Dependents1 = new ObservableCollection<Dependent>() });

            // TODO: The following assert should be changed to False once INotifyCollectionChanged is supported (Issue #445)
            Assert.True(entry.EntityType.FindNavigation("Dependents1").IsNonNotifyingCollection(entry));
        }

        [Fact]
        public void IsNonNotifyingCollection_returns_true_for_non_notifying_instances()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                BuildCollectionsModel(),
                EntityState.Detached,
                new Principal { Dependents1 = new List<Dependent>() });

            Assert.True(entry.EntityType.FindNavigation("Dependents1").IsNonNotifyingCollection(entry));
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
