// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public abstract class SidecarTest
    {
        protected readonly Model _model;

        protected SidecarTest()
        {
            _model = BuildModel();
        }

        [Fact]
        public void Can_check_if_property_value_can_be_stored()
        {
            var sidecar = CreateSidecar();

            Assert.True(sidecar.CanStoreValue(IdProperty));
            Assert.False(sidecar.CanStoreValue(NameProperty));
            Assert.True(sidecar.CanStoreValue(FkProperty));
        }

        [Fact]
        public void Can_check_if_value_is_stored()
        {
            var sidecar = CreateSidecar();

            Assert.False(sidecar.HasValue(IdProperty));
            Assert.False(sidecar.HasValue(NameProperty));
            Assert.False(sidecar.HasValue(FkProperty));

            sidecar[IdProperty] = 78;
            sidecar[FkProperty] = 89;

            Assert.True(sidecar.HasValue(IdProperty));
            Assert.False(sidecar.HasValue(NameProperty));
            Assert.True(sidecar.HasValue(FkProperty));
        }

        [Fact]
        public void Can_read_and_write_values()
        {
            var entity = new Banana { Id = 77, Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            Assert.Equal(77, sidecar[IdProperty]);
            Assert.Equal(88, sidecar[FkProperty]);

            sidecar[IdProperty] = 78;
            sidecar[FkProperty] = 89;

            Assert.Equal(78, sidecar[IdProperty]);
            Assert.Equal(89, sidecar[FkProperty]);

            Assert.Equal(77, entity.Id);
            Assert.Equal(88, entity.Fk);
        }

        [Fact]
        public void Can_read_and_write_null()
        {
            var entity = new Banana { Id = 77, Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar[FkProperty] = null;

            Assert.True(sidecar.HasValue(FkProperty));
            Assert.Null(sidecar[FkProperty]);

            Assert.Equal(88, entity.Fk);
        }

        [Fact]
        public void Can_commit_values_into_state_entry()
        {
            var entity = new Banana { Id = 77, Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar[FkProperty] = 89;
            sidecar.Commit();

            Assert.Equal(77, entity.Id);
            Assert.Equal(89, entity.Fk);
        }

        [Fact]
        public void Committing_detaches_sidecar()
        {
            var entity = new Banana { Id = 77, Name = "Stand", Fk = 88 };
            var stateEntry = CreateStateEntry(entity);

            var sidecar = stateEntry.AddSidecar(CreateSidecar(stateEntry));

            sidecar.Commit();

            Assert.Null(stateEntry.TryGetSidecar(sidecar.Name));
        }

        [Fact]
        public void Rolling_back_detaches_sidecar_without_committing_values()
        {
            var entity = new Banana { Id = 77, Name = "Stand", Fk = 88 };
            var stateEntry = CreateStateEntry(entity);

            var sidecar = stateEntry.AddSidecar(CreateSidecar(stateEntry));

            sidecar[FkProperty] = 89;
            sidecar.Rollback();

            Assert.Null(stateEntry.TryGetSidecar(sidecar.Name));

            Assert.Equal(77, entity.Id);
            Assert.Equal(88, entity.Fk);
        }

        [Fact]
        public void Can_snapshot_values()
        {
            var entity = new Banana { Id = 77, Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar.TakeSnapshot();

            Assert.Equal(77, sidecar[IdProperty]);
            Assert.Equal(88, sidecar[FkProperty]);

            sidecar[IdProperty] = 78;
            sidecar[FkProperty] = 89;

            Assert.Equal(78, sidecar[IdProperty]);
            Assert.Equal(89, sidecar[FkProperty]);

            Assert.Equal(77, entity.Id);
            Assert.Equal(88, entity.Fk);

            entity.Id = 76;
            entity.Fk = 90;

            Assert.Equal(78, sidecar[IdProperty]);
            Assert.Equal(89, sidecar[FkProperty]);

            Assert.Equal(76, entity.Id);
            Assert.Equal(90, entity.Fk);

            sidecar.TakeSnapshot();

            Assert.Equal(76, sidecar[IdProperty]);
            Assert.Equal(90, sidecar[FkProperty]);
        }

        [Fact]
        public void Can_snapshot_individual_values()
        {
            var entity = new Banana { Id = 77, Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar.TakeSnapshot(FkProperty);

            Assert.Equal(77, sidecar[IdProperty]);
            Assert.Equal(88, sidecar[FkProperty]);

            sidecar[FkProperty] = 89;

            Assert.Equal(77, sidecar[IdProperty]);
            Assert.Equal(89, sidecar[FkProperty]);

            Assert.Equal(77, entity.Id);
            Assert.Equal(88, entity.Fk);

            entity.Id = 76;
            entity.Fk = 90;

            Assert.Equal(76, sidecar[IdProperty]);
            Assert.Equal(89, sidecar[FkProperty]);

            Assert.Equal(76, entity.Id);
            Assert.Equal(90, entity.Fk);

            sidecar.TakeSnapshot(FkProperty);

            Assert.Equal(76, sidecar[IdProperty]);
            Assert.Equal(90, sidecar[FkProperty]);
        }

        [Fact]
        public void Can_snapshot_null_values()
        {
            var entity = new Banana { Id = 77, Name = "Stand", State = null };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar.TakeSnapshot();

            Assert.Null(sidecar[FkProperty]);
            Assert.Null(entity.State);

            sidecar[FkProperty] = 89;

            Assert.Equal(89, sidecar[FkProperty]);
            Assert.Null(entity.State);

            entity.Fk = 90;

            Assert.Equal(89, sidecar[FkProperty]);
            Assert.Equal(90, entity.Fk);

            sidecar.TakeSnapshot();

            Assert.Equal(90, sidecar[FkProperty]);
        }

        [Fact]
        public void Can_update_already_saved_values()
        {
            var entity = new Banana { Id = 77, Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar[IdProperty] = 78;
            entity.Id = 76;
            entity.Fk = 90;

            Assert.True(sidecar.HasValue(IdProperty));
            Assert.False(sidecar.HasValue(FkProperty));

            Assert.Equal(78, sidecar[IdProperty]);
            Assert.Equal(90, sidecar[FkProperty]);

            sidecar.UpdateSnapshot();

            Assert.True(sidecar.HasValue(IdProperty));
            Assert.False(sidecar.HasValue(FkProperty));

            Assert.Equal(76, sidecar[IdProperty]);
            Assert.Equal(90, sidecar[FkProperty]);
        }

        [Fact]
        public void Can_ensure_value_is_snapshotted_but_not_overwrite_existing_snapshot_value()
        {
            var entity = new Banana { Id = 77, Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar.EnsureSnapshot(IdProperty);

            Assert.True(sidecar.HasValue(IdProperty));
            Assert.Equal(77, sidecar[IdProperty]);

            entity.Id = 76;

            sidecar.EnsureSnapshot(IdProperty);

            Assert.True(sidecar.HasValue(IdProperty));
            Assert.Equal(77, sidecar[IdProperty]);
        }

        [Fact]
        public void Can_ensure_null_value_is_snapshotted_but_not_overwrite_existing_snapshot_value()
        {
            var entity = new Banana { Id = 77, Name = "Stand", State = null };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar.EnsureSnapshot(FkProperty);

            Assert.True(sidecar.HasValue(FkProperty));
            Assert.Null(sidecar[FkProperty]);

            entity.Fk = 90;

            sidecar.EnsureSnapshot(FkProperty);

            Assert.True(sidecar.HasValue(FkProperty));
            Assert.Null(sidecar[FkProperty]);
        }

        [Fact]
        public void Ensuring_snapshot_does_nothing_for_property_that_cannot_be_stored()
        {
            var entity = new Banana { Id = 77, Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar.EnsureSnapshot(NameProperty);

            Assert.False(sidecar.HasValue(NameProperty));
        }

        [Fact]
        public void Can_create_foreign_key_value_based_on_dependent_values()
        {
            var entityType = _model.GetEntityType("Banana");
            var foreignKey = entityType.ForeignKeys.Single();

            var entry = CreateStateEntry();
            var sidecar = CreateSidecar(entry);
            sidecar[foreignKey.Properties.Single()] = 42;

            var keyValue = sidecar.GetDependentKeyValue(foreignKey);
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(42, keyValue.Value);
        }

        [Fact]
        public void Can_create_foreign_key_value_based_on_principal_end_values()
        {
            var entityType = _model.GetEntityType("Banana");
            var foreignKey = entityType.ForeignKeys.Single();

            var entry = CreateStateEntry();
            var sidecar = CreateSidecar(entry);
            sidecar[foreignKey.ReferencedProperties.Single()] = 42;

            var keyValue = sidecar.GetPrincipalKeyValue(foreignKey);
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(42, keyValue.Value);
        }

        [Fact]
        public void Can_create_primary_key()
        {
            var entry = CreateStateEntry();
            var sidecar = CreateSidecar(entry);
            sidecar[IdProperty] = 42;

            var keyValue = sidecar.GetPrimaryKeyValue();
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(42, keyValue.Value);
        }

        [Fact]
        public void Can_create_composite_foreign_key_value_based_on_dependent_values()
        {
            var entityType = _model.GetEntityType("SomeMoreDependentEntity");
            var foreignKey = entityType.ForeignKeys.Single();

            var entry = CreateStateEntry(new SomeMoreDependentEntity());
            var sidecar = CreateSidecar(entry);
            sidecar[foreignKey.Properties[0]] = 77;
            sidecar[foreignKey.Properties[1]] = "CheeseAndOnion";

            var keyValue = (CompositeEntityKey)sidecar.GetDependentKeyValue(foreignKey);
            Assert.Equal(77, keyValue.Value[0]);
            Assert.Equal("CheeseAndOnion", keyValue.Value[1]);
        }

        [Fact]
        public void Can_create_composite_foreign_key_value_based_on_principal_end_values()
        {
            var dependentType = _model.GetEntityType("SomeMoreDependentEntity");
            var foreignKey = dependentType.ForeignKeys.Single();

            var entry = CreateStateEntry(new SomeDependentEntity());
            var sidecar = CreateSidecar(entry);
            sidecar[foreignKey.ReferencedProperties[0]] = 77;
            sidecar[foreignKey.ReferencedProperties[1]] = "PrawnCocktail";

            var keyValue = (CompositeEntityKey)sidecar.GetPrincipalKeyValue(foreignKey);
            Assert.Equal(77, keyValue.Value[0]);
            Assert.Equal("PrawnCocktail", keyValue.Value[1]);
        }

        protected abstract Sidecar CreateSidecar(StateEntry entry = null);

        protected StateEntry CreateStateEntry(Banana entity = null)
        {
            entity = entity ?? new Banana { Id = 77, Name = "Stand", Fk = 88 };

            return CreateStateEntry<Banana>(entity);
        }

        protected StateEntry CreateStateEntry<TEntity>(TEntity entity)
        {
            var configuration = TestHelpers.CreateContextConfiguration(_model);

            return configuration.Services.StateEntryFactory.Create(_model.GetEntityType(typeof(TEntity)), entity);
        }

        private static Model BuildModel()
        {
            var model = new Model();

            var entityType = new EntityType(typeof(Banana));
            
            var idProperty = entityType.AddProperty("Id", typeof(int), shadowProperty: false, concurrencyToken: true);
            idProperty.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;
            entityType.SetKey(idProperty);

            entityType.AddProperty("Name", typeof(string));
            entityType.AddProperty("State", typeof(string), shadowProperty: false, concurrencyToken: true);
            
            var fkProperty = entityType.AddProperty("Fk", typeof(int?), shadowProperty: false, concurrencyToken: true);
            entityType.AddForeignKey(new Key(new[] { idProperty }), fkProperty);

            model.AddEntityType(entityType);

            var entityType2 = new EntityType(typeof(SomeDependentEntity));
            model.AddEntityType(entityType2);
            var key2A = entityType2.AddProperty("Id1", typeof(int));
            var key2B = entityType2.AddProperty("Id2", typeof(string));
            entityType2.SetKey(key2A, key2B);
            var fk = entityType2.AddProperty("SomeEntityId", typeof(int));
            entityType2.AddForeignKey(entityType.GetKey(), new[] { fk });
            var justAProperty = entityType2.AddProperty("JustAProperty", typeof(int));
            justAProperty.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;
            justAProperty.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;

            var entityType5 = new EntityType(typeof(SomeMoreDependentEntity));
            model.AddEntityType(entityType5);
            var key5 = entityType5.AddProperty("Id", typeof(int));
            entityType5.SetKey(key5);
            var fk5A = entityType5.AddProperty("Fk1", typeof(int));
            var fk5B = entityType5.AddProperty("Fk2", typeof(string));
            entityType5.AddForeignKey(entityType2.GetKey(), new[] { fk5A, fk5B });

            return model;
        }

        protected IProperty IdProperty
        {
            get { return _model.GetEntityType(typeof(Banana)).GetProperty("Id"); }
        }

        protected IProperty NameProperty
        {
            get { return _model.GetEntityType(typeof(Banana)).GetProperty("Name"); }
        }

        protected IProperty FkProperty
        {
            get { return _model.GetEntityType(typeof(Banana)).GetProperty("Fk"); }
        }

        protected class Banana : INotifyPropertyChanged, INotifyPropertyChanging
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string State { get; set; }
            public int? Fk { get; set; }

#pragma warning disable 67
            public event PropertyChangedEventHandler PropertyChanged;
            public event PropertyChangingEventHandler PropertyChanging;
#pragma warning restore 67
        }

        protected class SomeDependentEntity
        {
            public int Id1 { get; set; }
            public string Id2 { get; set; }
            public int SomeEntityId { get; set; }
            public int JustAProperty { get; set; }
        }

        protected class SomeMoreDependentEntity
        {
            public int Id { get; set; }
            public int Fk1 { get; set; }
            public string Fk2 { get; set; }
        }
    }
}
