// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class ForeignKeysSnapshotTest
    {
        private readonly Model _model;

        public ForeignKeysSnapshotTest()
        {
            _model = BuildModel();
        }

        [Fact]
        public void Is_not_set_up_for_transparent_access_and_auto_commit()
        {
            var sidecar = CreateSidecar();

            Assert.False(sidecar.TransparentRead);
            Assert.False(sidecar.TransparentWrite);
            Assert.False(sidecar.AutoCommit);
        }

        [Fact]
        public void Has_expected_name()
        {
            Assert.Equal(Sidecar.WellKnownNames.ForeignKeysSnapshot, CreateSidecar().Name);
        }

        private Sidecar CreateSidecar(StateEntry entry = null)
        {
            return new ForeignKeysSnapshotFactory().Create(entry ?? CreateStateEntry());
        }

        [Fact]
        public void Can_check_if_property_value_can_be_stored()
        {
            var sidecar = CreateSidecar();

            Assert.False(sidecar.CanStoreValue(IdProperty));
            Assert.True(sidecar.CanStoreValue(FkProperty));
        }

        [Fact]
        public void Can_check_if_value_is_stored()
        {
            var sidecar = CreateSidecar();

            Assert.False(sidecar.HasValue(IdProperty));
            Assert.False(sidecar.HasValue(FkProperty));

            sidecar[FkProperty] = 78;

            Assert.False(sidecar.HasValue(IdProperty));
            Assert.True(sidecar.HasValue(FkProperty));
        }

        [Fact]
        public void Can_read_and_write_values()
        {
            var entity = new Banana { Id = 77, Fk = 78 };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            Assert.Equal(78, sidecar[FkProperty]);

            sidecar[FkProperty] = 79;

            Assert.Equal(79, sidecar[FkProperty]);

            Assert.Equal(77, entity.Id);
            Assert.Equal(78, entity.Fk);
        }

        [Fact]
        public void Can_read_and_write_null()
        {
            var entity = new Banana { Id = 77, Fk = 78 };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar[FkProperty] = null;

            Assert.True(sidecar.HasValue(FkProperty));
            Assert.Null(sidecar[FkProperty]);

            Assert.Equal(78, entity.Fk);
        }

        [Fact]
        public void Can_snapshot_values()
        {
            var entity = new Banana { Id = 77, Fk = 78 };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar.TakeSnapshot();

            Assert.Equal(78, sidecar[FkProperty]);

            sidecar[FkProperty] = 79;

            Assert.Equal(79, sidecar[FkProperty]);

            Assert.Equal(77, entity.Id);
            Assert.Equal(78, entity.Fk);

            entity.Fk = 80;

            Assert.Equal(79, sidecar[FkProperty]);

            Assert.Equal(77, entity.Id);
            Assert.Equal(80, entity.Fk);

            sidecar.TakeSnapshot();

            Assert.Equal(80, sidecar[FkProperty]);
        }

        [Fact]
        public void Can_snapshot_individual_values()
        {
            var entity = new Banana { Id = 77, Fk = 78 };
            var sidecar = CreateSidecar(CreateStateEntry(entity));

            sidecar.TakeSnapshot(FkProperty);

            Assert.Equal(78, sidecar[FkProperty]);

            sidecar[FkProperty] = 79;

            Assert.Equal(79, sidecar[FkProperty]);

            Assert.Equal(77, entity.Id);
            Assert.Equal(78, entity.Fk);

            entity.Fk = 80;

            Assert.Equal(79, sidecar[FkProperty]);

            Assert.Equal(77, entity.Id);
            Assert.Equal(80, entity.Fk);

            sidecar.TakeSnapshot(FkProperty);

            Assert.Equal(80, sidecar[FkProperty]);
        }

        protected StateEntry CreateStateEntry(Banana entity = null)
        {
            entity = entity ?? new Banana { Id = 77 };

            var configuration = TestHelpers.CreateContextConfiguration(BuildModel());

            return configuration.Services.StateEntryFactory.Create(_model.GetEntityType(typeof(Banana)), entity);
        }

        private static Model BuildModel()
        {
            var model = new Model();

            var entityType = new EntityType(typeof(Banana));
            var pkProperty = entityType.AddProperty("Id", typeof(int));
            var fkProperty = entityType.AddProperty("Fk", typeof(int));

            entityType.SetKey(pkProperty);
            entityType.AddForeignKey(entityType.GetKey(), fkProperty);

            model.AddEntityType(entityType);

            return model;
        }

        protected IProperty IdProperty
        {
            get { return _model.GetEntityType(typeof(Banana)).GetProperty("Id"); }
        }

        protected IProperty FkProperty
        {
            get { return _model.GetEntityType(typeof(Banana)).GetProperty("Fk"); }
        }

        protected class Banana : INotifyPropertyChanged, INotifyPropertyChanging
        {
            public int Id { get; set; }
            public int Fk { get; set; }

#pragma warning disable 67
            public event PropertyChangedEventHandler PropertyChanged;
            public event PropertyChangingEventHandler PropertyChanging;
#pragma warning restore 67
        }
    }
}
