// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
                var stateEntry = context.ChangeTracker.StateManager.GetOrCreateEntry(entity);

                Assert.Same(stateEntry, context.Entry(entity).StateEntry);
                Assert.Same(stateEntry, context.Entry((object)entity).StateEntry);
            }
        }

        [Fact]
        public void Can_get_and_change_state()
        {
            using (var context = new FreezerContext())
            {
                var entity = new Chunky();
                var stateEntry = context.Add(entity).StateEntry;

                context.Entry(entity).State = EntityState.Modified;
                Assert.Equal(EntityState.Modified, stateEntry.EntityState);
                Assert.Equal(EntityState.Modified, context.Entry(entity).State);

                context.Entry((object)entity).State = EntityState.Unchanged;
                Assert.Equal(EntityState.Unchanged, stateEntry.EntityState);
                Assert.Equal(EntityState.Unchanged, context.Entry((object)entity).State);
            }
        }

        [Fact]
        public void Can_use_entry_to_change_state_to_Added()
        {
            ChangeStateOnEntry(EntityState.Unknown, EntityState.Added);
            ChangeStateOnEntry(EntityState.Unchanged, EntityState.Added);
            ChangeStateOnEntry(EntityState.Deleted, EntityState.Added);
            ChangeStateOnEntry(EntityState.Modified, EntityState.Added);
            ChangeStateOnEntry(EntityState.Added, EntityState.Added);
        }

        [Fact]
        public void Can_use_entry_to_change_state_to_Unchanged()
        {
            ChangeStateOnEntry(EntityState.Unknown, EntityState.Unchanged);
            ChangeStateOnEntry(EntityState.Unchanged, EntityState.Unchanged);
            ChangeStateOnEntry(EntityState.Deleted, EntityState.Unchanged);
            ChangeStateOnEntry(EntityState.Modified, EntityState.Unchanged);
            ChangeStateOnEntry(EntityState.Added, EntityState.Unchanged);
        }

        [Fact]
        public void Can_use_entry_to_change_state_to_Modified()
        {
            ChangeStateOnEntry(EntityState.Unknown, EntityState.Modified);
            ChangeStateOnEntry(EntityState.Unchanged, EntityState.Modified);
            ChangeStateOnEntry(EntityState.Deleted, EntityState.Modified);
            ChangeStateOnEntry(EntityState.Modified, EntityState.Modified);
            ChangeStateOnEntry(EntityState.Added, EntityState.Modified);
        }

        [Fact]
        public void Can_use_entry_to_change_state_to_Deleted()
        {
            ChangeStateOnEntry(EntityState.Unknown, EntityState.Deleted);
            ChangeStateOnEntry(EntityState.Unchanged, EntityState.Deleted);
            ChangeStateOnEntry(EntityState.Deleted, EntityState.Deleted);
            ChangeStateOnEntry(EntityState.Modified, EntityState.Deleted);
            ChangeStateOnEntry(EntityState.Added, EntityState.Deleted);
        }

        [Fact]
        public void Can_use_entry_to_change_state_to_Unknown()
        {
            ChangeStateOnEntry(EntityState.Unknown, EntityState.Unknown);
            ChangeStateOnEntry(EntityState.Unchanged, EntityState.Unknown);
            ChangeStateOnEntry(EntityState.Deleted, EntityState.Unknown);
            ChangeStateOnEntry(EntityState.Modified, EntityState.Unknown);
            ChangeStateOnEntry(EntityState.Added, EntityState.Unknown);
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

                Assert.Equal("Monkey", context.Entry(entity).Property("Monkey").Name);
                Assert.Equal("Monkey", context.Entry((object)entity).Property("Monkey").Name);
            }
        }

        [Fact]
        public void Can_get_property_entry_by_lambda()
        {
            using (var context = new FreezerContext())
            {
                var entity = context.Add(new Chunky()).Entity;

                Assert.Equal("Monkey", context.Entry(entity).Property(e => e.Monkey).Name);
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
                : base(TestHelpers.CreateServiceProvider())
            {
            }

            public DbSet<Chunky> Icecream { get; set; }
        }
    }
}
