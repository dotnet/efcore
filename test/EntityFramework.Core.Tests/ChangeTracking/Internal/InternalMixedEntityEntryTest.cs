// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class InternalMixedEntityEntryTest : InternalEntityEntryTestBase
    {
        [Fact]
        public void Can_get_entity()
        {
            var model = BuildModel();
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entity = new SomeEntity();
            var entry = CreateInternalEntry(configuration, model.FindEntityType(typeof(SomeEntity).FullName), entity);

            Assert.Same(entity, entry.Entity);
        }

        [Fact]
        public void Can_set_and_get_property_value_from_CLR_object()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var nonKeyProperty = entityType.FindProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entity = new SomeEntity { Id = 77, Name = "Magic Tree House" };
            var entry = CreateInternalEntry(configuration, entityType, entity);

            Assert.Null(entry[keyProperty]); // In shadow
            Assert.Equal("Magic Tree House", entry[nonKeyProperty]);

            entry[keyProperty] = 78;
            entry[nonKeyProperty] = "Normal Tree House";

            Assert.Equal(77, entity.Id); // In shadow
            Assert.Equal("Normal Tree House", entity.Name);
        }

        [Fact]
        public void Asking_for_entity_instance_causes_it_to_be_materialized()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                 configuration,
                 entityType,
                 new SomeEntity { Id = 1, Name = "Kool" },
                 new ValueBuffer(new object[] { 1, "Kool" }));

            var entity = (SomeEntity)entry.Entity;

            Assert.Equal("Kool", entity.Name);
        }

        [Fact]
        public void All_original_values_can_be_accessed_for_entity_that_does_no_notifiction()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);

            AllOriginalValuesTest(model, entityType, new SomeEntity { Id = 1, Name = "Kool" });
        }

        [Fact]
        public void All_original_values_can_be_accessed_for_entity_that_does_changed_only_notifictions_if_eager_values_on()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(ChangedOnlyEntity).FullName);
            entityType.UseEagerSnapshots = true;

            AllOriginalValuesTest(model, entityType, new ChangedOnlyEntity { Id = 1, Name = "Kool" });
        }

        [Fact]
        public void Setting_CLR_property_with_snapshot_change_tracking_requires_DetectChanges()
        {
            SetPropertyClrTest(new SomeEntity { Id = 1, Name = "Kool" }, needsDetectChanges: true);
        }

        [Fact]
        public void Setting_CLR_property_with_changed_only_notifications_does_not_require_DetectChanges()
        {
            SetPropertyClrTest(new ChangedOnlyEntity { Id = 1, Name = "Kool" }, needsDetectChanges: false);
        }

        [Fact]
        public void Setting_CLR_property_with_full_notifications_does_not_require_DetectChanges()
        {
            SetPropertyClrTest(new FullNotificationEntity { Id = 1, Name = "Kool" }, needsDetectChanges: false);
        }

        [Fact]
        public void Original_values_are_not_tracked_unless_needed_by_default_for_properties_of_full_notifications_entity()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(FullNotificationEntity).FullName);
            var idProperty = entityType.FindProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration, 
                entityType, 
                new FullNotificationEntity { Id = 1, Name = "Kool" }, 
                new ValueBuffer(new object[] { 1, "Kool" }));

            Assert.Equal(
                CoreStrings.OriginalValueNotTracked("Id", typeof(FullNotificationEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => entry.OriginalValues[idProperty] = 1).Message);

            Assert.Equal(1, entry.OriginalValues[idProperty]);
        }

        protected override Model BuildModel()
        {
            var model = base.BuildModel();
            
            model.FindEntityType(typeof(SomeSimpleEntityBase)).FindProperty("Id").IsShadowProperty = true;
            model.FindEntityType(typeof(SomeEntity)).FindProperty("Name").IsConcurrencyToken = false;
            model.FindEntityType(typeof(SomeDependentEntity)).FindProperty("SomeEntityId").IsShadowProperty = true;

            return model;
        }
    }
}
