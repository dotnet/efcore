// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    public class SkipCollectionEntryTest
    {
        [ConditionalFact]
        public void Can_get_back_reference()
        {
            using var context = new FreezerContext();

            var entity = new Cherry();
            context.Add(entity);

            var entityEntry = context.Entry(entity);
            Assert.Same(entityEntry.Entity, entityEntry.Collection("Chunkies").EntityEntry.Entity);
        }

        [ConditionalFact]
        public void Can_get_back_reference_generic()
        {
            using var context = new FreezerContext();

            var entity = new Cherry();
            context.Add(entity);

            var entityEntry = context.Entry(entity);
            Assert.Same(entityEntry.Entity, entityEntry.Collection(e => e.Chunkies).EntityEntry.Entity);
        }

        [ConditionalFact]
        public void Can_get_metadata()
        {
            using var context = new FreezerContext();

            var entity = new Cherry();
            context.Add(entity);

            Assert.Equal("Chunkies", context.Entry(entity).Collection("Chunkies").Metadata.Name);
        }

        [ConditionalFact]
        public void Can_get_metadata_generic()
        {
            using var context = new FreezerContext();

            var entity = new Cherry();
            context.Add(entity);

            Assert.Equal("Chunkies", context.Entry(entity).Collection(e => e.Chunkies).Metadata.Name);
        }

        [ConditionalFact]
        public void Can_get_and_set_current_value()
        {
            using var context = new FreezerContext();

            var cherry = new Cherry();
            var chunky = new Chunky();
            context.AddRange(chunky, cherry);

            var collection = context.Entry(cherry).Collection("Chunkies");
            var inverseCollection = context.Entry(chunky).Collection("Cherries");

            Assert.Null(collection.CurrentValue);

            collection.CurrentValue = new List<Chunky> { chunky };

            Assert.Same(chunky, cherry.Chunkies.Single());
            Assert.Same(cherry, chunky.Cherries.Single());
            Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());
            Assert.Same(cherry, inverseCollection.CurrentValue.Cast<Cherry>().Single());
            Assert.Same(collection.FindEntry(chunky).GetInfrastructure(), context.Entry(chunky).GetInfrastructure());

            collection.CurrentValue = null;

            Assert.Empty(chunky.Cherries);
            Assert.Null(cherry.Chunkies);
            Assert.Null(collection.CurrentValue);
            Assert.Empty(inverseCollection.CurrentValue);
            Assert.Null(collection.FindEntry(chunky));
        }

        [ConditionalFact]
        public void Can_get_and_set_current_value_generic()
        {
            using var context = new FreezerContext();

            var cherry = new Cherry();
            var chunky = new Chunky();
            context.AddRange(chunky, cherry);

            var collection = context.Entry(cherry).Collection(e => e.Chunkies);
            var inverseCollection = context.Entry(chunky).Collection(e => e.Cherries);

            Assert.Null(collection.CurrentValue);

            collection.CurrentValue = new List<Chunky> { chunky };

            Assert.Same(chunky, cherry.Chunkies.Single());
            Assert.Same(cherry, chunky.Cherries.Single());
            Assert.Same(chunky, collection.CurrentValue.Single());
            Assert.Same(cherry, inverseCollection.CurrentValue.Single());
            Assert.Same(collection.FindEntry(chunky).GetInfrastructure(), context.Entry(chunky).GetInfrastructure());

            collection.CurrentValue = null;

            Assert.Empty(chunky.Cherries);
            Assert.Null(cherry.Chunkies);
            Assert.Null(collection.CurrentValue);
            Assert.Empty(inverseCollection.CurrentValue);
            Assert.Null(collection.FindEntry(chunky));
        }

        [ConditionalFact]
        public void Can_get_and_set_current_value_not_tracked()
        {
            using var context = new FreezerContext();

            var cherry = new Cherry();
            var chunky = new Chunky();

            var collection = context.Entry(cherry).Collection("Chunkies");
            var inverseCollection = context.Entry(chunky).Collection("Cherries");

            Assert.Null(collection.CurrentValue);

            collection.CurrentValue = new List<Chunky> { chunky };

            Assert.Same(chunky, cherry.Chunkies.Single());
            Assert.Null(chunky.Cherries);
            Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());
            Assert.Null(inverseCollection.CurrentValue);

            collection.CurrentValue = null;

            Assert.Null(chunky.Cherries);
            Assert.Null(cherry.Chunkies);
            Assert.Null(collection.CurrentValue);
            Assert.Null(inverseCollection.CurrentValue);
        }

        [ConditionalFact]
        public void Can_get_and_set_current_value_generic_not_tracked()
        {
            using var context = new FreezerContext();

            var cherry = new Cherry();
            var chunky = new Chunky();

            var collection = context.Entry(cherry).Collection(e => e.Chunkies);
            var inverseCollection = context.Entry(chunky).Collection(e => e.Cherries);

            Assert.Null(collection.CurrentValue);

            collection.CurrentValue = new List<Chunky> { chunky };

            Assert.Same(chunky, cherry.Chunkies.Single());
            Assert.Null(chunky.Cherries);
            Assert.Same(chunky, collection.CurrentValue.Single());
            Assert.Null(inverseCollection.CurrentValue);

            collection.CurrentValue = null;

            Assert.Null(chunky.Cherries);
            Assert.Null(cherry.Chunkies);
            Assert.Null(collection.CurrentValue);
            Assert.Null(inverseCollection.CurrentValue);
        }

        [ConditionalFact]
        public void Can_get_and_set_current_value_start_tracking()
        {
            using var context = new FreezerContext();

            var cherry = new Cherry();
            var chunky = new Chunky();
            context.Add(cherry);

            var collection = context.Entry(cherry).Collection("Chunkies");
            var inverseCollection = context.Entry(chunky).Collection("Cherries");

            Assert.Null(collection.CurrentValue);

            collection.CurrentValue = new List<Chunky> { chunky };

            Assert.Same(chunky, cherry.Chunkies.Single());
            Assert.Same(cherry, chunky.Cherries.Single());
            Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());
            Assert.Same(cherry, inverseCollection.CurrentValue.Cast<Cherry>().Single());

            Assert.Equal(EntityState.Added, context.Entry(cherry).State);
            Assert.Equal(EntityState.Added, context.Entry(chunky).State);

            collection.CurrentValue = null;

            Assert.Empty(chunky.Cherries);
            Assert.Null(cherry.Chunkies);
            Assert.Null(collection.CurrentValue);
            Assert.Empty(inverseCollection.CurrentValue);

            Assert.Equal(EntityState.Added, context.Entry(cherry).State);
            Assert.Equal(EntityState.Added, context.Entry(chunky).State);
        }

        [ConditionalFact]
        public void Can_get_and_set_current_value_start_tracking_generic()
        {
            using var context = new FreezerContext();

            var cherry = new Cherry();
            var chunky = new Chunky();
            context.Add(cherry);

            var collection = context.Entry(cherry).Collection(e => e.Chunkies);
            var inverseCollection = context.Entry(chunky).Collection(e => e.Cherries);

            Assert.Null(collection.CurrentValue);

            collection.CurrentValue = new List<Chunky> { chunky };

            Assert.Same(chunky, cherry.Chunkies.Single());
            Assert.Same(cherry, chunky.Cherries.Single());
            Assert.Same(chunky, collection.CurrentValue.Single());
            Assert.Same(cherry, inverseCollection.CurrentValue.Single());

            Assert.Equal(EntityState.Added, context.Entry(cherry).State);
            Assert.Equal(EntityState.Added, context.Entry(chunky).State);

            collection.CurrentValue = null;

            Assert.Empty(chunky.Cherries);
            Assert.Null(cherry.Chunkies);
            Assert.Null(collection.CurrentValue);
            Assert.Empty(inverseCollection.CurrentValue);

            Assert.Equal(EntityState.Added, context.Entry(cherry).State);
            Assert.Equal(EntityState.Added, context.Entry(chunky).State);
        }

        [ConditionalFact]
        public void IsModified_tracks_state_of_FK_property_principal()
        {
            using var context = new FreezerContext();

            var cherry = new Cherry();
            var chunky1 = new Chunky { Id = 1 };
            var chunky2 = new Chunky { Id = 2 };
            cherry.Chunkies = new List<Chunky> { chunky1, chunky2 };
            context.AttachRange(cherry, chunky1, chunky2);

            var collection = context.Entry(cherry).Collection(e => e.Chunkies);
            var inverseCollection1 = context.Entry(chunky1).Collection(e => e.Cherries);
            var inverseCollection2 = context.Entry(chunky2).Collection(e => e.Cherries);

            Assert.False(collection.IsModified);
            Assert.False(inverseCollection1.IsModified);
            Assert.False(inverseCollection2.IsModified);

            context.Entry(chunky1).State = EntityState.Deleted;

            Assert.True(collection.IsModified);
            Assert.False(inverseCollection1.IsModified);
            Assert.False(inverseCollection2.IsModified);

            context.Entry(chunky1).State = EntityState.Unchanged;

            Assert.False(collection.IsModified);
            Assert.False(inverseCollection1.IsModified);
            Assert.False(inverseCollection2.IsModified);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Detached, EntityState.Added)]
        [InlineData(EntityState.Added, EntityState.Added)]
        [InlineData(EntityState.Modified, EntityState.Added)]
        [InlineData(EntityState.Deleted, EntityState.Added)]
        [InlineData(EntityState.Unchanged, EntityState.Added)]
        [InlineData(EntityState.Detached, EntityState.Deleted)]
        [InlineData(EntityState.Added, EntityState.Deleted)]
        [InlineData(EntityState.Modified, EntityState.Deleted)]
        [InlineData(EntityState.Deleted, EntityState.Deleted)]
        [InlineData(EntityState.Unchanged, EntityState.Deleted)]
        public void IsModified_can_set_fk_to_modified_principal_with_Added_or_Deleted_dependents(
            EntityState principalState, EntityState dependentState)
        {
            using var context = new FreezerContext();

            var cherry = new Cherry();
            var chunky1 = new Chunky { Id = 1 };
            var chunky2 = new Chunky { Id = 2 };
            cherry.Chunkies = new List<Chunky> { chunky1, chunky2 };

            context.Entry(cherry).State = principalState;
            context.Entry(chunky1).State = dependentState;
            context.Entry(chunky2).State = dependentState;

            var collection = context.Entry(cherry).Collection(e => e.Chunkies);
            var inverseCollection1 = context.Entry(chunky1).Collection(e => e.Cherries);
            var inverseCollection2 = context.Entry(chunky2).Collection(e => e.Cherries);

            var principalIsModified = principalState == EntityState.Added || principalState == EntityState.Deleted;

            Assert.True(collection.IsModified);
            Assert.Equal(principalIsModified, inverseCollection1.IsModified);
            Assert.Equal(principalIsModified, inverseCollection2.IsModified);

            collection.IsModified = false;

            Assert.True(collection.IsModified);

            collection.IsModified = true;

            Assert.True(collection.IsModified);
            Assert.Equal(principalIsModified, inverseCollection1.IsModified);
            Assert.Equal(principalIsModified, inverseCollection2.IsModified);
            Assert.Equal(dependentState, context.Entry(chunky1).State);
            Assert.Equal(dependentState, context.Entry(chunky2).State);

            if (dependentState == EntityState.Deleted)
            {
                context.Entry(chunky1).State = EntityState.Detached;
                context.Entry(chunky2).State = EntityState.Detached;
            }
            else
            {
                context.Entry(chunky1).State = EntityState.Unchanged;
                context.Entry(chunky2).State = EntityState.Unchanged;
            }

            Assert.False(collection.IsModified);
            Assert.Equal(principalIsModified, inverseCollection1.IsModified);
            Assert.Equal(principalIsModified, inverseCollection2.IsModified);
        }

        private class Chunky
        {
            public int Id { get; set; }
            public ICollection<Cherry> Cherries { get; set; }
        }

        private class Cherry
        {
            public int Id { get; set; }
            public ICollection<Chunky> Chunkies { get; set; }
        }

        private class FreezerContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase(nameof(FreezerContext));

            public DbSet<Chunky> Icecream { get; set; }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Chunky>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<Cherry>().Property(e => e.Id).ValueGeneratedNever();
            }
        }
    }
}
