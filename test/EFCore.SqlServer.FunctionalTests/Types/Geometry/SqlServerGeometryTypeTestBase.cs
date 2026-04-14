// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Geometry;

using Geometry = NetTopologySuite.Geometries.Geometry;

public abstract class SqlServerGeometryTypeTestBase<T, TFixture>(TFixture fixture, ITestOutputHelper testOutputHelper)
    : SqlServerSpatialTypeTestBase<T, TFixture>(fixture, testOutputHelper)
    where T : Geometry
    where TFixture : SqlServerGeometryTypeTestBase<T, TFixture>.GeometryTypeFixture
{
    public abstract class GeometryTypeFixture : SqlServerSpatialTypeFixture
    {
        public override string? StoreType => "geometry";
    }
}
