// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class RelationshipsSnapshotTest
    {
        private readonly Model _model;

        public RelationshipsSnapshotTest()
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
            Assert.Equal(Sidecar.WellKnownNames.RelationshipsSnapshot, CreateSidecar().Name);
        }

        private Sidecar CreateSidecar(InternalEntityEntry entry = null)
        {
            return new RelationshipsSnapshotFactory().Create(entry ?? CreateInternalEntry());
        }

        [Fact]
        public void Can_store_properties_for_PKs_FKs_and_navigations()
        {
            var sidecar = CreateSidecar();

            Assert.False(sidecar.CanStoreValue(NameProperty));
            Assert.True(sidecar.CanStoreValue(IdProperty));
            Assert.True(sidecar.CanStoreValue(FkProperty));
            Assert.True(sidecar.CanStoreValue(CollectionNavigation));
            Assert.True(sidecar.CanStoreValue(ReferenceNavigation));
        }

        [Fact]
        public void Can_check_if_value_is_stored()
        {
            var sidecar = CreateSidecar();

            Assert.False(sidecar.HasValue(NameProperty));
            Assert.False(sidecar.HasValue(IdProperty));
            Assert.False(sidecar.HasValue(FkProperty));
            Assert.False(sidecar.HasValue(CollectionNavigation));
            Assert.False(sidecar.HasValue(ReferenceNavigation));

            sidecar[IdProperty] = 11;
            sidecar[FkProperty] = 78;
            sidecar[CollectionNavigation] = new List<Banana>();
            sidecar[ReferenceNavigation] = new Banana();

            Assert.False(sidecar.HasValue(NameProperty));
            Assert.True(sidecar.HasValue(IdProperty));
            Assert.True(sidecar.HasValue(FkProperty));
            Assert.True(sidecar.HasValue(CollectionNavigation));
            Assert.True(sidecar.HasValue(ReferenceNavigation));
        }

        [Fact]
        public void Can_read_and_write_values()
        {
            var originalBananas = new List<Banana>();
            var originalBanana = new Banana();
            var entity = new Banana { Id = 77, Fk = 78, TopBanana = originalBanana, LesserBananas = originalBananas };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            Assert.Equal(78, sidecar[FkProperty]);
            Assert.Same(originalBananas, sidecar[CollectionNavigation]);
            Assert.Same(originalBanana, sidecar[ReferenceNavigation]);

            var newBananas = new List<Banana>();
            var newBanana = new Banana();
            sidecar[FkProperty] = 79;
            sidecar[CollectionNavigation] = newBananas;
            sidecar[ReferenceNavigation] = newBanana;

            Assert.Equal(79, sidecar[FkProperty]);
            Assert.Same(newBananas, sidecar[CollectionNavigation]);
            Assert.Same(newBanana, sidecar[ReferenceNavigation]);

            Assert.Equal(77, entity.Id);
            Assert.Equal(78, entity.Fk);
            Assert.Same(originalBananas, entity.LesserBananas);
            Assert.Same(originalBanana, entity.TopBanana);
        }

        [Fact]
        public void Can_read_and_write_null()
        {
            var originalBananas = new List<Banana>();
            var originalBanana = new Banana();
            var entity = new Banana { Id = 77, Fk = 78, TopBanana = originalBanana, LesserBananas = originalBananas };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            sidecar[FkProperty] = null;
            sidecar[CollectionNavigation] = null;
            sidecar[ReferenceNavigation] = null;

            Assert.True(sidecar.HasValue(FkProperty));
            Assert.True(sidecar.HasValue(CollectionNavigation));
            Assert.True(sidecar.HasValue(ReferenceNavigation));

            Assert.Null(sidecar[FkProperty]);
            Assert.Null(sidecar[CollectionNavigation]);
            Assert.Null(sidecar[ReferenceNavigation]);

            Assert.Equal(78, entity.Fk);
            Assert.Same(originalBananas, entity.LesserBananas);
            Assert.Same(originalBanana, entity.TopBanana);
        }

        [Fact]
        public void Can_snapshot_scalar_values()
        {
            var originalBanana = new Banana();
            var entity = new Banana { Id = 77, Fk = 78, TopBanana = originalBanana };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            sidecar.TakeSnapshot();

            Assert.Equal(78, sidecar[FkProperty]);
            Assert.Same(originalBanana, sidecar[ReferenceNavigation]);

            var newBanana = new Banana();
            sidecar[FkProperty] = 79;
            sidecar[ReferenceNavigation] = newBanana;

            Assert.Equal(79, sidecar[FkProperty]);
            Assert.Same(newBanana, sidecar[ReferenceNavigation]);

            Assert.Equal(77, entity.Id);
            Assert.Equal(78, entity.Fk);
            Assert.Same(originalBanana, entity.TopBanana);

            var supplimentalBanana = new Banana();
            entity.Fk = 80;
            entity.TopBanana = supplimentalBanana;

            Assert.Equal(79, sidecar[FkProperty]);
            Assert.Same(newBanana, sidecar[ReferenceNavigation]);

            Assert.Equal(77, entity.Id);
            Assert.Equal(80, entity.Fk);
            Assert.Same(supplimentalBanana, entity.TopBanana);

            sidecar.TakeSnapshot();

            Assert.Equal(80, sidecar[FkProperty]);
            Assert.Same(supplimentalBanana, sidecar[ReferenceNavigation]);
        }

        [Fact]
        public void Can_snapshot_collection_values()
        {
            var originalBanana = new Banana();
            var originalBananas = new List<Banana> { originalBanana };
            var entity = new Banana { Id = 77, Fk = 78, LesserBananas = originalBananas };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            sidecar.TakeSnapshot();

            Assert.NotSame(originalBanana, sidecar[CollectionNavigation]);
            Assert.Equal(new object[] { originalBanana }, ((IEnumerable)sidecar[CollectionNavigation]).OfType<object>().ToArray());

            var newBanana = new Banana();
            var newBananas = new List<Banana> { newBanana };
            sidecar[CollectionNavigation] = newBananas;

            Assert.Same(newBananas, sidecar[CollectionNavigation]);
            Assert.Same(originalBananas, entity.LesserBananas);

            var supplimentalBanana = new Banana();
            entity.LesserBananas.Add(supplimentalBanana);

            Assert.Same(newBananas, sidecar[CollectionNavigation]);
            Assert.Equal(new object[] { newBanana }, ((IEnumerable)sidecar[CollectionNavigation]).OfType<object>().ToArray());

            Assert.Same(originalBananas, entity.LesserBananas);
            Assert.Equal(new[] { originalBanana, supplimentalBanana }, entity.LesserBananas.ToArray());

            sidecar.TakeSnapshot();

            Assert.Equal(new object[] { originalBanana, supplimentalBanana }, ((IEnumerable)sidecar[CollectionNavigation]).OfType<object>().ToArray());
        }

        [Fact]
        public void Can_snapshot_individual_scaler_values()
        {
            var originalBanana = new Banana();
            var entity = new Banana { Id = 77, Fk = 78, TopBanana = originalBanana };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            sidecar.TakeSnapshot(FkProperty);
            sidecar.TakeSnapshot(ReferenceNavigation);

            Assert.Equal(78, sidecar[FkProperty]);
            Assert.Same(originalBanana, sidecar[ReferenceNavigation]);

            var newBanana = new Banana();
            sidecar[FkProperty] = 79;
            sidecar[ReferenceNavigation] = newBanana;

            Assert.Equal(79, sidecar[FkProperty]);
            Assert.Same(newBanana, sidecar[ReferenceNavigation]);

            Assert.Equal(78, entity.Fk);
            Assert.Same(originalBanana, entity.TopBanana);

            var supplimentalBanana = new Banana();
            entity.Fk = 80;
            entity.TopBanana = supplimentalBanana;

            Assert.Equal(79, sidecar[FkProperty]);
            Assert.Same(newBanana, sidecar[ReferenceNavigation]);

            Assert.Equal(80, entity.Fk);
            Assert.Same(supplimentalBanana, entity.TopBanana);

            sidecar.TakeSnapshot(FkProperty);

            Assert.Equal(80, sidecar[FkProperty]);
            Assert.Same(newBanana, sidecar[ReferenceNavigation]);

            sidecar.TakeSnapshot(ReferenceNavigation);

            Assert.Equal(80, sidecar[FkProperty]);
            Assert.Same(supplimentalBanana, sidecar[ReferenceNavigation]);
        }

        [Fact]
        public void Can_snapshot_individual_collection_values()
        {
            var originalBanana = new Banana();
            var originalBananas = new List<Banana> { originalBanana };
            var entity = new Banana { Id = 77, Fk = 78, LesserBananas = originalBananas };
            var sidecar = CreateSidecar(CreateInternalEntry(entity));

            sidecar.TakeSnapshot(CollectionNavigation);

            Assert.NotSame(originalBanana, sidecar[CollectionNavigation]);
            Assert.Equal(new object[] { originalBanana }, ((IEnumerable)sidecar[CollectionNavigation]).OfType<object>().ToArray());

            var newBanana = new Banana();
            var newBananas = new List<Banana> { newBanana };
            sidecar[CollectionNavigation] = newBananas;

            Assert.Same(newBananas, sidecar[CollectionNavigation]);
            Assert.Same(originalBananas, entity.LesserBananas);

            var supplimentalBanana = new Banana();
            entity.LesserBananas.Add(supplimentalBanana);

            Assert.Same(newBananas, sidecar[CollectionNavigation]);
            Assert.Equal(new object[] { newBanana }, ((IEnumerable)sidecar[CollectionNavigation]).OfType<object>().ToArray());

            Assert.Same(originalBananas, entity.LesserBananas);
            Assert.Equal(new[] { originalBanana, supplimentalBanana }, entity.LesserBananas.ToArray());

            sidecar.TakeSnapshot(CollectionNavigation);

            Assert.Equal(new object[] { originalBanana, supplimentalBanana }, ((IEnumerable)sidecar[CollectionNavigation]).OfType<object>().ToArray());
        }

        protected InternalEntityEntry CreateInternalEntry(Banana entity = null)
        {
            entity = entity ?? new Banana { Id = 77 };

            var contextServices = TestHelpers.Instance.CreateContextServices(_model);

            return contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);
        }

        private static Model BuildModel()
        {
            var model = new Model();

            var entityType = model.AddEntityType(typeof(Banana));
            var pkProperty = entityType.GetOrAddProperty("Id", typeof(int));
            var fkProperty = entityType.GetOrAddProperty("Fk", typeof(int));

            entityType.GetOrSetPrimaryKey(pkProperty);
            var fk = entityType.GetOrAddForeignKey(fkProperty, entityType.GetPrimaryKey());

            entityType.GetOrAddProperty("Name", typeof(string));

            entityType.AddNavigation("LesserBananas", fk, pointsToPrincipal: false);
            entityType.AddNavigation("TopBanana", fk, pointsToPrincipal: true);

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

        protected IProperty NameProperty
        {
            get { return _model.GetEntityType(typeof(Banana)).GetProperty("Name"); }
        }

        protected INavigation CollectionNavigation
        {
            get { return _model.GetNavigations(ForeignKey).Single(n => n.Name == "LesserBananas"); }
        }

        protected INavigation ReferenceNavigation
        {
            get { return _model.GetNavigations(ForeignKey).Single(n => n.Name == "TopBanana"); }
        }

        private ForeignKey ForeignKey
        {
            get { return _model.GetEntityType(typeof(Banana)).GetForeignKeys().Single(); }
        }

        protected class Banana : INotifyPropertyChanged, INotifyPropertyChanging
        {
            public int Id { get; set; }
            public int Fk { get; set; }
            public string Name { get; set; }
            public ICollection<Banana> LesserBananas { get; set; }
            public Banana TopBanana { get; set; }

#pragma warning disable 67
            public event PropertyChangedEventHandler PropertyChanged;
            public event PropertyChangingEventHandler PropertyChanging;
#pragma warning restore 67
        }
    }
}
