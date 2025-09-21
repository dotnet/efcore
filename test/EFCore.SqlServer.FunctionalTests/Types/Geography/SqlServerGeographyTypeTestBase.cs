// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Geography;

using Geometry = NetTopologySuite.Geometries.Geometry;

public abstract class SqlServerGeographyTypeTestBase<T, TFixture>(TFixture fixture, ITestOutputHelper testOutputHelper)
    : SqlServerSpatialTypeTestBase<T, TFixture>(fixture, testOutputHelper)
    where T : Geometry
    where TFixture : SqlServerGeographyTypeTestBase<T, TFixture>.SqlServerGeographyTypeFixture
{
    public abstract class SqlServerGeographyTypeFixture : SqlServerSpatialTypeFixture
    {
        // SQL Server default to geography
        public override string? StoreType => null;
    }
}





