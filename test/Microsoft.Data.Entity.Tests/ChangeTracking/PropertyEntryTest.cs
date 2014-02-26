// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class PropertyEntryTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            var entityEntry = new EntityEntry<Cheese>(
                new ChangeTracker(BuildModel(), new Mock<ActiveIdentityGenerators>().Object),
                new Cheese());

            Assert.Equal(
                Strings.ArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => new PropertyEntry(entityEntry, "")).Message);
            Assert.Equal(
                "entityEntry",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new PropertyEntry(null, "Kake")).ParamName);

            Assert.Equal(
                Strings.ArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => new PropertyEntry<Random, int>(entityEntry, "")).Message);
            Assert.Equal(
                "entityEntry",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new PropertyEntry<Random, int>(null, "Kake")).ParamName);
        }

        [Fact]
        public void Can_get_name()
        {
            var entityEntry = new EntityEntry<Cheese>(
                new ChangeTracker(BuildModel(), new Mock<ActiveIdentityGenerators>().Object),
                new Cheese());

            Assert.Equal("Maturity", new PropertyEntry(entityEntry, "Maturity").Name);
            Assert.Equal("Maturity", new PropertyEntry<Cheese, string>(entityEntry, "Maturity").Name);
        }

        [Fact]
        public void Can_set_unchanged_properties_to_modified_and_back_to_unchanged()
        {
            var entityEntry = new EntityEntry<Cheese>(
                new ChangeTracker(BuildModel(), new Mock<ActiveIdentityGenerators>().Object),
                new Cheese()) { State = EntityState.Unchanged };

            var nameEntry = new PropertyEntry<Cheese, string>(entityEntry, "Name");
            var maturityEntry = new PropertyEntry<Cheese, string>(entityEntry, "Maturity");

            Assert.False(nameEntry.IsModified);
            Assert.False(maturityEntry.IsModified);

            maturityEntry.IsModified = true;

            Assert.False(nameEntry.IsModified);
            Assert.True(maturityEntry.IsModified);

            maturityEntry.IsModified = false;

            Assert.False(nameEntry.IsModified);
            Assert.False(maturityEntry.IsModified);
        }

        [Fact]
        public void Changing_property_states_for_Added_Deleted_and_Unknown_properties_has_no_effect()
        {
            var entityEntry = new EntityEntry<Cheese>(
                new ChangeTracker(BuildModel(), new Mock<ActiveIdentityGenerators>().Object),
                new Cheese()) { State = EntityState.Added };

            var nameEntry = new PropertyEntry<Cheese, string>(entityEntry, "Name");
            var maturityEntry = new PropertyEntry<Cheese, string>(entityEntry, "Maturity");

            maturityEntry.IsModified = true;

            Assert.False(nameEntry.IsModified);
            Assert.False(maturityEntry.IsModified);

            entityEntry.State = EntityState.Deleted;
            maturityEntry.IsModified = true;

            Assert.False(nameEntry.IsModified);
            Assert.False(maturityEntry.IsModified);

            entityEntry.State = EntityState.Unknown;
            maturityEntry.IsModified = true;

            Assert.False(nameEntry.IsModified);
            Assert.False(maturityEntry.IsModified);
        }

        [Fact]
        public void Changing_property_state_to_modified_marks_entity_as_Modified()
        {
            var entityEntry = new EntityEntry<Cheese>(
                new ChangeTracker(BuildModel(), new Mock<ActiveIdentityGenerators>().Object),
                new Cheese()) { State = EntityState.Unchanged };

            new PropertyEntry<Cheese, string>(entityEntry, "Maturity") { IsModified = true };

            Assert.Equal(EntityState.Modified, entityEntry.State);
        }

        [Fact]
        public void Changing_all_property_states_to_unchanged_marks_entity_as_Unchanged()
        {
            var entityEntry = new EntityEntry<Cheese>(
                new ChangeTracker(BuildModel(), new Mock<ActiveIdentityGenerators>().Object),
                new Cheese()) { State = EntityState.Modified };

            var idEntry = new PropertyEntry<Cheese, int>(entityEntry, "Id");
            var nameEntry = new PropertyEntry<Cheese, string>(entityEntry, "Name");
            var maturityEntry = new PropertyEntry<Cheese, string>(entityEntry, "Maturity");

            Assert.Equal(EntityState.Modified, entityEntry.State);

            idEntry.IsModified = false;
            nameEntry.IsModified = false;

            Assert.Equal(EntityState.Modified, entityEntry.State);

            maturityEntry.IsModified = false;

            Assert.Equal(EntityState.Unchanged, entityEntry.State);
        }

        #region Fixture

        public class Cheese
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Maturity { get; set; }
        }

        private static IModel BuildModel()
        {
            var model = new Model();
            var builder = new ModelBuilder(model);

            builder.Entity<Cheese>()
                .Key(e => e.Id)
                .Properties(
                    pb =>
                        {
                            pb.Property(c => c.Id);
                            pb.Property(c => c.Name);
                            pb.Property(c => c.Maturity);
                        });

            return model;
        }

        #endregion
    }
}
