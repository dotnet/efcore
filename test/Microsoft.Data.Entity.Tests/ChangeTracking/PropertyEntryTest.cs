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

        [Fact]
        public void Different_numbers_of_properties_are_packed_and_manipulated_correctly()
        {
            PropertyManipulation(0);
            PropertyManipulation(1);
            PropertyManipulation(28);
            PropertyManipulation(29);
            PropertyManipulation(30);
            PropertyManipulation(32);
            PropertyManipulation(33);
            PropertyManipulation(60);
            PropertyManipulation(61);
            PropertyManipulation(62);
            PropertyManipulation(64);
            PropertyManipulation(65);
        }

        public void PropertyManipulation(int propertyCount)
        {
            var model = new Model();
            var entityType = new EntityType(typeof(Pickle));
            model.AddEntityType(entityType);

            var propertyNames = new string[propertyCount];
            for (var i = 0; i < propertyCount; i++)
            {
                propertyNames[i] = "Prop" + i;
                entityType.AddProperty(new Property(typeof(Pickle).GetProperty(propertyNames[i])));
            }

            var entityEntry = new EntityEntry<Pickle>(
                new ChangeTracker(model, new Mock<ActiveIdentityGenerators>().Object),
                new Pickle()) { State = EntityState.Unchanged };

            for (var i = 0; i < propertyCount; i++)
            {
                entityEntry.Property(propertyNames[i]).IsModified = true;

                for (var j = 0; j < propertyCount; j++)
                {
                    Assert.Equal(j <= i, entityEntry.Property(propertyNames[j]).IsModified);
                }

                Assert.Equal(EntityState.Modified, entityEntry.State);
            }

            for (var i = 0; i < propertyCount; i++)
            {
                entityEntry.Property(propertyNames[i]).IsModified = false;

                for (var j = 0; j < propertyCount; j++)
                {
                    Assert.Equal(j > i, entityEntry.Property(propertyNames[j]).IsModified);
                }

                Assert.Equal(i == propertyCount - 1 ? EntityState.Unchanged : EntityState.Modified, entityEntry.State);
            }

            entityEntry.State = EntityState.Modified;

            for (var i = 0; i < propertyCount; i++)
            {
                Assert.True(entityEntry.Property(propertyNames[i]).IsModified);
            }

            entityEntry.State = EntityState.Unchanged;

            for (var i = 0; i < propertyCount; i++)
            {
                Assert.False(entityEntry.Property(propertyNames[i]).IsModified);
            }
        }

        #region Fixture

        public class Cheese
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Maturity { get; set; }
        }

        public class Pickle
        {
            public string Prop0 { get; set; }
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
            public string Prop3 { get; set; }
            public string Prop4 { get; set; }
            public string Prop5 { get; set; }
            public string Prop6 { get; set; }
            public string Prop7 { get; set; }
            public string Prop8 { get; set; }
            public string Prop9 { get; set; }
            public string Prop10 { get; set; }
            public string Prop11 { get; set; }
            public string Prop12 { get; set; }
            public string Prop13 { get; set; }
            public string Prop14 { get; set; }
            public string Prop15 { get; set; }
            public string Prop16 { get; set; }
            public string Prop17 { get; set; }
            public string Prop18 { get; set; }
            public string Prop19 { get; set; }
            public string Prop20 { get; set; }
            public string Prop21 { get; set; }
            public string Prop22 { get; set; }
            public string Prop23 { get; set; }
            public string Prop24 { get; set; }
            public string Prop25 { get; set; }
            public string Prop26 { get; set; }
            public string Prop27 { get; set; }
            public string Prop28 { get; set; }
            public string Prop29 { get; set; }
            public string Prop30 { get; set; }
            public string Prop31 { get; set; }
            public string Prop32 { get; set; }
            public string Prop33 { get; set; }
            public string Prop34 { get; set; }
            public string Prop35 { get; set; }
            public string Prop36 { get; set; }
            public string Prop37 { get; set; }
            public string Prop38 { get; set; }
            public string Prop39 { get; set; }
            public string Prop40 { get; set; }
            public string Prop41 { get; set; }
            public string Prop42 { get; set; }
            public string Prop43 { get; set; }
            public string Prop44 { get; set; }
            public string Prop45 { get; set; }
            public string Prop46 { get; set; }
            public string Prop47 { get; set; }
            public string Prop48 { get; set; }
            public string Prop49 { get; set; }
            public string Prop50 { get; set; }
            public string Prop51 { get; set; }
            public string Prop52 { get; set; }
            public string Prop53 { get; set; }
            public string Prop54 { get; set; }
            public string Prop55 { get; set; }
            public string Prop56 { get; set; }
            public string Prop57 { get; set; }
            public string Prop58 { get; set; }
            public string Prop59 { get; set; }
            public string Prop60 { get; set; }
            public string Prop61 { get; set; }
            public string Prop62 { get; set; }
            public string Prop63 { get; set; }
            public string Prop64 { get; set; }
        }

        private static IModel BuildModel()
        {
            var model = new Model();
            var builder = new ModelBuilder(model);

            builder.Entity<Cheese>();

            new SimpleTemporaryConvention().Apply(model);

            return model;
        }

        #endregion
    }
}
