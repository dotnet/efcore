// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Identity
{
    public class ForeignKeyValueGeneratorTest
    {
        [Fact]
        public void Foreign_key_value_is_obtained_from_reference_to_principal()
        {
            var model = BuildModel();

            var principal = new Category { Id = 11 };
            var dependent = new Product { Id = 21, Category = principal };

            var dependentEntry = CreateContextConfiguration(model).StateManager.GetOrCreateEntry(dependent);

            Assert.Equal(11, CreateValueGenerator().Next(dependentEntry, model.GetEntityType(typeof(Product)).GetProperty("CategoryId")));
        }

        [Fact]
        public void Foreign_key_value_is_obtained_from_tracked_principal_with_populated_collection()
        {
            var model = BuildModel();
            var manager = CreateContextConfiguration(model).StateManager;

            var principal = new Category { Id = 11 };
            var dependent = new Product { Id = 21 };
            principal.Products.Add(dependent);

            manager.StartTracking(manager.GetOrCreateEntry(principal));
            var dependentEntry = manager.GetOrCreateEntry(dependent);

            Assert.Equal(11, CreateValueGenerator().Next(dependentEntry, model.GetEntityType(typeof(Product)).GetProperty("CategoryId")));
        }

        [Fact]
        public void One_to_one_foreign_key_value_is_obtained_from_reference_to_principal()
        {
            var model = BuildModel();

            var principal = new Product { Id = 21 };
            var dependent = new ProductDetail { Product = principal };

            var dependentEntry = CreateContextConfiguration(model).StateManager.GetOrCreateEntry(dependent);

            Assert.Equal(21, CreateValueGenerator().Next(dependentEntry, model.GetEntityType(typeof(ProductDetail)).GetProperty("Id")));
        }

        [Fact]
        public void One_to_one_foreign_key_value_is_obtained_from_tracked_principal()
        {
            var model = BuildModel();
            var manager = CreateContextConfiguration(model).StateManager;

            var dependent = new ProductDetail();
            var principal = new Product { Id = 21, Detail = dependent };

            manager.StartTracking(manager.GetOrCreateEntry(principal));
            var dependentEntry = manager.GetOrCreateEntry(dependent);

            Assert.Equal(21, CreateValueGenerator().Next(dependentEntry, model.GetEntityType(typeof(ProductDetail)).GetProperty("Id")));
        }

        [Fact]
        public void Composite_foreign_key_value_is_obtained_from_reference_to_principal()
        {
            var model = BuildModel();

            var principal = new OrderLine { OrderId = 11, ProductId = 21 };
            var dependent = new OrderLineDetail { OrderLine = principal };

            var dependentEntry = CreateContextConfiguration(model).StateManager.GetOrCreateEntry(dependent);

            Assert.Equal(11, CreateValueGenerator().Next(dependentEntry, model.GetEntityType(typeof(OrderLineDetail)).GetProperty("OrderId")));
            Assert.Equal(21, CreateValueGenerator().Next(dependentEntry, model.GetEntityType(typeof(OrderLineDetail)).GetProperty("ProductId")));
        }

        [Fact]
        public void Composite_foreign_key_value_is_obtained_from_tracked_principal()
        {
            var model = BuildModel();
            var manager = CreateContextConfiguration(model).StateManager;

            var dependent = new OrderLineDetail();
            var principal = new OrderLine { OrderId = 11, ProductId = 21, Detail = dependent };

            manager.StartTracking(manager.GetOrCreateEntry(principal));
            var dependentEntry = manager.GetOrCreateEntry(dependent);

            Assert.Equal(11, CreateValueGenerator().Next(dependentEntry, model.GetEntityType(typeof(OrderLineDetail)).GetProperty("OrderId")));
            Assert.Equal(21, CreateValueGenerator().Next(dependentEntry, model.GetEntityType(typeof(OrderLineDetail)).GetProperty("ProductId")));
        }

        private static DbContextConfiguration CreateContextConfiguration(IModel model = null)
        {
            return TestHelpers.CreateContextConfiguration(model ?? BuildModel());
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
            private readonly ICollection<OrderLine> _orderLines = new List<OrderLine>();

            public int Id { get; set; }

            public int CategoryId { get; set; }
            public Category Category { get; set; }

            public ProductDetail Detail { get; set; }

            public ICollection<OrderLine> OrderLines
            {
                get { return _orderLines; }
            }
        }

        private class ProductDetail
        {
            public int Id { get; set; }

            public Product Product { get; set; }
        }

        private class Order
        {
            private readonly ICollection<OrderLine> _orderLines = new List<OrderLine>();

            public int Id { get; set; }

            public ICollection<OrderLine> OrderLines
            {
                get { return _orderLines; }
            }
        }

        private class OrderLine
        {
            public int OrderId { get; set; }
            public int ProductId { get; set; }

            public virtual Order Order { get; set; }
            public virtual Product Product { get; set; }

            public virtual OrderLineDetail Detail { get; set; }
        }

        private class OrderLineDetail
        {
            public int OrderId { get; set; }
            public int ProductId { get; set; }

            public virtual OrderLine OrderLine { get; set; }
        }

        private static IModel BuildModel()
        {
            var model = new Model();
            var builder = new ConventionModelBuilder(model);

            builder.Entity<Product>().OneToMany(e => e.OrderLines, e => e.Product);
            builder.Entity<Category>().OneToMany(e => e.Products, e => e.Category);
            builder.Entity<ProductDetail>();
            builder.Entity<Order>().OneToMany(e => e.OrderLines, e => e.Order);
            builder.Entity<OrderLine>().Key(e => new { e.OrderId, e.ProductId });
            builder.Entity<OrderLineDetail>().Key(e => new { e.OrderId, e.ProductId });

            var productType = model.GetEntityType(typeof(Product));
            var productDetailType = model.GetEntityType(typeof(ProductDetail));
            var orderLineType = model.GetEntityType(typeof(OrderLine));
            var orderLineDetailType = model.GetEntityType(typeof(OrderLineDetail));

            var productDetailFk = productDetailType.AddForeignKey(productType.GetKey(), productDetailType.GetProperty("Id"));
            productDetailFk.IsUnique = true;

            var orderLineFk = orderLineDetailType.AddForeignKey(orderLineType.GetKey(), orderLineDetailType.GetProperty("OrderId"), orderLineDetailType.GetProperty("ProductId"));
            orderLineFk.IsUnique = true;

            productDetailType.AddNavigation(new Navigation(productDetailFk, "Product", pointsToPrincipal: true));
            productType.AddNavigation(new Navigation(productDetailFk, "Detail", pointsToPrincipal: false));

            orderLineType.AddNavigation(new Navigation(orderLineFk, "Detail", pointsToPrincipal: false));
            orderLineDetailType.AddNavigation(new Navigation(orderLineFk, "OrderLine", pointsToPrincipal: true));

            return model;
        }

        private static ForeignKeyValueGenerator CreateValueGenerator()
        {
            return new ForeignKeyValueGenerator(new ClrPropertyGetterSource(), new ClrCollectionAccessorSource());
        }
    }
}
