// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class EntityEntryTest
    {
        [Fact]
        public void Can_obtain_entity_instance()
        {
            using (var context = new FreezerContext())
            {
                var entity = new Chunky();
                context.Add(entity);

                Assert.Same(entity, context.Entry(entity).Entity);
                Assert.Same(entity, context.Entry((object)entity).Entity);
            }
        }

        [Fact]
        public void Can_obtain_context()
        {
            using (var context = new FreezerContext())
            {
                var entity = context.Add(new Chunky()).Entity;

                Assert.Same(context, context.Entry(entity).Context);
                Assert.Same(context, context.Entry((object)entity).Context);
            }
        }

        [Fact]
        public void Can_obtain_underlying_state_entry()
        {
            using (var context = new FreezerContext())
            {
                var entity = context.Add(new Chunky()).Entity;
                var entry = context.ChangeTracker.GetService().GetOrCreateEntry(entity);

                Assert.Same(entry, context.Entry(entity).GetService());
                Assert.Same(entry, context.Entry((object)entity).GetService());
            }
        }

        [Fact]
        public void Can_get_metadata()
        {
            using (var context = new FreezerContext())
            {
                var entity = context.Add(new Chunky()).Entity;
                var entityType = context.Model.GetEntityType(typeof(Chunky));

                Assert.Same(entityType, context.Entry(entity).Metadata);
                Assert.Same(entityType, context.Entry((object)entity).Metadata);
            }
        }

        [Fact]
        public void Can_get_and_change_state()
        {
            using (var context = new FreezerContext())
            {
                var entity = new Chunky();
                var entry = context.Add(entity).GetService();

                context.Entry(entity).State = EntityState.Modified;
                Assert.Equal(EntityState.Modified, entry.EntityState);
                Assert.Equal(EntityState.Modified, context.Entry(entity).State);

                context.Entry((object)entity).State = EntityState.Unchanged;
                Assert.Equal(EntityState.Unchanged, entry.EntityState);
                Assert.Equal(EntityState.Unchanged, context.Entry((object)entity).State);
            }
        }

        [Fact]
        public void Can_use_entry_to_change_state_to_Added()
        {
            ChangeStateOnEntry(EntityState.Detached, EntityState.Added);
            ChangeStateOnEntry(EntityState.Unchanged, EntityState.Added);
            ChangeStateOnEntry(EntityState.Deleted, EntityState.Added);
            ChangeStateOnEntry(EntityState.Modified, EntityState.Added);
            ChangeStateOnEntry(EntityState.Added, EntityState.Added);
        }

        [Fact]
        public void Can_use_entry_to_change_state_to_Unchanged()
        {
            ChangeStateOnEntry(EntityState.Detached, EntityState.Unchanged);
            ChangeStateOnEntry(EntityState.Unchanged, EntityState.Unchanged);
            ChangeStateOnEntry(EntityState.Deleted, EntityState.Unchanged);
            ChangeStateOnEntry(EntityState.Modified, EntityState.Unchanged);
            ChangeStateOnEntry(EntityState.Added, EntityState.Unchanged);
        }

        [Fact]
        public void Can_use_entry_to_change_state_to_Modified()
        {
            ChangeStateOnEntry(EntityState.Detached, EntityState.Modified);
            ChangeStateOnEntry(EntityState.Unchanged, EntityState.Modified);
            ChangeStateOnEntry(EntityState.Deleted, EntityState.Modified);
            ChangeStateOnEntry(EntityState.Modified, EntityState.Modified);
            ChangeStateOnEntry(EntityState.Added, EntityState.Modified);
        }

        [Fact]
        public void Can_use_entry_to_change_state_to_Deleted()
        {
            ChangeStateOnEntry(EntityState.Detached, EntityState.Deleted);
            ChangeStateOnEntry(EntityState.Unchanged, EntityState.Deleted);
            ChangeStateOnEntry(EntityState.Deleted, EntityState.Deleted);
            ChangeStateOnEntry(EntityState.Modified, EntityState.Deleted);
            ChangeStateOnEntry(EntityState.Added, EntityState.Deleted);
        }

        [Fact]
        public void Can_use_entry_to_change_state_to_Unknown()
        {
            ChangeStateOnEntry(EntityState.Detached, EntityState.Detached);
            ChangeStateOnEntry(EntityState.Unchanged, EntityState.Detached);
            ChangeStateOnEntry(EntityState.Deleted, EntityState.Detached);
            ChangeStateOnEntry(EntityState.Modified, EntityState.Detached);
            ChangeStateOnEntry(EntityState.Added, EntityState.Detached);
        }

        private void ChangeStateOnEntry(EntityState initialState, EntityState expectedState)
        {
            using (var context = new FreezerContext())
            {
                var entry = context.Add(new Chunky());

                entry.State = initialState;
                entry.State = expectedState;

                Assert.Equal(expectedState, entry.State);
            }
        }

        [Fact]
        public void Can_get_property_entry_by_name()
        {
            using (var context = new FreezerContext())
            {
                var entity = context.Add(new Chunky()).Entity;

                Assert.Equal("Monkey", context.Entry(entity).Property("Monkey").Metadata.Name);
                Assert.Equal("Monkey", context.Entry((object)entity).Property("Monkey").Metadata.Name);
            }
        }

        [Fact]
        public void Can_get_generic_property_entry_by_name()
        {
            using (var context = new FreezerContext())
            {
                var entity = context.Add(new Chunky()).Entity;

                Assert.Equal("Monkey", context.Entry(entity).Property<int>("Monkey").Metadata.Name);
            }
        }

        [Fact]
        public void Throws_when_wrong_generic_type_is_used_while_getting_property_entry_by_name()
        {
            using (var context = new FreezerContext())
            {
                var entity = context.Add(new Chunky()).Entity;

                Assert.Equal(Strings.WrongGenericPropertyType("Monkey", entity.GetType(), typeof(int).Name, typeof(string).Name),
                    Assert.Throws<ArgumentException>(() => context.Entry(entity).Property<string>("Monkey")).Message);
            }
        }

        [Fact]
        public void Can_get_generic_property_entry_by_lambda()
        {
            using (var context = new FreezerContext())
            {
                var entity = context.Add(new Chunky()).Entity;

                Assert.Equal("Monkey", context.Entry(entity).Property(e => e.Monkey).Metadata.Name);
            }
        }

        [Fact]
        public void Throws_when_wrong_property_name_is_used_while_getting_property_entry_by_name()
        {
            using (var context = new FreezerContext())
            {
                var entity = context.Add(new Chunky()).Entity;

                Assert.Equal(Strings.PropertyNotFound("Chimp", entity.GetType()),
                    Assert.Throws<ModelItemNotFoundException>(() => context.Entry(entity).Property("Chimp").Metadata.Name).Message);
                Assert.Equal(Strings.PropertyNotFound("Chimp", entity.GetType()),
                    Assert.Throws<ModelItemNotFoundException>(() => context.Entry((object)entity).Property("Chimp").Metadata.Name).Message);
                Assert.Equal(Strings.PropertyNotFound("Chimp", entity.GetType()),
                    Assert.Throws<ModelItemNotFoundException>(() => context.Entry(entity).Property<int>("Chimp").Metadata.Name).Message);
            }
        }

        private class Chunky
        {
            public int Monkey { get; set; }
            public int Id { get; set; }
        }

        private class FreezerContext : DbContext
        {
            public FreezerContext()
                : base(TestHelpers.Instance.CreateServiceProvider())
            {
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
                => optionsBuilder.UseInMemoryDatabase();

            public DbSet<Chunky> Icecream { get; set; }
        }
    }
}
