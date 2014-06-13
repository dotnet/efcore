// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public abstract class SidecarTest
    {
        private readonly Model _model;

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
            Assert.True(sidecar.CanStoreValue(StateProperty));
        }

        [Fact]
        public void Can_check_if_value_is_stored()
        {
            var sidecar = CreateSidecar();

            Assert.False(sidecar.HasValue(IdProperty));
            Assert.False(sidecar.HasValue(NameProperty));
            Assert.False(sidecar.HasValue(StateProperty));

            sidecar[IdProperty] = 78;
            sidecar[StateProperty] = "Thawed";

            Assert.True(sidecar.HasValue(IdProperty));
            Assert.False(sidecar.HasValue(NameProperty));
            Assert.True(sidecar.HasValue(StateProperty));
        }

        [Fact]
        public void Can_read_and_write_values()
        {
            var entity = new Banana { Id = 77, Name = "Stand", State = "Frozen" };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            Assert.Equal(77, sidecar[IdProperty]);
            Assert.Equal("Frozen", sidecar[StateProperty]);

            sidecar[IdProperty] = 78;
            sidecar[StateProperty] = "Thawed";

            Assert.Equal(78, sidecar[IdProperty]);
            Assert.Equal("Thawed", sidecar[StateProperty]);

            Assert.Equal(77, entity.Id);
            Assert.Equal("Frozen", entity.State);
        }

        [Fact]
        public void Can_read_and_write_null()
        {
            var entity = new Banana { Id = 77, Name = "Stand", State = "Frozen" };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar[StateProperty] = null;

            Assert.True(sidecar.HasValue(StateProperty));
            Assert.Null(sidecar[StateProperty]);

            Assert.Equal("Frozen", entity.State);
        }

        [Fact]
        public void Can_commit_values_into_state_entry()
        {
            var entity = new Banana { Id = 77, Name = "Stand", State = "Frozen" };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar[StateProperty] = "Thawed";
            sidecar.Commit();

            Assert.Equal(77, entity.Id);
            Assert.Equal("Thawed", entity.State);
        }

        [Fact]
        public void Committing_detaches_sidecar()
        {
            var entity = new Banana { Id = 77, Name = "Stand", State = "Frozen" };
            var stateEntry = CreateStateEntry(entity);

            var sidecar = stateEntry.AddSidecar(CreateSidecar(stateEntry));

            sidecar.Commit();

            Assert.Null(stateEntry.TryGetSidecar(sidecar.Name));
        }

        [Fact]
        public void Rolling_back_detaches_sidecar_without_committing_values()
        {
            var entity = new Banana { Id = 77, Name = "Stand", State = "Frozen" };
            var stateEntry = CreateStateEntry(entity);

            var sidecar = stateEntry.AddSidecar(CreateSidecar(stateEntry));

            sidecar[StateProperty] = "Thawed";
            sidecar.Rollback();

            Assert.Null(stateEntry.TryGetSidecar(sidecar.Name));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Frozen", entity.State);
        }

        [Fact]
        public void Can_snapshot_values()
        {
            var entity = new Banana { Id = 77, Name = "Stand", State = "Frozen" };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar.TakeSnapshot();

            Assert.Equal(77, sidecar[IdProperty]);
            Assert.Equal("Frozen", sidecar[StateProperty]);

            sidecar[IdProperty] = 78;
            sidecar[StateProperty] = "Thawed";

            Assert.Equal(78, sidecar[IdProperty]);
            Assert.Equal("Thawed", sidecar[StateProperty]);

            Assert.Equal(77, entity.Id);
            Assert.Equal("Frozen", entity.State);

            entity.Id = 76;
            entity.State = "Banana vapor";

            Assert.Equal(78, sidecar[IdProperty]);
            Assert.Equal("Thawed", sidecar[StateProperty]);

            Assert.Equal(76, entity.Id);
            Assert.Equal("Banana vapor", entity.State);

            sidecar.TakeSnapshot();

            Assert.Equal(76, sidecar[IdProperty]);
            Assert.Equal("Banana vapor", sidecar[StateProperty]);
        }

        [Fact]
        public void Can_snapshot_individual_values()
        {
            var entity = new Banana { Id = 77, Name = "Stand", State = "Frozen" };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar.TakeSnapshot(StateProperty);

            Assert.Equal(77, sidecar[IdProperty]);
            Assert.Equal("Frozen", sidecar[StateProperty]);

            sidecar[StateProperty] = "Thawed";

            Assert.Equal(77, sidecar[IdProperty]);
            Assert.Equal("Thawed", sidecar[StateProperty]);

            Assert.Equal(77, entity.Id);
            Assert.Equal("Frozen", entity.State);

            entity.Id = 76;
            entity.State = "Banana vapor";

            Assert.Equal(76, sidecar[IdProperty]);
            Assert.Equal("Thawed", sidecar[StateProperty]);

            Assert.Equal(76, entity.Id);
            Assert.Equal("Banana vapor", entity.State);

            sidecar.TakeSnapshot(StateProperty);

            Assert.Equal(76, sidecar[IdProperty]);
            Assert.Equal("Banana vapor", sidecar[StateProperty]);
        }

        [Fact]
        public void Can_snapshot_null_values()
        {
            var entity = new Banana { Id = 77, Name = "Stand", State = null };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar.TakeSnapshot();

            Assert.Null(sidecar[StateProperty]);
            Assert.Null(entity.State);

            sidecar[StateProperty] = "Thawed";

            Assert.Equal("Thawed", sidecar[StateProperty]);
            Assert.Null(entity.State);

            entity.State = "Banana vapor";

            Assert.Equal("Thawed", sidecar[StateProperty]);
            Assert.Equal("Banana vapor", entity.State);

            sidecar.TakeSnapshot();

            Assert.Equal("Banana vapor", sidecar[StateProperty]);
        }

        [Fact]
        public void Can_update_already_saved_values()
        {
            var entity = new Banana { Id = 77, Name = "Stand", State = "Frozen" };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar[IdProperty] = 78;
            entity.Id = 76;
            entity.State = "Banana vapor";

            Assert.True(sidecar.HasValue(IdProperty));
            Assert.False(sidecar.HasValue(StateProperty));

            Assert.Equal(78, sidecar[IdProperty]);
            Assert.Equal("Banana vapor", sidecar[StateProperty]);

            sidecar.UpdateSnapshot();

            Assert.True(sidecar.HasValue(IdProperty));
            Assert.False(sidecar.HasValue(StateProperty));

            Assert.Equal(76, sidecar[IdProperty]);
            Assert.Equal("Banana vapor", sidecar[StateProperty]);
        }

        [Fact]
        public void Can_ensure_value_is_snapshotted_but_not_overwrite_existing_snapshot_value()
        {
            var entity = new Banana { Id = 77, Name = "Stand", State = "Frozen" };
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

            sidecar.EnsureSnapshot(StateProperty);

            Assert.True(sidecar.HasValue(StateProperty));
            Assert.Null(sidecar[StateProperty]);

            entity.State = "Banana vapor";

            sidecar.EnsureSnapshot(StateProperty);

            Assert.True(sidecar.HasValue(StateProperty));
            Assert.Null(sidecar[StateProperty]);
        }

        [Fact]
        public void Ensuring_snapshot_does_nothing_for_property_that_cannot_be_stored()
        {
            var entity = new Banana { Id = 77, Name = "Stand", State = "Frozen" };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar.EnsureSnapshot(NameProperty);

            Assert.False(sidecar.HasValue(NameProperty));
        }

        protected abstract Sidecar CreateSidecar(StateEntry entry = null);

        protected StateEntry CreateStateEntry(Banana entity = null)
        {
            entity = entity ?? new Banana { Id = 77, Name = "Stand", State = "Frozen" };

            var configuration = TestHelpers.CreateContextConfiguration(BuildModel());

            return configuration.Services.StateEntryFactory.Create(_model.GetEntityType(typeof(Banana)), entity);
        }

        private static Model BuildModel()
        {
            var model = new Model();

            var entityType = new EntityType(typeof(Banana));
            entityType.AddProperty("Id", typeof(int), shadowProperty: false, concurrencyToken: true);
            entityType.AddProperty("Name", typeof(string));
            entityType.AddProperty("State", typeof(string), shadowProperty: false, concurrencyToken: true);
            
            model.AddEntityType(entityType);

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

        protected IProperty StateProperty
        {
            get { return _model.GetEntityType(typeof(Banana)).GetProperty("State"); }
        }

        protected class Banana : INotifyPropertyChanged, INotifyPropertyChanging
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string State { get; set; }
            public int Fk { get; set; }

#pragma warning disable 67
            public event PropertyChangedEventHandler PropertyChanged;
            public event PropertyChangingEventHandler PropertyChanging;
#pragma warning restore 67
        }
    }
}
