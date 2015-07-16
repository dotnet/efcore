// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class InternalClrEntityEntryTest : InternalEntityEntryTest
    {
        [Fact]
        public void Can_get_entity()
        {
            var model = BuildModel();
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entity = new SomeEntity();
            var entry = CreateInternalEntry(configuration, model.GetEntityType(typeof(SomeEntity).FullName), entity);

            Assert.Same(entity, entry.Entity);
        }

        [Fact]
        public void Can_set_and_get_property_value_from_CLR_object()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var nonKeyProperty = entityType.GetProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entity = new SomeEntity { Id = 77, Name = "Magic Tree House" };
            var entry = CreateInternalEntry(configuration, entityType, entity);

            Assert.Equal(77, entry[keyProperty]);
            Assert.Equal("Magic Tree House", entry[nonKeyProperty]);

            entry[keyProperty] = 78;
            entry[nonKeyProperty] = "Normal Tree House";

            Assert.Equal(78, entity.Id);
            Assert.Equal("Normal Tree House", entity.Name);
        }

        [Fact]
        public void Asking_for_entity_instance_causes_it_to_be_materialized()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                 configuration,
                 entityType,
                 new SomeEntity { Id = 1, Name = "Kool" },
                 new ValueBuffer(new object[] { 1, "Kool" }));

            var entity = (SomeEntity)entry.Entity;

            Assert.Equal(1, entity.Id);
            Assert.Equal("Kool", entity.Name);
        }

        [Fact]
        public void All_original_values_can_be_accessed_for_entity_that_does_no_notifiction()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);

            AllOriginalValuesTest(model, entityType, new SomeEntity { Id = 1, Name = "Kool" });
        }

        [Fact]
        public void All_original_values_can_be_accessed_for_entity_that_does_changed_only_notifictions()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(ChangedOnlyEntity).FullName);

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
            var entityType = model.GetEntityType(typeof(FullNotificationEntity).FullName);
            var idProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new ValueBuffer(new object[] { 1, "Kool" }));

            Assert.Equal(
                Strings.OriginalValueNotTracked("Id", typeof(FullNotificationEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => entry.OriginalValues[idProperty] = 1).Message);

            Assert.Equal(
                Strings.OriginalValueNotTracked("Id", typeof(FullNotificationEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => entry.OriginalValues[idProperty]).Message);
        }

        [Fact]
        public void Setting_an_explicit_value_on_the_entity_marks_property_as_not_temporary()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            var entity = (SomeEntity)entry.Entity;

            entry.SetEntityState(EntityState.Added);
            entry.MarkAsTemporary(keyProperty);

            Assert.True(entry.HasTemporaryValue(keyProperty));

            entity.Id = 77;

            configuration.GetRequiredService<IChangeDetector>().DetectChanges(entry);

            Assert.False(entry.HasTemporaryValue(keyProperty));

            entry.SetEntityState(EntityState.Unchanged); // Does not throw
        }
    }
}
