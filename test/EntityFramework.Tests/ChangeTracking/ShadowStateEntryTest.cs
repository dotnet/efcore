// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class ShadowStateEntryTest : StateEntryTest
    {
        [Fact]
        public void Entity_is_null()
        {
            var model = BuildModel();
            var configuration = TestHelpers.CreateContextConfiguration(model);

            var entry = CreateStateEntry(configuration, model.GetEntityType(typeof(SomeEntity).FullName), (object)null);

            Assert.Null(entry.Entity);
        }

        [Fact]
        public void Original_values_are_not_tracked_unless_needed_by_default_for_shadow_properties()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var idProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.CreateContextConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));

            Assert.Equal(
                Strings.FormatOriginalValueNotTracked("Id", typeof(SomeEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => entry.OriginalValues[idProperty] = 1).Message);

            Assert.Equal(
                Strings.FormatOriginalValueNotTracked("Id", typeof(SomeEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => entry.OriginalValues[idProperty]).Message);
        }

        protected override Model BuildModel()
        {
            var model = new Model();

            var entityType1 = new EntityType(typeof(SomeEntity).FullName);
            model.AddEntityType(entityType1);
            var key1 = entityType1.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            key1.ValueGeneration = ValueGeneration.OnAdd;
            entityType1.GetOrSetPrimaryKey(key1);
            entityType1.GetOrAddProperty("Name", typeof(string), shadowProperty: true).IsConcurrencyToken = true;

            var entityType2 = new EntityType(typeof(SomeDependentEntity).FullName);
            model.AddEntityType(entityType2);
            var key2a = entityType2.GetOrAddProperty("Id1", typeof(int), shadowProperty: true);
            var key2b = entityType2.GetOrAddProperty("Id2", typeof(string), shadowProperty: true);
            entityType2.GetOrSetPrimaryKey(key2a, key2b);
            var fk = entityType2.GetOrAddProperty("SomeEntityId", typeof(int), shadowProperty: true);
            entityType2.GetOrAddForeignKey(entityType1.GetPrimaryKey(), new[] { fk });
            var justAProperty = entityType2.GetOrAddProperty("JustAProperty", typeof(int), shadowProperty: true);
            justAProperty.ValueGeneration = ValueGeneration.OnAdd;

            var entityType3 = new EntityType(typeof(FullNotificationEntity));
            model.AddEntityType(entityType3);
            entityType3.GetOrSetPrimaryKey(entityType3.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            entityType3.GetOrAddProperty("Name", typeof(string), shadowProperty: true).IsConcurrencyToken = true;

            var entityType4 = new EntityType(typeof(ChangedOnlyEntity));
            model.AddEntityType(entityType4);
            entityType4.GetOrSetPrimaryKey(entityType4.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            entityType4.GetOrAddProperty("Name", typeof(string), shadowProperty: true).IsConcurrencyToken = true;

            var entityType5 = new EntityType(typeof(SomeMoreDependentEntity).FullName);
            model.AddEntityType(entityType5);
            var key5 = entityType5.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            entityType5.GetOrSetPrimaryKey(key5);
            var fk5a = entityType5.GetOrAddProperty("Fk1", typeof(int), shadowProperty: true);
            var fk5b = entityType5.GetOrAddProperty("Fk2", typeof(string), shadowProperty: true);
            entityType5.GetOrAddForeignKey(entityType2.GetPrimaryKey(), new[] { fk5a, fk5b });

            return model;
        }
    }
}
