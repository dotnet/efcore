// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityEntryTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "changeTracker",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new EntityEntry(null, new Random())).ParamName);
            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new EntityEntry(new ChangeTracker(new Model()), null)).ParamName);
            Assert.Equal(
                "changeTracker",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new EntityEntry<Random>(null, new Random())).ParamName);
            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new EntityEntry<Random>(new ChangeTracker(new Model()), null)).ParamName);

            var entry = new EntityEntry(new ChangeTracker(BuildModel()), new Category());

            Assert.Equal(
                Strings.ArgumentIsEmpty("propertyName"),
                Assert.Throws<ArgumentException>(() => entry.Property("")).Message);

            var genericEntry = new EntityEntry<Category>(new ChangeTracker(BuildModel()), new Category());

            Assert.Equal(
                "propertyExpression",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => genericEntry.Property((Expression<Func<Category, int>>)null)).ParamName);
        }

        [Fact]
        public void Can_obtain_entity_instance()
        {
            var entity = new Category();
            var changeTracker = new ChangeTracker(BuildModel());

            Assert.Same(entity, new EntityEntry(changeTracker, entity).Entity);
            Assert.Same(entity, new EntityEntry<Category>(changeTracker, entity).Entity);
        }

        [Fact]
        public void New_entries_have_state_Unknown_and_are_not_tracked()
        {
            var entity = new Category();
            var changeTracker = new ChangeTracker(BuildModel());

            Assert.Equal(EntityState.Unknown, new EntityEntry(changeTracker, entity).State);
            Assert.Equal(EntityState.Unknown, new EntityEntry<Category>(changeTracker, entity).State);

            Assert.Equal(0, changeTracker.Entries().Count());
        }

        [Fact]
        public void Can_obtain_entity_key()
        {
            var entity = new Category { Id = 77 };
            var changeTracker = new ChangeTracker(BuildModel());

            Assert.Equal(77, new EntityEntry(changeTracker, entity).Key.Value);
            Assert.Equal(77, new EntityEntry<Category>(changeTracker, entity).Key.Value);
        }

        [Fact]
        public void Changing_state_from_Unknown_causes_entity_to_start_tracking()
        {
            var entity = new Category();
            var changeTracker = new ChangeTracker(BuildModel());

            new EntityEntry<Category>(changeTracker, entity) { State = EntityState.Added };

            Assert.Same(entity, changeTracker.Entries().Single().Entity);
            Assert.Equal(EntityState.Added, changeTracker.Entries().Single().State);
        }

        [Fact]
        public void Changing_state_to_Unknown_causes_entity_to_stop_tracking()
        {
            var entity = new Category();
            var changeTracker = new ChangeTracker(BuildModel());

            var entry = new EntityEntry<Category>(changeTracker, entity) { State = EntityState.Added };

            Assert.Equal(1, changeTracker.Entries().Count());

            entry.State = EntityState.Unknown;

            Assert.Equal(0, changeTracker.Entries().Count());
        }

        [Fact]
        public void Changing_state_to_Modified_or_Unchanged_causes_all_properties_to_be_marked_accordingly()
        {
            var entity = new Category();
            var changeTracker = new ChangeTracker(BuildModel());

            var entry = new EntityEntry<Category>(changeTracker, entity) { State = EntityState.Added };

            Assert.False(entry.Property(e => e.Id).IsModified);
            Assert.False(entry.Property(e => e.Name).IsModified);

            entry.State = EntityState.Modified;

            Assert.True(entry.Property(e => e.Id).IsModified);
            Assert.True(entry.Property(e => e.Name).IsModified);

            entry.State = EntityState.Unchanged;

            Assert.False(entry.Property(e => e.Id).IsModified);
            Assert.False(entry.Property(e => e.Name).IsModified);
        }

        [Fact]
        public void Can_get_property_entry_by_name()
        {
            var entity = new Category();
            var changeTracker = new ChangeTracker(BuildModel());

            var entry = new EntityEntry<Category>(changeTracker, entity) { State = EntityState.Added };

            Assert.Equal("Name", entry.Property("Name").Name);
        }

        [Fact]
        public void Can_get_property_entry_by_lambda()
        {
            var entity = new Category();
            var changeTracker = new ChangeTracker(BuildModel());

            var entry = new EntityEntry<Category>(changeTracker, entity) { State = EntityState.Added };

            Assert.Equal("Name", entry.Property(e => e.Name).Name);
        }

        #region Fixture

        public class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }
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

            return model;
        }

        #endregion
    }
}
