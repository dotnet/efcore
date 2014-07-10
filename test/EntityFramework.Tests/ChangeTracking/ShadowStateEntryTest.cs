// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ShadowStateEntryTest : StateEntryTest
    {
        [Fact]
        public void Entity_is_null()
        {
            var model = BuildModel();
            var configuration = TestHelpers.CreateContextConfiguration(model);

            var entry = CreateStateEntry(configuration, model.GetEntityType("SomeEntity"), (object)null);

            Assert.Null(entry.Entity);
        }

        [Fact]
        public void Original_values_are_not_tracked_unless_needed_by_default_for_shadow_properties()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var idProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.CreateContextConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));

            Assert.Equal(
                Strings.FormatOriginalValueNotTracked("Id", "SomeEntity"),
                Assert.Throws<InvalidOperationException>(() => entry.OriginalValues[idProperty] = 1).Message);

            Assert.Equal(
                Strings.FormatOriginalValueNotTracked("Id", "SomeEntity"),
                Assert.Throws<InvalidOperationException>(() => entry.OriginalValues[idProperty]).Message);
        }

        protected override Model BuildModel()
        {
            var model = new Model();

            var entityType1 = new EntityType("SomeEntity");
            model.AddEntityType(entityType1);
            var key1 = entityType1.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false);
            key1.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;
            key1.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
            entityType1.SetKey(key1);
            entityType1.AddProperty("Name", typeof(string), shadowProperty: true, concurrencyToken: true);

            var entityType2 = new EntityType("SomeDependentEntity");
            model.AddEntityType(entityType2);
            var key2a = entityType2.AddProperty("Id1", typeof(int), shadowProperty: true, concurrencyToken: false);
            var key2b = entityType2.AddProperty("Id2", typeof(string), shadowProperty: true, concurrencyToken: false);
            entityType2.SetKey(key2a, key2b);
            var fk = entityType2.AddProperty("SomeEntityId", typeof(int), shadowProperty: true, concurrencyToken: false);
            entityType2.AddForeignKey(entityType1.GetKey(), new[] { fk });
            var justAProperty = entityType2.AddProperty("JustAProperty", typeof(int), shadowProperty: true, concurrencyToken: false);
            justAProperty.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;
            justAProperty.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;

            var entityType3 = new EntityType(typeof(FullNotificationEntity));
            model.AddEntityType(entityType3);
            entityType3.SetKey(entityType3.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));
            entityType3.AddProperty("Name", typeof(string), shadowProperty: true, concurrencyToken: true);

            var entityType4 = new EntityType(typeof(ChangedOnlyEntity));
            model.AddEntityType(entityType4);
            entityType4.SetKey(entityType4.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));
            entityType4.AddProperty("Name", typeof(string), shadowProperty: true, concurrencyToken: true);

            var entityType5 = new EntityType("SomeMoreDependentEntity");
            model.AddEntityType(entityType5);
            var key5 = entityType5.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false);
            entityType5.SetKey(key5);
            var fk5a = entityType5.AddProperty("Fk1", typeof(int), shadowProperty: true, concurrencyToken: false);
            var fk5b = entityType5.AddProperty("Fk2", typeof(string), shadowProperty: true, concurrencyToken: false);
            entityType5.AddForeignKey(entityType2.GetKey(), new[] { fk5a, fk5b });

            return model;
        }
    }
}
