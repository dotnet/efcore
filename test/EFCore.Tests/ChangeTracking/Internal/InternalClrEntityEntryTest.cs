// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class InternalClrEntityEntryTest : InternalEntityEntryTestBase
    {
        [Fact]
        public void Can_get_entity()
        {
            var model = BuildModel();
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

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
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entity = new SomeEntity
            {
                Id = 77,
                Name = "Magic Tree House"
            };
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
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                entityType,
                new SomeEntity
                {
                    Id = 1,
                    Name = "Kool"
                },
                new ValueBuffer(new object[] { 1, "Kool" }));

            var entity = (SomeEntity)entry.Entity;

            Assert.Equal(1, entity.Id);
            Assert.Equal("Kool", entity.Name);
        }

        [Fact]
        public void All_original_values_can_be_accessed_for_entity_that_does_no_notification()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);

            AllOriginalValuesTest(
                model, entityType, new SomeEntity
                {
                    Id = 1,
                    Name = "Kool"
                });
        }

        [Fact]
        public void All_original_values_can_be_accessed_for_entity_that_does_changed_only_notifications()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(ChangedOnlyEntity).FullName);

            AllOriginalValuesTest(
                model, entityType, new ChangedOnlyEntity
                {
                    Id = 1,
                    Name = "Kool"
                });
        }

        [Fact]
        public void Setting_CLR_property_with_snapshot_change_tracking_requires_DetectChanges()
            => SetPropertyClrTest(
                new SomeEntity
                {
                    Id = 1,
                    Name = "Kool"
                }, needsDetectChanges: true);

        [Fact]
        public void Setting_CLR_property_with_changed_only_notifications_does_not_require_DetectChanges()
            => SetPropertyClrTest(
                new ChangedOnlyEntity
                {
                    Id = 1,
                    Name = "Kool"
                }, needsDetectChanges: false);

        [Fact]
        public void Setting_CLR_property_with_full_notifications_does_not_require_DetectChanges()
            => SetPropertyClrTest(
                new FullNotificationEntity
                {
                    Id = 1,
                    Name = "Kool"
                }, needsDetectChanges: false);

        [Fact]
        public void Setting_an_explicit_value_on_the_entity_marks_property_as_not_temporary()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            var entity = (SomeEntity)entry.Entity;

            entry.SetEntityState(EntityState.Added);
            entry.SetTemporaryValue(keyProperty, -1);

            Assert.True(entry.HasTemporaryValue(keyProperty));

            entity.Id = 77;

            configuration.GetRequiredService<IChangeDetector>().DetectChanges(entry);

            Assert.False(entry.HasTemporaryValue(keyProperty));

            entry.SetEntityState(EntityState.Unchanged); // Does not throw

            var nameProperty = entityType.FindProperty(nameof(SomeEntity.Name));
            Assert.True(entry.HasDefaultValue(nameProperty));

            entity.Name = "Name";

            Assert.False(entry.HasDefaultValue(nameProperty));
        }
    }
}
