// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class ClrStateEntryTest : StateEntryTest
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

            Assert.Equal(77, entry.GetPropertyValue(keyProperty));
            Assert.Equal("Magic Tree House", entry.GetPropertyValue(nonKeyProperty));

            entry.SetPropertyValue(keyProperty, 78);
            entry.SetPropertyValue(nonKeyProperty, "Normal Tree House");

            Assert.Equal(78, entity.Id);
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

            Assert.Equal(1, entity.Id);
            Assert.Equal("Kool", entity.Kool);
        }

        protected override StateEntry CreateStateEntry(ContextConfiguration stateManager, IEntityType entityType, object entity)
        {
            return new ClrStateEntry(stateManager, entityType, entity);
        }

        protected override StateEntry CreateStateEntry(ContextConfiguration stateManager, IEntityType entityType, object[] valueBuffer)
        {
            return new ClrStateEntry(stateManager, entityType, valueBuffer);
        }
    }
}
