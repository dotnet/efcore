// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class SpatialTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : SpatialFixtureBase, new()
{
    protected SpatialTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

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
    public virtual async void Mutation_of_tracked_values_does_not_mutate_values_in_store()
    {
        Point CreatePoint(double y = 2.2)
            => new(1.1, y, 3.3);

        Polygon CreatePolygon(double y = 2.2)
            => new(
                new LinearRing([new(1.1, 2.2), new(2.2, y), new(2.2, 1.1), new(1.1, 2.2)]));

        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var point = CreatePoint();
        var polygon = CreatePolygon();

        await ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                context.AddRange(
                    new PointEntity { Id = id1, Point = point },
                    new PolygonEntity { Id = id2, Polygon = polygon });

                return context.SaveChangesAsync();
            }, async context =>
            {
                point.X = 11.1;
                polygon.Coordinates[1].X = 11.1;

                var fromStore1 = await context.Set<PointEntity>().FirstAsync(p => p.Id == id1);
                var fromStore2 = await context.Set<PolygonEntity>().FirstAsync(p => p.Id == id2);

                Assert.Equal(CreatePoint(), fromStore1.Point);
                Assert.Equal(CreatePolygon(), fromStore2.Polygon);

                fromStore1.Point.Y = 22.2;
                fromStore2.Polygon.Coordinates[1].Y = 22.2;

                context.Entry(fromStore2).State = EntityState.Unchanged;

                await context.SaveChangesAsync();
            }, async context =>
            {
                var fromStore1 = await context.Set<PointEntity>().FirstAsync(p => p.Id == id1);
                var fromStore2 = await context.Set<PolygonEntity>().FirstAsync(p => p.Id == id2);

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

    protected virtual Task ExecuteWithStrategyInTransactionAsync(
        Func<SpatialContext, Task> testOperation,
        Func<SpatialContext, Task> nestedTestOperation1 = null,
        Func<SpatialContext, Task> nestedTestOperation2 = null)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction,
            testOperation, nestedTestOperation1, nestedTestOperation2);

    protected abstract void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction);

    protected SpatialContext CreateContext()
        => Fixture.CreateContext();
}
