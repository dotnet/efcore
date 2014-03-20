// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityContextTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            using (var context = new EarlyLearningCenter())
            {
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.Throws<ArgumentNullException>(() => context.Add<Random>(null)).ParamName);
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.ThrowsAsync<ArgumentNullException>(() => context.AddAsync<Random>(null)).Result.ParamName);
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.Throws<ArgumentNullException>(
                        () => context.AddAsync<Random>(null, new CancellationToken()).GetAwaiter().GetResult()).ParamName);
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.Throws<ArgumentNullException>(() => context.Update<Random>(null)).ParamName);
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.ThrowsAsync<ArgumentNullException>(() => context.UpdateAsync<Random>(null)).Result.ParamName);
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.ThrowsAsync<ArgumentNullException>(() => context.UpdateAsync<Random>(null, new CancellationToken())).Result.ParamName);
            }
        }

        [Fact]
        public void Can_add_new_entities_to_context()
        {
            TrackEntitiesTest((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);
        }

        [Fact]
        public void Can_add_new_entities_to_context_async()
        {
            TrackEntitiesTest((c, e) => c.AddAsync(e).Result, (c, e) => c.AddAsync(e).Result, EntityState.Added);

            TrackEntitiesTest(
                (c, e) => c.AddAsync(e, new CancellationToken()).Result,
                (c, e) => c.AddAsync(e, new CancellationToken()).Result,
                EntityState.Added);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_updated()
        {
            TrackEntitiesTest((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Modified);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_updated_async()
        {
            TrackEntitiesTest((c, e) => c.UpdateAsync(e).Result, (c, e) => c.UpdateAsync(e).Result, EntityState.Modified);

            TrackEntitiesTest(
                (c, e) => c.UpdateAsync(e, new CancellationToken()).Result,
                (c, e) => c.UpdateAsync(e, new CancellationToken()).Result,
                EntityState.Modified);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_deleted()
        {
            TrackEntitiesTest((c, e) => c.Delete(e), (c, e) => c.Delete(e), EntityState.Deleted);
        }

        private static void TrackEntitiesTest(
            Func<EntityContext, Category, Category> categoryAdder,
            Func<EntityContext, Product, Product> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter())
            {
                var category1 = new Category { Id = 1, Name = "Beverages" };
                var category2 = new Category { Id = 2, Name = "Foods" };
                var product1 = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var product2 = new Product { Id = 2, Name = "Bovril", Price = 4.99m };

                Assert.Same(category1, categoryAdder(context, category1));
                Assert.Same(category2, categoryAdder(context, category2));
                Assert.Same(product1, productAdder(context, product1));
                Assert.Same(product2, productAdder(context, product2));

                var categoryEntry = context.ChangeTracker.Entry(category1);
                Assert.Same(category1, categoryEntry.Entity);
                Assert.Equal(expectedState, categoryEntry.State);

                categoryEntry = context.ChangeTracker.Entry(category2);
                Assert.Same(category2, categoryEntry.Entity);
                Assert.Equal(expectedState, categoryEntry.State);

                var productEntry = context.ChangeTracker.Entry(product1);
                Assert.Same(product1, productEntry.Entity);
                Assert.Equal(expectedState, productEntry.State);

                productEntry = context.ChangeTracker.Entry(product2);
                Assert.Same(product2, productEntry.Entity);
                Assert.Equal(expectedState, productEntry.State);
            }
        }

        [Fact]
        public void Can_add_new_entities_to_context_with_key_generation()
        {
            TrackEntitiesWithKeyGenerationTest((c, e) => c.Add(e));
        }

        [Fact]
        public void Can_add_new_entities_to_context_with_key_generation_async()
        {
            TrackEntitiesWithKeyGenerationTest((c, e) => c.AddAsync(e).Result);
            TrackEntitiesWithKeyGenerationTest((c, e) => c.AddAsync(e, new CancellationToken()).Result);
        }

        private static void TrackEntitiesWithKeyGenerationTest(Func<EntityContext, TheGu, TheGu> adder)
        {
            using (var context = new EarlyLearningCenter())
            {
                var gu1 = new TheGu { ShirtColor = "Red" };
                var gu2 = new TheGu { ShirtColor = "Still Red" };

                Assert.Same(gu1, adder(context, gu1));
                Assert.Same(gu2, adder(context, gu2));
                Assert.NotEqual(default(Guid), gu1.Id);
                Assert.NotEqual(default(Guid), gu2.Id);
                Assert.NotEqual(gu1.Id, gu2.Id);

                var categoryEntry = context.ChangeTracker.Entry(gu1);
                Assert.Same(gu1, categoryEntry.Entity);
                Assert.Equal(EntityState.Added, categoryEntry.State);

                categoryEntry = context.ChangeTracker.Entry(gu2);
                Assert.Same(gu2, categoryEntry.Entity);
                Assert.Equal(EntityState.Added, categoryEntry.State);
            }
        }

        [Fact]
        public void Context_can_build_model_using_EntitySet_properties()
        {
            using (var context = new EarlyLearningCenter())
            {
                Assert.Equal(
                    new[] { "Category", "Product", "TheGu" },
                    context.Model.EntityTypes.Select(e => e.Name).ToArray());

                var categoryType = context.Model.GetEntityType(typeof(Category));
                Assert.Equal("Id", categoryType.GetKey().Properties.Single().Name);
                Assert.Equal(
                    new[] { "Id", "Name" },
                    categoryType.Properties.Select(p => p.Name).ToArray());

                var productType = context.Model.GetEntityType(typeof(Product));
                Assert.Equal("Id", productType.GetKey().Properties.Single().Name);
                Assert.Equal(
                    new[] { "Id", "Name", "Price" },
                    productType.Properties.Select(p => p.Name).ToArray());

                var guType = context.Model.GetEntityType(typeof(TheGu));
                Assert.Equal("Id", guType.GetKey().Properties.Single().Name);
                Assert.Equal(
                    new[] { "Id", "ShirtColor" },
                    guType.Properties.Select(p => p.Name).ToArray());
            }
        }

        [Fact]
        public void Context_will_use_explicit_model_if_set_in_config()
        {
            var model = new Model();
            model.AddEntityType(new EntityType(typeof(TheGu)));

            var configuration = new EntityConfigurationBuilder()
                .UseModel(model)
                .BuildConfiguration(); 

            using (var context = new EarlyLearningCenter(configuration))
            {
                Assert.Equal(
                    new[] { "TheGu" },
                    context.Model.EntityTypes.Select(e => e.Name).ToArray());
            }
        }

        [Fact]
        public void Context_initializes_all_EntitySet_properties_with_setters()
        {
            using (var context = new ContextWithSets())
            {
                Assert.NotNull(context.Products);
                Assert.NotNull(context.Categories);
                Assert.NotNull(context.GetGus());
                Assert.Null(context.NoSetter);
            }
        }

        public class ContextWithSets : EntityContext
        {
            private readonly EntitySet<Random> _noSetter = null;

            public EntitySet<Product> Products { get; set; }
            public EntitySet<Category> Categories { get; private set; }
            private EntitySet<TheGu> Gus { get; set; }

            public EntitySet<Random> NoSetter
            {
                get { return _noSetter; }
            }

            public EntitySet<TheGu> GetGus()
            {
                return Gus;
            }
        }

        [Fact]
        public void Set_and_non_generic_set_always_return_same_instance_returns_a_new_EntitySet_for_the_given_type()
        {
            using (var context = new ContextWithSets())
            {
                var entitySet = context.Set<Product>();
                Assert.NotNull(entitySet);
                Assert.Same(entitySet, context.Set<Product>());
                Assert.Same(entitySet, context.Set(typeof(Product)));
                Assert.Same(entitySet, context.Products);
            }
        }

        #region Fixture

        public class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        public class TheGu
        {
            public Guid Id { get; set; }
            public string ShirtColor { get; set; }
        }

        public class EarlyLearningCenter : EntityContext
        {
            public EarlyLearningCenter(EntityConfiguration configuration)
                : base(configuration)
            {
            }

            public EarlyLearningCenter()
            {
            }

            public EntitySet<Product> Products { get; set; }
            public EntitySet<Category> Categories { get; set; }
            public EntitySet<TheGu> Gus { get; set; }
        }

        #endregion
    }
}
