// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Extensions.DependencyInjection;
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
                var stateManger = context.GetService<IStateManager>();

                Assert.Same(stateManger, context.ChangeTracker.GetService());
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

                context.ChangeTracker.TrackGraph(category, e => e.Entry.State = EntityState.Modified);

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

                context.ChangeTracker.TrackGraph(product, e => e.Entry.State = EntityState.Modified);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Modified, context.Entry(product).State);
                Assert.Equal(EntityState.Modified, context.Entry(product.Category).State);

                Assert.Same(product, product.Category.Products[0]);
                Assert.Equal(product.Category.Id, product.CategoryId);
            }
        }

        [Fact]
        public void Can_attach_parent_with_one_to_one_children()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product = new Product { Id = 1, Details = new ProductDetails { Id = 1, Tag = new ProductDetailsTag { Id = 1 } } };

                context.ChangeTracker.TrackGraph(product, e => e.Entry.State = EntityState.Unchanged);

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product.Details).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product.Details.Tag).State);

                Assert.Same(product, product.Details.Product);
                Assert.Same(product.Details, product.Details.Tag.Details);
            }
        }

        [Fact]
        public void Can_attach_child_with_one_to_one_parents()
        {
            using (var context = new EarlyLearningCenter())
            {
                var tag = new ProductDetailsTag { Id = 1, Details = new ProductDetails { Id = 1, Product = new Product { Id = 1 } } };

                context.ChangeTracker.TrackGraph(tag, e => e.Entry.State = EntityState.Unchanged);

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Unchanged, context.Entry(tag).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(tag.Details).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(tag.Details.Product).State);

                Assert.Same(tag, tag.Details.Tag);
                Assert.Same(tag.Details, tag.Details.Product.Details);
            }
        }

        [Fact]
        public void Can_attach_entity_with_one_to_one_parent_and_child()
        {
            using (var context = new EarlyLearningCenter())
            {
                var details = new ProductDetails { Id = 1, Product = new Product { Id = 1 }, Tag = new ProductDetailsTag { Id = 1 } };

                context.ChangeTracker.TrackGraph(details, e => e.Entry.State = EntityState.Unchanged);

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Unchanged, context.Entry(details).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(details.Product).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(details.Tag).State);

                Assert.Same(details, details.Tag.Details);
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

                context.ChangeTracker.TrackGraph(category, e => e.Entry.State = EntityState.Modified);

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
                                new Product { Id = 1, CategoryId = 1, Details = new ProductDetails { Id = 1 } },
                                new Product { Id = 2, CategoryId = 1, Details = new ProductDetails { Id = 2 } },
                                new Product { Id = 3, CategoryId = 1, Details = new ProductDetails { Id = 3 } }
                            }
                    };

                context.ChangeTracker.TrackGraph(category, e =>
                    {
                        var product = e.Entry.Entity as Product;
                        if (product == null
                            || product.Id != 2)
                        {
                            e.Entry.State = EntityState.Unchanged;
                        }
                    });

                Assert.Equal(5, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[0]).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[0].Details).State);
                Assert.Equal(EntityState.Detached, context.Entry(category.Products[1]).State);
                Assert.Equal(EntityState.Detached, context.Entry(category.Products[1].Details).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[2]).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[2].Details).State);

                Assert.Same(category, category.Products[0].Category);
                Assert.Null(category.Products[1].Category);
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

                context.ChangeTracker.TrackGraph(details, e => { });

                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void Can_attach_parent_with_some_new_and_some_existing_entities()
        {
            KeyValueAttachTest((category, changeTracker) =>
                {
                    changeTracker.TrackGraph(
                        category,
                        e =>
                            {
                                var product = e.Entry.Entity as Product;
                                e.Entry.State = product != null && product.Id == 0 ? EntityState.Added : EntityState.Unchanged;
                            });
                });
        }

        [Fact]
        public void Can_attach_graph_using_built_in_tracker()
        {
            var tracker = new KeyValueEntityTracker(updateExistingEntities: false);

            KeyValueAttachTest((category, changeTracker) => changeTracker.TrackGraph(category, tracker.TrackEntity));
        }

        [Fact]
        public void Can_update_graph_using_built_in_tracker()
        {
            var tracker = new KeyValueEntityTracker(updateExistingEntities: true);

            KeyValueAttachTest((category, changeTracker) => changeTracker.TrackGraph(category, tracker.TrackEntity), expectModified: true);
        }

        private static void KeyValueAttachTest(Action<Category, ChangeTracker> tracker, bool expectModified = false)
        {
            using (var context = new EarlyLearningCenter())
            {
                var category = new Category
                    {
                        Id = 77,
                        Products = new List<Product>
                            {
                                new Product { Id = 77, CategoryId = expectModified ? 0 : 77 },
                                new Product { Id = 0, CategoryId = expectModified ? 0 : 77 },
                                new Product { Id = 78, CategoryId = expectModified ? 0 : 77 }
                            }
                    };

                tracker(category, context.ChangeTracker);

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                var nonAddedState = expectModified ? EntityState.Modified : EntityState.Unchanged;

                Assert.Equal(nonAddedState, context.Entry(category).State);
                Assert.Equal(nonAddedState, context.Entry(category.Products[0]).State);
                Assert.Equal(EntityState.Added, context.Entry(category.Products[1]).State);
                Assert.Equal(nonAddedState, context.Entry(category.Products[2]).State);

                Assert.Equal(77, category.Products[0].Id);
                Assert.Equal(1, category.Products[1].Id);
                Assert.Equal(78, category.Products[2].Id);

                Assert.Same(category, category.Products[0].Category);
                Assert.Same(category, category.Products[1].Category);
                Assert.Same(category, category.Products[2].Category);

                Assert.Equal(category.Id, category.Products[0].CategoryId);
                Assert.Equal(category.Id, category.Products[1].CategoryId);
                Assert.Equal(category.Id, category.Products[2].CategoryId);
            }
        }

        [Fact]
        public void Can_attach_graph_using_custom_delegate()
        {
            var tracker = new MyTracker(updateExistingEntities: false);

            using (var context = new EarlyLearningCenter())
            {
                var category = new Category
                    {
                        Id = 77,
                        Products = new List<Product>
                            {
                                new Product { Id = 77, CategoryId = 77 },
                                new Product { Id = 0, CategoryId = 77 },
                                new Product { Id = 78, CategoryId = 77 }
                            }
                    };

                context.ChangeTracker.TrackGraph(category, tracker.TrackEntity);

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[0]).State);
                Assert.Equal(EntityState.Added, context.Entry(category.Products[1]).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[2]).State);

                Assert.Equal(77, category.Products[0].Id);
                Assert.Equal(777, category.Products[1].Id);
                Assert.Equal(78, category.Products[2].Id);

                Assert.Same(category, category.Products[0].Category);
                Assert.Same(category, category.Products[1].Category);
                Assert.Same(category, category.Products[2].Category);

                Assert.Equal(category.Id, category.Products[0].CategoryId);
                Assert.Equal(category.Id, category.Products[1].CategoryId);
                Assert.Equal(category.Id, category.Products[2].CategoryId);
            }
        }

        private class MyTracker : KeyValueEntityTracker
        {
            public MyTracker(bool updateExistingEntities)
                : base(updateExistingEntities)
            {
            }

            public override EntityState DetermineState(EntityEntry entry)
            {
                if (!entry.IsKeySet)
                {
                    entry.GetService()[entry.Metadata.GetPrimaryKey().Properties.Single()] = 777;
                    return EntityState.Added;
                }

                return base.DetermineState(entry);
            }
        }

        [Fact] // Issue #1207
        public void Can_add_principal_and_then_identifying_dependents_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product1 = new Product
                    {
                        Details = new ProductDetails
                            {
                                Tag = new ProductDetailsTag
                                    {
                                        TagDetails = new ProductDetailsTagDetails()
                                    }
                            }
                    };
                var product2 = new Product
                    {
                        Details = new ProductDetails
                            {
                                Tag = new ProductDetailsTag
                                    {
                                        TagDetails = new ProductDetailsTagDetails()
                                    }
                            }
                    };

                context.Add(product1);
                context.Add(product1.Details);
                context.Add(product1.Details.Tag);
                context.Add(product1.Details.Tag.TagDetails);
                context.Add(product2);
                context.Add(product2.Details);
                context.Add(product2.Details.Tag);
                context.Add(product2.Details.Tag.TagDetails);

                AssertProductAndDetailsFixedUp(context, product1.Details.Tag.TagDetails, product2.Details.Tag.TagDetails);
            }
        }

        [Fact] // Issue #1207
        public void Can_add_identifying_dependents_and_then_principal_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var tagDetails1 = new ProductDetailsTagDetails
                    {
                        Tag = new ProductDetailsTag
                            {
                                Details = new ProductDetails
                                    { Product = new Product() }
                            }
                    };

                var tagDetails2 = new ProductDetailsTagDetails
                    {
                        Tag = new ProductDetailsTag
                            {
                                Details = new ProductDetails
                                    { Product = new Product() }
                            }
                    };

                context.Add(tagDetails1);
                context.Add(tagDetails1.Tag);
                context.Add(tagDetails1.Tag.Details);
                context.Add(tagDetails1.Tag.Details.Product);
                context.Add(tagDetails2);
                context.Add(tagDetails2.Tag);
                context.Add(tagDetails2.Tag.Details);
                context.Add(tagDetails2.Tag.Details.Product);

                AssertProductAndDetailsFixedUp(context, tagDetails1, tagDetails2);
            }
        }

        [Fact] // Issue #1207
        public void Can_add_identifying_dependents_and_then_principal_interleaved_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var tagDetails1 = new ProductDetailsTagDetails
                    {
                        Tag = new ProductDetailsTag
                            {
                                Details = new ProductDetails
                                    { Product = new Product() }
                            }
                    };

                var tagDetails2 = new ProductDetailsTagDetails
                    {
                        Tag = new ProductDetailsTag
                            {
                                Details = new ProductDetails
                                    { Product = new Product() }
                            }
                    };

                context.Add(tagDetails1);
                context.Add(tagDetails2);
                context.Add(tagDetails1.Tag);
                context.Add(tagDetails2.Tag);
                context.Add(tagDetails2.Tag.Details);
                context.Add(tagDetails1.Tag.Details);
                context.Add(tagDetails1.Tag.Details.Product);
                context.Add(tagDetails2.Tag.Details.Product);

                AssertProductAndDetailsFixedUp(context, tagDetails1, tagDetails2);
            }
        }

        [Fact] // Issue #1207
        public void Can_add_identifying_dependents_and_principal_starting_in_the_middle_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var tagDetails1 = new ProductDetailsTagDetails
                    {
                        Tag = new ProductDetailsTag
                            {
                                Details = new ProductDetails
                                    { Product = new Product() }
                            }
                    };

                var tagDetails2 = new ProductDetailsTagDetails
                    {
                        Tag = new ProductDetailsTag
                            {
                                Details = new ProductDetails
                                    { Product = new Product() }
                            }
                    };

                context.Add(tagDetails1.Tag);
                context.Add(tagDetails2.Tag);
                context.Add(tagDetails1);
                context.Add(tagDetails2);
                context.Add(tagDetails2.Tag.Details);
                context.Add(tagDetails1.Tag.Details);
                context.Add(tagDetails1.Tag.Details.Product);
                context.Add(tagDetails2.Tag.Details.Product);

                AssertProductAndDetailsFixedUp(context, tagDetails1, tagDetails2);
            }
        }

        [Fact] // Issue #1207
        public void Can_add_principal_and_identifying_dependents_starting_in_the_middle_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product1 = new Product
                    {
                        Details = new ProductDetails
                            {
                                Tag = new ProductDetailsTag
                                    {
                                        TagDetails = new ProductDetailsTagDetails()
                                    }
                            }
                    };
                var product2 = new Product
                    {
                        Details = new ProductDetails
                            {
                                Tag = new ProductDetailsTag
                                    {
                                        TagDetails = new ProductDetailsTagDetails()
                                    }
                            }
                    };

                context.Add(product1.Details);
                context.Add(product2.Details);
                context.Add(product1.Details.Tag.TagDetails);
                context.Add(product1);
                context.Add(product1.Details.Tag);
                context.Add(product2.Details.Tag);
                context.Add(product2.Details.Tag.TagDetails);
                context.Add(product2);

                AssertProductAndDetailsFixedUp(context, product1.Details.Tag.TagDetails, product2.Details.Tag.TagDetails);
            }
        }

        [Fact] // Issue #1207
        public void Can_add_identifying_dependents_and_principal_with_post_nav_fixup_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product1 = new Product();
                var details1 = new ProductDetails();
                var tag1 = new ProductDetailsTag();
                var tagDetails1 = new ProductDetailsTagDetails();

                var product2 = new Product();
                var details2 = new ProductDetails();
                var tag2 = new ProductDetailsTag();
                var tagDetails2 = new ProductDetailsTagDetails();

                context.Add(product1);
                context.Add(tagDetails2);
                context.Add(details1);
                context.Add(tag2);
                context.Add(details2);
                context.Add(tag1);
                context.Add(tagDetails1);
                context.Add(product2);

                product1.Details = details1;
                details1.Tag = tag1;
                tag1.TagDetails = tagDetails1;

                product2.Details = details2;
                details2.Tag = tag2;
                tag2.TagDetails = tagDetails2;

                context.ChangeTracker.DetectChanges();

                AssertProductAndDetailsFixedUp(context, product1.Details.Tag.TagDetails, product2.Details.Tag.TagDetails);
            }
        }

        [Fact] // Issue #1207
        public void Can_add_identifying_dependents_and_principal_with_reverse_post_nav_fixup_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product1 = new Product();
                var details1 = new ProductDetails();
                var tag1 = new ProductDetailsTag();
                var tagDetails1 = new ProductDetailsTagDetails();

                var product2 = new Product();
                var details2 = new ProductDetails();
                var tag2 = new ProductDetailsTag();
                var tagDetails2 = new ProductDetailsTagDetails();

                context.Add(product1);
                context.Add(tagDetails2);
                context.Add(details1);
                context.Add(tag2);
                context.Add(details2);
                context.Add(tag1);
                context.Add(tagDetails1);
                context.Add(product2);

                tagDetails1.Tag = tag1;
                tag1.Details = details1;
                details1.Product = product1;

                tagDetails2.Tag = tag2;
                tag2.Details = details2;
                details2.Product = product2;

                context.ChangeTracker.DetectChanges();

                AssertProductAndDetailsFixedUp(context, product1.Details.Tag.TagDetails, product2.Details.Tag.TagDetails);
            }
        }

        private static void AssertProductAndDetailsFixedUp(
            DbContext context,
            ProductDetailsTagDetails tagDetails1,
            ProductDetailsTagDetails tagDetails2)
        {
            Assert.Equal(8, context.ChangeTracker.Entries().Count());

            Assert.Equal(EntityState.Added, context.Entry(tagDetails1).State);
            Assert.Equal(EntityState.Added, context.Entry(tagDetails1.Tag).State);
            Assert.Equal(EntityState.Added, context.Entry(tagDetails1.Tag.Details).State);
            Assert.Equal(EntityState.Added, context.Entry(tagDetails1.Tag.Details.Product).State);

            Assert.Equal(EntityState.Added, context.Entry(tagDetails2).State);
            Assert.Equal(EntityState.Added, context.Entry(tagDetails2.Tag).State);
            Assert.Equal(EntityState.Added, context.Entry(tagDetails2.Tag.Details).State);
            Assert.Equal(EntityState.Added, context.Entry(tagDetails2.Tag.Details.Product).State);

            Assert.Equal(tagDetails1.Id, tagDetails1.Tag.Id);
            Assert.Equal(tagDetails1.Id, tagDetails1.Tag.Details.Id);
            Assert.Equal(tagDetails1.Id, tagDetails1.Tag.Details.Product.Id);
            Assert.True(tagDetails1.Id > 0);

            Assert.Equal(tagDetails2.Id, tagDetails2.Tag.Id);
            Assert.Equal(tagDetails2.Id, tagDetails2.Tag.Details.Id);
            Assert.Equal(tagDetails2.Id, tagDetails2.Tag.Details.Product.Id);
            Assert.True(tagDetails2.Id > 0);

            Assert.Same(tagDetails1, tagDetails1.Tag.TagDetails);
            Assert.Same(tagDetails1.Tag, tagDetails1.Tag.Details.Tag);
            Assert.Same(tagDetails1.Tag.Details, tagDetails1.Tag.Details.Product.Details);

            Assert.Same(tagDetails2, tagDetails2.Tag.TagDetails);
            Assert.Same(tagDetails2.Tag, tagDetails2.Tag.Details.Tag);
            Assert.Same(tagDetails2.Tag.Details, tagDetails2.Tag.Details.Product.Details);

            var product1 = tagDetails1.Tag.Details.Product;
            Assert.Same(product1, product1.Details.Product);
            Assert.Same(product1.Details, product1.Details.Tag.Details);
            Assert.Same(product1.Details.Tag, product1.Details.Tag.TagDetails.Tag);

            var product2 = tagDetails2.Tag.Details.Product;
            Assert.Same(product2, product2.Details.Product);
            Assert.Same(product2.Details, product2.Details.Tag.Details);
            Assert.Same(product2.Details.Tag, product2.Details.Tag.TagDetails.Tag);
        }

        [Fact] // Issue #1207
        public void Can_add_identifying_one_to_many_via_principal_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product1 = new Product();
                var product2 = new Product();

                var order1 = new Order();
                var order2 = new Order();

                var orderDetails1a = new OrderDetails { Order = order1, Product = product1 };
                var orderDetails1b = new OrderDetails { Order = order1, Product = product2 };
                var orderDetails2a = new OrderDetails { Order = order2, Product = product1 };
                var orderDetails2b = new OrderDetails { Order = order2, Product = product2 };

                context.Add(product1);
                context.Add(order1);
                context.Add(orderDetails1a);
                context.Add(orderDetails1b);
                context.Add(product2);
                context.Add(order2);
                context.Add(orderDetails2a);
                context.Add(orderDetails2b);

                AssertOrderAndDetailsFixedUp(context, orderDetails1a, orderDetails1b, orderDetails2a, orderDetails2b);
            }
        }

        [Fact] // Issue #1207
        public void Can_add_identifying_one_to_many_via_dependents_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product1 = new Product();
                var product2 = new Product();

                var order1 = new Order();
                var order2 = new Order();

                var orderDetails1a = new OrderDetails { Order = order1, Product = product1 };
                var orderDetails1b = new OrderDetails { Order = order1, Product = product2 };
                var orderDetails2a = new OrderDetails { Order = order2, Product = product1 };
                var orderDetails2b = new OrderDetails { Order = order2, Product = product2 };

                context.Add(orderDetails1a);
                context.Add(orderDetails2a);
                context.Add(orderDetails1b);
                context.Add(orderDetails2b);
                context.Add(order1);
                context.Add(product1);
                context.Add(order2);
                context.Add(product2);

                AssertOrderAndDetailsFixedUp(context, orderDetails1a, orderDetails1b, orderDetails2a, orderDetails2b);
            }
        }

        private static void AssertOrderAndDetailsFixedUp(
            DbContext context,
            OrderDetails orderDetails1a,
            OrderDetails orderDetails1b,
            OrderDetails orderDetails2a,
            OrderDetails orderDetails2b)
        {
            Assert.Equal(8, context.ChangeTracker.Entries().Count());

            Assert.Equal(EntityState.Added, context.Entry(orderDetails1a).State);
            Assert.Equal(EntityState.Added, context.Entry(orderDetails1b).State);
            Assert.Equal(EntityState.Added, context.Entry(orderDetails1a.Order).State);
            Assert.Equal(EntityState.Added, context.Entry(orderDetails1b.Product).State);

            Assert.Equal(EntityState.Added, context.Entry(orderDetails2a).State);
            Assert.Equal(EntityState.Added, context.Entry(orderDetails2b).State);
            Assert.Equal(EntityState.Added, context.Entry(orderDetails2a.Order).State);
            Assert.Equal(EntityState.Added, context.Entry(orderDetails2b.Product).State);

            Assert.Equal(orderDetails1a.OrderId, orderDetails1a.Order.Id);
            Assert.Equal(orderDetails1b.OrderId, orderDetails1b.Order.Id);
            Assert.Equal(orderDetails1a.ProductId, orderDetails1a.Product.Id);
            Assert.Equal(orderDetails1b.ProductId, orderDetails1b.Product.Id);
            Assert.True(orderDetails1a.OrderId > 0);
            Assert.True(orderDetails1b.OrderId > 0);
            Assert.True(orderDetails1a.ProductId > 0);
            Assert.True(orderDetails1b.ProductId > 0);

            Assert.Equal(orderDetails2a.OrderId, orderDetails2a.Order.Id);
            Assert.Equal(orderDetails2b.OrderId, orderDetails2b.Order.Id);
            Assert.Equal(orderDetails2a.ProductId, orderDetails2a.Product.Id);
            Assert.Equal(orderDetails2b.ProductId, orderDetails2b.Product.Id);
            Assert.True(orderDetails2a.OrderId > 0);
            Assert.True(orderDetails2b.OrderId > 0);
            Assert.True(orderDetails2a.ProductId > 0);
            Assert.True(orderDetails2b.ProductId > 0);

            Assert.Same(orderDetails1a.Order, orderDetails1b.Order);
            Assert.Same(orderDetails2a.Order, orderDetails2b.Order);

            Assert.Same(orderDetails1a.Product, orderDetails2a.Product);
            Assert.Same(orderDetails1b.Product, orderDetails2b.Product);

            Assert.Equal(2, orderDetails1a.Order.OrderDetails.Count);
            Assert.Equal(2, orderDetails2a.Order.OrderDetails.Count);

            Assert.Contains(orderDetails1a, orderDetails1a.Order.OrderDetails);
            Assert.Contains(orderDetails1b, orderDetails1a.Order.OrderDetails);
            Assert.Contains(orderDetails2a, orderDetails2a.Order.OrderDetails);
            Assert.Contains(orderDetails2b, orderDetails2a.Order.OrderDetails);

            Assert.Equal(2, orderDetails1a.Product.OrderDetails.Count);
            Assert.Equal(2, orderDetails1b.Product.OrderDetails.Count);

            Assert.Contains(orderDetails1a, orderDetails1a.Product.OrderDetails);
            Assert.Contains(orderDetails2a, orderDetails1a.Product.OrderDetails);
            Assert.Contains(orderDetails1b, orderDetails1b.Product.OrderDetails);
            Assert.Contains(orderDetails2b, orderDetails1b.Product.OrderDetails);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Entries_calls_DetectChanges_by_default(bool useGenericOverload)
        {
            using (var context = new EarlyLearningCenter())
            {
                var entry = context.Attach(new Product { Id = 1, CategoryId = 66 });

                entry.Entity.CategoryId = 77;

                Assert.Equal(EntityState.Unchanged, entry.State);

                if (useGenericOverload)
                {
                    context.ChangeTracker.Entries<Product>();
                }
                else
                {
                    context.ChangeTracker.Entries();
                }

                Assert.Equal(EntityState.Modified, entry.State);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Auto_DetectChanges_for_Entries_can_be_switched_off(bool useGenericOverload)
        {
            using (var context = new EarlyLearningCenter())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                var entry = context.Attach(new Product { Id = 1, CategoryId = 66 });

                entry.Entity.CategoryId = 77;

                Assert.Equal(EntityState.Unchanged, entry.State);

                if (useGenericOverload)
                {
                    context.ChangeTracker.Entries<Product>();
                }
                else
                {
                    context.ChangeTracker.Entries();
                }

                Assert.Equal(EntityState.Unchanged, entry.State);
            }
        }

        [Fact]
        public void Explicitly_calling_DetectChanges_works_even_if_auto_DetectChanges_is_switched_off()
        {
            using (var context = new EarlyLearningCenter())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                var entry = context.Attach(new Product { Id = 1, CategoryId = 66 });

                entry.Entity.CategoryId = 77;

                Assert.Equal(EntityState.Unchanged, entry.State);

                context.ChangeTracker.DetectChanges();

                Assert.Equal(EntityState.Modified, entry.State);
            }
        }

        [Fact]
        public void TrackGraph_does_not_call_DetectChanges()
        {
            var provider = TestHelpers.Instance.CreateServiceProvider(new ServiceCollection().AddScoped<IChangeDetector, ChangeDetectorProxy>());
            using (var context = new EarlyLearningCenter(provider))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                changeDetector.DetectChangesCalled = false;

                context.ChangeTracker.TrackGraph(CreateSimpleGraph(2), e => e.Entry.State = EntityState.Unchanged);

                Assert.False(changeDetector.DetectChangesCalled);

                context.ChangeTracker.DetectChanges();

                Assert.True(changeDetector.DetectChangesCalled);
            }
        }

        private static Product CreateSimpleGraph(int id) 
            => new Product { Id = id, Category = new Category { Id = id } };

        private class ChangeDetectorProxy : ChangeDetector
        {
            public ChangeDetectorProxy(IEntityGraphAttacher attacher)
                : base(attacher)
            {
            }

            public bool DetectChangesCalled { get; set; }

            public override void DetectChanges(InternalEntityEntry entry)
            {
                DetectChangesCalled = true;

                base.DetectChanges(entry);
            }

            public override void DetectChanges(IStateManager stateManager)
            {
                DetectChangesCalled = true;

                base.DetectChanges(stateManager);
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

            public List<OrderDetails> OrderDetails { get; set; }
        }

        private class ProductDetails
        {
            public int Id { get; set; }

            public Product Product { get; set; }

            public ProductDetailsTag Tag { get; set; }
        }

        private class ProductDetailsTag
        {
            public int Id { get; set; }

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

        private class EarlyLearningCenter : DbContext
        {
            public EarlyLearningCenter()
                : this(TestHelpers.Instance.CreateServiceProvider())
            {
            }

            public EarlyLearningCenter(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Category>().HasMany(e => e.Products).WithOne(e => e.Category);

                modelBuilder
                    .Entity<ProductDetailsTag>().HasOne(e => e.TagDetails).WithOne(e => e.Tag)
                    .HasForeignKey<ProductDetailsTagDetails>(e => e.Id);

                modelBuilder
                    .Entity<ProductDetails>().HasOne(e => e.Tag).WithOne(e => e.Details)
                    .HasForeignKey<ProductDetailsTag>(e => e.Id);

                modelBuilder
                    .Entity<Product>().HasOne(e => e.Details).WithOne(e => e.Product)
                    .HasForeignKey<ProductDetails>(e => e.Id);

                modelBuilder.Entity<OrderDetails>(b =>
                    {
                        b.HasKey(e => new { e.OrderId, e.ProductId });
                        b.HasOne(e => e.Order).WithMany(e => e.OrderDetails).HasForeignKey(e => e.OrderId);
                        b.HasOne(e => e.Product).WithMany(e => e.OrderDetails).HasForeignKey(e => e.ProductId);
                    });
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase();
        }

        public class KeyValueEntityTracker
        {
            private readonly bool _updateExistingEntities;

            public KeyValueEntityTracker(bool updateExistingEntities)
            {
                _updateExistingEntities = updateExistingEntities;
            }

            public virtual void TrackEntity(EntityEntryGraphNode node)
                => node.Entry.GetService().SetEntityState(DetermineState(node.Entry), acceptChanges: true);

            public virtual EntityState DetermineState(EntityEntry entry)
                => entry.IsKeySet
                    ? (_updateExistingEntities ? EntityState.Modified : EntityState.Unchanged)
                    : EntityState.Added;
        }
    }
}
