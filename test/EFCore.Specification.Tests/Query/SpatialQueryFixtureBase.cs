// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class SpatialQueryFixtureBase : SharedStoreFixtureBase<SpatialContext>, IQueryFixtureBase
    {
        private GeometryFactory _geometryFactory;

        public Func<DbContext> GetContextCreator()
            => () => CreateContext();

        public ISetSource GetExpectedData()
            => new SpatialData(GeometryFactory);

        public IReadOnlyDictionary<Type, object> GetEntitySorters()
            => null;

        public IReadOnlyDictionary<Type, object> GetEntityAsserters()
            => null;

        public virtual GeometryFactory GeometryFactory
            => LazyInitializer.EnsureInitialized(
                ref _geometryFactory,
                () => NtsGeometryServices.Instance.CreateGeometryFactory(srid: 0));

        protected override string StoreName
            => "SpatialQueryTest";

        public override SpatialContext CreateContext()
        {
            var context = base.CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<PointEntity>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<LineStringEntity>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<PolygonEntity>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<MultiLineStringEntity>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<GeoPointEntity>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Location).HasConversion(new GeoPointConverter(GeometryFactory));
                });
        }

        protected override void Seed(SpatialContext context)
            => SpatialContext.Seed(context, GeometryFactory);
    }
}
