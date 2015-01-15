// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class ChangeDetectorTest
    {
        [Fact]
        public void PropertyChanging_does_not_snapshot_if_eager_snapshots_are_in_use()
        {
            var contextServices = TestHelpers.CreateContextServices(BuildModel());
            var entry = CreateStateEntry<Product>(contextServices);

            Assert.True(entry.EntityType.UseEagerSnapshots);
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));

            contextServices
                .GetRequiredService<ChangeDetector>()
                .PropertyChanging(entry, entry.EntityType.GetProperty("DependentId"));

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));
        }

        [Fact]
        public void PropertyChanging_snapshots_original_and_FK_value_if_lazy_snapshots_are_in_use()
        {
            var contextServices = TestHelpers.CreateContextServices(BuildModelWithChanging());
            var entry = CreateStateEntry(contextServices, new ProductWithChanging { DependentId = 77 });

            Assert.False(entry.EntityType.UseEagerSnapshots);
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));

            var property = entry.EntityType.GetProperty("DependentId");

            contextServices
                .GetRequiredService<ChangeDetector>()
                .PropertyChanging(entry, property);

            Assert.Equal(77, entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues)[property]);
            Assert.True(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues).HasValue(property));
            Assert.Equal(77, entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)[property]);
            Assert.True(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot).HasValue(property));
        }

        [Fact]
        public void PropertyChanging_does_not_snapshot_original_values_for_properties_with_no_original_value_tracking()
        {
            var contextServices = TestHelpers.CreateContextServices(BuildModelWithChanging());
            var entry = CreateStateEntry<ProductWithChanging>(contextServices);

            Assert.False(entry.EntityType.UseEagerSnapshots);
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));

            contextServices
                .GetRequiredService<ChangeDetector>()
                .PropertyChanging(entry, entry.EntityType.GetProperty("Name"));

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));
        }

        [Fact]
        public void PropertyChanging_snapshots_reference_navigations_if_lazy_snapshots_are_in_use()
        {
            var contextServices = TestHelpers.CreateContextServices(BuildModelWithChanging());
            var category = new CategoryWithChanging();
            var entry = CreateStateEntry(contextServices, new ProductWithChanging { Category = category });

            Assert.False(entry.EntityType.UseEagerSnapshots);
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));

            var navigation = entry.EntityType.GetNavigation("Category");

            contextServices
                .GetRequiredService<ChangeDetector>()
                .PropertyChanging(entry, navigation);

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.True(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot).HasValue(navigation));
            Assert.Same(category, entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)[navigation]);
        }

        [Fact]
        public void PropertyChanging_snapshots_PK_for_relationships_if_lazy_snapshots_are_in_use()
        {
            var contextServices = TestHelpers.CreateContextServices(BuildModelWithChanging());
            var entry = CreateStateEntry(contextServices, new ProductWithChanging { Id = 77 });

            Assert.False(entry.EntityType.UseEagerSnapshots);
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));

            var property = entry.EntityType.GetProperty("Id");

            contextServices
                .GetRequiredService<ChangeDetector>()
                .PropertyChanging(entry, property);

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.True(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot).HasValue(property));
            Assert.Equal(77, entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)[property]);
        }

        [Fact]
        public void PropertyChanging_does_not_snapshot_notification_collections()
        {
            var contextServices = TestHelpers.CreateContextServices(BuildModelWithChanging());
            var entry = CreateStateEntry<CategoryWithChanging>(contextServices);

            Assert.False(entry.EntityType.UseEagerSnapshots);
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));

            contextServices
                .GetRequiredService<ChangeDetector>()
                .PropertyChanging(entry, entry.EntityType.GetNavigation("Products"));

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));
        }

        [Fact]
        public void Detects_scalar_property_change()
        {
            var contextServices = TestHelpers.CreateContextServices(BuildModel());

            var product = new Product { Name = "Oculus Rift" };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.Name = "Gear VR";
            contextServices.GetRequiredService<ChangeDetector>().DetectChanges(entry);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.True(entry.IsPropertyModified(entry.EntityType.GetProperty("Name")));
        }

        [Fact]
        public void Skips_detection_of_scalar_property_change_for_notification_entities()
        {
            var contextServices = TestHelpers.CreateContextServices(BuildModelWithChanged());

            var product = new ProductWithChanged { Name = "Oculus Rift" };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.Name = "Gear VR";
            contextServices.GetRequiredService<ChangeDetector>().DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.False(entry.IsPropertyModified(entry.EntityType.GetProperty("Name")));
        }

        [Fact]
        public void Detects_principal_key_change()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var stateManager = contextServices.GetRequiredService<StateManager>();
            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var category = new Category { Id = -1, PrincipalId = 77 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType, -1)));

            category.PrincipalId = 78;
            changeDetector.DetectChanges(entry);

            Assert.Equal(78, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("PrincipalId")]);
            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType, -1)));

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Same(entry, testListener.PrincipalKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("PrincipalId"), testListener.PrincipalKeyChange.Item2);
            Assert.Equal(77, testListener.PrincipalKeyChange.Item3);
            Assert.Equal(78, testListener.PrincipalKeyChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Detects_principal_key_changing_back_to_original_value()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var category = new Category { Id = -1, PrincipalId = 77 };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            category.PrincipalId = 78;
            changeDetector.DetectChanges(entry);

            category.PrincipalId = 77;
            changeDetector.DetectChanges(entry);

            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("PrincipalId")]);

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Same(entry, testListener.PrincipalKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("PrincipalId"), testListener.PrincipalKeyChange.Item2);
            Assert.Equal(78, testListener.PrincipalKeyChange.Item3);
            Assert.Equal(77, testListener.PrincipalKeyChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Reacts_to_principal_key_change_in_sidecar()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var category = new Category { Id = -1, PrincipalId = 77 };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            var property = entry.EntityType.GetProperty("PrincipalId");
            var sidecar = entry.AddSidecar(contextServices.GetRequiredService<StoreGeneratedValuesFactory>()
                .Create(entry, entry.EntityType.Properties));
            sidecar.TakeSnapshot();

            sidecar[property] = 78;
            changeDetector.DetectChanges(entry);

            Assert.Equal(78, entry.RelationshipsSnapshot[property]);

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Same(entry, testListener.PrincipalKeyChange.Item1);
            Assert.Same(property, testListener.PrincipalKeyChange.Item2);
            Assert.Equal(77, testListener.PrincipalKeyChange.Item3);
            Assert.Equal(78, testListener.PrincipalKeyChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Detects_primary_key_change()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var stateManager = contextServices.GetRequiredService<StateManager>();
            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var category = new Category { Id = -1 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType, -1)));

            category.Id = 78;
            changeDetector.DetectChanges(entry);

            Assert.Equal(78, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("Id")]);
            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType, 78)));

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Reacts_to_primary_key_change_in_sidecar()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var stateManager = contextServices.GetRequiredService<StateManager>();
            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();
            var storeGeneratedValuesFactory = contextServices.GetRequiredService<StoreGeneratedValuesFactory>();

            var category = new Category { Id = -1 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType, -1)));

            var property = entry.EntityType.GetProperty("Id");
            var sidecar = entry.AddSidecar(storeGeneratedValuesFactory.Create(entry, entry.EntityType.Properties));
            sidecar.TakeSnapshot();

            sidecar[property] = 78;
            changeDetector.DetectChanges(entry);

            Assert.Equal(78, entry.RelationshipsSnapshot[property]);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType, 78)));

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Ignores_no_change_to_principal_key()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var stateManager = contextServices.GetRequiredService<StateManager>();
            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var category = new Category { Id = -1, PrincipalId = 77 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType, -1)));

            category.PrincipalId = 77;
            changeDetector.DetectChanges(entry);

            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("PrincipalId")]);
            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType, -1)));

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Ignores_no_change_to_principal_key_in_sidecar()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var category = new Category { Id = -1, PrincipalId = 77 };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            var property = entry.EntityType.GetProperty("PrincipalId");
            var sidecar = entry.AddSidecar(contextServices.GetRequiredService<StoreGeneratedValuesFactory>()
                .Create(entry, entry.EntityType.Properties));
            sidecar.TakeSnapshot();

            sidecar[property] = 77;
            changeDetector.DetectChanges(entry);

            Assert.Equal(77, entry.RelationshipsSnapshot[property]);

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Detects_foreign_key_change()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var product = new Product { DependentId = 77 };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.DependentId = 78;
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(78, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("DependentId")]);

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Same(entry, testListener.ForeignKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("DependentId"), testListener.ForeignKeyChange.Item2);
            Assert.Equal(77, testListener.ForeignKeyChange.Item3);
            Assert.Equal(78, testListener.ForeignKeyChange.Item4);

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Detects_foreign_key_changing_back_to_original_value()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var product = new Product { DependentId = 77 };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.DependentId = 78;
            changeDetector.DetectChanges(entry);

            product.DependentId = 77;
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("DependentId")]);

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Same(entry, testListener.ForeignKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("DependentId"), testListener.ForeignKeyChange.Item2);
            Assert.Equal(78, testListener.ForeignKeyChange.Item3);
            Assert.Equal(77, testListener.ForeignKeyChange.Item4);

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Reacts_to_foreign_key_change_in_sidecar()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var product = new Product { DependentId = 77 };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            var property = entry.EntityType.GetProperty("DependentId");
            var sidecar = entry.AddSidecar(contextServices.GetRequiredService<StoreGeneratedValuesFactory>()
                .Create(entry, entry.EntityType.Properties));
            sidecar.TakeSnapshot();

            sidecar[property] = 78;
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(78, entry.RelationshipsSnapshot[property]);

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Same(entry, testListener.ForeignKeyChange.Item1);
            Assert.Same(property, testListener.ForeignKeyChange.Item2);
            Assert.Equal(77, testListener.ForeignKeyChange.Item3);
            Assert.Equal(78, testListener.ForeignKeyChange.Item4);

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Ignores_no_change_to_foreign_key()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var product = new Product { DependentId = 77 };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.DependentId = 77;
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("DependentId")]);

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Ignores_no_change_to_foreign_key_in_sidecar()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var product = new Product { DependentId = 77 };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            var property = entry.EntityType.GetProperty("DependentId");
            var sidecar = entry.AddSidecar(contextServices.GetRequiredService<StoreGeneratedValuesFactory>()
                .Create(entry, entry.EntityType.Properties));
            sidecar.TakeSnapshot();

            sidecar[property] = 77;
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(77, entry.RelationshipsSnapshot[property]);

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Detects_reference_navigation_change()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var originalCategory = new Category { PrincipalId = 1 };
            var product = new Product { Category = originalCategory, DependentId = 1 };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            var newCategory = new Category { PrincipalId = 2 };
            product.Category = newCategory;
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(newCategory, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Same(entry, testListener.ReferenceChange.Item1);
            Assert.Same(entry.EntityType.GetNavigation("Category"), testListener.ReferenceChange.Item2);
            Assert.Equal(originalCategory, testListener.ReferenceChange.Item3);
            Assert.Equal(newCategory, testListener.ReferenceChange.Item4);

            Assert.Same(entry, testListener.ForeignKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("DependentId"), testListener.ForeignKeyChange.Item2);
            Assert.Equal(1, testListener.ForeignKeyChange.Item3);
            Assert.Equal(2, testListener.ForeignKeyChange.Item4);

            Assert.Null(testListener.CollectionChange);
            Assert.Null(testListener.PrincipalKeyChange);
        }

        [Fact]
        public void Detects_reference_navigation_changing_back_to_original_value()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var originalCategory = new Category { PrincipalId = 1 };
            var product = new Product { Category = originalCategory, DependentId = 1 };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            var newCategory = new Category { PrincipalId = 2 };
            product.Category = newCategory;
            changeDetector.DetectChanges(entry);

            product.Category = originalCategory;
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(originalCategory, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Same(entry, testListener.ReferenceChange.Item1);
            Assert.Same(entry.EntityType.GetNavigation("Category"), testListener.ReferenceChange.Item2);
            Assert.Equal(newCategory, testListener.ReferenceChange.Item3);
            Assert.Equal(originalCategory, testListener.ReferenceChange.Item4);

            Assert.Same(entry, testListener.ForeignKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("DependentId"), testListener.ForeignKeyChange.Item2);
            Assert.Equal(2, testListener.ForeignKeyChange.Item3);
            Assert.Equal(1, testListener.ForeignKeyChange.Item4);

            Assert.Null(testListener.CollectionChange);
            Assert.Null(testListener.PrincipalKeyChange);
        }

        [Fact]
        public void Ignores_no_change_to_reference_navigation()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var category = new Category { PrincipalId = 1 };
            var product = new Product { Category = category, DependentId = 1 };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.Category = category;
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(category, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.CollectionChange);
            Assert.Null(testListener.PrincipalKeyChange);
        }

        [Fact]
        public void Detects_adding_to_collection_navigation()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var product1 = new Product { DependentId = 77 };
            var product2 = new Product { DependentId = 77 };
            var category = new Category { PrincipalId = 77, Products = { product1, product2 } };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            var product3 = new Product { DependentId = 77 };
            category.Products.Add(product3);
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(
                new[] { product1, product2, product3 },
                ((ICollection<object>)entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Products")])
                    .Cast<Product>()
                    .OrderBy(e => e.DependentId));

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Same(entry, testListener.CollectionChange.Item1);
            Assert.Same(entry.EntityType.GetNavigation("Products"), testListener.CollectionChange.Item2);
            Assert.Equal(new[] { product3 }, testListener.CollectionChange.Item3);
            Assert.Empty(testListener.CollectionChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);
        }

        [Fact]
        public void Detects_removing_from_collection_navigation()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var product1 = new Product { DependentId = 77 };
            var product2 = new Product { DependentId = 77 };
            var category = new Category { PrincipalId = 77, Products = { product1, product2 } };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            category.Products.Remove(product1);
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(
                new[] { product2 },
                ((ICollection<object>)entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Products")])
                    .Cast<Product>()
                    .OrderBy(e => e.DependentId));

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Same(entry, testListener.CollectionChange.Item1);
            Assert.Same(entry.EntityType.GetNavigation("Products"), testListener.CollectionChange.Item2);
            Assert.Empty(testListener.CollectionChange.Item3);
            Assert.Equal(new[] { product1 }, testListener.CollectionChange.Item4);

            Assert.Same(product1, testListener.ForeignKeyChange.Item1.Entity);
            Assert.Same(testListener.ForeignKeyChange.Item1.EntityType.GetProperty("DependentId"), testListener.ForeignKeyChange.Item2);
            Assert.Equal(77, testListener.ForeignKeyChange.Item3);
            Assert.Null(testListener.ForeignKeyChange.Item4);

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);
        }

        [Fact]
        public void Ignores_no_change_to_collection_navigation()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModel());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var product1 = new Product { DependentId = 77 };
            var product2 = new Product { DependentId = 77 };
            var category = new Category { PrincipalId = 77, Products = { product1, product2 } };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            category.Products.Remove(product1);
            category.Products.Add(product1);
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(
                new[] { product1, product2 },
                ((ICollection<object>)entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Products")])
                    .Cast<Product>()
                    .OrderBy(e => e.DependentId));

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Null(testListener.CollectionChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);
        }

        [Fact]
        public void Skips_detecting_changes_to_primary_principal_key_for_notification_entities()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModelWithChanged());

            var stateManager = contextServices.GetRequiredService<StateManager>();
            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var product = new ProductWithChanged { Id = 77 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Added);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType, 77)));

            product.Id = 78;
            changeDetector.DetectChanges(entry);

            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("Id")]);
            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType, 77)));

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Skips_detecting_changes_to_foreign_key_for_notification_entities()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModelWithChanged());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var product = new ProductWithChanged { DependentId = 77 };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.DependentId = 78;
            changeDetector.DetectChanges(entry);

            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("DependentId")]);

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        [Fact]
        public void Skips_detecting_changes_to_reference_navigation_for_notification_entities()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModelWithChanged());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var category = new CategoryWithChanged { Id = 1 };
            var product = new ProductWithChanged { Category = category, DependentId = 1 };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.Category = new CategoryWithChanged { Id = 2 };
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(category, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.CollectionChange);
            Assert.Null(testListener.PrincipalKeyChange);
        }

        [Fact]
        public void Skips_detecting_changes_to_notifying_collections()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModelWithChanged());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var product1 = new ProductWithChanged { DependentId = 77 };
            var product2 = new ProductWithChanged { DependentId = 77 };
            var category = new CategoryWithChanged
                {
                    Id = 77,
                    Products = new ObservableCollection<ProductWithChanged> { product1, product2 }
                };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            var product3 = new ProductWithChanged { DependentId = 77 };
            category.Products.Add(product3);
            changeDetector.DetectChanges(entry);

            // TODO: DetectChanges is actually used here until INotifyCollectionChanged is supported (Issue #445)
            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(
                new[] { product1, product2, product3 },
                ((ICollection<object>)entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Products")])
                    .Cast<ProductWithChanged>()
                    .OrderBy(e => e.DependentId));

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Same(entry, testListener.CollectionChange.Item1);
            Assert.Same(entry.EntityType.GetNavigation("Products"), testListener.CollectionChange.Item2);
            Assert.Equal(new[] { product3 }, testListener.CollectionChange.Item3);
            Assert.Empty(testListener.CollectionChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);
        }

        [Fact]
        public void Change_detection_still_happens_for_non_notifying_collections_on_notifying_entities()
        {
            var contextServices = TestHelpers.CreateContextServices(
                new ServiceCollection().AddScoped<IRelationshipListener, TestRelationshipListener>(),
                BuildModelWithChanged());

            var changeDetector = contextServices.GetRequiredService<ChangeDetector>();

            var product1 = new ProductWithChanged { DependentId = 77 };
            var product2 = new ProductWithChanged { DependentId = 77 };
            var category = new CategoryWithChanged
                {
                    Id = 77,
                    Products = new List<ProductWithChanged> { product1, product2 }
                };
            var entry = contextServices.GetRequiredService<StateManager>().GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            var product3 = new ProductWithChanged { DependentId = 77 };
            category.Products.Add(product3);
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(
                new[] { product1, product2, product3 },
                ((ICollection<object>)entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Products")])
                    .Cast<ProductWithChanged>()
                    .OrderBy(e => e.DependentId));

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IRelationshipListener>>()
                .OfType<TestRelationshipListener>()
                .Single();

            Assert.Same(entry, testListener.CollectionChange.Item1);
            Assert.Same(entry.EntityType.GetNavigation("Products"), testListener.CollectionChange.Item2);
            Assert.Equal(new[] { product3 }, testListener.CollectionChange.Item3);
            Assert.Empty(testListener.CollectionChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);
        }

        private class Category
        {
            public int Id { get; set; }
            public int? PrincipalId { get; set; }
            public string Name { get; set; }

            public virtual ICollection<Product> Products { get; } = new List<Product>();
        }

        private class Product
        {
            public Guid Id { get; set; }
            public int? DependentId { get; set; }
            public string Name { get; set; }

            public virtual Category Category { get; set; }
        }

        private static IModel BuildModel()
        {
            var builder = new ModelBuilder();

            builder.Entity<Product>();
            builder.Entity<Category>(b =>
                {
                    b.Property(e => e.Id).GenerateValueOnAdd(false);

                    b.OneToMany(e => e.Products, e => e.Category)
                        .ForeignKey(e => e.DependentId)
                        .ReferencedKey(e => e.PrincipalId);
                });

            return builder.Model;
        }

        private class CategoryWithChanging : INotifyPropertyChanging
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public virtual ICollection<ProductWithChanging> Products { get; } = new ObservableCollection<ProductWithChanging>();

            // Actual implementation not needed for tests
#pragma warning disable 67
            public event PropertyChangingEventHandler PropertyChanging;
#pragma warning restore 67
        }

        private class ProductWithChanging : INotifyPropertyChanging
        {
            public int Id { get; set; }
            public int? DependentId { get; set; }
            public string Name { get; set; }

            public virtual CategoryWithChanging Category { get; set; }

            // Actual implementation not needed for tests
#pragma warning disable 67
            public event PropertyChangingEventHandler PropertyChanging;
#pragma warning restore 67
        }

        private static IModel BuildModelWithChanging()
        {
            var builder = new ModelBuilder();

            builder.Entity<ProductWithChanging>();
            builder.Entity<CategoryWithChanging>(b =>
                {
                    b.Property(e => e.Id).GenerateValueOnAdd(false);
                    b.OneToMany(e => e.Products, e => e.Category).ForeignKey(e => e.DependentId);
                });

            return builder.Model;
        }

        private class CategoryWithChanged : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public virtual ICollection<ProductWithChanged> Products { get; set; } = new ObservableCollection<ProductWithChanged>();

            // Actual implementation not needed for tests
#pragma warning disable 67
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }

        private class ProductWithChanged : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public int? DependentId { get; set; }
            public string Name { get; set; }

            public virtual CategoryWithChanged Category { get; set; }

            // Actual implementation not needed for tests
#pragma warning disable 67
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }

        private static IModel BuildModelWithChanged()
        {
            var builder = new ModelBuilder();

            builder.Entity<ProductWithChanged>();
            builder.Entity<CategoryWithChanged>(b =>
                {
                    b.Property(e => e.Id).GenerateValueOnAdd(false);
                    b.OneToMany(e => e.Products, e => e.Category).ForeignKey(e => e.DependentId);
                });

            return builder.Model;
        }

        private static IServiceProvider CreateContextServices(StateEntryNotifier notifier, IModel model)
        {
            return TestHelpers.CreateContextServices(new ServiceCollection().AddInstance(notifier), model);
        }

        private static ClrStateEntry CreateStateEntry<TEntity>(IServiceProvider contextServices, TEntity entity = null)
            where TEntity : class, new()
        {
            return new ClrStateEntry(
                contextServices.GetRequiredService<StateManager>(),
                contextServices.GetRequiredService<DbContextService<IModel>>().Service.GetEntityType(typeof(TEntity)),
                contextServices.GetRequiredService<StateEntryMetadataServices>(), entity ?? new TEntity());
        }

        private class TestRelationshipListener : IRelationshipListener
        {
            public Tuple<StateEntry, IProperty, object, object> ForeignKeyChange { get; set; }
            public Tuple<StateEntry, IProperty, object, object> PrincipalKeyChange { get; set; }
            public Tuple<StateEntry, INavigation, object, object> ReferenceChange { get; set; }
            public Tuple<StateEntry, INavigation, ISet<object>, ISet<object>> CollectionChange { get; set; }

            public void ForeignKeyPropertyChanged(StateEntry entry, IProperty property, object oldValue, object newValue)
            {
                ForeignKeyChange = Tuple.Create(entry, property, oldValue, newValue);
            }

            public void NavigationReferenceChanged(StateEntry entry, INavigation navigation, object oldValue, object newValue)
            {
                ReferenceChange = Tuple.Create(entry, navigation, oldValue, newValue);
            }

            public void NavigationCollectionChanged(StateEntry entry, INavigation navigation, ISet<object> added, ISet<object> removed)
            {
                CollectionChange = Tuple.Create(entry, navigation, added, removed);
            }

            public void PrincipalKeyPropertyChanged(StateEntry entry, IProperty property, object oldValue, object newValue)
            {
                PrincipalKeyChange = Tuple.Create(entry, property, oldValue, newValue);
            }
        }
    }
}
