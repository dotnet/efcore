// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.ChangeTracking
{
    public class CollectionEntryTest
    {
        [Fact]
        public void Can_get_back_reference()
        {
            using (var context = new FreezerContext())
            {
                var entity = new Cherry();
                context.Add(entity);

                var entityEntry = context.Entry(entity);
                Assert.Same(entityEntry.Entity, entityEntry.Collection("Monkeys").EntityEntry.Entity);
            }
        }

        [Fact]
        public void Can_get_back_reference_generic()
        {
            using (var context = new FreezerContext())
            {
                var entity = new Cherry();
                context.Add(entity);

                var entityEntry = context.Entry(entity);
                Assert.Same(entityEntry.Entity, entityEntry.Collection(e => e.Monkeys).EntityEntry.Entity);
            }
        }

        [Fact]
        public void Can_get_metadata()
        {
            using (var context = new FreezerContext())
            {
                var entity = new Cherry();
                context.Add(entity);

                Assert.Equal("Monkeys", context.Entry(entity).Collection("Monkeys").Metadata.Name);
            }
        }

        [Fact]
        public void Can_get_metadata_generic()
        {
            using (var context = new FreezerContext())
            {
                var entity = new Cherry();
                context.Add(entity);

                Assert.Equal("Monkeys", context.Entry(entity).Collection(e => e.Monkeys).Metadata.Name);
            }
        }

        [Fact]
        public void Can_get_and_set_current_value()
        {
            using (var context = new FreezerContext())
            {
                var cherry = new Cherry();
                var chunky = new Chunky();
                context.AddRange(chunky, cherry);

                var collection = context.Entry(cherry).Collection("Monkeys");

                Assert.Null(collection.CurrentValue);

                collection.CurrentValue = new List<Chunky> { chunky };

                Assert.Same(cherry, chunky.Garcia);
                Assert.Same(chunky, cherry.Monkeys.Single());
                Assert.Equal(cherry.Id, chunky.GarciaId);
                Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());

                collection.CurrentValue = null;

                Assert.Null(chunky.Garcia);
                Assert.Null(cherry.Monkeys);
                Assert.Null(chunky.GarciaId);
                Assert.Null(collection.CurrentValue);
            }
        }

        [Fact]
        public void Can_get_and_set_current_value_generic()
        {
            using (var context = new FreezerContext())
            {
                var cherry = new Cherry();
                var chunky = new Chunky();
                context.AddRange(chunky, cherry);

                var collection = context.Entry(cherry).Collection(e => e.Monkeys);

                Assert.Null(collection.CurrentValue);

                collection.CurrentValue = new List<Chunky> { chunky };

                Assert.Same(cherry, chunky.Garcia);
                Assert.Same(chunky, cherry.Monkeys.Single());
                Assert.Equal(cherry.Id, chunky.GarciaId);
                Assert.Same(chunky, collection.CurrentValue.Single());

                collection.CurrentValue = null;

                Assert.Null(chunky.Garcia);
                Assert.Null(cherry.Monkeys);
                Assert.Null(chunky.GarciaId);
                Assert.Null(collection.CurrentValue);
            }
        }

        [Fact]
        public void Can_get_and_set_current_value_not_tracked()
        {
            using (var context = new FreezerContext())
            {
                var cherry = new Cherry();
                var chunky = new Chunky();

                var collection = context.Entry(cherry).Collection("Monkeys");

                Assert.Null(collection.CurrentValue);

                collection.CurrentValue = new List<Chunky> { chunky };

                Assert.Null(chunky.Garcia);
                Assert.Same(chunky, cherry.Monkeys.Single());
                Assert.Null(chunky.GarciaId);
                Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());

                collection.CurrentValue = null;

                Assert.Null(chunky.Garcia);
                Assert.Null(cherry.Monkeys);
                Assert.Null(chunky.GarciaId);
                Assert.Null(collection.CurrentValue);
            }
        }

        [Fact]
        public void Can_get_and_set_current_value_generic_not_tracked()
        {
            using (var context = new FreezerContext())
            {
                var cherry = new Cherry();
                var chunky = new Chunky();

                var collection = context.Entry(cherry).Collection(e => e.Monkeys);

                Assert.Null(collection.CurrentValue);

                collection.CurrentValue = new List<Chunky> { chunky };

                Assert.Null(chunky.Garcia);
                Assert.Same(chunky, cherry.Monkeys.Single());
                Assert.Null(chunky.GarciaId);
                Assert.Same(chunky, collection.CurrentValue.Single());

                collection.CurrentValue = null;

                Assert.Null(chunky.Garcia);
                Assert.Null(cherry.Monkeys);
                Assert.Null(chunky.GarciaId);
                Assert.Null(collection.CurrentValue);
            }
        }

        [Fact]
        public void Can_get_and_set_current_value_start_tracking()
        {
            using (var context = new FreezerContext())
            {
                var cherry = new Cherry();
                var chunky = new Chunky();
                context.Add(cherry);

                var collection = context.Entry(cherry).Collection("Monkeys");

                Assert.Null(collection.CurrentValue);

                collection.CurrentValue = new List<Chunky> { chunky };

                Assert.Same(cherry, chunky.Garcia);
                Assert.Same(chunky, cherry.Monkeys.Single());
                Assert.Equal(cherry.Id, chunky.GarciaId);
                Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());

                Assert.Equal(EntityState.Added, context.Entry(cherry).State);
                Assert.Equal(EntityState.Added, context.Entry(chunky).State);

                collection.CurrentValue = null;

                Assert.Null(chunky.Garcia);
                Assert.Null(cherry.Monkeys);
                Assert.Null(chunky.GarciaId);
                Assert.Null(collection.CurrentValue);

                Assert.Equal(EntityState.Added, context.Entry(cherry).State);
                Assert.Equal(EntityState.Added, context.Entry(chunky).State);
            }
        }

        [Fact]
        public void Can_get_and_set_current_value_start_tracking_generic()
        {
            using (var context = new FreezerContext())
            {
                var cherry = new Cherry();
                var chunky = new Chunky();
                context.Add(cherry);

                var collection = context.Entry(cherry).Collection(e => e.Monkeys);

                Assert.Null(collection.CurrentValue);

                collection.CurrentValue = new List<Chunky> { chunky };

                Assert.Same(cherry, chunky.Garcia);
                Assert.Same(chunky, cherry.Monkeys.Single());
                Assert.Equal(cherry.Id, chunky.GarciaId);
                Assert.Same(chunky, collection.CurrentValue.Single());

                Assert.Equal(EntityState.Added, context.Entry(cherry).State);
                Assert.Equal(EntityState.Added, context.Entry(chunky).State);

                collection.CurrentValue = null;

                Assert.Null(chunky.Garcia);
                Assert.Null(cherry.Monkeys);
                Assert.Null(chunky.GarciaId);
                Assert.Null(collection.CurrentValue);

                Assert.Equal(EntityState.Added, context.Entry(cherry).State);
                Assert.Equal(EntityState.Added, context.Entry(chunky).State);
            }
        }

        [Fact]
        public void Can_get_and_set_current_value_attched()
        {
            using (var context = new FreezerContext())
            {
                var cherry = new Cherry();
                var chunky = new Chunky();
                context.AttachRange(chunky, cherry);

                var collection = context.Entry(cherry).Collection("Monkeys");

                Assert.Null(collection.CurrentValue);

                collection.CurrentValue = new List<Chunky> { chunky };

                Assert.Same(cherry, chunky.Garcia);
                Assert.Same(chunky, cherry.Monkeys.Single());
                Assert.Equal(cherry.Id, chunky.GarciaId);
                Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());

                Assert.Equal(EntityState.Unchanged, context.Entry(cherry).State);
                Assert.Equal(EntityState.Modified, context.Entry(chunky).State);
                Assert.True(context.Entry(chunky).Property(e => e.GarciaId).IsModified);

                collection.CurrentValue = null;

                Assert.Null(chunky.Garcia);
                Assert.Null(cherry.Monkeys);
                Assert.Null(chunky.GarciaId);
                Assert.Null(collection.CurrentValue);

                Assert.Equal(EntityState.Unchanged, context.Entry(cherry).State);
                Assert.Equal(EntityState.Modified, context.Entry(chunky).State);
                Assert.True(context.Entry(chunky).Property(e => e.GarciaId).IsModified);
            }
        }

        [Fact]
        public void Can_get_and_set_current_value_generic_attched()
        {
            using (var context = new FreezerContext())
            {
                var cherry = new Cherry();
                var chunky = new Chunky();
                context.AttachRange(chunky, cherry);

                var collection = context.Entry(cherry).Collection(e => e.Monkeys);

                Assert.Null(collection.CurrentValue);

                collection.CurrentValue = new List<Chunky> { chunky };

                Assert.Same(cherry, chunky.Garcia);
                Assert.Same(chunky, cherry.Monkeys.Single());
                Assert.Equal(cherry.Id, chunky.GarciaId);
                Assert.Same(chunky, collection.CurrentValue.Single());

                Assert.Equal(EntityState.Unchanged, context.Entry(cherry).State);
                Assert.Equal(EntityState.Modified, context.Entry(chunky).State);
                Assert.True(context.Entry(chunky).Property(e => e.GarciaId).IsModified);

                collection.CurrentValue = null;

                Assert.Null(chunky.Garcia);
                Assert.Null(cherry.Monkeys);
                Assert.Null(chunky.GarciaId);
                Assert.Null(collection.CurrentValue);

                Assert.Equal(EntityState.Unchanged, context.Entry(cherry).State);
                Assert.Equal(EntityState.Modified, context.Entry(chunky).State);
                Assert.True(context.Entry(chunky).Property(e => e.GarciaId).IsModified);
            }
        }

        [Fact]
        public void IsModified_tracks_state_of_FK_property_principal()
        {
            using (var context = new FreezerContext())
            {
                var cherry = new Cherry();
                var chunky1 = new Chunky { Id = 1, Garcia = cherry };
                var chunky2 = new Chunky { Id = 2, Garcia = cherry };
                cherry.Monkeys = new List<Chunky> { chunky1, chunky2 };
                context.AttachRange(cherry, chunky1, chunky2);

                var collection = context.Entry(cherry).Collection(e => e.Monkeys);

                Assert.False(collection.IsModified);

                context.Entry(chunky1).State = EntityState.Modified;

                Assert.True(collection.IsModified);

                context.Entry(chunky1).State = EntityState.Unchanged;

                Assert.False(collection.IsModified);
            }
        }

        [Fact]
        public void IsModified_can_set_fk_to_modified_principal()
        {
            using (var context = new FreezerContext())
            {
                var cherry = new Cherry();
                var chunky1 = new Chunky { Id = 1, Garcia = cherry };
                var chunky2 = new Chunky { Id = 2, Garcia = cherry };
                cherry.Monkeys = new List<Chunky> { chunky1, chunky2 };
                context.AttachRange(cherry, chunky1, chunky2);

                var collection = context.Entry(cherry).Collection(e => e.Monkeys);

                Assert.False(collection.IsModified);

                collection.IsModified = true;

                Assert.True(collection.IsModified);
                Assert.True(context.Entry(chunky1).Property(e => e.GarciaId).IsModified);
                Assert.True(context.Entry(chunky2).Property(e => e.GarciaId).IsModified);

                collection.IsModified = false;

                Assert.False(collection.IsModified);
                Assert.False(context.Entry(chunky1).Property(e => e.GarciaId).IsModified);
                Assert.False(context.Entry(chunky2).Property(e => e.GarciaId).IsModified);
                Assert.Equal(EntityState.Unchanged, context.Entry(chunky1).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(chunky2).State);
            }
        }

        private class Chunky
        {
            public int Monkey { get; set; }
            public int Id { get; set; }

            public int? GarciaId { get; set; }
            public Cherry Garcia { get; set; }
        }

        private class Cherry
        {
            public int Garcia { get; set; }
            public int Id { get; set; }

            public ICollection<Chunky> Monkeys { get; set; }
        }

        private class FreezerContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase();

            public DbSet<Chunky> Icecream { get; set; }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Chunky>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<Cherry>().Property(e => e.Id).ValueGeneratedNever();
            }
        }
    }
}
