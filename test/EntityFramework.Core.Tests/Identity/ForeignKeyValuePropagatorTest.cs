// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class ForeignKeyValuePropagatorTest
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Foreign_key_value_is_obtained_from_reference_to_principal(bool async)
        {
            var model = BuildModel();

            var principal = new Category { Id = 11 };
            var dependent = new Product { Id = 21, Category = principal };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(dependent);
            var property = model.GetEntityType(typeof(Product)).GetProperty("CategoryId");

            await PropagateValue(contextServices.GetRequiredService<ForeignKeyValuePropagator>(), dependentEntry, property, async);

            Assert.Equal(11, dependentEntry[property]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Foreign_key_value_is_obtained_from_tracked_principal_with_populated_collection(bool async)
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<StateManager>();

            var principal = new Category { Id = 11 };
            var dependent = new Product { Id = 21 };
            principal.Products.Add(dependent);

            manager.StartTracking(manager.GetOrCreateEntry(principal));
            var dependentEntry = manager.GetOrCreateEntry(dependent);
            var property = model.GetEntityType(typeof(Product)).GetProperty("CategoryId");

            await PropagateValue(contextServices.GetRequiredService<ForeignKeyValuePropagator>(), dependentEntry, property, async);

            Assert.Equal(11, dependentEntry[property]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Non_identifying_foreign_key_value_is_not_generated_if_principal_key_not_set(bool async)
        {
            var model = BuildModel();

            var principal = new Category();
            var dependent = new Product { Id = 21, Category = principal };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(dependent);
            var property = model.GetEntityType(typeof(Product)).GetProperty("CategoryId");

            await PropagateValue(contextServices.GetRequiredService<ForeignKeyValuePropagator>(), dependentEntry, property, async);

            Assert.Equal(0, dependentEntry[property]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task One_to_one_foreign_key_value_is_obtained_from_reference_to_principal(bool async)
        {
            var model = BuildModel();

            var principal = new Product { Id = 21 };
            var dependent = new ProductDetail { Product = principal };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(dependent);
            var property = model.GetEntityType(typeof(ProductDetail)).GetProperty("Id");

            await PropagateValue(contextServices.GetRequiredService<ForeignKeyValuePropagator>(), dependentEntry, property, async);

            Assert.Equal(21, dependentEntry[property]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task One_to_one_foreign_key_value_is_obtained_from_tracked_principal(bool async)
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<StateManager>();

            var dependent = new ProductDetail();
            var principal = new Product { Id = 21, Detail = dependent };

            manager.StartTracking(manager.GetOrCreateEntry(principal));
            var dependentEntry = manager.GetOrCreateEntry(dependent);
            var property = model.GetEntityType(typeof(ProductDetail)).GetProperty("Id");

            await PropagateValue(contextServices.GetRequiredService<ForeignKeyValuePropagator>(), dependentEntry, property, async);

            Assert.Equal(21, dependentEntry[property]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Identifying_foreign_key_value_is_generated_if_principal_key_not_set(bool async)
        {
            var model = BuildModel();

            var principal = new Product();
            var dependent = new ProductDetail { Product = principal };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(dependent);
            var property = model.GetEntityType(typeof(ProductDetail)).GetProperty("Id");

            await PropagateValue(contextServices.GetRequiredService<ForeignKeyValuePropagator>(), dependentEntry, property, async);

            Assert.Equal(1, dependentEntry[property]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Composite_foreign_key_value_is_obtained_from_reference_to_principal(bool async)
        {
            var model = BuildModel();

            var principal = new OrderLine { OrderId = 11, ProductId = 21 };
            var dependent = new OrderLineDetail { OrderLine = principal };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(dependent);
            var property1 = model.GetEntityType(typeof(OrderLineDetail)).GetProperty("OrderId");
            var property2 = model.GetEntityType(typeof(OrderLineDetail)).GetProperty("ProductId");

            var valuePropagator = contextServices.GetRequiredService<ForeignKeyValuePropagator>();
            await PropagateValue(valuePropagator, dependentEntry, property1, async);
            await PropagateValue(valuePropagator, dependentEntry, property2, async);

            Assert.Equal(11, dependentEntry[property1]);
            Assert.Equal(21, dependentEntry[property2]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Composite_foreign_key_value_is_obtained_from_tracked_principal(bool async)
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<StateManager>();

            var dependent = new OrderLineDetail();
            var principal = new OrderLine { OrderId = 11, ProductId = 21, Detail = dependent };

            manager.StartTracking(manager.GetOrCreateEntry(principal));
            var dependentEntry = manager.GetOrCreateEntry(dependent);
            var property1 = model.GetEntityType(typeof(OrderLineDetail)).GetProperty("OrderId");
            var property2 = model.GetEntityType(typeof(OrderLineDetail)).GetProperty("ProductId");

            var valuePropagator = contextServices.GetRequiredService<ForeignKeyValuePropagator>();
            await PropagateValue(valuePropagator, dependentEntry, property1, async);
            await PropagateValue(valuePropagator, dependentEntry, property2, async);

            Assert.Equal(11, dependentEntry[property1]);
            Assert.Equal(21, dependentEntry[property2]);
        }

        private static IServiceProvider CreateContextServices(IModel model = null)
        {
            return TestHelpers.CreateContextServices(model ?? BuildModel());
        }

        private static async Task PropagateValue(
            ForeignKeyValuePropagator valuePropagator,
            StateEntry dependentEntry,
            IProperty property,
            bool async)
        {
            if (async)
            {
                await valuePropagator.PropagateValueAsync(dependentEntry, property);
            }
            else
            {
                valuePropagator.PropagateValue(dependentEntry, property);
            }
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
            var builder = new ModelBuilder(model);

            builder.Entity<Product>(b =>
            {
                b.OneToMany(e => e.OrderLines, e => e.Product);
                b.OneToOne(e => e.Detail, e => e.Product);
            });

            builder.Entity<Category>().OneToMany(e => e.Products, e => e.Category);

            builder.Entity<ProductDetail>();

            builder.Entity<Order>().OneToMany(e => e.OrderLines, e => e.Order);

            builder.Entity<OrderLineDetail>().Key(e => new { e.OrderId, e.ProductId });

            builder.Entity<OrderLine>(b =>
            {
                b.Key(e => new { e.OrderId, e.ProductId });
                b.OneToOne(e => e.Detail, e => e.OrderLine);
            });

            return model;
        }
    }
}
