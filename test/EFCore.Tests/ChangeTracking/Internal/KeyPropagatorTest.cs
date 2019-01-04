// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class KeyPropagatorTest
    {
        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Foreign_key_value_is_obtained_from_reference_to_principal(bool generateTemporary, bool async)
        {
            var model = BuildModel(generateTemporary);

            var principal = new Category
            {
                Id = 11
            };
            var dependent = new Product
            {
                Id = 21,
                Category = principal
            };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
            var property = model.FindEntityType(typeof(Product)).FindProperty("CategoryId");
            var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

            PropagateValue(keyPropagator, dependentEntry, property, async);

            Assert.Equal(11, dependentEntry[property]);
            Assert.False(dependentEntry.HasTemporaryValue(property));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Foreign_key_value_is_obtained_from_tracked_principal_with_populated_collection(bool generateTemporary, bool async)
        {
            var model = BuildModel(generateTemporary);
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal = new Category
            {
                Id = 11
            };
            var dependent = new Product
            {
                Id = 21
            };
            principal.Products.Add(dependent);

            manager.GetOrCreateEntry(principal).SetEntityState(EntityState.Unchanged);
            var dependentEntry = manager.GetOrCreateEntry(dependent);
            var property = model.FindEntityType(typeof(Product)).FindProperty("CategoryId");
            var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

            PropagateValue(keyPropagator, dependentEntry, property, async);

            Assert.Equal(11, dependentEntry[property]);
            Assert.False(dependentEntry.HasTemporaryValue(property));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Non_identifying_foreign_key_value_is_not_generated_if_principal_key_not_set(bool generateTemporary, bool async)
        {
            var model = BuildModel(generateTemporary);

            var principal = new Category();
            var dependent = new Product
            {
                Id = 21,
                Category = principal
            };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
            var property = model.FindEntityType(typeof(Product)).FindProperty("CategoryId");
            var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

            PropagateValue(keyPropagator, dependentEntry, property, async);

            Assert.Equal(0, dependentEntry[property]);
            Assert.False(dependentEntry.HasTemporaryValue(property));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void One_to_one_foreign_key_value_is_obtained_from_reference_to_principal(bool generateTemporary, bool async)
        {
            var model = BuildModel(generateTemporary);

            var principal = new Product
            {
                Id = 21
            };
            var dependent = new ProductDetail
            {
                Product = principal
            };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
            var property = model.FindEntityType(typeof(ProductDetail)).FindProperty("Id");
            var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

            PropagateValue(keyPropagator, dependentEntry, property, async);

            Assert.Equal(21, dependentEntry[property]);
            Assert.False(dependentEntry.HasTemporaryValue(property));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void One_to_one_foreign_key_value_is_obtained_from_tracked_principal(bool generateTemporary, bool async)
        {
            var model = BuildModel(generateTemporary);
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var dependent = new ProductDetail();
            var principal = new Product
            {
                Id = 21,
                Detail = dependent
            };

            manager.GetOrCreateEntry(principal).SetEntityState(EntityState.Unchanged);
            var dependentEntry = manager.GetOrCreateEntry(dependent);
            var property = model.FindEntityType(typeof(ProductDetail)).FindProperty("Id");
            var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

            PropagateValue(keyPropagator, dependentEntry, property, async);

            Assert.Equal(21, dependentEntry[property]);
            Assert.False(dependentEntry.HasTemporaryValue(property));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Identifying_foreign_key_value_is_generated_if_principal_key_not_set(bool generateTemporary, bool async)
        {
            var model = BuildModel(generateTemporary);

            var principal = new Product();
            var dependent = new ProductDetail
            {
                Product = principal
            };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
            var property = model.FindEntityType(typeof(ProductDetail)).FindProperty("Id");
            var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

            PropagateValue(keyPropagator, dependentEntry, property, async);

            Assert.NotEqual(0, dependentEntry[property]);
            Assert.Equal(generateTemporary, dependentEntry.HasTemporaryValue(property));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Identifying_foreign_key_value_is_propagated_if_principal_key_is_generated(bool generateTemporary, bool async)
        {
            var model = BuildModel(generateTemporary);

            var principal = new Product();
            var dependent = new ProductDetail
            {
                Product = principal
            };

            var contextServices = CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var principalEntry = stateManager.GetOrCreateEntry(principal);
            principalEntry.SetEntityState(EntityState.Added);
            var dependentEntry = stateManager.GetOrCreateEntry(dependent);
            var principalProperty = model.FindEntityType(typeof(Product)).FindProperty(nameof(Product.Id));
            var dependentProperty = model.FindEntityType(typeof(ProductDetail)).FindProperty(nameof(ProductDetail.Id));
            var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

            PropagateValue(keyPropagator, dependentEntry, dependentProperty, async);

            Assert.NotEqual(0, principalEntry[principalProperty]);
            Assert.Equal(generateTemporary, principalEntry.HasTemporaryValue(principalProperty));
            Assert.NotEqual(0, dependentEntry[dependentProperty]);
            Assert.Equal(generateTemporary, dependentEntry.HasTemporaryValue(dependentProperty));
            Assert.Equal(principalEntry[principalProperty], dependentEntry[dependentProperty]);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Composite_foreign_key_value_is_obtained_from_reference_to_principal(bool generateTemporary, bool async)
        {
            var model = BuildModel(generateTemporary);

            var principal = new OrderLine
            {
                OrderId = 11,
                ProductId = 21
            };
            var dependent = new OrderLineDetail
            {
                OrderLine = principal
            };

            var contextServices = CreateContextServices(model);
            var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
            var property1 = model.FindEntityType(typeof(OrderLineDetail)).FindProperty("OrderId");
            var property2 = model.FindEntityType(typeof(OrderLineDetail)).FindProperty("ProductId");
            var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

            PropagateValue(keyPropagator, dependentEntry, property1, async);
            PropagateValue(keyPropagator, dependentEntry, property2, async);

            Assert.Equal(11, dependentEntry[property1]);
            Assert.False(dependentEntry.HasTemporaryValue(property1));
            Assert.Equal(21, dependentEntry[property2]);
            Assert.False(dependentEntry.HasTemporaryValue(property1));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Composite_foreign_key_value_is_obtained_from_tracked_principal(bool generateTemporary, bool async)
        {
            var model = BuildModel(generateTemporary);
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var dependent = new OrderLineDetail();
            var principal = new OrderLine
            {
                OrderId = 11,
                ProductId = 21,
                Detail = dependent
            };

            manager.GetOrCreateEntry(principal).SetEntityState(EntityState.Unchanged);
            var dependentEntry = manager.GetOrCreateEntry(dependent);
            var property1 = model.FindEntityType(typeof(OrderLineDetail)).FindProperty("OrderId");
            var property2 = model.FindEntityType(typeof(OrderLineDetail)).FindProperty("ProductId");
            var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

            PropagateValue(keyPropagator, dependentEntry, property1, async);
            PropagateValue(keyPropagator, dependentEntry, property2, async);

            Assert.Equal(11, dependentEntry[property1]);
            Assert.False(dependentEntry.HasTemporaryValue(property1));
            Assert.Equal(21, dependentEntry[property2]);
            Assert.False(dependentEntry.HasTemporaryValue(property1));
        }

        private static IServiceProvider CreateContextServices(IModel model)
            => InMemoryTestHelpers.Instance.CreateContextServices(model);

        private static void PropagateValue(IKeyPropagator keyPropagator, InternalEntityEntry dependentEntry, IProperty property, bool async)
        {
            if (async)
            {
                keyPropagator.PropagateValueAsync(dependentEntry, property).GetAwaiter().GetResult();
            }
            else
            {
                keyPropagator.PropagateValue(dependentEntry, property);
            }
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

        private static IModel BuildModel(bool generateTemporary = false)
        {
            var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<BaseType>();

            builder.Entity<Product>(
                b =>
                {
                    b.HasMany(e => e.OrderLines).WithOne(e => e.Product);
                    b.HasOne(e => e.Detail).WithOne(e => e.Product).HasForeignKey<ProductDetail>(e => e.Id);
                });

            builder.Entity<Category>().HasMany(e => e.Products).WithOne(e => e.Category);

            builder.Entity<ProductDetail>().Property(p => p.Id);

            builder.Entity<Order>().HasMany(e => e.OrderLines).WithOne(e => e.Order);

            builder.Entity<OrderLineDetail>().HasKey(
                e => new
                {
                    e.OrderId,
                    e.ProductId
                });

            builder.Entity<OrderLine>(
                b =>
                {
                    b.HasKey(
                        e => new
                        {
                            e.OrderId,
                            e.ProductId
                        });
                    b.HasOne(e => e.Detail).WithOne(e => e.OrderLine).HasForeignKey<OrderLineDetail>(
                        e => new
                        {
                            e.OrderId,
                            e.ProductId
                        });
                });

            if (generateTemporary)
            {
                foreach (var entityType in builder.Model.GetEntityTypes())
                {
                    foreach (var property in entityType.GetDeclaredProperties())
                    {
                        if (property.ValueGenerated == ValueGenerated.OnAdd)
                        {
                            property.SetValueGeneratorFactory((p, _) => new TemporaryNumberValueGeneratorFactory().Create(p));
                        }
                    }
                }
            }

            return builder.Model;
        }
    }
}
