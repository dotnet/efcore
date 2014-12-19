// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Moq;
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
        public void PropertyChanged_entities_do_not_require_DetectChanges()
        {
            var contextServices = TestHelpers.CreateContextServices(BuildModelWithChanged());
            var entry = CreateStateEntry<CategoryWithChanged>(contextServices);

            // TODO: The following assert should be changed to False once INotifyCollectionChanged is supported (Issue #445)
            Assert.True(contextServices
                .GetRequiredService<ChangeDetector>()
                .RequiresDetectChanges(entry));
        }

        [Fact]
        public void Non_PropertyChanged_entities_do_require_DetectChanges()
        {
            var contextServices = TestHelpers.CreateContextServices(BuildModelWithChanging());
            var entry = CreateStateEntry<CategoryWithChanging>(contextServices);

            Assert.True(contextServices
                .GetRequiredService<ChangeDetector>()
                .RequiresDetectChanges(entry));
        }

        [Fact]
        public void PropertyChanged_entities_with_non_notifying_collections_require_DetectChanges()
        {
            var contextServices = TestHelpers.CreateContextServices(BuildModelWithChanged());
            var entry = CreateStateEntry(contextServices, new CategoryWithChanged { Products = new List<ProductWithChanged>()});

            Assert.True(contextServices
                .GetRequiredService<ChangeDetector>()
                .RequiresDetectChanges(entry));
        }

        [Fact]
        public void Detects_principal_key_change()
        {
            var notifierMock = new Mock<StateEntryNotifier>();
            var model = BuildModel();
            var contextServices = CreateContextServices(notifierMock.Object, model);
            var stateManager = contextServices.GetRequiredService<StateManager>();

            var entityType = model.GetEntityType(typeof(Category));
            var keyProperty = entityType.GetProperty("PrincipalId");

            var category = new Category { Id = -1, PrincipalId = 77 };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[keyProperty] = 77;
            principalEntry.SetEntityState(EntityState.Added);

            var changeDetector = new ChangeDetector(new DbContextService<IModel>(model));

            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));

            category.PrincipalId = 78;
            changeDetector.PropertyChanged(principalEntry, keyProperty);

            notifierMock.Verify(m => m.PrincipalKeyPropertyChanged(principalEntry, keyProperty, 77, 78));

            Assert.Equal(78, principalEntry.RelationshipsSnapshot[keyProperty]);
            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));
        }

        [Fact]
        public void Reacts_to_principal_key_change_in_sidecar()
        {
            var notifierMock = new Mock<StateEntryNotifier>();
            var model = BuildModel();
            var contextServices = CreateContextServices(notifierMock.Object, model);
            var stateManager = contextServices.GetRequiredService<StateManager>();

            var entityType = model.GetEntityType(typeof(Category));
            var keyProperty = entityType.GetProperty("PrincipalId");

            var category = new Category { Id = -1, PrincipalId = 77 };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[keyProperty] = 77;
            principalEntry.SetEntityState(EntityState.Added);

            var changeDetector = new ChangeDetector(new DbContextService<IModel>(model));

            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));

            principalEntry.AddSidecar(new StoreGeneratedValuesFactory().Create(principalEntry, new IProperty[0]))[keyProperty] = 78;
            changeDetector.SidecarPropertyChanged(principalEntry, keyProperty);

            notifierMock.Verify(m => m.PrincipalKeyPropertyChanged(principalEntry, keyProperty, 77, 78));

            Assert.Equal(78, principalEntry.RelationshipsSnapshot[keyProperty]);
            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));
        }

        [Fact]
        public void Detects_primary_key_change()
        {
            var notifierMock = new Mock<StateEntryNotifier>();
            var model = BuildModel();
            var contextServices = CreateContextServices(notifierMock.Object, model);
            var stateManager = contextServices.GetRequiredService<StateManager>();

            var entityType = model.GetEntityType(typeof(Category));
            var keyProperty = entityType.GetProperty("Id");

            var category = new Category { Id = -1 };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[keyProperty] = -1;
            principalEntry.SetEntityState(EntityState.Added);

            var changeDetector = new ChangeDetector(new DbContextService<IModel>(model));

            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));

            category.Id = 1;
            changeDetector.PropertyChanged(principalEntry, keyProperty);

            notifierMock.Verify(m => m.PrincipalKeyPropertyChanged(principalEntry, keyProperty, -1, 1));

            Assert.Equal(1, principalEntry.RelationshipsSnapshot[keyProperty]);
            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, 1)));
        }

        [Fact]
        public void Reacts_to_primary_key_change_in_sidecar()
        {
            var notifierMock = new Mock<StateEntryNotifier>();
            var model = BuildModel();
            var contextServices = CreateContextServices(notifierMock.Object, model);
            var stateManager = contextServices.GetRequiredService<StateManager>();

            var entityType = model.GetEntityType(typeof(Category));
            var keyProperty = entityType.GetProperty("Id");

            var category = new Category { Id = -1 };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[keyProperty] = -1;
            principalEntry.SetEntityState(EntityState.Added);

            var changeDetector = new ChangeDetector(new DbContextService<IModel>(model));

            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));

            principalEntry.AddSidecar(new StoreGeneratedValuesFactory().Create(principalEntry, new IProperty[0]))[keyProperty] = 1;
            changeDetector.SidecarPropertyChanged(principalEntry, keyProperty);

            notifierMock.Verify(m => m.PrincipalKeyPropertyChanged(principalEntry, keyProperty, -1, 1));

            Assert.Equal(1, principalEntry.RelationshipsSnapshot[keyProperty]);
            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, 1)));
        }

        [Fact]
        public void Ignores_non_principal_key_change()
        {
            var notifierMock = new Mock<StateEntryNotifier>();
            var model = BuildModel();
            var contextServices = CreateContextServices(notifierMock.Object, model);
            var stateManager = contextServices.GetRequiredService<StateManager>();

            var entityType = model.GetEntityType(typeof(Category));
            var property = entityType.GetProperty("Name");

            var category = new Category { Id = -1, Name = "Blue" };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[property] = "Blue";
            principalEntry.SetEntityState(EntityState.Added);

            var changeDetector = new ChangeDetector(new DbContextService<IModel>(model));

            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));

            category.Name = "Red";
            changeDetector.PropertyChanged(principalEntry, property);

            notifierMock.Verify(m => m.PrincipalKeyPropertyChanged(
                It.IsAny<StateEntry>(), It.IsAny<IProperty>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);

            Assert.Equal("Blue", principalEntry.RelationshipsSnapshot[property]);
            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));
        }

        [Fact]
        public void Ignores_non_principal_key_change_in_sidecar()
        {
            var notifierMock = new Mock<StateEntryNotifier>();
            var model = BuildModel();
            var contextServices = CreateContextServices(notifierMock.Object, model);
            var stateManager = contextServices.GetRequiredService<StateManager>();

            var entityType = model.GetEntityType(typeof(Category));
            var property = entityType.GetProperty("Name");

            var category = new Category { Id = -1, Name = "Blue" };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[property] = "Blue";
            principalEntry.SetEntityState(EntityState.Added);

            var changeDetector = new ChangeDetector(new DbContextService<IModel>(model));

            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));

            principalEntry.AddSidecar(new StoreGeneratedValuesFactory().Create(principalEntry, new IProperty[0]))[property] = "Red";
            changeDetector.SidecarPropertyChanged(principalEntry, property);

            notifierMock.Verify(m => m.PrincipalKeyPropertyChanged(
                It.IsAny<StateEntry>(), It.IsAny<IProperty>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);

            Assert.Equal("Blue", principalEntry.RelationshipsSnapshot[property]);
            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));
        }

        [Fact]
        public void Ignores_no_change_to_principal_key()
        {
            var notifierMock = new Mock<StateEntryNotifier>();
            var model = BuildModel();
            var contextServices = CreateContextServices(notifierMock.Object, model);
            var stateManager = contextServices.GetRequiredService<StateManager>();

            var entityType = model.GetEntityType(typeof(Category));
            var keyProperty = entityType.GetProperty("PrincipalId");

            var category = new Category { Id = -1, PrincipalId = 77 };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[keyProperty] = 77;
            principalEntry.SetEntityState(EntityState.Added);

            var changeDetector = new ChangeDetector(new DbContextService<IModel>(model));

            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));

            changeDetector.PropertyChanged(principalEntry, keyProperty);

            notifierMock.Verify(m => m.PrincipalKeyPropertyChanged(
                It.IsAny<StateEntry>(), It.IsAny<IProperty>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);

            Assert.Equal(77, principalEntry.RelationshipsSnapshot[keyProperty]);
            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));
        }

        [Fact]
        public void Ignores_no_change_to_principal_key_in_sidecar()
        {
            var notifierMock = new Mock<StateEntryNotifier>();
            var model = BuildModel();
            var contextServices = CreateContextServices(notifierMock.Object, model);
            var stateManager = contextServices.GetRequiredService<StateManager>();

            var entityType = model.GetEntityType(typeof(Category));
            var keyProperty = entityType.GetProperty("PrincipalId");

            var category = new Category { Id = -1, PrincipalId = 77 };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[keyProperty] = 77;
            principalEntry.SetEntityState(EntityState.Added);

            var changeDetector = new ChangeDetector(new DbContextService<IModel>(model));

            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));

            principalEntry.AddSidecar(new StoreGeneratedValuesFactory().Create(principalEntry, new IProperty[0]))[keyProperty] = 77;
            changeDetector.PropertyChanged(principalEntry, keyProperty);

            notifierMock.Verify(m => m.PrincipalKeyPropertyChanged(
                It.IsAny<StateEntry>(), It.IsAny<IProperty>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);

            Assert.Equal(77, principalEntry.RelationshipsSnapshot[keyProperty]);
            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));
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
    }
}
