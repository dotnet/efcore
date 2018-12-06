// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NetTopologySuite;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class SpatialQueryFixtureBase : SharedStoreFixtureBase<SpatialContext>, IQueryFixtureBase
    {
        private IGeometryFactory _geometryFactory;

        protected SpatialQueryFixtureBase()
        {
            QueryAsserter = new QueryAsserter<SpatialContext>(
                CreateContext,
                new SpatialData(GeometryFactory),
                entitySorters: null,
                entityAsserters: null);
        }

        public QueryAsserterBase QueryAsserter { get; set; }

        public virtual IGeometryFactory GeometryFactory
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
