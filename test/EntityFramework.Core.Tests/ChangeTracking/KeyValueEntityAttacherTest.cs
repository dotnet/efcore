// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class KeyValueEntityAttacherTest
    {
        [Fact]
        public async Task Entities_with_default_value_object_key_values_are_made_Added_sync()
        {
            await Entities_with_default_value_object_key_values_are_made_Added(async: false);
        }

        [Fact]
        public async Task Entities_with_default_value_object_key_values_are_made_Added_async()
        {
            await Entities_with_default_value_object_key_values_are_made_Added(async: true);
        }

        private static async Task Entities_with_default_value_object_key_values_are_made_Added(bool async)
        {
            using (var context = new StoteInTheSnow())
            {
                var entry = context.Entry(new Stoat());
                await HandleEntity(async, entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Added, entry.State);
                Assert.Equal(1, entry.Entity.Id);

                entry = context.Entry(new Stoat { Id = 77 });
                await HandleEntity(async, entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Unchanged, entry.State);
                Assert.Equal(77, entry.Entity.Id);

                entry = context.Entry(new Stoat { Id = 78 });
                await HandleEntity(async, entry, updateExistingEntities: true);

                Assert.Equal(EntityState.Modified, entry.State);
                Assert.Equal(78, entry.Entity.Id);
            }
        }

        [Fact]
        public async Task Entities_with_default_reference_key_values_are_made_Added_sync()
        {
            await Entities_with_default_reference_key_values_are_made_Added(async: false);
        }

        [Fact]
        public async Task Entities_with_default_reference_key_values_are_made_Added_async()
        {
            await Entities_with_default_reference_key_values_are_made_Added(async: true);
        }

        private static async Task Entities_with_default_reference_key_values_are_made_Added(bool async)
        {
            using (var context = new StoteInTheSnow())
            {
                var entry = context.Entry(new StoatInACoat());
                await HandleEntity(async, entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Added, entry.State);
                Assert.NotEqual(Guid.NewGuid(), Guid.Parse(entry.Entity.Id));

                entry = context.Entry(new StoatInACoat { Id = "Brrrr! It's chilly." });
                await HandleEntity(async, entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Unchanged, entry.State);
                Assert.Equal("Brrrr! It's chilly.", entry.Entity.Id);

                entry = context.Entry(new StoatInACoat { Id = "Hot chocolate please!" });
                await HandleEntity(async, entry, updateExistingEntities: true);

                Assert.Equal(EntityState.Modified, entry.State);
                Assert.Equal("Hot chocolate please!", entry.Entity.Id);
            }
        }

        [Fact]
        public async Task Entities_with_composite_key_with_any_default_values_are_made_Added_sync()
        {
            await Entities_with_composite_key_with_any_default_values_are_made_Added(async: false);
        }

        [Fact]
        public async Task Entities_with_composite_key_with_any_default_values_are_made_Added_async()
        {
            await Entities_with_composite_key_with_any_default_values_are_made_Added(async: true);
        }

        private static async Task Entities_with_composite_key_with_any_default_values_are_made_Added(bool async)
        {
            using (var context = new StoteInTheSnow())
            {
                var entry = context.Entry(new CompositeStoat());
                await HandleEntity(async, entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Added, entry.State);
                Assert.NotEqual(Guid.NewGuid(), entry.Entity.Id1);
                Assert.NotEqual(Guid.NewGuid(), Guid.Parse(entry.Entity.Id2));

                var guid = Guid.NewGuid();
                entry = context.Entry(new CompositeStoat { Id1 = guid });
                await HandleEntity(async, entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Added, entry.State);
                Assert.Equal(guid, entry.Entity.Id1);
                Assert.NotEqual(Guid.NewGuid(), Guid.Parse(entry.Entity.Id2));

                entry = context.Entry(new CompositeStoat { Id2 = "Ready for winter!" });
                await HandleEntity(async, entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Added, entry.State);
                Assert.NotEqual(Guid.NewGuid(), entry.Entity.Id1);
                Assert.Equal("Ready for winter!", entry.Entity.Id2);

                entry = context.Entry(new CompositeStoat { Id1 = guid, Id2 = "Ready for winter!" });
                await HandleEntity(async, entry, updateExistingEntities: false);

                Assert.Equal(EntityState.Unchanged, entry.State);
                Assert.Equal(guid, entry.Entity.Id1);
                Assert.Equal("Ready for winter!", entry.Entity.Id2);

                entry = context.Entry(new CompositeStoat { Id1 = guid, Id2 = "Little black eyes" });
                await HandleEntity(async, entry, updateExistingEntities: true);

                Assert.Equal(EntityState.Modified, entry.State);
                Assert.Equal(guid, entry.Entity.Id1);
                Assert.Equal("Little black eyes", entry.Entity.Id2);
            }
        }

        private static async Task HandleEntity(bool async, EntityEntry entry, bool updateExistingEntities)
        {
            var attacher = new KeyValueEntityAttacher(updateExistingEntities: updateExistingEntities);

            if (async)
            {
                await attacher.HandleEntityAsync(entry);
            }
            else
            {
                attacher.HandleEntity(entry);
            }
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
                : base(TestHelpers.CreateServiceProvider())
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
