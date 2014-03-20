// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class FixupTest
    {
        [Fact]
        public void Navigation_fixup_happens_when_new_entities_are_tracked()
        {
            using (var context = new FixupContext())
            {
                context.Add(new Category { Id = 11 });
                context.Add(new Category { Id = 12 });
                context.Add(new Category { Id = 13 });

                context.Add(new Product { Id = 21, CategoryId = 11 });
                AssertAllFixedUp(context);
                context.Add(new Product { Id = 22, CategoryId = 11 });
                AssertAllFixedUp(context);
                context.Add(new Product { Id = 23, CategoryId = 11 });
                AssertAllFixedUp(context);
                context.Add(new Product { Id = 24, CategoryId = 12 });
                AssertAllFixedUp(context);
                context.Add(new Product { Id = 25, CategoryId = 12 });
                AssertAllFixedUp(context);

                context.Add(new SpecialOffer { Id = 31, ProductId = 22 });
                AssertAllFixedUp(context);
                context.Add(new SpecialOffer { Id = 32, ProductId = 22 });
                AssertAllFixedUp(context);
                context.Add(new SpecialOffer { Id = 33, ProductId = 24 });
                AssertAllFixedUp(context);
                context.Add(new SpecialOffer { Id = 34, ProductId = 24 });
                AssertAllFixedUp(context);
                context.Add(new SpecialOffer { Id = 35, ProductId = 24 });
                AssertAllFixedUp(context);

                Assert.Equal(3, context.ChangeTracker.Entries<Category>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Product>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<SpecialOffer>().Count());
            }
        }

        [Fact]
        public void Navigation_fixup_happens_when_entities_are_materialized()
        {
            using (var context = new FixupContext())
            {
                var categoryType = context.Model.GetEntityType(typeof(Category));
                var productType = context.Model.GetEntityType(typeof(Product));
                var offerType = context.Model.GetEntityType(typeof(SpecialOffer));

                var stateManager = context.ChangeTracker.StateManager;

                stateManager.GetOrMaterializeEntry(categoryType, new object[] { 11 });
                stateManager.GetOrMaterializeEntry(categoryType, new object[] { 12 });
                stateManager.GetOrMaterializeEntry(categoryType, new object[] { 13 });


                stateManager.GetOrMaterializeEntry(productType, new object[] { 11, 21 });
                AssertAllFixedUp(context);
                stateManager.GetOrMaterializeEntry(productType, new object[] { 11, 22 });
                AssertAllFixedUp(context);
                stateManager.GetOrMaterializeEntry(productType, new object[] { 11, 23 });
                AssertAllFixedUp(context);
                stateManager.GetOrMaterializeEntry(productType, new object[] { 12, 24 });
                AssertAllFixedUp(context);
                stateManager.GetOrMaterializeEntry(productType, new object[] { 12, 25 });
                AssertAllFixedUp(context);

                stateManager.GetOrMaterializeEntry(offerType, new object[] { 31, 22});
                AssertAllFixedUp(context);
                stateManager.GetOrMaterializeEntry(offerType, new object[] { 32, 22 });
                AssertAllFixedUp(context);
                stateManager.GetOrMaterializeEntry(offerType, new object[] { 33, 24 });
                AssertAllFixedUp(context);
                stateManager.GetOrMaterializeEntry(offerType, new object[] { 34, 24 });
                AssertAllFixedUp(context);
                stateManager.GetOrMaterializeEntry(offerType, new object[] { 35, 24 });
                AssertAllFixedUp(context);

                Assert.Equal(3, context.ChangeTracker.Entries<Category>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Product>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<SpecialOffer>().Count());
            }
        }

        public void AssertAllFixedUp(EntityContext context)
        {
            foreach (var entry in context.ChangeTracker.Entries<Product>())
            {
                var product = entry.Entity;
                if (product.CategoryId == 11
                    || product.CategoryId == 12)
                {
                    Assert.Equal(product.CategoryId, product.Category.Id);
                    Assert.Contains(product, product.Category.Products);
                }
                else
                {
                    Assert.Null(product.Category);
                }
            }

            foreach (var entry in context.ChangeTracker.Entries<SpecialOffer>())
            {
                var offer = entry.Entity;
                if (offer.ProductId == 22
                    || offer.ProductId == 24)
                {
                    Assert.Equal(offer.ProductId, offer.Product.Id);
                    Assert.Contains(offer, offer.Product.SpecialOffers);
                }
                else
                {
                    Assert.Null(offer.Product);
                }
            }
        }

        #region Fixture

        private class Category
        {
            public Category()
            {
                Products = new List<Product>();
            }

            public int Id { get; set; }

            public ICollection<Product> Products { get; set; }
        }

        private class Product
        {
            public Product()
            {
                SpecialOffers = new List<SpecialOffer>();
            }

            public int Id { get; set; }
            public int CategoryId { get; set; }

            public Category Category { get; set; }
            public ICollection<SpecialOffer> SpecialOffers { get; set; }
        }

        private class SpecialOffer
        {
            public int Id { get; set; }
            public int ProductId { get; set; }

            public Product Product { get; set; }
        }

        private class FixupContext : EntityContext
        {
            protected override void OnModelCreating(ModelBuilder builder)
            {
                var model = builder.Model;

                builder.Entity<Product>();
                builder.Entity<Category>();
                builder.Entity<SpecialOffer>();

                new SimpleTemporaryConvention().Apply(model);

                var categoryType = model.GetEntityType(typeof(Category));
                var productType = model.GetEntityType(typeof(Product));
                var offerType = model.GetEntityType(typeof(SpecialOffer));

                var categoryIdFk
                    = productType.AddForeignKey(
                        categoryType.GetKey(), new[] { productType.GetProperty("CategoryId") });
                categoryIdFk.StorageName = "Category_Products";

                var productIdFk
                    = offerType.AddForeignKey(
                        productType.GetKey(), new[] { offerType.GetProperty("ProductId") });
                productIdFk.StorageName = "Product_Offers";

                categoryType.AddNavigation(new CollectionNavigation(categoryIdFk, "Products"));
                productType.AddNavigation(new Navigation(categoryIdFk, "Category"));
                productType.AddNavigation(new CollectionNavigation(productIdFk, "SpecialOffers"));
                offerType.AddNavigation(new Navigation(productIdFk, "Product"));
            }
        }

        #endregion
    }
}
