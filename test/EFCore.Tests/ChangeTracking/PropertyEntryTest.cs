// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    public class PropertyEntryTest
    {
        [ConditionalFact]
        public void Setting_IsModified_should_not_be_dependent_on_other_properties()
        {
            Guid id;

            using (var context = new UserContext())
            {
                id = context.Add(
                    new User { Name = "A", LongName = "B" }).Entity.Id;

                context.SaveChanges();
            }

            using (var context = new UserContext())
            {
                var user = context.Attach(
                    new User
                    {
                        Id = id,
                        Name = "NewA",
                        LongName = "NewB"
                    }).Entity;

                context.Entry(user).Property(x => x.Name).IsModified = false;
                context.Entry(user).Property(x => x.LongName).IsModified = true;

                Assert.False(context.Entry(user).Property(x => x.Name).IsModified);
                Assert.True(context.Entry(user).Property(x => x.LongName).IsModified);

                context.SaveChanges();
            }

            using (var context = new UserContext())
            {
                var user = context.Find<User>(id);

                Assert.Equal("A", user.Name);
                Assert.Equal("NewB", user.LongName);
            }
        }

        [ConditionalFact]
        public void SetValues_with_IsModified_can_mark_a_set_of_values_as_changed()
        {
            Guid id;

            using (var context = new UserContext())
            {
                id = context.Add(
                    new User { Name = "A", LongName = "B" }).Entity.Id;

                context.SaveChanges();
            }

            using (var context = new UserContext())
            {
                var disconnectedEntity = new User { Id = id, LongName = "NewLongName" };
                var trackedEntity = context.Find<User>(id);

                Assert.Equal("A", trackedEntity.Name);
                Assert.Equal("B", trackedEntity.LongName);

                var entry = context.Entry(trackedEntity);

                entry.CurrentValues.SetValues(disconnectedEntity);

                Assert.Null(trackedEntity.Name);
                Assert.Equal("NewLongName", trackedEntity.LongName);

                Assert.False(entry.Property(e => e.Id).IsModified);
                Assert.True(entry.Property(e => e.Name).IsModified);
                Assert.True(entry.Property(e => e.LongName).IsModified);

                var internalEntry = entry.GetInfrastructure();

                Assert.False(internalEntry.IsConceptualNull(entry.Property(e => e.Id).Metadata));
                Assert.False(internalEntry.IsConceptualNull(entry.Property(e => e.Name).Metadata));
                Assert.False(internalEntry.IsConceptualNull(entry.Property(e => e.LongName).Metadata));

                foreach (var property in entry.Properties)
                {
                    property.IsModified = property.Metadata.Name == "LongName";
                }

                Assert.False(entry.Property(e => e.Id).IsModified);
                Assert.False(entry.Property(e => e.Name).IsModified);
                Assert.True(entry.Property(e => e.LongName).IsModified);

                Assert.False(internalEntry.IsConceptualNull(entry.Property(e => e.Id).Metadata));
                Assert.False(internalEntry.IsConceptualNull(entry.Property(e => e.Name).Metadata));
                Assert.False(internalEntry.IsConceptualNull(entry.Property(e => e.LongName).Metadata));

                context.SaveChanges();
            }
        }

        private class User
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string LongName { get; set; }
        }

        private class UserContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase(GetType().FullName);
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<User>(
                    b =>
                    {
                        b.Property(e => e.Name).IsRequired();
                        b.Property(e => e.LongName).IsRequired();
                    });
            }
        }

        [ConditionalFact]
        public void Setting_IsModified_is_not_reset_by_OriginalValues()
        {
            Guid id;
            using (var context = new UserContext())
            {
                id = context.Add(
                    new User
                    {
                        Id = Guid.NewGuid(),
                        Name = "A",
                        LongName = "B"
                    }).Entity.Id;

                context.SaveChanges();
            }

            using (var context = new UserContext())
            {
                var user = context.Update(
                    new User { Id = id }).Entity;

                user.Name = "A2";
                user.LongName = "B2";

                context.Entry(user).Property(x => x.Name).IsModified = false;
                Assert.False(context.Entry(user).Property(x => x.Name).IsModified);

                context.SaveChanges();
            }

            using (var context = new UserContext())
            {
                var user = context.Find<User>(id);

                Assert.Equal("A", user.Name);
                Assert.Equal("B2", user.LongName);
            }
        }

        [ConditionalFact]
        public void Can_get_name()
            => Can_get_name_helper<Wotty>();

        [ConditionalFact]
        public void Can_get_name_with_object_field()
            => Can_get_name_helper<ObjectWotty>();

        private void Can_get_name_helper<TWotty>()
            where TWotty : IWotty, new()
        {
            using (var context = new PrimateContext())
            {
                var entry = context
                    .Entry(
                        new TWotty
                        {
                            Id = 1,
                            Primate = "Monkey",
                            RequiredPrimate = "Tarsier"
                        })
                    .GetInfrastructure();

                entry.SetEntityState(EntityState.Unchanged);

                Assert.Equal("Primate", new PropertyEntry(entry, "Primate").Metadata.Name);
            }
        }

        [ConditionalFact]
        public void Can_get_current_value()
            => Can_get_current_value_helper<Wotty>();

        [ConditionalFact]
        public void Can_get_current_value_with_object_field()
            => Can_get_current_value_helper<ObjectWotty>();

        private void Can_get_current_value_helper<TWotty>()
            where TWotty : IWotty, new()
        {
            using (var context = new PrimateContext())
            {
                var entry = context
                    .Entry(
                        new TWotty
                        {
                            Id = 1,
                            Primate = "Monkey",
                            RequiredPrimate = "Tarsier"
                        })
                    .GetInfrastructure();

                entry.SetEntityState(EntityState.Unchanged);

                Assert.Equal("Monkey", new PropertyEntry(entry, "Primate").CurrentValue);
                Assert.Equal("Tarsier", new PropertyEntry(entry, "RequiredPrimate").CurrentValue);
            }
        }

        [ConditionalFact]
        public void Can_set_current_value()
            => Can_set_current_value_helper<Wotty>();

        [ConditionalFact]
        public void Can_set_current_value_with_object_field()
            => Can_set_current_value_helper<ObjectWotty>();

        private void Can_set_current_value_helper<TWotty>()
            where TWotty : IWotty, new()
        {
            using (var context = new PrimateContext())
            {
                var entity = new TWotty
                {
                    Id = 1,
                    Primate = "Monkey",
                    RequiredPrimate = "Tarsier"
                };
                var entry = context.Entry(entity).GetInfrastructure();
                entry.SetEntityState(EntityState.Unchanged);

                new PropertyEntry(entry, "Primate").CurrentValue = "Chimp";
                new PropertyEntry(entry, "RequiredPrimate").CurrentValue = "Bushbaby";

                Assert.Equal("Chimp", entity.Primate);
                Assert.Equal("Bushbaby", entity.RequiredPrimate);

                context.ChangeTracker.DetectChanges();

                Assert.Equal("Chimp", entity.Primate);
                Assert.Equal("Bushbaby", entity.RequiredPrimate);
            }
        }

        [ConditionalFact]
        public void Can_set_current_value_to_null()
            => Can_set_current_value_to_null_helper<Wotty>();

        [ConditionalFact]
        public void Can_set_current_value_to_null_with_object_field()
            => Can_set_current_value_to_null_helper<ObjectWotty>();

        private void Can_set_current_value_to_null_helper<TWotty>()
            where TWotty : IWotty, new()
        {
            using (var context = new PrimateContext())
            {
                var entity = new TWotty
                {
                    Id = 1,
                    Primate = "Monkey",
                    RequiredPrimate = "Tarsier"
                };
                var entry = context.Entry(entity).GetInfrastructure();
                entry.SetEntityState(EntityState.Unchanged);

                new PropertyEntry(entry, "Primate").CurrentValue = null;
                new PropertyEntry(entry, "RequiredPrimate").CurrentValue = null;

                Assert.Null(entity.Primate);
                Assert.Null(entity.RequiredPrimate);

                context.ChangeTracker.DetectChanges();

                Assert.Null(entity.Primate);
                Assert.Null(entity.RequiredPrimate);
            }
        }

        [ConditionalFact]
        public void Can_set_and_get_original_value()
            => Can_set_and_get_original_value_helper<Wotty>();

        [ConditionalFact]
        public void Can_set_and_get_original_value_with_object_field()
            => Can_set_and_get_original_value_helper<ObjectWotty>();

        private void Can_set_and_get_original_value_helper<TWotty>()
            where TWotty : IWotty, new()
        {
            using (var context = new PrimateContext())
            {
                var entity = new TWotty
                {
                    Id = 1,
                    Primate = "Monkey",
                    RequiredPrimate = "Tarsier"
                };
                var entry = context.Entry(entity).GetInfrastructure();
                entry.SetEntityState(EntityState.Unchanged);

                Assert.Equal("Monkey", new PropertyEntry(entry, "Primate").OriginalValue);
                Assert.Equal("Tarsier", new PropertyEntry(entry, "RequiredPrimate").OriginalValue);

                new PropertyEntry(entry, "Primate").OriginalValue = "Chimp";
                new PropertyEntry(entry, "RequiredPrimate").OriginalValue = "Bushbaby";

                Assert.Equal("Chimp", new PropertyEntry(entry, "Primate").OriginalValue);
                Assert.Equal("Monkey", entity.Primate);

                Assert.Equal("Bushbaby", new PropertyEntry(entry, "RequiredPrimate").OriginalValue);
                Assert.Equal("Tarsier", entity.RequiredPrimate);

                context.ChangeTracker.DetectChanges();

                Assert.Equal("Chimp", new PropertyEntry(entry, "Primate").OriginalValue);
                Assert.Equal("Monkey", entity.Primate);

                Assert.Equal("Bushbaby", new PropertyEntry(entry, "RequiredPrimate").OriginalValue);
                Assert.Equal("Tarsier", entity.RequiredPrimate);
            }
        }

        [ConditionalFact]
        public void Can_set_and_get_original_value_starting_null()
            => Can_set_and_get_original_value_starting_null_helper<Wotty>();

        [ConditionalFact]
        public void Can_set_and_get_original_value_starting_null_with_object_field()
            => Can_set_and_get_original_value_starting_null_helper<ObjectWotty>();

        private void Can_set_and_get_original_value_starting_null_helper<TWotty>()
            where TWotty : IWotty, new()
        {
            using (var context = new PrimateContext())
            {
                var entity = new TWotty { Id = 1 };
                var entry = context.Entry(entity).GetInfrastructure();
                entry.SetEntityState(EntityState.Unchanged);

                Assert.Null(new PropertyEntry(entry, "Primate").OriginalValue);
                Assert.Null(new PropertyEntry(entry, "RequiredPrimate").OriginalValue);

                new PropertyEntry(entry, "Primate").OriginalValue = "Chimp";
                new PropertyEntry(entry, "RequiredPrimate").OriginalValue = "Bushbaby";

                Assert.Equal("Chimp", new PropertyEntry(entry, "Primate").OriginalValue);
                Assert.Null(entity.Primate);

                Assert.Equal("Bushbaby", new PropertyEntry(entry, "RequiredPrimate").OriginalValue);
                Assert.Null(entity.RequiredPrimate);

                context.ChangeTracker.DetectChanges();

                Assert.Equal("Chimp", new PropertyEntry(entry, "Primate").OriginalValue);
                Assert.Null(entity.Primate);

                Assert.Equal("Bushbaby", new PropertyEntry(entry, "RequiredPrimate").OriginalValue);
                Assert.Null(entity.RequiredPrimate);
            }
        }

        [ConditionalFact]
        public void Can_set_original_value_to_null()
            => Can_set_original_value_to_null_helper<Wotty>();

        [ConditionalFact]
        public void Can_set_original_value_to_null_with_object_field()
            => Can_set_original_value_to_null_helper<ObjectWotty>();

        private void Can_set_original_value_to_null_helper<TWotty>()
            where TWotty : IWotty, new()
        {
            using (var context = new PrimateContext())
            {
                var entity = new TWotty
                {
                    Id = 1,
                    Primate = "Monkey",
                    RequiredPrimate = "Tarsier"
                };
                var entry = context.Entry(entity).GetInfrastructure();
                entry.SetEntityState(EntityState.Unchanged);

                new PropertyEntry(entry, "Primate").OriginalValue = null;
                new PropertyEntry(entry, "RequiredPrimate").OriginalValue = null;

                Assert.Null(new PropertyEntry(entry, "Primate").OriginalValue);
                Assert.Null(new PropertyEntry(entry, "RequiredPrimate").OriginalValue);

                context.ChangeTracker.DetectChanges();

                Assert.Null(new PropertyEntry(entry, "Primate").OriginalValue);
                Assert.Null(new PropertyEntry(entry, "RequiredPrimate").OriginalValue);
            }
        }

        [ConditionalFact]
        public void Can_set_and_clear_modified_on_Modified_entity()
            => Can_set_and_clear_modified_on_Modified_entity_helper<Wotty>();

        [ConditionalFact]
        public void Can_set_and_clear_modified_on_Modified_entity_with_object_field()
            => Can_set_and_clear_modified_on_Modified_entity_helper<ObjectWotty>();

        private void Can_set_and_clear_modified_on_Modified_entity_helper<TWotty>()
            where TWotty : IWotty, new()
        {
            using (var context = new PrimateContext())
            {
                var entity = new TWotty { Id = 1 };
                var entry = context.Entry(entity).GetInfrastructure();
                entry.SetEntityState(EntityState.Modified);

                Assert.True(new PropertyEntry(entry, "Primate").IsModified);
                Assert.True(new PropertyEntry(entry, "RequiredPrimate").IsModified);

                context.ChangeTracker.DetectChanges();

                Assert.True(new PropertyEntry(entry, "Primate").IsModified);
                Assert.True(new PropertyEntry(entry, "RequiredPrimate").IsModified);

                new PropertyEntry(entry, "Primate").IsModified = false;
                new PropertyEntry(entry, "RequiredPrimate").IsModified = false;

                Assert.False(new PropertyEntry(entry, "Primate").IsModified);
                Assert.False(new PropertyEntry(entry, "RequiredPrimate").IsModified);

                context.ChangeTracker.DetectChanges();

                Assert.False(new PropertyEntry(entry, "Primate").IsModified);
                Assert.False(new PropertyEntry(entry, "RequiredPrimate").IsModified);

                new PropertyEntry(entry, "Primate").IsModified = true;
                new PropertyEntry(entry, "RequiredPrimate").IsModified = true;

                Assert.True(new PropertyEntry(entry, "Primate").IsModified);
                Assert.True(new PropertyEntry(entry, "RequiredPrimate").IsModified);

                context.ChangeTracker.DetectChanges();

                Assert.True(new PropertyEntry(entry, "Primate").IsModified);
                Assert.True(new PropertyEntry(entry, "RequiredPrimate").IsModified);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Deleted)]
        public void Can_set_and_clear_modified_on_Added_or_Deleted_entity(EntityState initialState)
            => Can_set_and_clear_modified_on_Added_or_Deleted_entity_helper<Wotty>(initialState);

        [ConditionalTheory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Deleted)]
        public void Can_set_and_clear_modified_on_Added_or_Deleted_entity_with_object_field(EntityState initialState)
            => Can_set_and_clear_modified_on_Added_or_Deleted_entity_helper<ObjectWotty>(initialState);

        private void Can_set_and_clear_modified_on_Added_or_Deleted_entity_helper<TWotty>(EntityState initialState)
            where TWotty : IWotty, new()
        {
            using (var context = new PrimateContext())
            {
                var entity = new TWotty { Id = 1 };
                var entry = context.Entry(entity).GetInfrastructure();
                entry.SetEntityState(initialState);

                Assert.False(new PropertyEntry(entry, "Primate").IsModified);
                Assert.False(new PropertyEntry(entry, "RequiredPrimate").IsModified);

                new PropertyEntry(entry, "Primate").IsModified = true;
                new PropertyEntry(entry, "RequiredPrimate").IsModified = true;
                Assert.False(new PropertyEntry(entry, "Primate").IsModified);
                Assert.False(new PropertyEntry(entry, "RequiredPrimate").IsModified);

                context.ChangeTracker.DetectChanges();
                Assert.False(new PropertyEntry(entry, "Primate").IsModified);
                Assert.False(new PropertyEntry(entry, "RequiredPrimate").IsModified);

                new PropertyEntry(entry, "Primate").IsModified = false;
                new PropertyEntry(entry, "RequiredPrimate").IsModified = false;
                Assert.False(new PropertyEntry(entry, "Primate").IsModified);
                Assert.False(new PropertyEntry(entry, "RequiredPrimate").IsModified);

                context.ChangeTracker.DetectChanges();
                Assert.False(new PropertyEntry(entry, "Primate").IsModified);
                Assert.False(new PropertyEntry(entry, "RequiredPrimate").IsModified);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Detached)]
        [InlineData(EntityState.Unchanged)]
        public void Can_set_and_clear_modified_on_Unchanged_or_Detached_entity(EntityState initialState)
            => Can_set_and_clear_modified_on_Unchanged_or_Detached_entity_helper<Wotty>(initialState);

        [ConditionalTheory]
        [InlineData(EntityState.Detached)]
        [InlineData(EntityState.Unchanged)]
        public void Can_set_and_clear_modified_on_Unchanged_or_Detached_entity_with_object_field(EntityState initialState)
            => Can_set_and_clear_modified_on_Unchanged_or_Detached_entity_helper<ObjectWotty>(initialState);

        private void Can_set_and_clear_modified_on_Unchanged_or_Detached_entity_helper<TWotty>(EntityState initialState)
            where TWotty : IWotty, new()
        {
            using (var context = new PrimateContext())
            {
                var entity = new TWotty { Id = 1 };
                var entry = context.Entry(entity).GetInfrastructure();
                entry.SetEntityState(initialState);

                Assert.False(new PropertyEntry(entry, "Primate").IsModified);
                Assert.False(new PropertyEntry(entry, "RequiredPrimate").IsModified);

                new PropertyEntry(entry, "Primate").IsModified = true;
                new PropertyEntry(entry, "RequiredPrimate").IsModified = true;
                Assert.True(new PropertyEntry(entry, "Primate").IsModified);
                Assert.True(new PropertyEntry(entry, "RequiredPrimate").IsModified);

                context.ChangeTracker.DetectChanges();
                Assert.True(new PropertyEntry(entry, "Primate").IsModified);
                Assert.True(new PropertyEntry(entry, "RequiredPrimate").IsModified);

                new PropertyEntry(entry, "Primate").IsModified = false;
                new PropertyEntry(entry, "RequiredPrimate").IsModified = false;
                Assert.False(new PropertyEntry(entry, "Primate").IsModified);
                Assert.False(new PropertyEntry(entry, "RequiredPrimate").IsModified);

                context.ChangeTracker.DetectChanges();
                Assert.False(new PropertyEntry(entry, "Primate").IsModified);
                Assert.False(new PropertyEntry(entry, "RequiredPrimate").IsModified);
            }
        }

        [ConditionalFact]
        public void Can_reject_changes_when_clearing_modified_flag()
            => Can_reject_changes_when_clearing_modified_flag_helper<Wotty>();

        [ConditionalFact]
        public void Can_reject_changes_when_clearing_modified_flag_with_object_field()
            => Can_reject_changes_when_clearing_modified_flag_helper<ObjectWotty>();

        private void Can_reject_changes_when_clearing_modified_flag_helper<TWotty>()
            where TWotty : IWotty, new()
        {
            using (var context = new PrimateContext())
            {
                var entity = new TWotty
                {
                    Id = 1,
                    Primate = "Monkey",
                    Marmate = "Bovril",
                    RequiredPrimate = "Tarsier"
                };
                var entry = context.Entry(entity).GetInfrastructure();
                entry.SetEntityState(EntityState.Unchanged);

                var primateEntry = new PropertyEntry(entry, "Primate") { OriginalValue = "Chimp", IsModified = true };

                var marmateEntry = new PropertyEntry(entry, "Marmate") { OriginalValue = "Marmite", IsModified = true };

                var requiredEntry = new PropertyEntry(entry, "RequiredPrimate") { OriginalValue = "Bushbaby", IsModified = true };

                Assert.Equal(EntityState.Modified, entry.EntityState);
                Assert.Equal("Monkey", entity.Primate);
                Assert.Equal("Bovril", entity.Marmate);
                Assert.Equal("Tarsier", entity.RequiredPrimate);

                context.ChangeTracker.DetectChanges();
                Assert.Equal(EntityState.Modified, entry.EntityState);
                Assert.Equal("Monkey", entity.Primate);
                Assert.Equal("Bovril", entity.Marmate);
                Assert.Equal("Tarsier", entity.RequiredPrimate);

                primateEntry.IsModified = false;

                Assert.Equal(EntityState.Modified, entry.EntityState);
                Assert.Equal("Chimp", entity.Primate);
                Assert.Equal("Bovril", entity.Marmate);
                Assert.Equal("Tarsier", entity.RequiredPrimate);

                context.ChangeTracker.DetectChanges();
                Assert.Equal(EntityState.Modified, entry.EntityState);
                Assert.Equal("Chimp", entity.Primate);
                Assert.Equal("Bovril", entity.Marmate);
                Assert.Equal("Tarsier", entity.RequiredPrimate);

                marmateEntry.IsModified = false;

                Assert.Equal(EntityState.Modified, entry.EntityState);
                Assert.Equal("Chimp", entity.Primate);
                Assert.Equal("Marmite", entity.Marmate);
                Assert.Equal("Tarsier", entity.RequiredPrimate);

                context.ChangeTracker.DetectChanges();
                Assert.Equal(EntityState.Modified, entry.EntityState);
                Assert.Equal("Chimp", entity.Primate);
                Assert.Equal("Marmite", entity.Marmate);
                Assert.Equal("Tarsier", entity.RequiredPrimate);

                requiredEntry.IsModified = false;

                Assert.Equal(EntityState.Unchanged, entry.EntityState);
                Assert.Equal("Chimp", entity.Primate);
                Assert.Equal("Marmite", entity.Marmate);
                Assert.Equal("Bushbaby", entity.RequiredPrimate);

                context.ChangeTracker.DetectChanges();
                Assert.Equal(EntityState.Unchanged, entry.EntityState);
                Assert.Equal("Chimp", entity.Primate);
                Assert.Equal("Marmite", entity.Marmate);
                Assert.Equal("Bushbaby", entity.RequiredPrimate);
            }
        }

        [ConditionalFact]
        public void Can_get_name_generic()
            => Can_get_name_generic_helper<Wotty>();

        [ConditionalFact]
        public void Can_get_name_generic_with_object_field()
            => Can_get_name_generic_helper<ObjectWotty>();

        private void Can_get_name_generic_helper<TWotty>()
            where TWotty : class, IWotty, new()
        {
            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                new TWotty { Id = 1, Primate = "Monkey" });

            Assert.Equal("Primate", new PropertyEntry<Wotty, string>(entry, "Primate").Metadata.Name);
        }

        [ConditionalFact]
        public void Can_get_current_value_generic()
            => Can_get_current_value_generic_helper<Wotty>();

        [ConditionalFact]
        public void Can_get_current_value_generic_with_object_field()
            => Can_get_current_value_generic_helper<ObjectWotty>();

        private void Can_get_current_value_generic_helper<TWotty>()
            where TWotty : class, IWotty, new()
        {
            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                new TWotty { Id = 1, Primate = "Monkey" });

            Assert.Equal("Monkey", new PropertyEntry<Wotty, string>(entry, "Primate").CurrentValue);
        }

        [ConditionalFact]
        public void Can_set_current_value_generic()
            => Can_set_current_value_generic_helper<Wotty>();

        [ConditionalFact]
        public void Can_set_current_value_generic_with_object_field()
            => Can_set_current_value_generic_helper<ObjectWotty>();

        private void Can_set_current_value_generic_helper<TWotty>()
            where TWotty : class, IWotty, new()
        {
            var entity = new TWotty { Id = 1, Primate = "Monkey" };

            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                entity);

            new PropertyEntry<Wotty, string>(entry, "Primate").CurrentValue = "Chimp";

            Assert.Equal("Chimp", entity.Primate);
        }

        [ConditionalFact]
        public void Can_set_current_value_to_null_generic()
            => Can_set_current_value_to_null_generic_helper<Wotty>();

        [ConditionalFact]
        public void Can_set_current_value_to_null_generic_with_object_field()
            => Can_set_current_value_to_null_generic_helper<ObjectWotty>();

        private void Can_set_current_value_to_null_generic_helper<TWotty>()
            where TWotty : class, IWotty, new()
        {
            var entity = new TWotty { Id = 1, Primate = "Monkey" };

            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                entity);

            new PropertyEntry<Wotty, string>(entry, "Primate").CurrentValue = null;

            Assert.Null(entity.Primate);
        }

        [ConditionalFact]
        public void Can_set_and_get_original_value_generic()
            => Can_set_and_get_original_value_generic_helper<Wotty>();

        [ConditionalFact]
        public void Can_set_and_get_original_value_generic_with_object_field()
            => Can_set_and_get_original_value_generic_helper<ObjectWotty>();

        private void Can_set_and_get_original_value_generic_helper<TWotty>()
            where TWotty : class, IWotty, new()
        {
            var entity = new TWotty { Id = 1, Primate = "Monkey" };

            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                entity);

            Assert.Equal("Monkey", new PropertyEntry<Wotty, string>(entry, "Primate").OriginalValue);

            new PropertyEntry<Wotty, string>(entry, "Primate").OriginalValue = "Chimp";

            Assert.Equal("Chimp", new PropertyEntry<Wotty, string>(entry, "Primate").OriginalValue);
            Assert.Equal("Monkey", entity.Primate);
        }

        [ConditionalFact]
        public void Can_set_original_value_to_null_generic()
            => Can_set_original_value_to_null_generic_helper<Wotty>();

        [ConditionalFact]
        public void Can_set_original_value_to_null_generic_with_object_field()
            => Can_set_original_value_to_null_generic_helper<ObjectWotty>();

        private void Can_set_original_value_to_null_generic_helper<TWotty>()
            where TWotty : class, IWotty, new()
        {
            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                new TWotty { Id = 1, Primate = "Monkey" });

            new PropertyEntry<Wotty, string>(entry, "Primate").OriginalValue = null;

            Assert.Null(new PropertyEntry<Wotty, string>(entry, "Primate").OriginalValue);
        }

        [ConditionalFact]
        public void Can_set_and_clear_modified_generic()
            => Can_set_and_clear_modified_generic_helper<Wotty>();

        [ConditionalFact]
        public void Can_set_and_clear_modified_generic_with_object_field()
            => Can_set_and_clear_modified_generic_helper<ObjectWotty>();

        private void Can_set_and_clear_modified_generic_helper<TWotty>()
            where TWotty : class, IWotty, new()
        {
            var entity = new TWotty { Id = 1, Primate = "Monkey" };

            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                entity);

            Assert.False(new PropertyEntry<Wotty, string>(entry, "Primate").IsModified);

            new PropertyEntry(entry, "Primate").IsModified = true;

            Assert.True(new PropertyEntry<Wotty, string>(entry, "Primate").IsModified);

            new PropertyEntry(entry, "Primate").IsModified = false;

            Assert.False(new PropertyEntry<Wotty, string>(entry, "Primate").IsModified);
        }

        [ConditionalFact]
        public void Can_set_and_get_original_value_notifying_entities()
        {
            var entity = new NotifyingWotty { Id = 1, Primate = "Monkey" };

            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                entity);

            Assert.Equal("Monkey", new PropertyEntry(entry, "Primate").OriginalValue);

            new PropertyEntry(entry, "Primate").OriginalValue = "Chimp";

            Assert.Equal("Chimp", new PropertyEntry(entry, "Primate").OriginalValue);
            Assert.Equal("Monkey", entity.Primate);
        }

        [ConditionalFact]
        public void Can_set_original_value_to_null_notifying_entities()
        {
            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                new NotifyingWotty { Id = 1, Primate = "Monkey" });

            new PropertyEntry(entry, "Primate").OriginalValue = null;

            Assert.Null(new PropertyEntry(entry, "Primate").OriginalValue);
        }

        [ConditionalFact]
        public void Can_set_and_get_original_value_generic_notifying_entities()
        {
            var entity = new NotifyingWotty { Id = 1, Primate = "Monkey" };

            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                entity);

            Assert.Equal("Monkey", new PropertyEntry<NotifyingWotty, string>(entry, "Primate").OriginalValue);

            new PropertyEntry<NotifyingWotty, string>(entry, "Primate").OriginalValue = "Chimp";

            Assert.Equal("Chimp", new PropertyEntry<NotifyingWotty, string>(entry, "Primate").OriginalValue);
            Assert.Equal("Monkey", entity.Primate);
        }

        [ConditionalFact]
        public void Can_set_original_value_to_null_generic_notifying_entities()
        {
            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                new NotifyingWotty { Id = 1, Primate = "Monkey" });

            new PropertyEntry<NotifyingWotty, string>(entry, "Primate").OriginalValue = null;

            Assert.Null(new PropertyEntry<NotifyingWotty, string>(entry, "Primate").OriginalValue);
        }

        [ConditionalFact]
        public void Can_set_and_get_concurrency_token_original_value_full_notification_entities()
        {
            var entity = new FullyNotifyingWotty { Id = 1, ConcurrentPrimate = "Monkey" };

            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                entity);

            Assert.Equal("Monkey", new PropertyEntry(entry, "ConcurrentPrimate").OriginalValue);

            new PropertyEntry(entry, "ConcurrentPrimate").OriginalValue = "Chimp";

            Assert.Equal("Chimp", new PropertyEntry(entry, "ConcurrentPrimate").OriginalValue);
            Assert.Equal("Monkey", entity.ConcurrentPrimate);
        }

        [ConditionalFact]
        public void Can_set_concurrency_token_original_value_to_null_full_notification_entities()
        {
            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                new FullyNotifyingWotty { Id = 1, ConcurrentPrimate = "Monkey" });

            new PropertyEntry(entry, "ConcurrentPrimate").OriginalValue = null;

            Assert.Null(new PropertyEntry(entry, "ConcurrentPrimate").OriginalValue);
        }

        [ConditionalFact]
        public void Can_set_and_get_concurrency_token_original_value_generic_full_notification_entities()
        {
            var entity = new FullyNotifyingWotty { Id = 1, ConcurrentPrimate = "Monkey" };

            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                entity);

            Assert.Equal("Monkey", new PropertyEntry<FullyNotifyingWotty, string>(entry, "ConcurrentPrimate").OriginalValue);

            new PropertyEntry<FullyNotifyingWotty, string>(entry, "ConcurrentPrimate").OriginalValue = "Chimp";

            Assert.Equal("Chimp", new PropertyEntry<FullyNotifyingWotty, string>(entry, "ConcurrentPrimate").OriginalValue);
            Assert.Equal("Monkey", entity.ConcurrentPrimate);
        }

        [ConditionalFact]
        public void Can_set_concurrency_token_original_value_to_null_generic_full_notification_entities()
        {
            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                new FullyNotifyingWotty { Id = 1, ConcurrentPrimate = "Monkey" });

            new PropertyEntry<FullyNotifyingWotty, string>(entry, "ConcurrentPrimate").OriginalValue = null;

            Assert.Null(new PropertyEntry<FullyNotifyingWotty, string>(entry, "ConcurrentPrimate").OriginalValue);
        }

        [ConditionalFact]
        public void Cannot_set_or_get_original_value_when_not_tracked()
        {
            var entity = new FullyNotifyingWotty { Id = 1, Primate = "Monkey" };

            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                entity);

            var propertyEntry = new PropertyEntry(entry, "Primate");

            Assert.Equal(
                CoreStrings.OriginalValueNotTracked("Primate", "FullyNotifyingWotty"),
                Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue).Message);

            Assert.Equal(
                CoreStrings.OriginalValueNotTracked("Primate", "FullyNotifyingWotty"),
                Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = "Chimp").Message);
        }

        [ConditionalFact]
        public void Cannot_set_or_get_original_value_when_not_tracked_generic()
        {
            var entity = new FullyNotifyingWotty { Id = 1, ConcurrentPrimate = "Monkey" };

            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                entity);

            var propertyEntry = new PropertyEntry<FullyNotifyingWotty, string>(entry, "Primate");

            Assert.Equal(
                CoreStrings.OriginalValueNotTracked("Primate", "FullyNotifyingWotty"),
                Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue).Message);

            Assert.Equal(
                CoreStrings.OriginalValueNotTracked("Primate", "FullyNotifyingWotty"),
                Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = "Chimp").Message);
        }

        [ConditionalFact]
        public void Can_set_or_get_original_value_when_property_explicitly_marked_to_be_tracked()
        {
            var entity = new FullyNotifyingWotty { Id = 1, Primate = "Monkey" };

            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues),
                EntityState.Unchanged,
                entity);

            Assert.Equal("Monkey", new PropertyEntry(entry, "Primate").OriginalValue);

            new PropertyEntry(entry, "Primate").OriginalValue = "Chimp";

            Assert.Equal("Chimp", new PropertyEntry(entry, "Primate").OriginalValue);
            Assert.Equal("Monkey", entity.Primate);
        }

        [ConditionalFact]
        public void Can_set_or_get_original_value_when_property_explicitly_marked_to_be_tracked_generic()
        {
            var entity = new FullyNotifyingWotty { Id = 1, Primate = "Monkey" };

            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
                BuildModel(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues),
                EntityState.Unchanged,
                entity);

            Assert.Equal("Monkey", new PropertyEntry<FullyNotifyingWotty, string>(entry, "Primate").OriginalValue);

            new PropertyEntry<FullyNotifyingWotty, string>(entry, "Primate").OriginalValue = "Chimp";

            Assert.Equal("Chimp", new PropertyEntry<FullyNotifyingWotty, string>(entry, "Primate").OriginalValue);
            Assert.Equal("Monkey", entity.Primate);
        }

        private interface IWotty
        {
            int Id { get; set; }
            string Primate { get; set; }
            string RequiredPrimate { get; set; }
            string Marmate { get; set; }
        }

        private class ObjectWotty : IWotty
        {
            private object _id;
            private object _primate;
            private object _requiredPrimate;
            private object _marmate;

            public int Id
            {
                get => (int)_id;
                set => _id = value;
            }

            public string Primate
            {
                get => (string)_primate;
                set => _primate = value;
            }

            public string RequiredPrimate
            {
                get => (string)_requiredPrimate;
                set => _requiredPrimate = value;
            }

            public string Marmate
            {
                get => (string)_marmate;
                set => _marmate = value;
            }
        }

        private class Wotty : IWotty
        {
            public int Id { get; set; }
            public string Primate { get; set; }
            public string RequiredPrimate { get; set; }
            public string Marmate { get; set; }
        }

        private class FullyNotifyingWotty : HasChangedAndChanging
        {
            private int _id;
            private string _primate;
            private string _concurrentprimate;

            public int Id
            {
                get => _id;
                set
                {
                    if (_id != value)
                    {
                        OnPropertyChanging();
                        _id = value;
                        OnPropertyChanged();
                    }
                }
            }

            public string Primate
            {
                get => _primate;
                set
                {
                    if (_primate != value)
                    {
                        OnPropertyChanging();
                        _primate = value;
                        OnPropertyChanged();
                    }
                }
            }

            public string ConcurrentPrimate
            {
                get => _concurrentprimate;
                set
                {
                    if (_concurrentprimate != value)
                    {
                        OnPropertyChanging();
                        _concurrentprimate = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        private class NotifyingWotty : HasChanged
        {
            private int _id;
            private string _primate;

            public int Id
            {
                get => _id;
                set
                {
                    if (_id != value)
                    {
                        _id = value;
                        OnPropertyChanged();
                    }
                }
            }

            public string Primate
            {
                get => _primate;
                set
                {
                    if (_primate != value)
                    {
                        _primate = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        private abstract class HasChanged : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private abstract class HasChangedAndChanging : HasChanged, INotifyPropertyChanging
        {
            public event PropertyChangingEventHandler PropertyChanging;

            protected void OnPropertyChanging([CallerMemberName] string propertyName = "")
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        public static IModel BuildModel(
            ChangeTrackingStrategy fullNotificationStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications,
            ModelBuilder builder = null,
            bool finalize = true)
        {
            builder ??= InMemoryTestHelpers.Instance.CreateConventionBuilder();

            builder.HasChangeTrackingStrategy(fullNotificationStrategy);

            builder.Entity<Wotty>(
                b =>
                {
                    b.Property(e => e.RequiredPrimate).IsRequired();
                    b.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
                });

            builder.Entity<ObjectWotty>(
                b =>
                {
                    b.Property(e => e.RequiredPrimate).IsRequired();
                    b.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
                });

            builder.Entity<NotifyingWotty>(
                b => b.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications));

            builder.Entity<FullyNotifyingWotty>(
                b =>
                {
                    b.HasChangeTrackingStrategy(fullNotificationStrategy);
                    b.Property(e => e.ConcurrentPrimate).IsConcurrencyToken();
                });

            return finalize ? builder.Model.FinalizeModel() : builder.Model;
        }

        private class PrimateContext : DbContext
        {
            private readonly ChangeTrackingStrategy _fullNotificationStrategy;

            public PrimateContext(ChangeTrackingStrategy fullNotificationStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications)
            {
                _fullNotificationStrategy = fullNotificationStrategy;
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase(GetType().FullName);
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                BuildModel(_fullNotificationStrategy, modelBuilder, finalize: false);
            }
        }
    }
}
