// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ForeignKeyExtensionsTest
    {
        [Fact]
        public void Gets_root_principals_for_one_to_one_FKs()
        {
            var model = BuildModel();

            Assert.Equal(
                new[] { model.GetEntityType(typeof(Product)).GetProperty("Id") },
                model.GetEntityType(typeof(ProductDetails)).ForeignKeys.Single().GetRootPrincipals(0));

            Assert.Equal(
                new[] { model.GetEntityType(typeof(Product)).GetProperty("Id") },
                model.GetEntityType(typeof(ProductDetailsTag)).ForeignKeys.Single().GetRootPrincipals(0));

            Assert.Equal(
                new[] { model.GetEntityType(typeof(ProductDetails)).GetProperty("Id2") },
                model.GetEntityType(typeof(ProductDetailsTag)).ForeignKeys.Single().GetRootPrincipals(1));

            Assert.Equal(
                new[] { model.GetEntityType(typeof(ProductDetails)).GetProperty("Id2") },
                model.GetEntityType(typeof(ProductDetailsTagDetails)).ForeignKeys.Single().GetRootPrincipals(0));
        }

        [Fact]
        public void Gets_root_principals_for_one_to_many_identifying_FKs()
        {
            var model = BuildModel();

            Assert.Equal(
                new[] { model.GetEntityType(typeof(Order)).GetProperty("Id") },
                model.GetEntityType(typeof(OrderDetails)).ForeignKeys.Single(k => k.Properties.First().Name == "OrderId").GetRootPrincipals(0));

            Assert.Equal(
                new[] { model.GetEntityType(typeof(Product)).GetProperty("Id") },
                model.GetEntityType(typeof(OrderDetails)).ForeignKeys.Single(k => k.Properties.First().Name == "ProductId").GetRootPrincipals(0));
        }

        private class Category
        {
            public int Id { get; set; }

            public List<Product> Products { get; set; }
        }

        private class Product
        {
            public int Id { get; set; }

            public int CategoryId { get; set; }
            public Category Category { get; set; }

            public ProductDetails Details { get; set; }

            public List<OrderDetails> OrderDetails { get; set; }
        }

        private class ProductDetails
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }

            public Product Product { get; set; }

            public ProductDetailsTag Tag { get; set; }
        }

        private class ProductDetailsTag
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }

            public ProductDetails Details { get; set; }

            public ProductDetailsTagDetails TagDetails { get; set; }
        }

        private class ProductDetailsTagDetails
        {
            public int Id { get; set; }

            public ProductDetailsTag Tag { get; set; }
        }

        private class Order
        {
            public int Id { get; set; }

            public List<OrderDetails> OrderDetails { get; set; }
        }

        private class OrderDetails
        {
            public int OrderId { get; set; }
            public int ProductId { get; set; }

            public Order Order { get; set; }
            public Product Product { get; set; }
        }

        private IModel BuildModel()
        {
            var modelBuilder = TestHelpers.Instance.CreateConventionBuilder();

            modelBuilder
                .Entity<Category>()
                .HasMany(e => e.Products)
                .WithOne(e => e.Category);

            modelBuilder
                .Entity<ProductDetailsTag>(b =>
                    {
                        b.Key(e => new { e.Id1, e.Id2 });
                        b.HasOne(e => e.TagDetails)
                            .WithOne(e => e.Tag)
                            .ReferencedKey<ProductDetailsTag>(e => e.Id2)
                            .ForeignKey<ProductDetailsTagDetails>(e => e.Id);
                    });

            modelBuilder
                .Entity<ProductDetails>(b =>
                    {
                        b.Key(e => new { e.Id1, e.Id2 });
                        b.HasOne(e => e.Tag)
                            .WithOne(e => e.Details)
                            .ForeignKey<ProductDetailsTag>(e => new { e.Id1, e.Id2 });
                    });

            modelBuilder
                .Entity<Product>()
                .HasOne(e => e.Details)
                .WithOne(e => e.Product)
                .ForeignKey<ProductDetails>(e => new { e.Id1 });

            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => new { e.OrderId, e.ProductId });
                    b.HasOne(e => e.Order)
                        .WithMany(e => e.OrderDetails)
                        .ForeignKey(e => e.OrderId);
                    b.HasOne(e => e.Product)
                        .WithMany(e => e.OrderDetails)
                        .ForeignKey(e => e.ProductId);
                });

            return modelBuilder.Model;
        }
    }
}
