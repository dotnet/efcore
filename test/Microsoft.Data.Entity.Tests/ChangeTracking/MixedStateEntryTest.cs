// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class MixedStateEntryTest : StateEntryTest
    {
        [Fact]
        public void Can_get_entity()
        {
            var model = BuildModel();
            var configuration = CreateConfiguration(model);

            var entity = new SomeEntity();
            var entry = CreateStateEntry(configuration, model.GetEntityType("SomeEntity"), entity);

            Assert.Same(entity, entry.Entity);
        }

        [Fact]
        public void Can_set_and_get_property_value_from_CLR_object()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");
            var nonKeyProperty = entityType.GetProperty("Kool");
            var configuration = CreateConfiguration(model);

            var entity = new SomeEntity { Id = 77, Kool = "Magic Tree House" };
            var entry = CreateStateEntry(configuration, entityType, entity);

            Assert.Null(entry.GetPropertyValue(keyProperty)); // In shadow
            Assert.Equal("Magic Tree House", entry.GetPropertyValue(nonKeyProperty));

            entry.SetPropertyValue(keyProperty, 78);
            entry.SetPropertyValue(nonKeyProperty, "Normal Tree House");

            Assert.Equal(77, entity.Id); // In shadow
            Assert.Equal("Normal Tree House", entity.Kool);
        }

        [Fact]
        public void Asking_for_entity_instance_causes_it_to_be_materialized()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new object[] { 1, "Kool" });

            var entity = (SomeEntity)entry.Entity;

            Assert.Equal("Kool", entity.Kool);
        }

        protected override IModel BuildModel()
        {
            var model = new Model();

            var entityType1 = new EntityType(typeof(SomeEntity));
            model.AddEntityType(entityType1);
            var key1 = entityType1.AddProperty("Id", typeof(int), shadowProperty: true);
            entityType1.SetKey(key1);
            entityType1.AddProperty("Kool", typeof(string), shadowProperty: false);

            var entityType2 = new EntityType(typeof(SomeDependentEntity));
            model.AddEntityType(entityType2);
            var key2 = entityType2.AddProperty("Id", typeof(int), shadowProperty: false);
            entityType2.SetKey(key2);
            var fk = entityType2.AddProperty("SomeEntityId", typeof(int), shadowProperty: true);
            entityType2.AddForeignKey(entityType1.GetKey(), new[] { fk });

            return model;
        }
    }
}
