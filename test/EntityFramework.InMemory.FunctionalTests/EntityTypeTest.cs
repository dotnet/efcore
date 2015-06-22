// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class EntityTypeTest : IClassFixture<InMemoryFixture>
    {
        public class Root
        {
        }

        public class Leaf : Root
        {
        }

        [Fact]
        public void Original_value_index_maintenance_when_inheritance()
        {
            var model = new Model();
            var root = model.AddEntityType(typeof(Root));
            var leaf = model.AddEntityType(typeof(Leaf));

            var a = leaf.AddProperty("A", typeof(int), shadowProperty: true);

            leaf.BaseType = root;

            var b = root.AddProperty("B", typeof(int), shadowProperty: true);

            Assert.Equal(0, b.GetOriginalValueIndex());
            Assert.Equal(1, a.GetOriginalValueIndex());
        }

        [Fact]
        public void Introduce_duplicate_property_when_inheritance()
        {
            var model = new Model();
            var root = model.AddEntityType(typeof(Root));
            var leaf = model.AddEntityType(typeof(Leaf));

            leaf.AddProperty("A", typeof(int), shadowProperty: true);

            leaf.BaseType = root;

            Assert.Throws<InvalidOperationException>(() =>
                root.AddProperty("A", typeof(int), shadowProperty: true));
        }

        [Fact]
        public void Introduce_duplicate_property_when_inheritance2()
        {
            var model = new Model();
            var root = model.AddEntityType(typeof(Root));
            var leaf = model.AddEntityType(typeof(Leaf));

            leaf.AddProperty("A", typeof(int), shadowProperty: true);
            root.AddProperty("A", typeof(int), shadowProperty: true);
            
            Assert.Throws<InvalidOperationException>(() => leaf.BaseType = root);
        }

        [Fact]
        public void Can_use_different_entity_types_end_to_end()
        {
            Can_add_update_delete_end_to_end<Private>();
            Can_add_update_delete_end_to_end<object>();
            Can_add_update_delete_end_to_end<List<Private>>();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Private
        {
        }

        private void Can_add_update_delete_end_to_end<T>()
            where T : class
        {
            var type = typeof(T);
            var model = new Model();

            var entityType = model.AddEntityType(type);
            var idProperty = entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            var nameProperty = entityType.GetOrAddProperty("Name", typeof(string), shadowProperty: true);
            entityType.GetOrSetPrimaryKey(idProperty);

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseModel(model);
            optionsBuilder.UseInMemoryDatabase();

            T entity;
            using (var context = new DbContext(_fixture.ServiceProvider, optionsBuilder.Options))
            {
                var entry = context.ChangeTracker.GetService().CreateNewEntry(entityType);
                entity = (T)entry.Entity;

                entry[idProperty] = 42;
                entry[nameProperty] = "The";

                entry.SetEntityState(EntityState.Added);

                context.SaveChanges();
            }

            using (var context = new DbContext(_fixture.ServiceProvider, optionsBuilder.Options))
            {
                var entityFromStore = context.Set<T>().Single();
                var entityEntry = context.Entry(entityFromStore);

                Assert.NotSame(entity, entityFromStore);
                Assert.Equal(42, entityEntry.Property(idProperty.Name).CurrentValue);
                Assert.Equal("The", entityEntry.Property(nameProperty.Name).CurrentValue);

                entityEntry.GetService()[nameProperty] = "A";

                context.Update(entityFromStore);

                context.SaveChanges();
            }

            using (var context = new DbContext(_fixture.ServiceProvider, optionsBuilder.Options))
            {
                var entityFromStore = context.Set<T>().Single();
                var entry = context.Entry(entityFromStore);

                Assert.Equal("A", entry.Property(nameProperty.Name).CurrentValue);

                context.Remove(entityFromStore);

                context.SaveChanges();
            }

            using (var context = new DbContext(_fixture.ServiceProvider, optionsBuilder.Options))
            {
                Assert.Equal(0, context.Set<T>().Count());
            }
        }

        private readonly InMemoryFixture _fixture;

        public EntityTypeTest(InMemoryFixture fixture)
        {
            _fixture = fixture;
        }
    }
}
