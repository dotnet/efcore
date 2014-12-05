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
