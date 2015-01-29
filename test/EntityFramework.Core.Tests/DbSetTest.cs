// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.ChangeTracking;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class DbSetTest
    {
        [Fact]
        public void Can_add_new_entities_to_context()
        {
            TrackEntitiesTest((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_attached()
        {
            TrackEntitiesTest((c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Unchanged);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_updated()
        {
            TrackEntitiesTest((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Modified);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_deleted()
        {
            TrackEntitiesTest((c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);
        }

        private static void TrackEntitiesTest(
            Func<DbSet<Category>, Category, EntityEntry<Category>> categoryAdder,
            Func<DbSet<Product>, Product, EntityEntry<Product>> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter())
            {
                var category1 = new Category { Id = 1, Name = "Beverages" };
                var category2 = new Category { Id = 2, Name = "Foods" };
                var product1 = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var product2 = new Product { Id = 2, Name = "Bovril", Price = 4.99m };

                var categoryEntry1 = categoryAdder(context.Categories, category1);
                var categoryEntry2 = categoryAdder(context.Categories, category2);
                var productEntry1 = productAdder(context.Products, product1);
                var productEntry2 = productAdder(context.Products, product2);

                Assert.Same(category1, categoryEntry1.Entity);
                Assert.Same(category2, categoryEntry2.Entity);
                Assert.Same(product1, productEntry1.Entity);
                Assert.Same(product2, productEntry2.Entity);

                Assert.Same(category1, categoryEntry1.Entity);
                Assert.Equal(expectedState, categoryEntry2.State);
                Assert.Same(category2, categoryEntry2.Entity);
                Assert.Equal(expectedState, categoryEntry2.State);

                Assert.Same(product1, productEntry1.Entity);
                Assert.Equal(expectedState, productEntry1.State);
                Assert.Same(product2, productEntry2.Entity);
                Assert.Equal(expectedState, productEntry2.State);

                Assert.Same(categoryEntry1.StateEntry, context.Entry(category1).StateEntry);
                Assert.Same(categoryEntry2.StateEntry, context.Entry(category2).StateEntry);
                Assert.Same(productEntry1.StateEntry, context.Entry(product1).StateEntry);
                Assert.Same(productEntry2.StateEntry, context.Entry(product2).StateEntry);
            }
        }

        [Fact]
        public void Can_add_multiple_new_entities_to_context()
        {
            TrackMultipleEntitiesTest((c, e) => c.Add(e[0], e[1]), (c, e) => c.Add(e[0], e[1]), EntityState.Added);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_context_to_be_attached()
        {
            TrackMultipleEntitiesTest((c, e) => c.Attach(e[0], e[1]), (c, e) => c.Attach(e[0], e[1]), EntityState.Unchanged);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_context_to_be_updated()
        {
            TrackMultipleEntitiesTest((c, e) => c.Update(e[0], e[1]), (c, e) => c.Update(e[0], e[1]), EntityState.Modified);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_context_to_be_deleted()
        {
            TrackMultipleEntitiesTest((c, e) => c.Remove(e[0], e[1]), (c, e) => c.Remove(e[0], e[1]), EntityState.Deleted);
        }

        private static void TrackMultipleEntitiesTest(
            Func<DbSet<Category>, Category[], IReadOnlyList<EntityEntry<Category>>> categoryAdder,
            Func<DbSet<Product>, Product[], IReadOnlyList<EntityEntry<Product>>> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter())
            {
                var category1 = new Category { Id = 1, Name = "Beverages" };
                var category2 = new Category { Id = 2, Name = "Foods" };
                var product1 = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var product2 = new Product { Id = 2, Name = "Bovril", Price = 4.99m };

                var categoryEntries = categoryAdder(context.Categories, new[] { category1, category2 });
                var productEntries = productAdder(context.Products, new[] { product1, product2 });

                Assert.Same(category1, categoryEntries[0].Entity);
                Assert.Same(category2, categoryEntries[1].Entity);
                Assert.Same(product1, productEntries[0].Entity);
                Assert.Same(product2, productEntries[1].Entity);

                Assert.Same(category1, categoryEntries[0].Entity);
                Assert.Equal(expectedState, categoryEntries[0].State);
                Assert.Same(category2, categoryEntries[1].Entity);
                Assert.Equal(expectedState, categoryEntries[1].State);

                Assert.Same(product1, productEntries[0].Entity);
                Assert.Equal(expectedState, productEntries[0].State);
                Assert.Same(product2, productEntries[1].Entity);
                Assert.Equal(expectedState, productEntries[1].State);

                Assert.Same(categoryEntries[0].StateEntry, context.Entry(category1).StateEntry);
                Assert.Same(categoryEntries[1].StateEntry, context.Entry(category2).StateEntry);
                Assert.Same(productEntries[0].StateEntry, context.Entry(product1).StateEntry);
                Assert.Same(productEntries[1].StateEntry, context.Entry(product2).StateEntry);
            }
        }

        [Fact]
        public void Can_add_no_new_entities_to_context()
        {
            TrackNoEntitiesTest(c => c.Add(new Category[0]), c => c.Add(new Product[0]), EntityState.Added);
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_attached()
        {
            TrackNoEntitiesTest(c => c.Attach(new Category[0]), c => c.Attach(new Product[0]), EntityState.Unchanged);
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_updated()
        {
            TrackNoEntitiesTest(c => c.Update(new Category[0]), c => c.Update(new Product[0]), EntityState.Modified);
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_deleted()
        {
            TrackNoEntitiesTest(c => c.Remove(new Category[0]), c => c.Remove(new Product[0]), EntityState.Deleted);
        }

        private static void TrackNoEntitiesTest(
            Func<DbSet<Category>, IReadOnlyList<EntityEntry<Category>>> categoryAdder,
            Func<DbSet<Product>, IReadOnlyList<EntityEntry<Product>>> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter())
            {
                Assert.Empty(categoryAdder(context.Categories));
                Assert.Empty(productAdder(context.Products));
            }
        }

        [Fact]
        public void Can_use_Add_to_change_entity_state()
        {
            ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Unknown, EntityState.Added);
            ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Unchanged, EntityState.Added);
            ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Deleted, EntityState.Added);
            ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Modified, EntityState.Added);
            ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Added, EntityState.Added);
        }

        [Fact]
        public void Can_use_Attach_to_change_entity_state()
        {
            ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Unknown, EntityState.Unchanged);
            ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Unchanged, EntityState.Unchanged);
            ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Deleted, EntityState.Unchanged);
            ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Modified, EntityState.Unchanged);
            ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Added, EntityState.Unchanged);
        }

        [Fact]
        public void Can_use_Update_to_change_entity_state()
        {
            ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Unknown, EntityState.Modified);
            ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Unchanged, EntityState.Modified);
            ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Deleted, EntityState.Modified);
            ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Modified, EntityState.Modified);
            ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Added, EntityState.Modified);
        }

        [Fact]
        public void Can_use_Remove_to_change_entity_state()
        {
            ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Unknown, EntityState.Deleted);
            ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Unchanged, EntityState.Deleted);
            ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Deleted, EntityState.Deleted);
            ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Modified, EntityState.Deleted);
            ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Added, EntityState.Unknown);
        }

        private void ChangeStateWithMethod(Action<EarlyLearningCenter, Category> action, EntityState initialState, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter())
            {
                var entity = new Category { Name = "Beverages" };
                var entry = context.Entry(entity);

                entry.State = initialState;

                action(context, entity);

                Assert.Equal(expectedState, entry.State);
            }
        }

        [Fact]
        public void Can_add_new_entities_to_context_with_key_generation()
        {
            TrackEntitiesWithKeyGenerationTest((c, e) => c.Add(e).Entity);
        }

        private static void TrackEntitiesWithKeyGenerationTest(Func<DbSet<TheGu>, TheGu, TheGu> adder)
        {
            using (var context = new EarlyLearningCenter())
            {
                var gu1 = new TheGu { ShirtColor = "Red" };
                var gu2 = new TheGu { ShirtColor = "Still Red" };

                Assert.Same(gu1, adder(context.Gus, gu1));
                Assert.Same(gu2, adder(context.Gus, gu2));
                Assert.NotEqual(default(Guid), gu1.Id);
                Assert.NotEqual(default(Guid), gu2.Id);
                Assert.NotEqual(gu1.Id, gu2.Id);

                var categoryEntry = context.Entry(gu1);
                Assert.Same(gu1, categoryEntry.Entity);
                Assert.Equal(EntityState.Added, categoryEntry.State);

                categoryEntry = context.Entry(gu2);
                Assert.Same(gu2, categoryEntry.Entity);
                Assert.Equal(EntityState.Added, categoryEntry.State);
            }
        }

        private class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        private class TheGu
        {
            public Guid Id { get; set; }
            public string ShirtColor { get; set; }
        }

        private class EarlyLearningCenter : DbContext
        {
            public EarlyLearningCenter()
                : base(TestHelpers.CreateServiceProvider())
            {
            }

            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<TheGu> Gus { get; set; }

            protected internal override void OnConfiguring(DbContextOptions options)
            {
                options.UseInMemoryStore(persist: false);
            }
        }
    }
}
