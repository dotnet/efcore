// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests
{
    public class DbSetTest
    {
        [Fact]
        public void Can_add_existing_entities_to_context_to_be_deleted()
        {
            TrackEntitiesTest((c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);
        }

        [Fact]
        public void Can_add_new_entities_to_context_graph()
        {
            TrackEntitiesTest((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_attached_graph()
        {
            TrackEntitiesTest((c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Unchanged);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_updated_graph()
        {
            TrackEntitiesTest((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Modified);
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

                Assert.Same(categoryEntry1.GetInfrastructure(), context.Entry(category1).GetInfrastructure());
                Assert.Same(categoryEntry2.GetInfrastructure(), context.Entry(category2).GetInfrastructure());
                Assert.Same(productEntry1.GetInfrastructure(), context.Entry(product1).GetInfrastructure());
                Assert.Same(productEntry2.GetInfrastructure(), context.Entry(product2).GetInfrastructure());
            }
        }

        [Fact]
        public void Can_add_multiple_new_entities_to_set()
        {
            TrackMultipleEntitiesTest((c, e) => c.Categories.AddRange(e[0], e[1]), (c, e) => c.Products.AddRange(e[0], e[1]), EntityState.Added);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_set_to_be_attached()
        {
            TrackMultipleEntitiesTest((c, e) => c.Categories.AttachRange(e[0], e[1]), (c, e) => c.Products.AttachRange(e[0], e[1]), EntityState.Unchanged);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_set_to_be_updated()
        {
            TrackMultipleEntitiesTest((c, e) => c.Categories.UpdateRange(e[0], e[1]), (c, e) => c.Products.UpdateRange(e[0], e[1]), EntityState.Modified);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_set_to_be_deleted()
        {
            TrackMultipleEntitiesTest((c, e) => c.Categories.RemoveRange(e[0], e[1]), (c, e) => c.Products.RemoveRange(e[0], e[1]), EntityState.Deleted);
        }

        private static void TrackMultipleEntitiesTest(
            Action<EarlyLearningCenter, Category[]> categoryAdder,
            Action<EarlyLearningCenter, Product[]> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter())
            {
                var category1 = new Category { Id = 1, Name = "Beverages" };
                var category2 = new Category { Id = 2, Name = "Foods" };
                var product1 = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var product2 = new Product { Id = 2, Name = "Bovril", Price = 4.99m };

                categoryAdder(context, new[] { category1, category2 });
                productAdder(context, new[] { product1, product2 });

                Assert.Same(category1, context.Entry(category1).Entity);
                Assert.Same(category2, context.Entry(category2).Entity);
                Assert.Same(product1, context.Entry(product1).Entity);
                Assert.Same(product2, context.Entry(product2).Entity);

                Assert.Same(category1, context.Entry(category1).Entity);
                Assert.Equal(expectedState, context.Entry(category1).State);
                Assert.Same(category2, context.Entry(category2).Entity);
                Assert.Equal(expectedState, context.Entry(category2).State);

                Assert.Same(product1, context.Entry(product1).Entity);
                Assert.Equal(expectedState, context.Entry(product1).State);
                Assert.Same(product2, context.Entry(product2).Entity);
                Assert.Equal(expectedState, context.Entry(product2).State);
            }
        }

        [Fact]
        public void Can_add_no_new_entities_to_set()
        {
            TrackNoEntitiesTest(c => c.Categories.AddRange(), c => c.Products.AddRange());
        }

        [Fact]
        public void Can_add_no_existing_entities_to_set_to_be_attached()
        {
            TrackNoEntitiesTest(c => c.Categories.AttachRange(), c => c.Products.AttachRange());
        }

        [Fact]
        public void Can_add_no_existing_entities_to_set_to_be_updated()
        {
            TrackNoEntitiesTest(c => c.Categories.UpdateRange(), c => c.Products.UpdateRange());
        }

        [Fact]
        public void Can_add_no_existing_entities_to_set_to_be_deleted()
        {
            TrackNoEntitiesTest(c => c.Categories.RemoveRange(), c => c.Products.RemoveRange());
        }

        private static void TrackNoEntitiesTest(Action<EarlyLearningCenter> categoryAdder, Action<EarlyLearningCenter> productAdder)
        {
            using (var context = new EarlyLearningCenter())
            {
                categoryAdder(context);
                productAdder(context);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_set_to_be_deleted_Enumerable()
        {
            TrackMultipleEntitiesTestEnumerable((c, e) => c.Categories.RemoveRange(e), (c, e) => c.Products.RemoveRange(e), EntityState.Deleted);
        }

        [Fact]
        public void Can_add_multiple_new_entities_to_set_Enumerable_graph()
        {
            TrackMultipleEntitiesTestEnumerable((c, e) => c.Categories.AddRange(e), (c, e) => c.Products.AddRange(e), EntityState.Added);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_set_to_be_attached_Enumerable_graph()
        {
            TrackMultipleEntitiesTestEnumerable((c, e) => c.Categories.AttachRange(e), (c, e) => c.Products.AttachRange(e), EntityState.Unchanged);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_set_to_be_updated_Enumerable_graph()
        {
            TrackMultipleEntitiesTestEnumerable((c, e) => c.Categories.UpdateRange(e), (c, e) => c.Products.UpdateRange(e), EntityState.Modified);
        }

        private static void TrackMultipleEntitiesTestEnumerable(
            Action<EarlyLearningCenter, IEnumerable<Category>> categoryAdder,
            Action<EarlyLearningCenter, IEnumerable<Product>> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter())
            {
                var category1 = new Category { Id = 1, Name = "Beverages" };
                var category2 = new Category { Id = 2, Name = "Foods" };
                var product1 = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var product2 = new Product { Id = 2, Name = "Bovril", Price = 4.99m };

                categoryAdder(context, new List<Category> { category1, category2 });
                productAdder(context, new List<Product> { product1, product2 });

                Assert.Same(category1, context.Entry(category1).Entity);
                Assert.Same(category2, context.Entry(category2).Entity);
                Assert.Same(product1, context.Entry(product1).Entity);
                Assert.Same(product2, context.Entry(product2).Entity);

                Assert.Same(category1, context.Entry(category1).Entity);
                Assert.Equal(expectedState, context.Entry(category1).State);
                Assert.Same(category2, context.Entry(category2).Entity);
                Assert.Equal(expectedState, context.Entry(category2).State);

                Assert.Same(product1, context.Entry(product1).Entity);
                Assert.Equal(expectedState, context.Entry(product1).State);
                Assert.Same(product2, context.Entry(product2).Entity);
                Assert.Equal(expectedState, context.Entry(product2).State);
            }
        }

        [Fact]
        public void Can_add_no_existing_entities_to_set_to_be_deleted_Enumerable()
        {
            TrackNoEntitiesTestEnumerable((c, e) => c.Categories.RemoveRange(e), (c, e) => c.Products.RemoveRange(e));
        }

        [Fact]
        public void Can_add_no_new_entities_to_set_Enumerable_graph()
        {
            TrackNoEntitiesTestEnumerable((c, e) => c.Categories.AddRange(e), (c, e) => c.Products.AddRange(e));
        }

        [Fact]
        public void Can_add_no_existing_entities_to_set_to_be_attached_Enumerable_graph()
        {
            TrackNoEntitiesTestEnumerable((c, e) => c.Categories.AttachRange(e), (c, e) => c.Products.AttachRange(e));
        }

        [Fact]
        public void Can_add_no_existing_entities_to_set_to_be_updated_Enumerable_graph()
        {
            TrackNoEntitiesTestEnumerable((c, e) => c.Categories.UpdateRange(e), (c, e) => c.Products.UpdateRange(e));
        }

        private static void TrackNoEntitiesTestEnumerable(
            Action<EarlyLearningCenter, IEnumerable<Category>> categoryAdder,
            Action<EarlyLearningCenter, IEnumerable<Product>> productAdder)
        {
            using (var context = new EarlyLearningCenter())
            {
                categoryAdder(context, new HashSet<Category>());
                productAdder(context, new HashSet<Product>());
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        [Fact]
        public void Can_use_Add_to_change_entity_state()
        {
            ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Detached, EntityState.Added);
            ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Unchanged, EntityState.Added);
            ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Deleted, EntityState.Added);
            ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Modified, EntityState.Added);
            ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Added, EntityState.Added);
        }

        [Fact]
        public void Can_use_Attach_to_change_entity_state()
        {
            ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Detached, EntityState.Unchanged);
            ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Unchanged, EntityState.Unchanged);
            ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Deleted, EntityState.Unchanged);
            ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Modified, EntityState.Unchanged);
            ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Added, EntityState.Unchanged);
        }

        [Fact]
        public void Can_use_Update_to_change_entity_state()
        {
            ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Detached, EntityState.Modified);
            ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Unchanged, EntityState.Modified);
            ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Deleted, EntityState.Modified);
            ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Modified, EntityState.Modified);
            ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Added, EntityState.Modified);
        }

        [Fact]
        public void Can_use_Remove_to_change_entity_state()
        {
            ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Detached, EntityState.Deleted);
            ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Unchanged, EntityState.Deleted);
            ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Deleted, EntityState.Deleted);
            ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Modified, EntityState.Deleted);
            ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Added, EntityState.Detached);
        }

        private void ChangeStateWithMethod(Action<EarlyLearningCenter, Category> action, EntityState initialState, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter())
            {
                var entity = new Category { Id = 1, Name = "Beverages" };
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

        [Fact]
        public void Can_get_scoped_service_provider()
        {
            using (var context = new EarlyLearningCenter())
            {
                Assert.Same(
                    ((IInfrastructure<IServiceProvider>)context).Instance,
                    ((IInfrastructure<IServiceProvider>)context.Products).Instance);
            }
        }

#if NET451
        [Fact]
        public void Throws_when_using_with_IListSource()
        {
            using (var context = new EarlyLearningCenter())
            {
                Assert.Equal(CoreStrings.DataBindingWithIListSource,
                    Assert.Throws<NotSupportedException>(() => ((IListSource)context.Gus).GetList()).Message);
            }
        }
#endif

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
                : base(TestHelpers.Instance.CreateServiceProvider())
            {
            }

            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<TheGu> Gus { get; set; }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase();
            }
        }
    }
}
