// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public abstract class SidecarTest
    {
        protected readonly Model _model;

        protected SidecarTest()
        {
            _model = BuildModel();
        }

        [Fact]
        public virtual void Can_check_if_property_value_can_be_stored()
        {
            var sidecar = CreateSidecar();

            Assert.True(sidecar.CanStoreValue(IdProperty));
            Assert.False(sidecar.CanStoreValue(NameProperty));
            Assert.True(sidecar.CanStoreValue(FkProperty));
        }

        [Fact]
        public virtual void Can_check_if_value_is_stored()
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
        public virtual void Can_read_and_write_values()
        {
            var entity = new Banana { Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            Assert.Equal(-1, sidecar[IdProperty]);
            Assert.Equal(88, sidecar[FkProperty]);

            sidecar[IdProperty] = 78;
            sidecar[FkProperty] = 89;

            Assert.Equal(78, sidecar[IdProperty]);
            Assert.Equal(89, sidecar[FkProperty]);

            Assert.Equal(-1, entity.Id);
            Assert.Equal(88, entity.Fk);
        }

        [Fact]
        public virtual void Can_read_and_write_null()
        {
            var entity = new Banana { Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            sidecar[FkProperty] = null;

            Assert.True(sidecar.HasValue(FkProperty));
            Assert.Null(sidecar[FkProperty]);

            Assert.Equal(88, entity.Fk);
        }

        [Fact]
        public virtual void Can_commit_values_into_state_entry()
        {
            var entity = new Banana { Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            sidecar[FkProperty] = 89;
            sidecar.Commit();

            Assert.Equal(-1, entity.Id);
            Assert.Equal(89, entity.Fk);
        }

        [Fact]
        public virtual void Committing_detaches_sidecar()
        {
            var entity = new Banana { Name = "Stand", Fk = 88 };
            var entry = CreateInternalEntry(entity);

            var sidecar = entry.AddSidecar(CreateSidecar(entry));

            sidecar.Commit();

            Assert.Null(entry.TryGetSidecar(sidecar.Name));
        }

        [Fact]
        public virtual void Rolling_back_detaches_sidecar_without_committing_values()
        {
            var entity = new Banana { Name = "Stand", Fk = 88 };
            var entry = CreateInternalEntry(entity);

            var sidecar = entry.AddSidecar(CreateSidecar(entry));

            sidecar[FkProperty] = 89;
            sidecar.Rollback();

            Assert.Null(entry.TryGetSidecar(sidecar.Name));

            Assert.Equal(-1, entity.Id);
            Assert.Equal(88, entity.Fk);
        }

        [Fact]
        public virtual void Can_snapshot_values()
        {
            var entity = new Banana { Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            sidecar.TakeSnapshot();

            Assert.Equal(-1, sidecar[IdProperty]);
            Assert.Equal(88, sidecar[FkProperty]);

            sidecar[IdProperty] = 78;
            sidecar[FkProperty] = 89;

            Assert.Equal(78, sidecar[IdProperty]);
            Assert.Equal(89, sidecar[FkProperty]);

            Assert.Equal(-1, entity.Id);
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
        public virtual void Can_snapshot_individual_values()
        {
            var entity = new Banana { Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            sidecar.TakeSnapshot(FkProperty);

            Assert.Equal(-1, sidecar[IdProperty]);
            Assert.Equal(88, sidecar[FkProperty]);

            sidecar[FkProperty] = 89;

            Assert.Equal(-1, sidecar[IdProperty]);
            Assert.Equal(89, sidecar[FkProperty]);

            Assert.Equal(-1, entity.Id);
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
        public virtual void Can_snapshot_null_values()
        {
            var entity = new Banana { Name = "Stand", State = null };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

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
        public virtual void Can_update_already_saved_values()
        {
            var entity = new Banana { Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

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
        public virtual void Can_ensure_value_is_snapshotted_but_not_overwrite_existing_snapshot_value()
        {
            var entity = new Banana { Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            sidecar.EnsureSnapshot(IdProperty);

            Assert.True(sidecar.HasValue(IdProperty));
            Assert.Equal(-1, sidecar[IdProperty]);

            entity.Id = 76;

            sidecar.EnsureSnapshot(IdProperty);

            Assert.True(sidecar.HasValue(IdProperty));
            Assert.Equal(-1, sidecar[IdProperty]);
        }

        [Fact]
        public virtual void Can_ensure_null_value_is_snapshotted_but_not_overwrite_existing_snapshot_value()
        {
            var entity = new Banana { Name = "Stand", State = null };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            sidecar.EnsureSnapshot(FkProperty);

            Assert.True(sidecar.HasValue(FkProperty));
            Assert.Null(sidecar[FkProperty]);

            entity.Fk = 90;

            sidecar.EnsureSnapshot(FkProperty);

            Assert.True(sidecar.HasValue(FkProperty));
            Assert.Null(sidecar[FkProperty]);
        }

        [Fact]
        public virtual void Ensuring_snapshot_does_nothing_for_property_that_cannot_be_stored()
        {
            var entity = new Banana { Name = "Stand", Fk = 88 };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            sidecar.EnsureSnapshot(NameProperty);

            Assert.False(sidecar.HasValue(NameProperty));
        }

        [Fact]
        public virtual void Can_create_foreign_key_value_based_on_dependent_values()
        {
            var entityType = _model.GetEntityType(typeof(Banana).FullName);
            var foreignKey = entityType.GetForeignKeys().Single();

            var entry = CreateInternalEntry();
            var sidecar = CreateSidecar(entry);
            sidecar[foreignKey.Properties.Single()] = 42;

            var keyValue = sidecar.GetDependentKeyValue(foreignKey);
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(42, keyValue.Value);
        }

        [Fact]
        public virtual void Can_create_foreign_key_value_based_on_principal_end_values()
        {
            var entityType = _model.GetEntityType(typeof(Banana).FullName);
            var foreignKey = entityType.GetForeignKeys().Single();

            var entry = CreateInternalEntry();
            var sidecar = CreateSidecar(entry);
            sidecar[foreignKey.PrincipalKey.Properties.Single()] = 42;

            var keyValue = sidecar.GetPrincipalKeyValue(foreignKey);
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(42, keyValue.Value);
        }

        [Fact]
        public virtual void Can_create_primary_key()
        {
            var entry = CreateInternalEntry();
            var sidecar = CreateSidecar(entry);
            sidecar[IdProperty] = 42;

            var keyValue = sidecar.GetPrimaryKeyValue();
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(42, keyValue.Value);
        }

        [Fact]
        public virtual void Can_create_composite_foreign_key_value_based_on_dependent_values()
        {
            var entityType = _model.GetEntityType(typeof(SomeMoreDependentEntity).FullName);
            var foreignKey = entityType.GetForeignKeys().Single();

            var entry = TestHelpers.Instance.CreateInternalEntry<SomeMoreDependentEntity>(_model);
            var sidecar = CreateSidecar(entry);
            sidecar[foreignKey.Properties[0]] = 77;
            sidecar[foreignKey.Properties[1]] = "CheeseAndOnion";

            var keyValue = (CompositeEntityKey)sidecar.GetDependentKeyValue(foreignKey);
            Assert.Equal(77, keyValue.Value[0]);
            Assert.Equal("CheeseAndOnion", keyValue.Value[1]);
        }

        [Fact]
        public virtual void Can_create_composite_foreign_key_value_based_on_principal_end_values()
        {
            var dependentType = _model.GetEntityType(typeof(SomeMoreDependentEntity).FullName);
            var foreignKey = dependentType.GetForeignKeys().Single();

            var entry = TestHelpers.Instance.CreateInternalEntry<SomeDependentEntity>(_model);
            var sidecar = CreateSidecar(entry);
            sidecar[foreignKey.PrincipalKey.Properties[0]] = 77;
            sidecar[foreignKey.PrincipalKey.Properties[1]] = "PrawnCocktail";

            var keyValue = (CompositeEntityKey)sidecar.GetPrincipalKeyValue(foreignKey);
            Assert.Equal(77, keyValue.Value[0]);
            Assert.Equal("PrawnCocktail", keyValue.Value[1]);
        }

        protected abstract Sidecar CreateSidecar(InternalEntityEntry entry = null);

        protected InternalEntityEntry CreateInternalEntry(Banana entity = null)
        {
            entity = entity ?? new Banana { Name = "Stand", Fk = 88 };

            var customServices = new ServiceCollection()
                .AddScoped<IValueGeneratorSelector, TestInMemoryValueGeneratorSelector>();

            var entry = TestHelpers.Instance.CreateContextServices(customServices, _model)
                .GetRequiredService<IStateManager>()
                .GetOrCreateEntry(entity);

            entry.SetEntityState(EntityState.Added);

            return entry;
        }

        private static Model BuildModel()
        {
            var model = new Model();

            var entityType = model.AddEntityType(typeof(Banana));

            var idProperty = entityType.GetOrAddProperty("Id", typeof(int));
            idProperty.IsConcurrencyToken = true;
            idProperty.IsValueGeneratedOnAdd = true;
            entityType.GetOrSetPrimaryKey(idProperty);

            entityType.GetOrAddProperty("Name", typeof(string));
            entityType.GetOrAddProperty("State", typeof(string)).IsConcurrencyToken = true;

            var fkProperty = entityType.GetOrAddProperty("Fk", typeof(int?));
            fkProperty.IsConcurrencyToken = true;
            entityType.GetOrAddForeignKey(fkProperty, new Key(new[] { idProperty }));

            var entityType2 = model.AddEntityType(typeof(SomeDependentEntity));
            var key2A = entityType2.GetOrAddProperty("Id1", typeof(int));
            var key2B = entityType2.GetOrAddProperty("Id2", typeof(string));
            entityType2.GetOrSetPrimaryKey(new[] { key2A, key2B });
            var fk = entityType2.GetOrAddProperty("SomeEntityId", typeof(int));
            entityType2.GetOrAddForeignKey(new[] { fk }, entityType.GetPrimaryKey());
            var justAProperty = entityType2.GetOrAddProperty("JustAProperty", typeof(int));
            justAProperty.IsValueGeneratedOnAdd = true;

            var entityType5 = model.AddEntityType(typeof(SomeMoreDependentEntity));
            var key5 = entityType5.GetOrAddProperty("Id", typeof(int));
            entityType5.GetOrSetPrimaryKey(key5);
            var fk5A = entityType5.GetOrAddProperty("Fk1", typeof(int));
            var fk5B = entityType5.GetOrAddProperty("Fk2", typeof(string));
            entityType5.GetOrAddForeignKey(new[] { fk5A, fk5B }, entityType2.GetPrimaryKey());

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

        public class TestInMemoryValueGeneratorSelector : InMemoryValueGeneratorSelector
        {
            private readonly TemporaryNumberValueGeneratorFactory _inMemoryFactory = new TemporaryNumberValueGeneratorFactory();

            public TestInMemoryValueGeneratorSelector(IValueGeneratorCache cache)
                : base(cache)
            {
            }

            public override ValueGenerator Create(IProperty property, IEntityType entityType)
            {
                return property.ClrType == typeof(int)
                    ? _inMemoryFactory.Create(property)
                    : base.Create(property, entityType);
            }
        }
    }
}
