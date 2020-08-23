// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NetTopologySuite.Geometries;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class SpatialTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : SpatialFixtureBase, new()
    {
        protected SpatialTestBase(TFixture fixture)
            => Fixture = fixture;

        protected virtual TFixture Fixture { get; }

        [ConditionalFact]
        public virtual void Values_are_copied_into_change_tracker()
        {
            using var db = Fixture.CreateContext();
            var entity = new PointEntity { Id = Guid.NewGuid(), Point = new Point(0, 0) };
            db.Attach(entity);

            entity.Point.X = 1;

            Assert.Equal(0, db.Entry(entity).Property(e => e.Point).OriginalValue.X);
        }

        [ConditionalFact]
        public virtual void Values_arent_compared_by_reference()
        {
            using var db = Fixture.CreateContext();
            var entity = new PointEntity { Id = Guid.NewGuid(), Point = new Point(0, 0) };
            db.Attach(entity);

            entity.Point = new Point(0, 0);

            Assert.False(db.Entry(entity).Property(e => e.Point).IsModified);
        }

        [ConditionalFact]
        public virtual void Mutation_of_tracked_values_does_not_mutate_values_in_store()
        {
            Point CreatePoint(double y = 2.2)
                => new Point(1.1, y, 3.3);

            Polygon CreatePolygon(double y = 2.2)
                => new Polygon(
                    new LinearRing(
                        new[] { new Coordinate(1.1, 2.2), new Coordinate(2.2, y), new Coordinate(2.2, 1.1), new Coordinate(1.1, 2.2) }));

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var point = CreatePoint();
            var polygon = CreatePolygon();

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.AddRange(
                        new PointEntity { Id = id1, Point = point },
                        new PolygonEntity { Id = id2, Polygon = polygon });

                    context.SaveChanges();
                },
                context =>
                {
                    point.X = 11.1;
                    polygon.Coordinates[1].X = 11.1;

                    var fromStore1 = context.Set<PointEntity>().First(p => p.Id == id1);
                    var fromStore2 = context.Set<PolygonEntity>().First(p => p.Id == id2);

                    Assert.Equal(CreatePoint(), fromStore1.Point);
                    Assert.Equal(CreatePolygon(), fromStore2.Polygon);

                    fromStore1.Point.Y = 22.2;
                    fromStore2.Polygon.Coordinates[1].Y = 22.2;

                    context.Entry(fromStore2).State = EntityState.Unchanged;

                    context.SaveChanges();
                },
                context =>
                {
                    var fromStore1 = context.Set<PointEntity>().First(p => p.Id == id1);
                    var fromStore2 = context.Set<PolygonEntity>().First(p => p.Id == id2);

                    Assert.Equal(CreatePoint(22.2), fromStore1.Point);
                    Assert.Equal(CreatePolygon(), fromStore2.Polygon);
                });
        }

        [ConditionalFact]
        public virtual void Translators_handle_static_members()
        {
            using var db = Fixture.CreateContext();
            (from e in db.Set<PointEntity>()
             orderby e.Id
             select new
             {
                 e.Id,
                 e.Point,
                 Point.Empty,
                 DateTime.UtcNow,
                 Guid = Guid.NewGuid()
             }).FirstOrDefault();
        }

        [ConditionalFact]
        public virtual void Can_roundtrip_Z_and_M()
        {
            using var db = Fixture.CreateContext();
            var entity = db.Set<PointEntity>()
                .FirstOrDefault(e => e.Id == PointEntity.WellKnownId);

            Assert.NotNull(entity);
            Assert.NotNull(entity.Point);
            Assert.True(double.IsNaN(entity.Point.Z));
            Assert.True(double.IsNaN(entity.Point.M));
            Assert.Equal(0, entity.PointZ.Z);
            Assert.True(double.IsNaN(entity.PointZ.M));
            Assert.True(double.IsNaN(entity.PointM.Z));
            Assert.Equal(0, entity.PointM.M);
            Assert.Equal(0, entity.PointZM.Z);
            Assert.Equal(0, entity.PointZM.M);
        }

        protected virtual void ExecuteWithStrategyInTransaction(
            Action<SpatialContext> testOperation,
            Action<SpatialContext> nestedTestOperation1 = null,
            Action<SpatialContext> nestedTestOperation2 = null)
            => TestHelpers.ExecuteWithStrategyInTransaction(
                CreateContext, UseTransaction,
                testOperation, nestedTestOperation1, nestedTestOperation2);

        protected abstract void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction);

        protected SpatialContext CreateContext()
            => Fixture.CreateContext();
    }
}
