// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class KeyValueEntityTrackerTest
    {
        [Fact]
        public void Entities_with_default_value_object_key_values_are_made_Added()
        {
            using (var context = new StoteInTheSnow())
            {
                var entry = context.Entry(new Stoat());
                TrackEntity(entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Added, entry.State);
                Assert.Equal(1, entry.Entity.Id);

                entry = context.Entry(new Stoat { Id = 77 });
                TrackEntity(entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Unchanged, entry.State);
                Assert.Equal(77, entry.Entity.Id);

                entry = context.Entry(new Stoat { Id = 78 });
                TrackEntity(entry, updateExistingEntities: true);

                Assert.Equal(EntityState.Modified, entry.State);
                Assert.Equal(78, entry.Entity.Id);
            }
        }

        [Fact]
        public void Entities_with_default_reference_key_values_are_made_Added()
        {
            using (var context = new StoteInTheSnow())
            {
                var entry = context.Entry(new StoatInACoat());
                TrackEntity(entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Added, entry.State);
                Assert.NotEqual(Guid.NewGuid(), Guid.Parse(entry.Entity.Id));

                entry = context.Entry(new StoatInACoat { Id = "Brrrr! It's chilly." });
                TrackEntity(entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Unchanged, entry.State);
                Assert.Equal("Brrrr! It's chilly.", entry.Entity.Id);

                entry = context.Entry(new StoatInACoat { Id = "Hot chocolate please!" });
                TrackEntity(entry, updateExistingEntities: true);

                Assert.Equal(EntityState.Modified, entry.State);
                Assert.Equal("Hot chocolate please!", entry.Entity.Id);
            }
        }

        [Fact]
        public void Entities_with_composite_key_with_any_default_values_are_made_Added()
        {
            using (var context = new StoteInTheSnow())
            {
                var entry = context.Entry(new CompositeStoat());
                TrackEntity(entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Added, entry.State);
                Assert.NotEqual(Guid.NewGuid(), entry.Entity.Id1);
                Assert.NotEqual(Guid.NewGuid(), Guid.Parse(entry.Entity.Id2));

                var guid = Guid.NewGuid();
                entry = context.Entry(new CompositeStoat { Id1 = guid });
                TrackEntity(entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Added, entry.State);
                Assert.Equal(guid, entry.Entity.Id1);
                Assert.NotEqual(Guid.NewGuid(), Guid.Parse(entry.Entity.Id2));

                entry = context.Entry(new CompositeStoat { Id2 = "Ready for winter!" });
                TrackEntity(entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Added, entry.State);
                Assert.NotEqual(Guid.NewGuid(), entry.Entity.Id1);
                Assert.Equal("Ready for winter!", entry.Entity.Id2);

                entry = context.Entry(new CompositeStoat { Id1 = guid, Id2 = "Ready for winter!" });
                TrackEntity(entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Unchanged, entry.State);
                Assert.Equal(guid, entry.Entity.Id1);
                Assert.Equal("Ready for winter!", entry.Entity.Id2);

                entry = context.Entry(new CompositeStoat { Id1 = guid, Id2 = "Little black eyes" });
                TrackEntity(entry, updateExistingEntities: true);

                Assert.Equal(EntityState.Modified, entry.State);
                Assert.Equal(guid, entry.Entity.Id1);
                Assert.Equal("Little black eyes", entry.Entity.Id2);
            }
        }

        private void TrackEntity(EntityEntry entry, bool updateExistingEntities)
        {
            new KeyValueEntityTracker(updateExistingEntities: updateExistingEntities).TrackEntity(entry);
        }

        private class Stoat
        {
            public int Id { get; set; }
        }

        private class StoatInACoat
        {
            public string Id { get; set; }
        }

        private class CompositeStoat
        {
            public Guid Id1 { get; set; }
            public string Id2 { get; set; }
        }

        private class StoteInTheSnow : DbContext
        {
            public StoteInTheSnow()
                : base(TestHelpers.Instance.CreateServiceProvider())
            {
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Stoat>();
                modelBuilder.Entity<StoatInACoat>();

                modelBuilder.Entity<CompositeStoat>(b =>
                    {
                        b.Key(e => new { e.Id1, e.Id2 });
                        b.Property(e => e.Id1).GenerateValueOnAdd();
                        b.Property(e => e.Id2).GenerateValueOnAdd();
                    });
            }
        }
    }
}
