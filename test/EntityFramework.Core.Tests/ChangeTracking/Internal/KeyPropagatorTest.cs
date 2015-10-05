// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class KeyPropagatorTest
    {
        [Fact]
        public void Foreign_key_value_is_obtained_from_reference_to_principal()
        {
            var model = BuildModel();

            var principal = new Category { Id = 11 };
            var dependent = new Product { Id = 21, Category = principal };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
            var property = model.GetEntityType(typeof(Product)).GetProperty("CategoryId");

            PropagateValue(contextServices.GetRequiredService<IKeyPropagator>(), dependentEntry, property);

            Assert.Equal(11, dependentEntry[property]);
        }

        [Fact]
        public void Foreign_key_value_is_obtained_from_tracked_principal_with_populated_collection()
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal = new Category { Id = 11 };
            var dependent = new Product { Id = 21 };
            principal.Products.Add(dependent);

            manager.StartTracking(manager.GetOrCreateEntry(principal));
            var dependentEntry = manager.GetOrCreateEntry(dependent);
            var property = model.GetEntityType(typeof(Product)).GetProperty("CategoryId");

            PropagateValue(contextServices.GetRequiredService<IKeyPropagator>(), dependentEntry, property);

            Assert.Equal(11, dependentEntry[property]);
        }

        [Fact]
        public void Non_identifying_foreign_key_value_is_not_generated_if_principal_key_not_set()
        {
            var model = BuildModel();

            var principal = new Category();
            var dependent = new Product { Id = 21, Category = principal };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
            var property = model.GetEntityType(typeof(Product)).GetProperty("CategoryId");

            PropagateValue(contextServices.GetRequiredService<IKeyPropagator>(), dependentEntry, property);

            Assert.Equal(0, dependentEntry[property]);
        }

        [Fact]
        public void One_to_one_foreign_key_value_is_obtained_from_reference_to_principal()
        {
            var model = BuildModel();

            var principal = new Product { Id = 21 };
            var dependent = new ProductDetail { Product = principal };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
            var property = model.GetEntityType(typeof(ProductDetail)).GetProperty("Id");

            PropagateValue(contextServices.GetRequiredService<IKeyPropagator>(), dependentEntry, property);

            Assert.Equal(21, dependentEntry[property]);
        }

        [Fact]
        public void One_to_one_foreign_key_value_is_obtained_from_tracked_principal()
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var dependent = new ProductDetail();
            var principal = new Product { Id = 21, Detail = dependent };

            manager.StartTracking(manager.GetOrCreateEntry(principal));
            var dependentEntry = manager.GetOrCreateEntry(dependent);
            var property = model.GetEntityType(typeof(ProductDetail)).GetProperty("Id");

            PropagateValue(contextServices.GetRequiredService<IKeyPropagator>(), dependentEntry, property);

            Assert.Equal(21, dependentEntry[property]);
        }

        [Fact]
        public void Identifying_foreign_key_value_is_generated_if_principal_key_not_set()
        {
            var model = BuildModel();

            var principal = new Product();
            var dependent = new ProductDetail { Product = principal };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
            var property = model.GetEntityType(typeof(ProductDetail)).GetProperty("Id");

            PropagateValue(contextServices.GetRequiredService<IKeyPropagator>(), dependentEntry, property);

            Assert.Equal(1, dependentEntry[property]);
        }

        [Fact]
        public void Composite_foreign_key_value_is_obtained_from_reference_to_principal()
        {
            var model = BuildModel();

            var principal = new OrderLine { OrderId = 11, ProductId = 21 };
            var dependent = new OrderLineDetail { OrderLine = principal };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
            var property1 = model.GetEntityType(typeof(OrderLineDetail)).GetProperty("OrderId");
            var property2 = model.GetEntityType(typeof(OrderLineDetail)).GetProperty("ProductId");

            var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();
            PropagateValue(keyPropagator, dependentEntry, property1);
            PropagateValue(keyPropagator, dependentEntry, property2);

            Assert.Equal(11, dependentEntry[property1]);
            Assert.Equal(21, dependentEntry[property2]);
        }

        [Fact]
        public void Composite_foreign_key_value_is_obtained_from_tracked_principal()
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var dependent = new OrderLineDetail();
            var principal = new OrderLine { OrderId = 11, ProductId = 21, Detail = dependent };

            manager.StartTracking(manager.GetOrCreateEntry(principal));
            var dependentEntry = manager.GetOrCreateEntry(dependent);
            var property1 = model.GetEntityType(typeof(OrderLineDetail)).GetProperty("OrderId");
            var property2 = model.GetEntityType(typeof(OrderLineDetail)).GetProperty("ProductId");

            var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();
            PropagateValue(keyPropagator, dependentEntry, property1);
            PropagateValue(keyPropagator, dependentEntry, property2);

            Assert.Equal(11, dependentEntry[property1]);
            Assert.Equal(21, dependentEntry[property2]);
        }

        private static IServiceProvider CreateContextServices(IModel model = null)
        {
            return TestHelpers.Instance.CreateContextServices(model ?? BuildModel());
        }

        private static void PropagateValue(IKeyPropagator keyPropagator, InternalEntityEntry dependentEntry, IProperty property)
        {
            keyPropagator.PropagateValue(dependentEntry, property);
        }

        private class BaseType
        {
            public int Id { get; set; }
        }

        private class Category : BaseType
        {
            public ICollection<Product> Products { get; } = new List<Product>();
        }

        private class Product : BaseType
        {
            public int CategoryId { get; set; }

            public Category Category { get; set; }

            public ProductDetail Detail { get; set; }

            public ICollection<OrderLine> OrderLines { get; } = new List<OrderLine>();
        }

        private class ProductDetail
        {
            public int Id { get; set; }
            public Product Product { get; set; }
        }

        private class Order : BaseType
        {
            public ICollection<OrderLine> OrderLines { get; } = new List<OrderLine>();
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
            var builder = TestHelpers.Instance.CreateConventionBuilder(model);

            builder.Entity<BaseType>();

            builder.Entity<Product>(b =>
                {
                    b.HasMany(e => e.OrderLines).WithOne(e => e.Product);
                    b.HasOne(e => e.Detail).WithOne(e => e.Product).HasForeignKey<ProductDetail>(e => e.Id);
                });

            builder.Entity<Category>().HasMany(e => e.Products).WithOne(e => e.Category);

            builder.Entity<ProductDetail>();

            builder.Entity<Order>().HasMany(e => e.OrderLines).WithOne(e => e.Order);

            builder.Entity<OrderLineDetail>().HasKey(e => new { e.OrderId, e.ProductId });

            builder.Entity<OrderLine>(b =>
                {
                    b.HasKey(e => new { e.OrderId, e.ProductId });
                    b.HasOne(e => e.Detail).WithOne(e => e.OrderLine);
                });

            return model;
        }
    }
}
