// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
            var fixer = new NavigationFixer(
                Mock.Of<StateManager>(), new ClrCollectionAccessorSource(), new ClrPropertySetterSource());

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

            var fixer = new NavigationFixer(manager, new ClrCollectionAccessorSource(), new ClrPropertySetterSource());
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

            var fixer = new NavigationFixer(manager, new ClrCollectionAccessorSource(), new ClrPropertySetterSource());
            fixer.StateChanged(principalEntry, EntityState.Unknown);

            Assert.Same(dependent1.Category, principal);
            Assert.Null(dependent2.Category);
            Assert.Same(dependent3.Category, principal);

            Assert.Contains(dependent1, principal.Products);
            Assert.DoesNotContain(dependent2, principal.Products);
            Assert.Contains(dependent3, principal.Products);
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
        }

        private static IModel BuildModel()
        {
            var model = new Model();
            var builder = new ConventionModelBuilder(model);

            builder.Entity<Product>();
            builder.Entity<Category>();

            var categoryType = model.GetEntityType(typeof(Category));
            var productType = model.GetEntityType(typeof(Product));

            var categoryIdFk
                = productType.AddForeignKey(categoryType.GetKey(), productType.GetProperty("CategoryId"));

            categoryIdFk.StorageName = "Category_Products";

            categoryType.AddNavigation(new Navigation(categoryIdFk, "Products"));
            productType.AddNavigation(new Navigation(categoryIdFk, "Category"));

            return model;
        }
    }
}
