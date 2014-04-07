// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, model.GetEntityType("SomeEntity"), (object)null);

            Assert.Null(entry.Entity);
        }

        [Fact]
        public void Original_values_are_not_tracked_unless_needed_by_default_for_shadow_properties()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var idProperty = entityType.GetProperty("Id");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));

            Assert.Equal(
                Strings.FormatOriginalValueNotTracked("Id", "SomeEntity"),
                Assert.Throws<InvalidOperationException>(() => entry.SetPropertyOriginalValue(idProperty, 1)).Message);

            Assert.Equal(
                Strings.FormatOriginalValueNotTracked("Id", "SomeEntity"),
                Assert.Throws<InvalidOperationException>(() => entry.GetPropertyOriginalValue(idProperty)).Message);
        }

        protected override Model BuildModel()
        {
            var model = new Model();

            var entityType1 = new EntityType("SomeEntity");
            model.AddEntityType(entityType1);
            var key1 = entityType1.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false);
            entityType1.SetKey(key1);
            entityType1.AddProperty("Name", typeof(string), shadowProperty: true, concurrencyToken: true);

            var entityType2 = new EntityType("SomeDependentEntity");
            model.AddEntityType(entityType2);
            var key2 = entityType2.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false);
            entityType2.SetKey(key2);
            var fk = entityType2.AddProperty("SomeEntityId", typeof(int), shadowProperty: true, concurrencyToken: false);
            entityType2.AddForeignKey(entityType1.GetKey(), new[] { fk });

            var entityType3 = new EntityType(typeof(FullNotificationEntity));
            model.AddEntityType(entityType3);
            entityType3.SetKey(entityType3.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));
            entityType3.AddProperty("Name", typeof(string), shadowProperty: true, concurrencyToken: true);

            var entityType4 = new EntityType(typeof(ChangedOnlyEntity));
            model.AddEntityType(entityType4);
            entityType4.SetKey(entityType4.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));
            entityType4.AddProperty("Name", typeof(string), shadowProperty: true, concurrencyToken: true);
            
            return model;
        }
    }
}
