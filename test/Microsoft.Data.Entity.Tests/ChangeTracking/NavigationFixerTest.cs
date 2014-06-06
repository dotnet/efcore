// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class NavigationFixerTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            var fixer = new NavigationFixer(Mock.Of<StateManager>(), CreateAccessorSource());

            Assert.Equal(
                "entry",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => fixer.StateChanged(null, EntityState.Unknown)).ParamName);
            Assert.Equal(
                Strings.FormatInvalidEnumValue("oldState", typeof(EntityState)),
                Assert.Throws<ArgumentException>(() => fixer.StateChanged(new Mock<StateEntry>().Object, (EntityState)77)).Message);
        }

        [Fact]
        public void Does_fixup_of_related_principals()
        {
            var manager = CreateStateManager();

            var principal1 = new Category { Id = 11 };
            var principal2 = new Category { Id = 12 };
            var dependent = new Product { Id = 21, CategoryId = 12 };

            manager.StartTracking(manager.GetOrCreateEntry(principal1));
            manager.StartTracking(manager.GetOrCreateEntry(principal2));

            var dependentEntry = manager.GetOrCreateEntry(dependent);
            manager.StartTracking(dependentEntry);

            var fixer = new NavigationFixer(manager, CreateAccessorSource());
            fixer.StateChanged(dependentEntry, EntityState.Unknown);

            Assert.Same(dependent.Category, principal2);
            Assert.Contains(dependent, principal2.Products);
            Assert.DoesNotContain(dependent, principal1.Products);
        }

        [Fact]
        public void Does_fixup_of_related_dependents()
        {
            var manager = CreateStateManager();

            var dependent1 = new Product { Id = 21, CategoryId = 11 };
            var dependent2 = new Product { Id = 22, CategoryId = 12 };
            var dependent3 = new Product { Id = 23, CategoryId = 11 };

            var principal = new Category { Id = 11 };

            manager.StartTracking(manager.GetOrCreateEntry(dependent1));
            manager.StartTracking(manager.GetOrCreateEntry(dependent2));
            manager.StartTracking(manager.GetOrCreateEntry(dependent3));

            var principalEntry = manager.GetOrCreateEntry(principal);
            manager.StartTracking(principalEntry);

            var fixer = new NavigationFixer(manager, CreateAccessorSource());
            fixer.StateChanged(principalEntry, EntityState.Unknown);

            Assert.Same(dependent1.Category, principal);
            Assert.Null(dependent2.Category);
            Assert.Same(dependent3.Category, principal);

            Assert.Contains(dependent1, principal.Products);
            Assert.DoesNotContain(dependent2, principal.Products);
            Assert.Contains(dependent3, principal.Products);
        }

        [Fact]
        public void Does_fixup_of_one_to_one_relationship()
        {
            var manager = CreateStateManager();

            var principal1 = new Product { Id = 21 };
            var principal2 = new Product { Id = 22 };
            var principal3 = new Product { Id = 23 };

            var dependent1 = new ProductDetail { Id = 21 };
            var dependent2 = new ProductDetail { Id = 22 };
            var dependent4 = new ProductDetail { Id = 24 };

            var principalEntry1 = manager.GetOrCreateEntry(principal1);
            var principalEntry2 = manager.GetOrCreateEntry(principal2);
            var principalEntry3 = manager.GetOrCreateEntry(principal3);

            var dependentEntry1 = manager.GetOrCreateEntry(dependent1);
            var dependentEntry2 = manager.GetOrCreateEntry(dependent2);
            var dependentEntry4 = manager.GetOrCreateEntry(dependent4);

            manager.StartTracking(principalEntry1);
            manager.StartTracking(principalEntry2);
            manager.StartTracking(principalEntry3);

            manager.StartTracking(dependentEntry1);
            manager.StartTracking(dependentEntry2);
            manager.StartTracking(dependentEntry4);

            var fixer = new NavigationFixer(manager, CreateAccessorSource());

            Assert.Null(principal1.Detail);
            Assert.Null(dependent1.Product);

            fixer.StateChanged(principalEntry1, EntityState.Unknown);

            Assert.Same(principal1, dependent1.Product);
            Assert.Same(dependent1, principal1.Detail);

            Assert.Null(principal2.Detail);
            Assert.Null(dependent2.Product);

            fixer.StateChanged(dependentEntry2, EntityState.Unknown);

            Assert.Same(principal2, dependent2.Product);
            Assert.Same(dependent2, principal2.Detail);

            Assert.Null(principal3.Detail);
            Assert.Null(dependent4.Product);

            fixer.StateChanged(principalEntry3, EntityState.Unknown);
            fixer.StateChanged(dependentEntry4, EntityState.Unknown);

            Assert.Null(principal3.Detail);
            Assert.Null(dependent4.Product);
        }

        [Fact]
        public void Does_fixup_of_one_to_one_self_referencing_relationship()
        {
            var manager = CreateStateManager();

            var entity1 = new Product { Id = 21, AlternateProductId = 22 };
            var entity2 = new Product { Id = 22, AlternateProductId = 23 };
            var entity3 = new Product { Id = 23 };

            var entry1 = manager.GetOrCreateEntry(entity1);
            var entry2 = manager.GetOrCreateEntry(entity2);
            var entry3 = manager.GetOrCreateEntry(entity3);

            manager.StartTracking(entry1);
            manager.StartTracking(entry2);
            manager.StartTracking(entry3);

            var fixer = new NavigationFixer(manager, CreateAccessorSource());

            Assert.Null(entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);
            
            Assert.Null(entity2.AlternateProduct);
            Assert.Null(entity2.OriginalProduct);
            
            Assert.Null(entity3.AlternateProduct);
            Assert.Null(entity3.OriginalProduct);

            fixer.StateChanged(entry1, EntityState.Unknown);

            Assert.Same(entity2, entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Null(entity2.AlternateProduct);
            Assert.Same(entity1, entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Null(entity3.OriginalProduct);

            fixer.StateChanged(entry3, EntityState.Unknown);

            Assert.Same(entity2, entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Same(entity3, entity2.AlternateProduct);
            Assert.Same(entity1, entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Same(entity2, entity3.OriginalProduct);
        }

        private static StateManager CreateStateManager()
        {
            return TestHelpers.CreateContextConfiguration(BuildModel()).Services.StateManager;
        }

        private class Category
        {
            private readonly ICollection<Product> _products = new List<Product>();

            public int Id { get; set; }

            public ICollection<Product> Products
            {
                get { return _products; }
            }
        }

        private class Product
        {
            public int Id { get; set; }

            public int CategoryId { get; set; }
            public Category Category { get; set; }

            public ProductDetail Detail { get; set; }

            public int? AlternateProductId { get; set; }
            public Product AlternateProduct { get; set; }
            public Product OriginalProduct { get; set; }
        }

        private class ProductDetail : IEnumerable<Product>
        {
            public int Id { get; set; }

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

        private static IModel BuildModel()
        {
            var model = new Model();
            var builder = new ConventionModelBuilder(model);

            builder.Entity<Product>();
            builder.Entity<Category>();
            builder.Entity<ProductDetail>();

            var categoryType = model.GetEntityType(typeof(Category));
            var productType = model.GetEntityType(typeof(Product));
            var productDetailType = model.GetEntityType(typeof(ProductDetail));

            var categoryFk = productType.AddForeignKey(categoryType.GetKey(), productType.GetProperty("CategoryId"));
            var alternateProductFk = productType.AddForeignKey(productType.GetKey(), productType.GetProperty("AlternateProductId"));
            var productDetailFk = productDetailType.AddForeignKey(productType.GetKey(), productDetailType.GetProperty("Id"));

            categoryType.AddNavigation(new Navigation(categoryFk, "Products", pointsToPrincipal: false));
            productType.AddNavigation(new Navigation(categoryFk, "Category", pointsToPrincipal: true));

            productType.AddNavigation(new Navigation(alternateProductFk, "AlternateProduct", pointsToPrincipal: true));
            productType.AddNavigation(new Navigation(alternateProductFk, "OriginalProduct", pointsToPrincipal: false));

            productDetailType.AddNavigation(new Navigation(productDetailFk, "Product", pointsToPrincipal: true));
            productType.AddNavigation(new Navigation(productDetailFk, "Detail", pointsToPrincipal: false));

            return model;
        }

        private static NavigationAccessorSource CreateAccessorSource()
        {
            return new NavigationAccessorSource(new ClrPropertyGetterSource(), new ClrPropertySetterSource(), new ClrCollectionAccessorSource());
        }
    }
}
