// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using NetTopologySuite.Geometries;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
#if !Test21
    public abstract class SpatialTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : SpatialFixtureBase, new()
    {
        protected SpatialTestBase(TFixture fixture)
             => Fixture = fixture;

        protected virtual TFixture Fixture { get; }

        [ConditionalFact]
        public virtual void Values_are_copied_into_change_tracker()
        {
            using (var db = Fixture.CreateContext())
            {
                var entity = new PointEntity
                {
                    Id = 1,
                    Point = new Point(0, 0)
                };
                db.Attach(entity);

                entity.Point.X = 1;

                Assert.Equal(0, db.Entry(entity).Property(e => e.Point).OriginalValue.X);
            }
        }

        [ConditionalFact]
        public virtual void Values_arent_compared_by_reference()
        {
            using (var db = Fixture.CreateContext())
            {
                var entity = new PointEntity
                {
                    Id = 1,
                    Point = new Point(0, 0)
                };
                db.Attach(entity);

                entity.Point = new Point(0, 0);

                Assert.False(db.Entry(entity).Property(e => e.Point).IsModified);
            }
        }
    }
#endif
}
