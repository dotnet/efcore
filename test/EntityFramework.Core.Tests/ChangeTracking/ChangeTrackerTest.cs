// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class ChangeTrackerTest
    {
        [Fact]
        public void Can_get_all_entries()
        {
            using (var context = new EarlyLearningCenter())
            {
                var category = context.Add(new Category()).Entity;
                var product = context.Add(new Product()).Entity;

                Assert.Equal(
                    new object[] { category, product },
                    context.ChangeTracker.Entries().Select(e => e.Entity).OrderBy(e => e.GetType().Name));
            }
        }

        [Fact]
        public void Can_get_all_entities_for_an_entity_of_a_given_type()
        {
            using (var context = new EarlyLearningCenter())
            {
                var category = context.Add(new Category()).Entity;
                var product = context.Add(new Product()).Entity;

                Assert.Equal(
                    new object[] { product },
                    context.ChangeTracker.Entries<Product>().Select(e => e.Entity).OrderBy(e => e.GetType().Name));

                Assert.Equal(
                    new object[] { category },
                    context.ChangeTracker.Entries<Category>().Select(e => e.Entity).OrderBy(e => e.GetType().Name));

                Assert.Equal(
                    new object[] { category, product },
                    context.ChangeTracker.Entries<object>().Select(e => e.Entity).OrderBy(e => e.GetType().Name));
            }
        }

        [Fact]
        public void Can_get_state_manager()
        {
            using (var context = new EarlyLearningCenter())
            {
                var stateManger = ((IDbContextServices)context).ScopedServiceProvider.GetRequiredService<StateManager>();

                Assert.Same(stateManger, context.ChangeTracker.StateManager);
            }
        }

        [Fact]
        public void Can_get_Context()
        {
            using (var context = new EarlyLearningCenter())
            {
                Assert.Same(context, context.ChangeTracker.Context);
            }
        }

        [Fact]
        public void Can_attach_parent_with_child_collection()
        {
            using (var context = new EarlyLearningCenter())
            {
                var category = new Category
                    {
                        Id = 1,
                        Products = new List<Product>
                            {
                                new Product { Id = 1 },
                                new Product { Id = 2 },
                                new Product { Id = 3 }
                            }
                    };

                context.ChangeTracker.AttachGraph(category, e => e.SetState(EntityState.Modified));

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Modified, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(category.Products[0]).State);
                Assert.Equal(EntityState.Modified, context.Entry(category.Products[1]).State);
                Assert.Equal(EntityState.Modified, context.Entry(category.Products[2]).State);

                Assert.Same(category, category.Products[0].Category);
                Assert.Same(category, category.Products[1].Category);
                Assert.Same(category, category.Products[2].Category);

                Assert.Equal(category.Id, category.Products[0].CategoryId);
                Assert.Equal(category.Id, category.Products[1].CategoryId);
                Assert.Equal(category.Id, category.Products[2].CategoryId);
            }
        }

        [Fact]
        public void Can_attach_child_with_reference_to_parent()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product = new Product { Id = 1, Category = new Category { Id = 1 } };

                context.ChangeTracker.AttachGraph(product, e => e.SetState(EntityState.Modified));

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Modified, context.Entry(product).State);
                Assert.Equal(EntityState.Modified, context.Entry(product.Category).State);

                Assert.Same(product, product.Category.Products[0]);
                Assert.Equal(product.Category.Id, product.CategoryId);
            }
        }

        [Fact]
        public void Can_attach_parent_with_one_to_one_child()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product = new Product { Id = 1, Details = new ProductDetails { Id = 1 } };

                context.ChangeTracker.AttachGraph(product, e => e.SetState(EntityState.Unchanged));

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product.Details).State);

                Assert.Same(product, product.Details.Product);
            }
        }

        [Fact]
        public void Can_attach_child_with_one_to_one_parent()
        {
            using (var context = new EarlyLearningCenter())
            {
                var details = new ProductDetails { Id = 1, Product = new Product { Id = 1 } };

                context.ChangeTracker.AttachGraph(details, e => e.SetState(EntityState.Unchanged));

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Unchanged, context.Entry(details).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(details.Product).State);

                Assert.Same(details, details.Product.Details);
            }
        }

        [Fact]
        public void Entities_that_are_already_tracked_will_not_get_attached()
        {
            using (var context = new EarlyLearningCenter())
            {
                var existingProduct = context.Attach(new Product { Id = 2, CategoryId = 1 }).Entity;

                var category = new Category
                {
                    Id = 1,
                    Products = new List<Product>
                            {
                                new Product { Id = 1 },
                                existingProduct,
                                new Product { Id = 3 }
                            }
                };

                context.ChangeTracker.AttachGraph(category, e => e.SetState(EntityState.Modified));

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Modified, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(category.Products[0]).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[1]).State);
                Assert.Equal(EntityState.Modified, context.Entry(category.Products[2]).State);

                Assert.Same(category, category.Products[0].Category);
                Assert.Same(category, category.Products[1].Category);
                Assert.Same(category, category.Products[2].Category);

                Assert.Equal(category.Id, category.Products[0].CategoryId);
                Assert.Equal(category.Id, category.Products[1].CategoryId);
                Assert.Equal(category.Id, category.Products[2].CategoryId);
            }
        }

        [Fact]
        public void Further_graph_traversal_stops_if_an_entity_is_not_attached()
        {
            using (var context = new EarlyLearningCenter())
            {

                var category = new Category
                {
                    Id = 1,
                    Products = new List<Product>
                            {
                                new Product { Id = 1, Details = new ProductDetails { Id = 1 }},
                                new Product { Id = 2, Details = new ProductDetails { Id = 2 }},
                                new Product { Id = 3, Details = new ProductDetails { Id = 3 }}
                            }
                };

                context.ChangeTracker.AttachGraph(category, e =>
                    {
                        var product = e.Entity as Product;
                        if (product == null
                            || product.Id != 2)
                        {
                            e.SetState(EntityState.Unchanged);
                        }
                    });

                Assert.Equal(5, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[0]).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[0].Details).State);
                Assert.Equal(EntityState.Unknown, context.Entry(category.Products[1]).State);
                Assert.Equal(EntityState.Unknown, context.Entry(category.Products[1].Details).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[2]).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[2].Details).State);

                Assert.Same(category, category.Products[0].Category);
                Assert.Same(category, category.Products[1].Category);
                Assert.Same(category, category.Products[2].Category);

                Assert.Equal(category.Id, category.Products[0].CategoryId);
                Assert.Equal(category.Id, category.Products[1].CategoryId);
                Assert.Equal(category.Id, category.Products[2].CategoryId);

                Assert.Same(category.Products[0], category.Products[0].Details.Product);
                Assert.Null(category.Products[1].Details.Product);
                Assert.Same(category.Products[2], category.Products[2].Details.Product);
            }
        }

        [Fact]
        public void Graph_iterator_does_not_go_visit_Apple()
        {
            using (var context = new EarlyLearningCenter())
            {
                var details = new ProductDetails { Id = 1, Product = new Product { Id = 1 } };
                details.Product.Details = details;

                context.ChangeTracker.AttachGraph(details, e => { });

                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
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
        }

        private class ProductDetails
        {
            public int Id { get; set; }
            public Product Product { get; set; }
        }

        private class EarlyLearningCenter : DbContext
        {
            public EarlyLearningCenter()
                : base(TestHelpers.CreateServiceProvider())
            {
            }

            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Category>()
                    .OneToMany(e => e.Products, e => e.Category);

                modelBuilder
                    .Entity<Product>()
                    .OneToOne(e => e.Details, e => e.Product);
            }
        }
    }
}
