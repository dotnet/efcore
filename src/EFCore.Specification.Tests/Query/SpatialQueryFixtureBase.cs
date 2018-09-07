// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
#if !Test21
    public abstract class SpatialQueryFixtureBase : SharedStoreFixtureBase<SpatialContext>, IQueryFixtureBase
    {
        public SpatialQueryFixtureBase()
        {
            QueryAsserter = new QueryAsserter<SpatialContext>(
                CreateContext,
                new SpatialData(),
                entitySorters: null,
                entityAsserters: null);
        }

        public QueryAsserterBase QueryAsserter { get; set; }

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
        }

        protected override void Seed(SpatialContext context)
            => SpatialContext.Seed(context);
    }
#endif
}
