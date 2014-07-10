// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ChangeDetectorTest
    {
        [Fact]
        public void Detects_principal_key_change()
        {
            var model = BuildModel();
            var configuration = TestHelpers.CreateContextConfiguration(model);
            var stateManager = configuration.Services.StateManager;

            var entityType = model.GetEntityType(typeof(Category));
            var keyProperty = entityType.GetProperty("PrincipalId");

            var category = new Category { Id = -1, PrincipalId = 77 };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[keyProperty] = 77;
            principalEntry.EntityState = EntityState.Added;

            var notifierMock = new Mock<StateEntryNotifier>();
            var changeDetector = new ChangeDetector(configuration, notifierMock.Object);

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
            var model = BuildModel();
            var configuration = TestHelpers.CreateContextConfiguration(model);
            var stateManager = configuration.Services.StateManager;

            var entityType = model.GetEntityType(typeof(Category));
            var keyProperty = entityType.GetProperty("PrincipalId");

            var category = new Category { Id = -1, PrincipalId = 77 };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[keyProperty] = 77;
            principalEntry.EntityState = EntityState.Added;

            var notifierMock = new Mock<StateEntryNotifier>();
            var changeDetector = new ChangeDetector(configuration, notifierMock.Object);

            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));

            principalEntry.AddSidecar(new StoreGeneratedValuesFactory().Create(principalEntry))[keyProperty] = 78;
            changeDetector.SidecarPropertyChanged(principalEntry, keyProperty);

            notifierMock.Verify(m => m.PrincipalKeyPropertyChanged(principalEntry, keyProperty, 77, 78));

            Assert.Equal(78, principalEntry.RelationshipsSnapshot[keyProperty]);
            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));
        }

        [Fact]
        public void Detects_primary_key_change()
        {
            var model = BuildModel();
            var configuration = TestHelpers.CreateContextConfiguration(model);
            var stateManager = configuration.Services.StateManager;

            var entityType = model.GetEntityType(typeof(Category));
            var keyProperty = entityType.GetProperty("Id");

            var category = new Category { Id = -1 };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[keyProperty] = -1;
            principalEntry.EntityState = EntityState.Added;

            var notifierMock = new Mock<StateEntryNotifier>();
            var changeDetector = new ChangeDetector(configuration, notifierMock.Object);

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
            var model = BuildModel();
            var configuration = TestHelpers.CreateContextConfiguration(model);
            var stateManager = configuration.Services.StateManager;

            var entityType = model.GetEntityType(typeof(Category));
            var keyProperty = entityType.GetProperty("Id");

            var category = new Category { Id = -1 };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[keyProperty] = -1;
            principalEntry.EntityState = EntityState.Added;

            var notifierMock = new Mock<StateEntryNotifier>();
            var changeDetector = new ChangeDetector(configuration, notifierMock.Object);

            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));

            principalEntry.AddSidecar(new StoreGeneratedValuesFactory().Create(principalEntry))[keyProperty] = 1;
            changeDetector.SidecarPropertyChanged(principalEntry, keyProperty);

            notifierMock.Verify(m => m.PrincipalKeyPropertyChanged(principalEntry, keyProperty, -1, 1));

            Assert.Equal(1, principalEntry.RelationshipsSnapshot[keyProperty]);
            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, 1)));
        }

        [Fact]
        public void Ignores_non_principal_key_change()
        {
            var model = BuildModel();
            var configuration = TestHelpers.CreateContextConfiguration(model);
            var stateManager = configuration.Services.StateManager;

            var entityType = model.GetEntityType(typeof(Category));
            var property = entityType.GetProperty("Name");

            var category = new Category { Id = -1, Name = "Blue" };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[property] = "Blue";
            principalEntry.EntityState = EntityState.Added;

            var notifierMock = new Mock<StateEntryNotifier>();
            var changeDetector = new ChangeDetector(configuration, notifierMock.Object);

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
            var model = BuildModel();
            var configuration = TestHelpers.CreateContextConfiguration(model);
            var stateManager = configuration.Services.StateManager;

            var entityType = model.GetEntityType(typeof(Category));
            var property = entityType.GetProperty("Name");

            var category = new Category { Id = -1, Name = "Blue" };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[property] = "Blue";
            principalEntry.EntityState = EntityState.Added;

            var notifierMock = new Mock<StateEntryNotifier>();
            var changeDetector = new ChangeDetector(configuration, notifierMock.Object);

            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));

            principalEntry.AddSidecar(new StoreGeneratedValuesFactory().Create(principalEntry))[property] = "Red";
            changeDetector.SidecarPropertyChanged(principalEntry, property);

            notifierMock.Verify(m => m.PrincipalKeyPropertyChanged(
                It.IsAny<StateEntry>(), It.IsAny<IProperty>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);

            Assert.Equal("Blue", principalEntry.RelationshipsSnapshot[property]);
            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));
        }

        [Fact]
        public void Ignores_no_change_to_principal_key()
        {
            var model = BuildModel();
            var configuration = TestHelpers.CreateContextConfiguration(model);
            var stateManager = configuration.Services.StateManager;

            var entityType = model.GetEntityType(typeof(Category));
            var keyProperty = entityType.GetProperty("PrincipalId");

            var category = new Category { Id = -1, PrincipalId = 77 };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[keyProperty] = 77;
            principalEntry.EntityState = EntityState.Added;

            var notifierMock = new Mock<StateEntryNotifier>();
            var changeDetector = new ChangeDetector(configuration, notifierMock.Object);

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
            var model = BuildModel();
            var configuration = TestHelpers.CreateContextConfiguration(model);
            var stateManager = configuration.Services.StateManager;

            var entityType = model.GetEntityType(typeof(Category));
            var keyProperty = entityType.GetProperty("PrincipalId");

            var category = new Category { Id = -1, PrincipalId = 77 };
            var principalEntry = stateManager.StartTracking(stateManager.GetOrCreateEntry(category));
            principalEntry.RelationshipsSnapshot[keyProperty] = 77;
            principalEntry.EntityState = EntityState.Added;

            var notifierMock = new Mock<StateEntryNotifier>();
            var changeDetector = new ChangeDetector(configuration, notifierMock.Object);

            Assert.Same(principalEntry, stateManager.TryGetEntry(new SimpleEntityKey<int>(entityType, -1)));

            principalEntry.AddSidecar(new StoreGeneratedValuesFactory().Create(principalEntry))[keyProperty] = 77;
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
        }

        private class Product
        {
            public Guid Id { get; set; }
            public int? DependentId { get; set; }
            public string Name { get; set; }
        }

        private static IModel BuildModel()
        {
            var model = new Model();
            var builder = new ConventionModelBuilder(model);

            builder.Entity<Product>();
            builder.Entity<Category>();

            var productType = model.GetEntityType(typeof(Product));
            var categoryType = model.GetEntityType(typeof(Category));

            categoryType.GetProperty("Id").ValueGenerationOnAdd = ValueGenerationOnAdd.None;

            productType.AddForeignKey(new Key(new[] { categoryType.GetProperty("PrincipalId") }), productType.GetProperty("DependentId"));

            return model;
        }
    }
}
