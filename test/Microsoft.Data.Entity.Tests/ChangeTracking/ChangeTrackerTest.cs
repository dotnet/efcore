// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ChangeTrackerTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "model",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new ChangeTracker(null)).ParamName);

            var changeTracker = new ChangeTracker(new Model());

            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => changeTracker.Entry(null)).ParamName);
            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => changeTracker.Entry<Random>(null)).ParamName);
        }

        [Fact]
        public void Entry_returns_tracking_entry_if_entity_is_already_tracked_otherwise_new_entry()
        {
            var tracker = new ChangeTracker(BuildModel());

            var category = new Category();
            var entry = tracker.Entry(category);

            Assert.IsType<EntityEntry<Category>>(entry);
            Assert.Equal(EntityState.Unknown, entry.State);
            Assert.Same(category, entry.Entity);
            entry.State = EntityState.Added;

            var entry2 = tracker.Entry(category);

            Assert.Same(entry.Entry, entry2.Entry);
            Assert.Equal(EntityState.Added, entry.State);
            Assert.Equal(EntityState.Added, entry2.State);
        }

        [Fact]
        public void Non_generic_Entry_returns_tracking_entry_if_entity_is_already_tracked_otherwise_new_entry()
        {
            var tracker = new ChangeTracker(BuildModel());

            var category = new Category();
            var entry = tracker.Entry((object)category);

            Assert.IsType<EntityEntry>(entry);
            Assert.Equal(EntityState.Unknown, entry.State);
            Assert.Same(category, entry.Entity);
            entry.State = EntityState.Added;

            var entry2 = tracker.Entry((object)category);

            Assert.Same(entry.Entry, entry2.Entry);
            Assert.Equal(EntityState.Added, entry.State);
            Assert.Equal(EntityState.Added, entry2.State);
        }

        [Fact]
        public void Can_get_all_entities()
        {
            var tracker = new ChangeTracker(BuildModel());

            new EntityEntry(tracker, new Category { Id = 77 }) { State = EntityState.Added };
            new EntityEntry(tracker, new Category { Id = 78 }) { State = EntityState.Added };
            new EntityEntry(tracker, new Product { Id = 77 }) { State = EntityState.Added };
            new EntityEntry(tracker, new Product { Id = 78 }) { State = EntityState.Added };

            Assert.Equal(4, tracker.Entries().Count());

            Assert.Equal(
                new[] { 77, 78 },
                tracker.Entries()
                    .Select(e => e.Entity)
                    .OfType<Category>()
                    .Select(e => e.Id)
                    .OrderBy(k => k)
                    .ToArray());

            Assert.Equal(
                new[] { 77, 78 },
                tracker.Entries()
                    .Select(e => e.Entity)
                    .OfType<Product>()
                    .Select(e => e.Id)
                    .OrderBy(k => k)
                    .ToArray());
        }

        [Fact]
        public void Can_get_all_entities_of_a_given_type()
        {
            var tracker = new ChangeTracker(BuildModel());

            new EntityEntry(tracker, new Category { Id = 77 }) { State = EntityState.Added };
            new EntityEntry(tracker, new Category { Id = 78 }) { State = EntityState.Added };
            new EntityEntry(tracker, new Product { Id = 77 }) { State = EntityState.Added };
            new EntityEntry(tracker, new Product { Id = 78 }) { State = EntityState.Added };

            Assert.Equal(4, tracker.Entries().Count());

            Assert.Equal(
                new[] { 77, 78 },
                tracker.Entries<Category>()
                    .Select(e => e.Entity)
                    .Select(e => e.Id)
                    .OrderBy(k => k)
                    .ToArray());

            Assert.Equal(
                new[] { 77, 78 },
                tracker.Entries<Product>()
                    .Select(e => e.Entity)
                    .Select(e => e.Id)
                    .OrderBy(k => k)
                    .ToArray());
        }

        [Fact]
        public void Can_get_model()
        {
            var model = BuildModel();
            Assert.Same(model, new ChangeTracker(model).Model);
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

        private static IModel BuildModel()
        {
            var model = new Model();
            var builder = new ModelBuilder(model);

            builder.Entity<Category>()
                .Key(e => e.Id)
                .Properties(
                    pb =>
                    {
                        pb.Property(c => c.Id);
                        pb.Property(c => c.Name);
                    });

            builder.Entity<Product>()
                .Key(e => e.Id)
                .Properties(
                    pb =>
                    {
                        pb.Property(c => c.Id);
                        pb.Property(c => c.Name);
                        pb.Property(c => c.Price);
                    });

            return model;
        }

        #endregion
    }
}
