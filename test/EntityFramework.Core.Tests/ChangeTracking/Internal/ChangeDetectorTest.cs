// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class ChangeDetectorTest
    {
        [Fact]
        public void PropertyChanging_does_not_snapshot_if_eager_snapshots_are_in_use()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModel());
            var entry = CreateInternalEntry<Product>(contextServices);

            Assert.True(entry.EntityType.UseEagerSnapshots());
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));

            contextServices
                .GetRequiredService<IChangeDetector>()
                .PropertyChanging(entry, entry.EntityType.GetProperty("DependentId"));

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));
        }

        [Fact]
        public void PropertyChanging_snapshots_original_and_FK_value_if_lazy_snapshots_are_in_use()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModelWithChanging());
            var entry = CreateInternalEntry(contextServices, new ProductWithChanging { DependentId = 77 });

            Assert.False(entry.EntityType.UseEagerSnapshots());
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));

            var property = entry.EntityType.GetProperty("DependentId");

            contextServices
                .GetRequiredService<IChangeDetector>()
                .PropertyChanging(entry, property);

            Assert.Equal(77, entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues)[property]);
            Assert.True(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues).HasValue(property));
            Assert.Equal(77, entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)[property]);
            Assert.True(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot).HasValue(property));
        }

        [Fact]
        public void PropertyChanging_does_not_snapshot_original_values_for_properties_with_no_original_value_tracking()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModelWithChanging());
            var entry = CreateInternalEntry<ProductWithChanging>(contextServices);

            Assert.False(entry.EntityType.UseEagerSnapshots());
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));

            contextServices
                .GetRequiredService<IChangeDetector>()
                .PropertyChanging(entry, entry.EntityType.GetProperty("Name"));

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));
        }

        [Fact]
        public void PropertyChanging_snapshots_reference_navigations_if_lazy_snapshots_are_in_use()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModelWithChanging());
            var category = new CategoryWithChanging();
            var entry = CreateInternalEntry(contextServices, new ProductWithChanging { Category = category });

            Assert.False(entry.EntityType.UseEagerSnapshots());
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));

            var navigation = entry.EntityType.GetNavigation("Category");

            contextServices
                .GetRequiredService<IChangeDetector>()
                .PropertyChanging(entry, navigation);

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.True(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot).HasValue(navigation));
            Assert.Same(category, entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)[navigation]);
        }

        [Fact]
        public void PropertyChanging_snapshots_PK_for_relationships_if_lazy_snapshots_are_in_use()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModelWithChanging());
            var entry = CreateInternalEntry(contextServices, new ProductWithChanging { Id = 77 });

            Assert.False(entry.EntityType.UseEagerSnapshots());
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));

            var property = entry.EntityType.GetProperty("Id");

            contextServices
                .GetRequiredService<IChangeDetector>()
                .PropertyChanging(entry, property);

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.True(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot).HasValue(property));
            Assert.Equal(77, entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)[property]);
        }

        [Fact]
        public void PropertyChanging_does_not_snapshot_notification_collections()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModelWithChanging());
            var entry = CreateInternalEntry<CategoryWithChanging>(contextServices);

            Assert.False(entry.EntityType.UseEagerSnapshots());
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));

            contextServices
                .GetRequiredService<IChangeDetector>()
                .PropertyChanging(entry, entry.EntityType.GetNavigation("Products"));

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));
        }

        [Fact]
        public void Detects_scalar_property_change()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

            var product = new Product { Id = Guid.NewGuid(), Name = "Oculus Rift" };
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.Name = "Gear VR";

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.True(entry.IsPropertyModified(entry.EntityType.GetProperty("Name")));
        }

        [Fact]
        public void Skips_detection_of_scalar_property_change_for_notification_entities()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModelWithChanged());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

            var product = new ProductWithChanged { Id = 1, Name = "Oculus Rift" };
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.Name = "Gear VR";

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.False(entry.IsPropertyModified(entry.EntityType.GetProperty("Name")));
        }

        [Fact]
        public void Detects_principal_key_change()
        {
            var contextServices = CreateContextServices();

            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

            var category = new Category { Id = -1, PrincipalId = 77 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), -1)));

            category.PrincipalId = 78;

            changeDetector.DetectChanges(entry);

            Assert.Equal(78, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("PrincipalId")]);
            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), -1)));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.PrincipalKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("PrincipalId"), testListener.PrincipalKeyChange.Item2);
            Assert.Equal(77, testListener.PrincipalKeyChange.Item3);
            Assert.Equal(78, testListener.PrincipalKeyChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Detects_principal_key_changing_back_to_original_value()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var category = new Category { Id = -1, PrincipalId = 77 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            category.PrincipalId = 78;

            changeDetector.DetectChanges(entry);

            category.PrincipalId = 77;

            changeDetector.DetectChanges(entry);

            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("PrincipalId")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.PrincipalKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("PrincipalId"), testListener.PrincipalKeyChange.Item2);
            Assert.Equal(78, testListener.PrincipalKeyChange.Item3);
            Assert.Equal(77, testListener.PrincipalKeyChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Reacts_to_principal_key_change_in_sidecar()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var category = new Category { Id = -1, PrincipalId = 77 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            var property = entry.EntityType.GetProperty("PrincipalId");
            var sidecar = entry.AddSidecar(contextServices.GetRequiredService<IStoreGeneratedValuesFactory>()
                .Create(entry, entry.EntityType.GetProperties()));
            sidecar.TakeSnapshot();

            sidecar[property] = 78;

            changeDetector.DetectChanges(entry);

            Assert.Equal(78, entry.RelationshipsSnapshot[property]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.PrincipalKeyChange.Item1);
            Assert.Same(property, testListener.PrincipalKeyChange.Item2);
            Assert.Equal(77, testListener.PrincipalKeyChange.Item3);
            Assert.Equal(78, testListener.PrincipalKeyChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Detects_primary_key_change()
        {
            var contextServices = CreateContextServices();

            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

            var category = new Category { Id = -1 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), -1)));

            category.Id = 78;

            changeDetector.DetectChanges(entry);

            Assert.Equal(78, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("Id")]);
            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), 78)));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.PrincipalKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("Id"), testListener.PrincipalKeyChange.Item2);
            Assert.Equal(-1, testListener.PrincipalKeyChange.Item3);
            Assert.Equal(78, testListener.PrincipalKeyChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Reacts_to_primary_key_change_in_sidecar()
        {
            var contextServices = CreateContextServices();

            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var storeGeneratedValuesFactory = contextServices.GetRequiredService<IStoreGeneratedValuesFactory>();

            var category = new Category { Id = -1 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), -1)));

            var property = entry.EntityType.GetProperty("Id");
            var sidecar = entry.AddSidecar(storeGeneratedValuesFactory.Create(entry, entry.EntityType.GetProperties()));
            sidecar.TakeSnapshot();

            sidecar[property] = 78;

            changeDetector.DetectChanges(entry);

            Assert.Equal(78, entry.RelationshipsSnapshot[property]);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), 78)));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.PrincipalKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("Id"), testListener.PrincipalKeyChange.Item2);
            Assert.Equal(-1, testListener.PrincipalKeyChange.Item3);
            Assert.Equal(78, testListener.PrincipalKeyChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Ignores_no_change_to_principal_key()
        {
            var contextServices = CreateContextServices();

            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

            var category = new Category { Id = -1, PrincipalId = 77 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), -1)));

            category.PrincipalId = 77;

            changeDetector.DetectChanges(entry);

            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("PrincipalId")]);
            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), -1)));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Ignores_no_change_to_principal_key_in_sidecar()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var category = new Category { Id = -1, PrincipalId = 77 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            var property = entry.EntityType.GetProperty("PrincipalId");
            var sidecar = entry.AddSidecar(contextServices.GetRequiredService<IStoreGeneratedValuesFactory>()
                .Create(entry, entry.EntityType.GetProperties()));
            sidecar.TakeSnapshot();

            sidecar[property] = 77;

            changeDetector.DetectChanges(entry);

            Assert.Equal(77, entry.RelationshipsSnapshot[property]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Detects_foreign_key_change()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.DependentId = 78;

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(78, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("DependentId")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.ForeignKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("DependentId"), testListener.ForeignKeyChange.Item2);
            Assert.Equal(77, testListener.ForeignKeyChange.Item3);
            Assert.Equal(78, testListener.ForeignKeyChange.Item4);

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Detects_foreign_key_changing_back_to_original_value()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.DependentId = 78;

            changeDetector.DetectChanges(entry);

            product.DependentId = 77;

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("DependentId")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.ForeignKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("DependentId"), testListener.ForeignKeyChange.Item2);
            Assert.Equal(78, testListener.ForeignKeyChange.Item3);
            Assert.Equal(77, testListener.ForeignKeyChange.Item4);

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Reacts_to_foreign_key_change_in_sidecar()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            var property = entry.EntityType.GetProperty("DependentId");
            var sidecar = entry.AddSidecar(contextServices.GetRequiredService<IStoreGeneratedValuesFactory>()
                .Create(entry, entry.EntityType.GetProperties()));
            sidecar.TakeSnapshot();

            sidecar[property] = 78;

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(78, entry.RelationshipsSnapshot[property]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.ForeignKeyChange.Item1);
            Assert.Same(property, testListener.ForeignKeyChange.Item2);
            Assert.Equal(77, testListener.ForeignKeyChange.Item3);
            Assert.Equal(78, testListener.ForeignKeyChange.Item4);

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Ignores_no_change_to_foreign_key()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.DependentId = 77;

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("DependentId")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Ignores_no_change_to_foreign_key_in_sidecar()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            var property = entry.EntityType.GetProperty("DependentId");
            var sidecar = entry.AddSidecar(contextServices.GetRequiredService<IStoreGeneratedValuesFactory>()
                .Create(entry, entry.EntityType.GetProperties()));
            sidecar.TakeSnapshot();

            sidecar[property] = 77;

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(77, entry.RelationshipsSnapshot[property]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Detects_reference_navigation_change()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var originalCategory = new Category { Id = 77, PrincipalId = 1 };
            var product = new Product { Id = Guid.NewGuid(), Category = originalCategory, DependentId = 1 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            var newCategory = new Category { Id = 1, PrincipalId = 2 };
            product.Category = newCategory;

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(newCategory, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

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

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Detects_reference_navigation_changing_back_to_original_value()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var originalCategory = new Category { Id = 77, PrincipalId = 1 };
            var product = new Product { Id = Guid.NewGuid(), Category = originalCategory, DependentId = 1 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            var newCategory = new Category { Id = 99, PrincipalId = 2 };
            product.Category = newCategory;

            changeDetector.DetectChanges(entry);

            product.Category = originalCategory;

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(originalCategory, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

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

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Ignores_no_change_to_reference_navigation()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var category = new Category { Id = 55, PrincipalId = 1 };
            var product = new Product { Id = Guid.NewGuid(), Category = category, DependentId = 1 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.Category = category;

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(category, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.CollectionChange);
            Assert.Null(testListener.PrincipalKeyChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Detects_adding_to_collection_navigation()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product1 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            var product2 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            var category = new Category { Id = 1, PrincipalId = 77, Products = { product1, product2 } };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            var product3 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            category.Products.Add(product3);

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(
                new[] { product1, product2, product3 },
                ((ICollection<object>)entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Products")])
                    .Cast<Product>()
                    .OrderBy(e => e.DependentId));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.CollectionChange.Item1);
            Assert.Same(entry.EntityType.GetNavigation("Products"), testListener.CollectionChange.Item2);
            Assert.Equal(new[] { product3 }, testListener.CollectionChange.Item3);
            Assert.Empty(testListener.CollectionChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Detects_removing_from_collection_navigation()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product1 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            var product2 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            var category = new Category { Id = 1, PrincipalId = 77, Products = { product1, product2 } };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            category.Products.Remove(product1);

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(
                new[] { product2 },
                ((ICollection<object>)entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Products")])
                    .Cast<Product>()
                    .OrderBy(e => e.DependentId));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

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

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Ignores_no_change_to_collection_navigation()
        {
            var contextServices = CreateContextServices();

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product1 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            var product2 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            var category = new Category { Id = 1, PrincipalId = 77, Products = { product1, product2 } };
            var entry = stateManager.GetOrCreateEntry(category);
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

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Null(testListener.CollectionChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Skips_detecting_changes_to_primary_principal_key_for_notification_entities()
        {
            var contextServices = CreateContextServices(BuildModelWithChanged());

            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

            var product = new ProductWithChanged { Id = 77 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Added);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), 77)));

            product.Id = 78;

            changeDetector.DetectChanges(entry);

            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("Id")]);
            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), 77)));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Skips_detecting_changes_to_foreign_key_for_notification_entities()
        {
            var contextServices = CreateContextServices(BuildModelWithChanged());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product = new ProductWithChanged { Id = 1, DependentId = 77 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.DependentId = 78;

            changeDetector.DetectChanges(entry);

            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("DependentId")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Skips_detecting_changes_to_reference_navigation_for_notification_entities()
        {
            var contextServices = CreateContextServices(BuildModelWithChanged());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var category = new CategoryWithChanged { Id = 1 };
            var product = new ProductWithChanged { Id = 2, Category = category, DependentId = 1 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.Category = new CategoryWithChanged { Id = 2 };

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(category, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.CollectionChange);
            Assert.Null(testListener.PrincipalKeyChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Skips_detecting_changes_to_notifying_collections()
        {
            var contextServices = CreateContextServices(BuildModelWithChanged());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product1 = new ProductWithChanged { Id = 1, DependentId = 77 };
            var product2 = new ProductWithChanged { Id = 2, DependentId = 77 };
            var category = new CategoryWithChanged
                {
                    Id = 77,
                    Products = new ObservableCollection<ProductWithChanged> { product1, product2 }
                };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            var product3 = new ProductWithChanged { Id = 3, DependentId = 77 };
            category.Products.Add(product3);

            changeDetector.DetectChanges(entry);

            // TODO: DetectChanges is actually used here until INotifyCollectionChanged is supported (Issue #445)
            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(
                new[] { product1, product2, product3 },
                ((ICollection<object>)entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Products")])
                    .Cast<ProductWithChanged>()
                    .OrderBy(e => e.DependentId));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.CollectionChange.Item1);
            Assert.Same(entry.EntityType.GetNavigation("Products"), testListener.CollectionChange.Item2);
            Assert.Equal(new[] { product3 }, testListener.CollectionChange.Item3);
            Assert.Empty(testListener.CollectionChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Change_detection_still_happens_for_non_notifying_collections_on_notifying_entities()
        {
            var contextServices = CreateContextServices(BuildModelWithChanged());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product1 = new ProductWithChanged { Id = 1, DependentId = 77 };
            var product2 = new ProductWithChanged { Id = 2, DependentId = 77 };
            var category = new CategoryWithChanged
                {
                    Id = 77,
                    Products = new List<ProductWithChanged> { product1, product2 }
                };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            var product3 = new ProductWithChanged { Id = 3, DependentId = 77 };
            category.Products.Add(product3);

            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(
                new[] { product1, product2, product3 },
                ((ICollection<object>)entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Products")])
                    .Cast<ProductWithChanged>()
                    .OrderBy(e => e.DependentId));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.CollectionChange.Item1);
            Assert.Same(entry.EntityType.GetNavigation("Products"), testListener.CollectionChange.Item2);
            Assert.Equal(new[] { product3 }, testListener.CollectionChange.Item3);
            Assert.Empty(testListener.CollectionChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Brings_in_single_new_entity_set_on_reference_navigation()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var originalCategory = new Category { Id = 77, PrincipalId = 1 };
            var product = new Product { Id = Guid.NewGuid(), Category = originalCategory, DependentId = 1 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            var newCategory = new Category { PrincipalId = 2, Tag = new CategoryTag() };
            product.Category = newCategory;

            changeDetector.DetectChanges(stateManager);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(newCategory, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            Assert.Equal(newCategory.PrincipalId, product.DependentId);
            Assert.Same(newCategory, product.Category);
            Assert.Equal(new[] { product }, newCategory.Products.ToArray());
            Assert.Empty(originalCategory.Products);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(newCategory).EntityState);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(newCategory.Tag).EntityState);

            changeDetector.DetectChanges(stateManager);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(newCategory.Tag).EntityState);
        }

        [Fact]
        public void Brings_in_new_entity_set_on_principal_of_one_to_one_navigation()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var category = new Category { Id = 1, TagId = 77 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            var tag = new CategoryTag();
            category.Tag = tag;

            changeDetector.DetectChanges(stateManager);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(tag, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Tag")]);

            Assert.Equal(category.TagId, 77);
            Assert.Equal(tag.CategoryId, 77);
            Assert.Same(tag, category.Tag);
            Assert.Same(category, tag.Category);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(tag).EntityState);
        }

        [Fact]
        public void Brings_in_new_entity_set_on_dependent_of_one_to_one_navigation()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var tag = new CategoryTag { Id = 1 };
            var entry = stateManager.GetOrCreateEntry(tag);
            entry.SetEntityState(EntityState.Unchanged);

            var category = new Category { TagId = 77 };
            tag.Category = category;

            changeDetector.DetectChanges(stateManager);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(category, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            Assert.Equal(category.TagId, 77);
            Assert.Equal(tag.CategoryId, 77);
            Assert.Same(tag, category.Tag);
            Assert.Same(category, tag.Category);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(category).EntityState);
        }

        [Fact]
        public void Brings_in_single_new_entity_set_on_collection_navigation()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product1 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            var product2 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
            var category = new Category { Id = 1, PrincipalId = 77, Products = { product1, product2 } };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            var product3 = new Product { Tag = new ProductTag() };
            category.Products.Add(product3);

            changeDetector.DetectChanges(stateManager);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);

            Assert.Equal(category.PrincipalId, product3.DependentId);
            Assert.Same(category, product3.Category);
            Assert.Equal(new[] { product1, product2, product3 }.OrderBy(e => e.Id), category.Products.OrderBy(e => e.Id).ToArray());

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(product3).EntityState);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(product3.Tag).EntityState);

            changeDetector.DetectChanges(stateManager);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(product3.Tag).EntityState);
        }

        [Fact]
        public void Brings_in_new_entity_set_on_principal_of_one_to_one_self_ref()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var wife = new Person();
            var entry = stateManager.GetOrCreateEntry(wife);
            entry.SetEntityState(EntityState.Added);

            var husband = new Person();
            wife.Husband = husband;

            changeDetector.DetectChanges(stateManager);

            Assert.Equal(EntityState.Added, entry.EntityState);
            Assert.Equal(husband, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Husband")]);

            Assert.NotEqual(0, husband.Id);
            Assert.NotEqual(0, wife.Id);
            Assert.NotEqual(wife.Id, husband.Id);
            Assert.Equal(husband.Id, wife.HusbandId);
            Assert.Same(husband, wife.Husband);
            Assert.Same(wife, husband.Wife);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(husband).EntityState);
        }

        [Fact]
        public void Brings_in_new_entity_set_on_dependent_of_one_to_one_self_ref()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var husband = new Person();
            var entry = stateManager.GetOrCreateEntry(husband);
            entry.SetEntityState(EntityState.Added);

            var wife = new Person();
            husband.Wife = wife;

            changeDetector.DetectChanges(stateManager);

            Assert.Equal(EntityState.Added, entry.EntityState);
            Assert.Equal(wife, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Wife")]);

            Assert.NotEqual(0, husband.Id);
            Assert.NotEqual(0, wife.Id);
            Assert.NotEqual(wife.Id, husband.Id);
            Assert.Equal(husband.Id, wife.HusbandId);
            Assert.Same(wife, husband.Wife);
            Assert.Same(husband, wife.Husband);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(wife).EntityState);
        }

        [Fact]
        public void Handles_notification_of_principal_key_change()
        {
            var contextServices = CreateContextServices(BuildNotifyingModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var category = new NotifyingCategory { Id = -1, PrincipalId = 77 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), -1)));

            category.PrincipalId = 78;

            Assert.Equal(78, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("PrincipalId")]);
            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), -1)));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.PrincipalKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("PrincipalId"), testListener.PrincipalKeyChange.Item2);
            Assert.Equal(77, testListener.PrincipalKeyChange.Item3);
            Assert.Equal(78, testListener.PrincipalKeyChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Handles_notification_of_principal_key_changing_back_to_original_value()
        {
            var contextServices = CreateContextServices(BuildNotifyingModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var category = new NotifyingCategory { Id = -1, PrincipalId = 77 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            category.PrincipalId = 78;
            category.PrincipalId = 77;

            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("PrincipalId")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.PrincipalKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("PrincipalId"), testListener.PrincipalKeyChange.Item2);
            Assert.Equal(78, testListener.PrincipalKeyChange.Item3);
            Assert.Equal(77, testListener.PrincipalKeyChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Handles_notification_of_primary_key_change()
        {
            var contextServices = CreateContextServices(BuildNotifyingModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var category = new NotifyingCategory { Id = -1 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), -1)));

            category.Id = 78;

            Assert.Equal(78, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("Id")]);
            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), 78)));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.PrincipalKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("Id"), testListener.PrincipalKeyChange.Item2);
            Assert.Equal(-1, testListener.PrincipalKeyChange.Item3);
            Assert.Equal(78, testListener.PrincipalKeyChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Handles_notification_of_no_change_to_principal_key()
        {
            var contextServices = CreateContextServices(BuildNotifyingModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var category = new NotifyingCategory { Id = -1, PrincipalId = 77 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Added);

            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), -1)));

            category.PrincipalId = 77;

            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("PrincipalId")]);
            Assert.Same(entry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entry.EntityType.GetPrimaryKey(), -1)));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Handles_notification_of_foreign_key_change()
        {
            var contextServices = CreateContextServices(BuildNotifyingModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.DependentId = 78;

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(78, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("DependentId")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.ForeignKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("DependentId"), testListener.ForeignKeyChange.Item2);
            Assert.Equal(77, testListener.ForeignKeyChange.Item3);
            Assert.Equal(78, testListener.ForeignKeyChange.Item4);

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Handles_notification_of_foreign_key_changing_back_to_original_value()
        {
            var contextServices = CreateContextServices(BuildNotifyingModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.DependentId = 78;
            product.DependentId = 77;

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("DependentId")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.ForeignKeyChange.Item1);
            Assert.Same(entry.EntityType.GetProperty("DependentId"), testListener.ForeignKeyChange.Item2);
            Assert.Equal(78, testListener.ForeignKeyChange.Item3);
            Assert.Equal(77, testListener.ForeignKeyChange.Item4);

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Handles_notification_of_no_change_to_foreign_key()
        {
            var contextServices = CreateContextServices(BuildNotifyingModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.DependentId = 77;

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(77, entry.RelationshipsSnapshot[entry.EntityType.GetProperty("DependentId")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Handles_notification_of_reference_navigation_change()
        {
            var contextServices = CreateContextServices(BuildNotifyingModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var originalCategory = new NotifyingCategory { Id = 66, PrincipalId = 1 };
            var product = new NotifyingProduct { Id = Guid.NewGuid(), Category = originalCategory, DependentId = 1 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            var newCategory = new NotifyingCategory { Id = 67, PrincipalId = 2 };
            product.Category = newCategory;

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(newCategory, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

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

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Handles_notification_of_reference_navigation_changing_back_to_original_value()
        {
            var contextServices = CreateContextServices(BuildNotifyingModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var originalCategory = new NotifyingCategory { Id = 77, PrincipalId = 1 };
            var product = new NotifyingProduct { Id = Guid.NewGuid(), Category = originalCategory, DependentId = 1 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            var newCategory = new NotifyingCategory { Id = 78, PrincipalId = 2 };

            product.Category = newCategory;
            product.Category = originalCategory;

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(originalCategory, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

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

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Handles_notification_of_no_change_to_reference_navigation()
        {
            var contextServices = CreateContextServices(BuildNotifyingModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var category = new NotifyingCategory { Id = 77, PrincipalId = 1 };
            var product = new NotifyingProduct { Id = Guid.NewGuid(), Category = category, DependentId = 1 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            product.Category = category;

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(category, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.CollectionChange);
            Assert.Null(testListener.PrincipalKeyChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Handles_notification_of_adding_to_collection_navigation()
        {
            var contextServices = CreateContextServices(BuildNotifyingModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product1 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
            var product2 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
            var category = new NotifyingCategory { Id = 1, PrincipalId = 77, Products = { product1, product2 } };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            var product3 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
            category.Products.Add(product3);

            // DetectChanges still needed here because INotifyCollectionChanged not supported (Issue #445)
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(
                new[] { product1, product2, product3 },
                ((ICollection<object>)entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Products")])
                    .Cast<NotifyingProduct>()
                    .OrderBy(e => e.DependentId));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.CollectionChange.Item1);
            Assert.Same(entry.EntityType.GetNavigation("Products"), testListener.CollectionChange.Item2);
            Assert.Equal(new[] { product3 }, testListener.CollectionChange.Item3);
            Assert.Empty(testListener.CollectionChange.Item4);

            Assert.Same(product3, testListener.ReferenceChange.Item1.Entity);
            Assert.Same(testListener.ReferenceChange.Item1.EntityType.GetNavigation("Category"), testListener.ReferenceChange.Item2);
            Assert.Null(testListener.ReferenceChange.Item3);
            Assert.Equal(category, testListener.ReferenceChange.Item4);

            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.PrincipalKeyChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Handles_notification_of_removing_from_collection_navigation()
        {
            var contextServices = CreateContextServices(BuildNotifyingModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product1 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
            var product2 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
            var category = new NotifyingCategory { Id = 1, PrincipalId = 77, Products = { product1, product2 } };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            product1.Category = category;
            product2.Category = category;

            category.Products.Remove(product1);

            // DetectChanges still needed here because INotifyCollectionChanged not supported (Issue #445)
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(
                new[] { product2 },
                ((ICollection<object>)entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Products")])
                    .Cast<NotifyingProduct>()
                    .OrderBy(e => e.DependentId));

            var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

            Assert.Same(entry, testListener.CollectionChange.Item1);
            Assert.Same(entry.EntityType.GetNavigation("Products"), testListener.CollectionChange.Item2);
            Assert.Empty(testListener.CollectionChange.Item3);
            Assert.Equal(new[] { product1 }, testListener.CollectionChange.Item4);

            Assert.Same(product1, testListener.ForeignKeyChange.Item1.Entity);
            Assert.Same(testListener.ForeignKeyChange.Item1.EntityType.GetProperty("DependentId"), testListener.ForeignKeyChange.Item2);
            Assert.Equal(77, testListener.ForeignKeyChange.Item3);
            Assert.Null(testListener.ForeignKeyChange.Item4);

            Assert.Same(product1, testListener.ReferenceChange.Item1.Entity);
            Assert.Same(testListener.ReferenceChange.Item1.EntityType.GetNavigation("Category"), testListener.ReferenceChange.Item2);
            Assert.Equal(category, testListener.ReferenceChange.Item3);
            Assert.Null(testListener.ReferenceChange.Item4);

            Assert.Null(testListener.PrincipalKeyChange);

            AssertDetectChangesNoOp(changeDetector, stateManager, testListener);
        }

        [Fact]
        public void Brings_in_single_new_entity_on_notification_of_set_on_reference_navigation()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildNotifyingModel());

            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var originalCategory = new NotifyingCategory { PrincipalId = 1 };
            var product = new NotifyingProduct { Id = Guid.NewGuid(), Category = originalCategory, DependentId = 1 };
            var entry = stateManager.GetOrCreateEntry(product);
            entry.SetEntityState(EntityState.Unchanged);

            var newCategory = new NotifyingCategory { PrincipalId = 2, Tag = new NotifyingCategoryTag() };
            product.Category = newCategory;

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(newCategory, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            Assert.Equal(newCategory.PrincipalId, product.DependentId);
            Assert.Same(newCategory, product.Category);
            Assert.Equal(new[] { product }, newCategory.Products.ToArray());
            Assert.Empty(originalCategory.Products);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(newCategory).EntityState);
            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(newCategory.Tag).EntityState);
        }

        [Fact]
        public void Brings_in_new_entity_on_notification_of_set_on_principal_of_one_to_one_navigation()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildNotifyingModel());

            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var category = new NotifyingCategory { Id = 1, TagId = 77 };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            var tag = new NotifyingCategoryTag { Id = 2 };
            category.Tag = tag;

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(tag, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Tag")]);

            Assert.Equal(category.TagId, 77);
            Assert.Equal(tag.CategoryId, 77);
            Assert.Same(tag, category.Tag);
            Assert.Same(category, tag.Category);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(tag).EntityState);
        }

        [Fact]
        public void Brings_in_new_entity_on_notification_of_set_on_dependent_of_one_to_one_navigation()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildNotifyingModel());

            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var tag = new NotifyingCategoryTag { Id = 1 };
            var entry = stateManager.GetOrCreateEntry(tag);
            entry.SetEntityState(EntityState.Unchanged);

            var category = new NotifyingCategory { TagId = 77 };
            tag.Category = category;

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(category, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Category")]);

            Assert.Equal(category.TagId, 77);
            Assert.Equal(tag.CategoryId, 77);
            Assert.Same(tag, category.Tag);
            Assert.Same(category, tag.Category);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(category).EntityState);
        }

        [Fact]
        public void Brings_in_single_new_entity_on_notification_of_set_on_collection_navigation()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildNotifyingModel());

            var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var product1 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
            var product2 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
            var category = new NotifyingCategory { Id = 1, PrincipalId = 77, Products = { product1, product2 } };
            var entry = stateManager.GetOrCreateEntry(category);
            entry.SetEntityState(EntityState.Unchanged);

            var product3 = new NotifyingProduct { Tag = new NotifyingProductTag() };
            category.Products.Add(product3);

            // DetectChanges still needed here because INotifyCollectionChanged not supported (Issue #445)
            changeDetector.DetectChanges(entry);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);

            Assert.Equal(category.PrincipalId, product3.DependentId);
            Assert.Same(category, product3.Category);
            Assert.Equal(new[] { product1, product2, product3 }.OrderBy(e => e.Id), category.Products.OrderBy(e => e.Id).ToArray());

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(product3).EntityState);
            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(product3.Tag).EntityState);
        }

        [Fact]
        public void Brings_in_new_entity_on_notification_of_set_on_principal_of_one_to_one_self_ref()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildNotifyingModel());

            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var wife = new NotifyingPerson();
            var entry = stateManager.GetOrCreateEntry(wife);
            entry.SetEntityState(EntityState.Added);

            var husband = new NotifyingPerson();
            wife.Husband = husband;

            Assert.Equal(EntityState.Added, entry.EntityState);
            Assert.Equal(husband, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Husband")]);

            Assert.NotEqual(0, husband.Id);
            Assert.NotEqual(0, wife.Id);
            Assert.NotEqual(wife.Id, husband.Id);
            Assert.Equal(husband.Id, wife.HusbandId);
            Assert.Same(husband, wife.Husband);
            Assert.Same(wife, husband.Wife);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(husband).EntityState);
        }

        [Fact]
        public void Brings_in_new_entity_on_notification_of_set_on_dependent_of_one_to_one_self_ref()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(BuildNotifyingModel());

            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var husband = new NotifyingPerson();
            var entry = stateManager.GetOrCreateEntry(husband);
            entry.SetEntityState(EntityState.Added);

            var wife = new NotifyingPerson();
            husband.Wife = wife;

            Assert.Equal(EntityState.Added, entry.EntityState);
            Assert.Equal(wife, entry.RelationshipsSnapshot[entry.EntityType.GetNavigation("Wife")]);

            Assert.NotEqual(0, husband.Id);
            Assert.NotEqual(0, wife.Id);
            Assert.NotEqual(wife.Id, husband.Id);
            Assert.Equal(husband.Id, wife.HusbandId);
            Assert.Same(wife, husband.Wife);
            Assert.Same(husband, wife.Husband);

            Assert.Equal(EntityState.Added, stateManager.GetOrCreateEntry(wife).EntityState);
        }

        private class Category
        {
            public int Id { get; set; }
            public int? PrincipalId { get; set; }
            public string Name { get; set; }

            public virtual ICollection<Product> Products { get; } = new List<Product>();

            public int TagId { get; set; }
            public CategoryTag Tag { get; set; }
        }

        private class CategoryTag
        {
            public int Id { get; set; }

            public int CategoryId { get; set; }
            public Category Category { get; set; }
        }

        private class Product
        {
            public Guid Id { get; set; }
            public int? DependentId { get; set; }
            public string Name { get; set; }

            public virtual Category Category { get; set; }

            public int TagId { get; set; }
            public ProductTag Tag { get; set; }
        }

        private class ProductTag
        {
            public int Id { get; set; }

            public int ProductId { get; set; }
            public Product Product { get; set; }
        }

        private class Person
        {
            public int Id { get; set; }

            public int HusbandId { get; set; }
            public Person Husband { get; set; }
            public Person Wife { get; set; }
        }

        private static IModel BuildModel()
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<Product>(b =>
                {
                    b.HasOne(e => e.Tag).WithOne(e => e.Product)
                        .HasPrincipalKey<Product>(e => e.TagId)
                        .HasForeignKey<ProductTag>(e => e.ProductId);
                    b.Property(e => e.TagId).Metadata.RequiresValueGenerator = false;
                });

            builder.Entity<Category>(b =>
                {
                    b.HasMany(e => e.Products).WithOne(e => e.Category)
                        .HasForeignKey(e => e.DependentId)
                        .HasPrincipalKey(e => e.PrincipalId);
                    b.Property(e => e.PrincipalId).Metadata.RequiresValueGenerator = false;

                    b.HasOne(e => e.Tag).WithOne(e => e.Category)
                        .HasForeignKey<CategoryTag>(e => e.CategoryId)
                        .HasPrincipalKey<Category>(e => e.TagId);
                    b.Property(e => e.TagId).Metadata.RequiresValueGenerator = false;
                });

            builder.Entity<Person>()
                .HasOne(e => e.Husband).WithOne(e => e.Wife)
                .HasForeignKey<Person>(e => e.HusbandId);

            return builder.Model;
        }

        private class NotifyingCategory : NotifyingEntity
        {
            private int _id;
            private int? _principalId;
            private string _name;
            private int _tagId;
            private NotifyingCategoryTag _tag;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public int? PrincipalId
            {
                get { return _principalId; }
                set { SetWithNotify(value, ref _principalId); }
            }

            public string Name
            {
                get { return _name; }
                set { SetWithNotify(value, ref _name); }
            }

            public virtual ICollection<NotifyingProduct> Products { get; } = new ObservableCollection<NotifyingProduct>();

            public int TagId
            {
                get { return _tagId; }
                set { SetWithNotify(value, ref _tagId); }
            }

            public NotifyingCategoryTag Tag
            {
                get { return _tag; }
                set { SetWithNotify(value, ref _tag); }
            }
        }

        private class NotifyingCategoryTag : NotifyingEntity
        {
            private int _id;
            private int _categoryId;
            private NotifyingCategory _category;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public int CategoryId
            {
                get { return _categoryId; }
                set { SetWithNotify(value, ref _categoryId); }
            }

            public NotifyingCategory Category
            {
                get { return _category; }
                set { SetWithNotify(value, ref _category); }
            }
        }

        private class NotifyingProduct : NotifyingEntity
        {
            private Guid _id;
            private int? _dependentId;
            private string _name;
            private NotifyingCategory _category;
            private int _tagId;
            private NotifyingProductTag _tag;

            public Guid Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public int? DependentId
            {
                get { return _dependentId; }
                set { SetWithNotify(value, ref _dependentId); }
            }

            public string Name
            {
                get { return _name; }
                set { SetWithNotify(value, ref _name); }
            }

            public virtual NotifyingCategory Category
            {
                get { return _category; }
                set { SetWithNotify(value, ref _category); }
            }

            public int TagId
            {
                get { return _tagId; }
                set { SetWithNotify(value, ref _tagId); }
            }

            public NotifyingProductTag Tag
            {
                get { return _tag; }
                set { SetWithNotify(value, ref _tag); }
            }
        }

        private class NotifyingProductTag : NotifyingEntity
        {
            private int _id;
            private int _productId;
            private NotifyingProduct _product;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public int ProductId
            {
                get { return _productId; }
                set { SetWithNotify(value, ref _productId); }
            }

            public NotifyingProduct Product
            {
                get { return _product; }
                set { SetWithNotify(value, ref _product); }
            }
        }

        private class NotifyingPerson : NotifyingEntity
        {
            private int _id;
            private int _husbandId;
            private NotifyingPerson _husband;
            private NotifyingPerson _wife;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public int HusbandId
            {
                get { return _husbandId; }
                set { SetWithNotify(value, ref _husbandId); }
            }

            public NotifyingPerson Husband
            {
                get { return _husband; }
                set { SetWithNotify(value, ref _husband); }
            }

            public NotifyingPerson Wife
            {
                get { return _wife; }
                set { SetWithNotify(value, ref _wife); }
            }
        }

        private class NotifyingEntity : INotifyPropertyChanging, INotifyPropertyChanged
        {
            protected void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
            {
                // Intentionally not checking if new value is different for robustness of handler code
                NotifyChanging(propertyName);
                field = value;
                NotifyChanged(propertyName);
            }

            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private void NotifyChanging(string propertyName)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        private static IModel BuildNotifyingModel()
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<NotifyingProduct>(b =>
                {
                    b.HasOne(e => e.Tag).WithOne(e => e.Product)
                        .HasPrincipalKey<NotifyingProduct>(e => e.TagId)
                        .HasForeignKey<NotifyingProductTag>(e => e.ProductId);
                    b.Property(e => e.TagId).Metadata.RequiresValueGenerator = false;
                });


            builder.Entity<NotifyingCategory>(b =>
                {
                    b.HasMany(e => e.Products).WithOne(e => e.Category)
                        .HasForeignKey(e => e.DependentId)
                        .HasPrincipalKey(e => e.PrincipalId);
                    b.Property(e => e.PrincipalId).Metadata.RequiresValueGenerator = false;

                    b.HasOne(e => e.Tag).WithOne(e => e.Category)
                        .HasForeignKey<NotifyingCategoryTag>(e => e.CategoryId)
                        .HasPrincipalKey<NotifyingCategory>(e => e.TagId);
                    b.Property(e => e.TagId).Metadata.RequiresValueGenerator = false;
                });

            builder.Entity<NotifyingPerson>()
                .HasOne(e => e.Husband).WithOne(e => e.Wife)
                .HasForeignKey<NotifyingPerson>(e => e.HusbandId);

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
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<ProductWithChanging>();
            builder.Entity<CategoryWithChanging>()
                .HasMany(e => e.Products).WithOne(e => e.Category)
                .HasForeignKey(e => e.DependentId);

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
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<ProductWithChanged>();
            builder.Entity<CategoryWithChanged>()
                .HasMany(e => e.Products).WithOne(e => e.Category)
                .HasForeignKey(e => e.DependentId);

            return builder.Model;
        }

        private static InternalClrEntityEntry CreateInternalEntry<TEntity>(IServiceProvider contextServices, TEntity entity = null)
            where TEntity : class, new()
        {
            return new InternalClrEntityEntry(
                contextServices.GetRequiredService<IStateManager>(),
                contextServices.GetRequiredService<IModel>().GetEntityType(typeof(TEntity)),
                contextServices.GetRequiredService<IEntityEntryMetadataServices>(), entity ?? new TEntity());
        }

        private static void AssertDetectChangesNoOp(
            IChangeDetector changeDetector, IStateManager stateManager, TestRelationshipListener testListener)
        {
            testListener.PrincipalKeyChange = null;
            testListener.ForeignKeyChange = null;
            testListener.ReferenceChange = null;
            testListener.CollectionChange = null;

            changeDetector.DetectChanges(stateManager);

            Assert.Null(testListener.PrincipalKeyChange);
            Assert.Null(testListener.ForeignKeyChange);
            Assert.Null(testListener.ReferenceChange);
            Assert.Null(testListener.CollectionChange);
        }

        private static IServiceProvider CreateContextServices(IModel model = null)
        {
            return TestHelpers.Instance.CreateContextServices(
                new ServiceCollection()
                    .AddScoped<TestRelationshipListener>()
                    .AddScoped<IForeignKeyListener>(p => p.GetRequiredService<TestRelationshipListener>())
                    .AddScoped<INavigationListener>(p => p.GetRequiredService<TestRelationshipListener>())
                    .AddScoped<IKeyListener>(p => p.GetRequiredService<TestRelationshipListener>()),
                model ?? BuildModel());
        }

        private class TestRelationshipListener : IForeignKeyListener, INavigationListener, IKeyListener
        {
            public Tuple<InternalEntityEntry, IProperty, object, object> ForeignKeyChange { get; set; }
            public Tuple<InternalEntityEntry, IProperty, object, object> PrincipalKeyChange { get; set; }
            public Tuple<InternalEntityEntry, INavigation, object, object> ReferenceChange { get; set; }
            public Tuple<InternalEntityEntry, INavigation, ISet<object>, ISet<object>> CollectionChange { get; set; }

            public void ForeignKeyPropertyChanged(InternalEntityEntry entry, IProperty property, object oldValue, object newValue)
            {
                ForeignKeyChange = Tuple.Create(entry, property, oldValue, newValue);
            }

            public void NavigationReferenceChanged(InternalEntityEntry entry, INavigation navigation, object oldValue, object newValue)
            {
                ReferenceChange = Tuple.Create(entry, navigation, oldValue, newValue);
            }

            public void NavigationCollectionChanged(InternalEntityEntry entry, INavigation navigation, ISet<object> added, ISet<object> removed)
            {
                CollectionChange = Tuple.Create(entry, navigation, added, removed);
            }

            public void KeyPropertyChanged(InternalEntityEntry entry, IProperty property, object oldValue, object newValue)
            {
                PrincipalKeyChange = Tuple.Create(entry, property, oldValue, newValue);
            }
        }
    }
}
