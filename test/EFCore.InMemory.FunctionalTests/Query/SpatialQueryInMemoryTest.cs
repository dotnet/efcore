// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SpatialQueryInMemoryTest : SpatialQueryTestBase<SpatialQueryInMemoryFixture>
    {
        public SpatialQueryInMemoryTest(SpatialQueryInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task Area(bool isAsync)
            => base.Area(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task Boundary(bool isAsync)
            => base.Boundary(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task Centroid(bool isAsync)
            => base.Centroid(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task Dimension(bool isAsync)
            => base.Dimension(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task EndPoint(bool isAsync)
            => base.EndPoint(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task Envelope(bool isAsync)
            => base.Envelope(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task ExteriorRing(bool isAsync)
            => base.ExteriorRing(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task GeometryType(bool isAsync)
            => base.GeometryType(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task ICurve_IsClosed(bool isAsync)
            => base.ICurve_IsClosed(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task IGeometryCollection_Count(bool isAsync)
            => base.IGeometryCollection_Count(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task IMultiCurve_IsClosed(bool isAsync)
            => base.IMultiCurve_IsClosed(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task InteriorPoint(bool isAsync)
            => base.InteriorPoint(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task IsEmpty(bool isAsync)
            => base.IsEmpty(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task IsRing(bool isAsync)
            => base.IsRing(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task IsSimple(bool isAsync)
            => base.IsSimple(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task IsValid(bool isAsync)
            => base.IsValid(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task Length(bool isAsync)
            => base.Length(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task LineString_Count(bool isAsync)
            => base.LineString_Count(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task M(bool isAsync)
            => base.M(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task NumGeometries(bool isAsync)
            => base.NumGeometries(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task NumInteriorRings(bool isAsync)
            => base.NumInteriorRings(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task NumPoints(bool isAsync)
            => base.NumPoints(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task OgcGeometryType(bool isAsync)
            => base.OgcGeometryType(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task PointOnSurface(bool isAsync)
            => base.PointOnSurface(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task SRID(bool isAsync)
            => base.SRID(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task SRID_geometry(bool isAsync)
            => base.SRID_geometry(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task StartPoint(bool isAsync)
            => base.StartPoint(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task X(bool isAsync)
            => base.X(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task Y(bool isAsync)
            => base.Y(isAsync);

        [ConditionalTheory(Skip = "Issue #16963. Nullable error")]
        public override Task Z(bool isAsync)
            => base.Z(isAsync);
    }
}
