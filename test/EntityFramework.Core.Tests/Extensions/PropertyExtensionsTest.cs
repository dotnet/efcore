// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class PropertyExtensionsTest
    {
        [Fact]
        public void Get_generation_property_returns_null_for_property_without_generator()
        {
            var model = new Model();

            var entityType = new EntityType("Entity", model);
            var property = entityType.AddProperty("Property", typeof(int));

            Assert.Null(property.GetGenerationProperty());
        }

        [Fact]
        public void Get_generation_property_returns_same_property_on_property_with_generator()
        {
            var model = new Model();

            var entityType = new EntityType("Entity", model);
            var property = entityType.AddProperty("Property", typeof(int));

            property.RequiresValueGenerator = true;

            Assert.Equal(property, property.GetGenerationProperty());
        }

        [Fact]
        public void Get_generation_property_returns_generation_property_from_foreign_key_chain()
        {
            var model = new Model();

            var firstType = new EntityType("First", model);
            var firstProperty = firstType.AddProperty("ID", typeof(int));
            var firstKey = firstType.AddKey(firstProperty);

            var secondType = new EntityType("Second", model);
            var secondProperty = secondType.AddProperty("ID", typeof(int));
            var secondKey = secondType.AddKey(secondProperty);
            secondType.AddForeignKey(secondProperty, firstKey, firstType);

            var thirdType = new EntityType("Third", model);
            var thirdProperty = thirdType.AddProperty("ID", typeof(int));
            thirdType.AddForeignKey(thirdProperty, secondKey, secondType);

            firstProperty.RequiresValueGenerator = true;

            Assert.Equal(firstProperty, thirdProperty.GetGenerationProperty());
        }

        [Fact]
        public void Get_generation_property_returns_generation_property_from_foreign_key_tree()
        {
            var model = new Model();

            var leftType = new EntityType("Left", model);
            var leftId = leftType.AddProperty("Id", typeof(int));
            var leftKey = leftType.AddKey(leftId);

            var rightType = new EntityType("Right", model);
            var rightId1 = rightType.AddProperty("Id1", typeof(int));
            var rightId2 = rightType.AddProperty("Id2", typeof(int));
            var rightKey = rightType.AddKey(new[] { rightId1, rightId2 });

            var middleType = new EntityType("Middle", model);
            var middleProperty1 = middleType.AddProperty("FK1", typeof(int));
            var middleProperty2 = middleType.AddProperty("FK2", typeof(int));
            var middleKey1 = middleType.AddKey(middleProperty1);
            middleType.AddForeignKey(middleProperty1, leftKey, leftType);
            middleType.AddForeignKey(new[] { middleProperty2, middleProperty1 }, rightKey, rightType);

            var endType = new EntityType("End", model);
            var endProperty = endType.AddProperty("FK", typeof(int));

            endType.AddForeignKey(endProperty, middleKey1, middleType);

            rightId2.RequiresValueGenerator = true;

            Assert.Equal(rightId2, endProperty.GetGenerationProperty());
        }

        [Fact]
        public void Get_generation_property_returns_generation_property_from_foreign_key_graph_with_cycle()
        {
            var model = new Model();

            var leafType = new EntityType("leaf", model);
            var leafId1 = leafType.AddProperty("Id1", typeof(int));
            var leafId2 = leafType.AddProperty("Id2", typeof(int));
            var leafKey = leafType.AddKey(new[] { leafId1, leafId2 });

            var firstType = new EntityType("First", model);
            var firstId = firstType.AddProperty("Id", typeof(int));
            var firstKey = firstType.AddKey(firstId);
            
            var secondType = new EntityType("Second", model);
            var secondId1 = secondType.AddProperty("Id1", typeof(int));
            var secondId2 = secondType.AddProperty("Id2", typeof(int));
            var secondKey = secondType.AddKey(secondId1);

            firstType.AddForeignKey(firstId, secondKey, secondType);
            secondType.AddForeignKey(secondId1, firstKey, firstType);
            secondType.AddForeignKey(new[] { secondId1, secondId2 }, leafKey, leafType);

            leafId1.RequiresValueGenerator = true;

            Assert.Equal(leafId1, secondId1.GetGenerationProperty());
        }

        [Fact]
        public void Get_generation_property_for_one_to_one_FKs()
        {
            var model = BuildModel();

            Assert.Equal(
                model.GetEntityType(typeof(Product)).GetProperty("Id"),
                model.GetEntityType(typeof(ProductDetails)).GetForeignKeys().Single().Properties[0].GetGenerationProperty());

            Assert.Equal(
                model.GetEntityType(typeof(Product)).GetProperty("Id"),
                model.GetEntityType(typeof(ProductDetailsTag)).GetForeignKeys().Single().Properties[0].GetGenerationProperty());

            Assert.Equal(
                model.GetEntityType(typeof(ProductDetails)).GetProperty("Id2"),
                model.GetEntityType(typeof(ProductDetailsTag)).GetForeignKeys().Single().Properties[1].GetGenerationProperty());

            Assert.Equal(
                model.GetEntityType(typeof(ProductDetails)).GetProperty("Id2"),
                model.GetEntityType(typeof(ProductDetailsTagDetails)).GetForeignKeys().Single().Properties[0].GetGenerationProperty());
        }

        [Fact]
        public void Get_generation_property_for_one_to_many_identifying_FKs()
        {
            var model = BuildModel();

            Assert.Equal(
                model.GetEntityType(typeof(Order)).GetProperty("Id"),
                model.GetEntityType(typeof(OrderDetails)).GetForeignKeys().Single(k => k.Properties.First().Name == "OrderId").Properties[0].GetGenerationProperty());

            Assert.Equal(
                model.GetEntityType(typeof(Product)).GetProperty("Id"),
                model.GetEntityType(typeof(OrderDetails)).GetForeignKeys().Single(k => k.Properties.First().Name == "ProductId").Properties[0].GetGenerationProperty());
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
                    b.HasKey(e => new { e.Id1, e.Id2 });
                    b.HasOne(e => e.TagDetails)
                        .WithOne(e => e.Tag)
                        .HasPrincipalKey<ProductDetailsTag>(e => e.Id2)
                        .HasForeignKey<ProductDetailsTagDetails>(e => e.Id);
                });

            modelBuilder
                .Entity<ProductDetails>(b =>
                {
                    b.HasKey(e => new { e.Id1, e.Id2 });
                    b.HasOne(e => e.Tag)
                        .WithOne(e => e.Details)
                        .HasForeignKey<ProductDetailsTag>(e => new { e.Id1, e.Id2 });
                });

            modelBuilder
                .Entity<Product>()
                .HasOne(e => e.Details)
                .WithOne(e => e.Product)
                .HasForeignKey<ProductDetails>(e => new { e.Id1 });

            modelBuilder.Entity<OrderDetails>(b =>
            {
                b.HasKey(e => new { e.OrderId, e.ProductId });
                b.HasOne(e => e.Order)
                    .WithMany(e => e.OrderDetails)
                    .HasForeignKey(e => e.OrderId);
                b.HasOne(e => e.Product)
                    .WithMany(e => e.OrderDetails)
                    .HasForeignKey(e => e.ProductId);
            });

            return modelBuilder.Model;
        }
    }
}
