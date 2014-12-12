// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public void Can_attach_parent_with_one_to_one_children()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product = new Product { Id = 1, Details = new ProductDetails { Id = 1, Tag = new ProductDetailsTag { Id = 1 } } };

                context.ChangeTracker.AttachGraph(product, e => e.SetState(EntityState.Unchanged));

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

                context.ChangeTracker.AttachGraph(tag, e => e.SetState(EntityState.Unchanged));

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

                context.ChangeTracker.AttachGraph(details, e => e.SetState(EntityState.Unchanged));

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
                                new Product { Id = 1, Details = new ProductDetails { Id = 1 } },
                                new Product { Id = 2, Details = new ProductDetails { Id = 2 } },
                                new Product { Id = 3, Details = new ProductDetails { Id = 3 } }
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

        [Fact]
        public async Task Can_attach_parent_with_some_new_and_some_existing_entities()
        {
            await KeyValueAttachTestAsync((category, changeTracker) =>
                {
                    changeTracker.AttachGraph(
                        category,
                        e =>
                            {
                                var product = e.Entity as Product;
                                e.SetState(product != null && product.Id == 0 ? EntityState.Added : EntityState.Unchanged);
                            });

                    return Task.FromResult(0);
                });
        }

        [Fact]
        public async Task Can_attach_parent_with_some_new_and_some_existing_entities_async()
        {
            await KeyValueAttachTestAsync(async (category, changeTracker) =>
                await changeTracker.AttachGraphAsync(
                    category,
                    async (e, c) =>
                        {
                            var product = e.Entity as Product;
                            await e.SetStateAsync(product != null && product.Id == 0 ? EntityState.Added : EntityState.Unchanged, c);
                        }));
        }

        [Fact]
        public async Task Can_attach_graph_using_built_in_attacher()
        {
            await KeyValueAttachTestAsync((category, changeTracker) =>
                {
                    changeTracker.AttachGraph(category);

                    return Task.FromResult(0);
                });
        }

        [Fact]
        public async Task Can_attach_graph_using_built_in_attacher_async()
        {
            await KeyValueAttachTestAsync(
                async (category, changeTracker) => await changeTracker.AttachGraphAsync(category));
        }

        [Fact]
        public async Task Can_update_graph_using_built_in_attacher()
        {
            await KeyValueAttachTestAsync((category, changeTracker) =>
                {
                    changeTracker.UpdateGraph(category);

                    return Task.FromResult(0);
                }, expectModified: true);
        }

        [Fact]
        public async Task Can_update_graph_using_built_in_attacher_async()
        {
            await KeyValueAttachTestAsync(
                async (category, changeTracker) => await changeTracker.UpdateGraphAsync(category),
                expectModified: true);
        }

        private static async Task KeyValueAttachTestAsync(
            Func<Category, ChangeTracker, Task> attacher,
            bool expectModified = false)
        {
            using (var context = new EarlyLearningCenter())
            {
                var category = new Category
                    {
                        Id = 77,
                        Products = new List<Product>
                            {
                                new Product { Id = 77 },
                                new Product { Id = 0 },
                                new Product { Id = 78 }
                            }
                    };

                await attacher(category, context.ChangeTracker);

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
        public async Task Can_attach_graph_using_custom_delegate()
        {
            var attacher = new MyAttacher(updateExistingEntities: false);

            await CustomAttacherTestAsync((category, changeTracker) =>
                {
                    changeTracker.AttachGraph(category, attacher.HandleEntity);

                    return Task.FromResult(0);
                });
        }

        [Fact]
        public async Task Can_attach_graph_using_custom_delegate_async()
        {
            var attacher = new MyAttacher(updateExistingEntities: true);

            await CustomAttacherTestAsync(
                async (category, changeTracker) => await changeTracker.AttachGraphAsync(category, attacher.HandleEntityAsync),
                expectModified: true);
        }

        [Fact]
        public async Task Can_attach_graph_using_custom_attacher()
        {
            await CustomAttacherTestAsync((category, changeTracker) =>
                {
                    changeTracker.AttachGraph(category);

                    return Task.FromResult(0);
                });
        }

        [Fact]
        public async Task Can_attach_graph_using_custom_attacher_async()
        {
            await CustomAttacherTestAsync(
                async (category, changeTracker) => await changeTracker.AttachGraphAsync(category));
        }

        [Fact]
        public async Task Can_update_graph_using_custom_attacher()
        {
            await CustomAttacherTestAsync((category, changeTracker) =>
                {
                    changeTracker.UpdateGraph(category);

                    return Task.FromResult(0);
                }, expectModified: true);
        }

        [Fact]
        public async Task Can_update_graph_using_custom_attacher_async()
        {
            await CustomAttacherTestAsync(
                async (category, changeTracker) => await changeTracker.UpdateGraphAsync(category),
                expectModified: true);
        }

        private class MyAttacher : KeyValueEntityAttacher
        {
            public MyAttacher(bool updateExistingEntities)
                : base(updateExistingEntities)
            {
            }

            public override EntityState DetermineState(EntityEntry entry)
            {
                if (!entry.IsKeySet)
                {
                    entry.StateEntry[entry.StateEntry.EntityType.GetPrimaryKey().Properties.Single()] = 777;
                    return EntityState.Added;
                }

                return base.DetermineState(entry);
            }
        }

        private class MyAttacherFactory : EntityAttacherFactory
        {
            public override IEntityAttacher CreateForAttach()
            {
                return new MyAttacher(updateExistingEntities: false);
            }

            public override IEntityAttacher CreateForUpdate()
            {
                return new MyAttacher(updateExistingEntities: true);
            }
        }

        private static async Task CustomAttacherTestAsync(
            Func<Category, ChangeTracker, Task> attacher,
            bool expectModified = false)
        {
            var customServices = new ServiceCollection().AddSingleton<EntityAttacherFactory, MyAttacherFactory>();

            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider(customServices)))
            {
                var category = new Category
                    {
                        Id = 77,
                        Products = new List<Product>
                            {
                                new Product { Id = 77 },
                                new Product { Id = 0 },
                                new Product { Id = 78 }
                            }
                    };

                await attacher(category, context.ChangeTracker);

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                var nonAddedState = expectModified ? EntityState.Modified : EntityState.Unchanged;

                Assert.Equal(nonAddedState, context.Entry(category).State);
                Assert.Equal(nonAddedState, context.Entry(category.Products[0]).State);
                Assert.Equal(EntityState.Added, context.Entry(category.Products[1]).State);
                Assert.Equal(nonAddedState, context.Entry(category.Products[2]).State);

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
                : this(TestHelpers.CreateServiceProvider())
            {
            }

            public EarlyLearningCenter(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Category>()
                    .OneToMany(e => e.Products, e => e.Category);

                modelBuilder
                    .Entity<ProductDetailsTag>()
                    .OneToOne(e => e.TagDetails, e => e.Tag)
                    .ForeignKey<ProductDetailsTagDetails>(e => e.Id);

                modelBuilder
                    .Entity<ProductDetails>()
                    .OneToOne(e => e.Tag, e => e.Details)
                    .ForeignKey<ProductDetailsTag>(e => e.Id);

                modelBuilder
                    .Entity<Product>()
                    .OneToOne(e => e.Details, e => e.Product)
                    .ForeignKey<ProductDetails>(e => e.Id);

                modelBuilder.Entity<OrderDetails>(b =>
                    {
                        b.Key(e => new { e.OrderId, e.ProductId });
                        b.ManyToOne(e => e.Order, e => e.OrderDetails).ForeignKey(e => e.OrderId);
                        b.ManyToOne(e => e.Product, e => e.OrderDetails).ForeignKey(e => e.ProductId);
                    });
            }
        }
    }
}
