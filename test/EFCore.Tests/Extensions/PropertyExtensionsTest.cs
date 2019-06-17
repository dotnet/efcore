// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public class PropertyExtensionsTest
    {
        [ConditionalFact]
        public virtual void Asking_for_type_mapping_before_finalize_throws()
        {
            var model = CreateModel();

            var entityType = model.AddEntityType("Entity");
            var property = entityType.AddProperty("Property", typeof(int));

            Assert.Equal(
                CoreStrings.ModelNotFinalized(nameof(PropertyExtensions.GetTypeMapping)),
                Assert.Throws<InvalidOperationException>(
                    () => property.GetTypeMapping()).Message);
        }

        [ConditionalFact]
        public virtual void Properties_can_have_store_type_set()
        {
            var model = CreateModel();

            var entityType = model.AddEntityType("Entity");
            var property = entityType.AddProperty("Property", typeof(int));

            Assert.Null(property.GetProviderClrType());

            property.SetProviderClrType(typeof(long));
            Assert.Same(typeof(long), property.GetProviderClrType());

            property.SetProviderClrType(null);
            Assert.Null(property.GetProviderClrType());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_value_converter_set()
        {
            var model = CreateModel();

            var entityType = model.AddEntityType("Entity");
            var property = entityType.AddProperty("Property", typeof(int));
            var converter = CastingConverter<int, decimal>.DefaultInfo.Create();

            Assert.Null(property.GetValueConverter());

            property.SetValueConverter(converter);
            Assert.Same(converter, property.GetValueConverter());

            property.SetValueConverter(null);
            Assert.Null(property.GetValueConverter());
        }

        [ConditionalFact]
        public virtual void Value_converter_type_is_checked()
        {
            var model = CreateModel();

            var entityType = model.AddEntityType("Entity");
            var property1 = entityType.AddProperty("Property1", typeof(int));
            var property2 = entityType.AddProperty("Property2", typeof(int?));

            property1.SetValueConverter(new CastingConverter<int, decimal>());
            property1.SetValueConverter(new CastingConverter<int?, decimal>());
            property2.SetValueConverter(new CastingConverter<int, decimal>());
            property2.SetValueConverter(new CastingConverter<int?, decimal>());

            Assert.Equal(
                CoreStrings.ConverterPropertyMismatch("long", "Entity", "Property1", "int"),
                Assert.Throws<InvalidOperationException>(
                    () => property1.SetValueConverter(new CastingConverter<long, decimal>())).Message);
        }

        [ConditionalFact]
        public void Get_generation_property_returns_null_for_property_without_generator()
        {
            var model = CreateModel();

            var entityType = model.AddEntityType("Entity");
            var property = entityType.AddProperty("Property", typeof(int));

            Assert.Null(property.GetGenerationProperty());
        }

        [ConditionalFact]
        public void Get_generation_property_returns_same_property_on_property_with_generator()
        {
            var model = CreateModel();

            var entityType = model.AddEntityType("Entity");
            var property = entityType.AddProperty("Property", typeof(int));
            entityType.AddKey(property);

            property.ValueGenerated = ValueGenerated.OnAdd;

            Assert.Equal(property, property.GetGenerationProperty());
        }

        [ConditionalFact]
        public void Get_generation_property_returns_generation_property_from_foreign_key_chain()
        {
            var model = CreateModel();

            var firstType = model.AddEntityType("First");
            var firstProperty = firstType.AddProperty("ID", typeof(int));
            var firstKey = firstType.AddKey(firstProperty);

            var secondType = model.AddEntityType("Second");
            var secondProperty = secondType.AddProperty("ID", typeof(int));
            var secondKey = secondType.AddKey(secondProperty);
            secondType.AddForeignKey(secondProperty, firstKey, firstType);

            var thirdType = model.AddEntityType("Third");
            var thirdProperty = thirdType.AddProperty("ID", typeof(int));
            thirdType.AddForeignKey(thirdProperty, secondKey, secondType);

            firstProperty.ValueGenerated = ValueGenerated.OnAdd;

            Assert.Equal(firstProperty, thirdProperty.GetGenerationProperty());
        }

        [ConditionalFact]
        public void Get_generation_property_returns_generation_property_from_foreign_key_tree()
        {
            var model = CreateModel();

            var leftType = model.AddEntityType("Left");
            var leftId = leftType.AddProperty("Id", typeof(int));
            var leftKey = leftType.AddKey(leftId);

            var rightType = model.AddEntityType("Right");
            var rightId1 = rightType.AddProperty("Id1", typeof(int));
            var rightId2 = rightType.AddProperty("Id2", typeof(int));
            var rightKey = rightType.AddKey(new[] { rightId1, rightId2 });

            var middleType = model.AddEntityType("Middle");
            var middleProperty1 = middleType.AddProperty("FK1", typeof(int));
            var middleProperty2 = middleType.AddProperty("FK2", typeof(int));
            var middleKey1 = middleType.AddKey(middleProperty1);
            middleType.AddForeignKey(middleProperty1, leftKey, leftType);
            middleType.AddForeignKey(new[] { middleProperty2, middleProperty1 }, rightKey, rightType);

            var endType = model.AddEntityType("End");
            var endProperty = endType.AddProperty("FK", typeof(int));

            endType.AddForeignKey(endProperty, middleKey1, middleType);

            rightId2.ValueGenerated = ValueGenerated.OnAdd;

            Assert.Equal(rightId2, endProperty.GetGenerationProperty());
        }

        [ConditionalFact]
        public void Get_generation_property_returns_generation_property_from_foreign_key_graph_with_cycle()
        {
            var model = CreateModel();

            var leafType = model.AddEntityType("leaf");
            var leafId1 = leafType.AddProperty("Id1", typeof(int));
            var leafId2 = leafType.AddProperty("Id2", typeof(int));
            var leafKey = leafType.AddKey(new[] { leafId1, leafId2 });

            var firstType = model.AddEntityType("First");
            var firstId = firstType.AddProperty("Id", typeof(int));
            var firstKey = firstType.AddKey(firstId);

            var secondType = model.AddEntityType("Second");
            var secondId1 = secondType.AddProperty("Id1", typeof(int));
            var secondId2 = secondType.AddProperty("Id2", typeof(int));
            var secondKey = secondType.AddKey(secondId1);

            firstType.AddForeignKey(firstId, secondKey, secondType);
            secondType.AddForeignKey(secondId1, firstKey, firstType);
            secondType.AddForeignKey(new[] { secondId1, secondId2 }, leafKey, leafType);

            leafId1.ValueGenerated = ValueGenerated.OnAdd;

            Assert.Equal(leafId1, secondId1.GetGenerationProperty());
        }

        [ConditionalFact]
        public void Get_generation_property_for_one_to_one_FKs()
        {
            var model = BuildModel();

            Assert.Equal(
                model.FindEntityType(typeof(Product)).FindProperty("Id"),
                model.FindEntityType(typeof(ProductDetails)).GetForeignKeys().Single().Properties[0].GetGenerationProperty());

            Assert.Equal(
                model.FindEntityType(typeof(Product)).FindProperty("Id"),
                model.FindEntityType(typeof(ProductDetailsTag)).GetForeignKeys().Single().Properties[0].GetGenerationProperty());

            Assert.Equal(
                model.FindEntityType(typeof(ProductDetails)).FindProperty("Id2"),
                model.FindEntityType(typeof(ProductDetailsTag)).GetForeignKeys().Single().Properties[1].GetGenerationProperty());

            Assert.Equal(
                model.FindEntityType(typeof(ProductDetails)).FindProperty("Id2"),
                model.FindEntityType(typeof(ProductDetailsTagDetails)).GetForeignKeys().Single().Properties[0].GetGenerationProperty());
        }

        [ConditionalFact]
        public void Get_generation_property_for_one_to_many_identifying_FKs()
        {
            var model = BuildModel();

            Assert.Equal(
                model.FindEntityType(typeof(Order)).FindProperty("Id"),
                model.FindEntityType(typeof(OrderDetails)).GetForeignKeys().Single(k => k.Properties.First().Name == "OrderId")
                    .Properties[0].GetGenerationProperty());

            Assert.Equal(
                model.FindEntityType(typeof(Product)).FindProperty("Id"),
                model.FindEntityType(typeof(OrderDetails)).GetForeignKeys().Single(k => k.Properties.First().Name == "ProductId")
                    .Properties[0].GetGenerationProperty());
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

        private static IMutableModel CreateModel() => new Model();

        private IMutableModel BuildModel()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder
                .Entity<Category>()
                .HasMany(e => e.Products)
                .WithOne(e => e.Category);

            modelBuilder
                .Entity<ProductDetailsTag>(
                    b =>
                    {
                        b.HasKey(
                            e => new
                            {
                                e.Id1,
                                e.Id2
                            });
                        b.HasOne(e => e.TagDetails)
                            .WithOne(e => e.Tag)
                            .HasPrincipalKey<ProductDetailsTag>(e => e.Id2)
                            .HasForeignKey<ProductDetailsTagDetails>(e => e.Id);
                    });

            modelBuilder
                .Entity<ProductDetails>(
                    b =>
                    {
                        b.HasKey(
                            e => new
                            {
                                e.Id1,
                                e.Id2
                            });
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                        b.HasOne(e => e.Tag)
                            .WithOne(e => e.Details)
                            .HasForeignKey<ProductDetailsTag>(
                                e => new
                                {
                                    e.Id1,
                                    e.Id2
                                });
                    });

            modelBuilder
                .Entity<Product>()
                .HasOne(e => e.Details)
                .WithOne(e => e.Product)
                .HasForeignKey<ProductDetails>(
                    e => new
                    {
                        e.Id1
                    });

            modelBuilder.Entity<OrderDetails>(
                b =>
                {
                    b.HasKey(
                        e => new
                        {
                            e.OrderId,
                            e.ProductId
                        });
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
