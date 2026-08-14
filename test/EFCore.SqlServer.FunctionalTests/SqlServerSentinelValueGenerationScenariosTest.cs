// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SqlServerSentinelValueGenerationScenariosTest : SqlServerValueGenerationScenariosTestBase
{
    protected override string DatabaseName
        => "SqlServerSentinelValueGenerationScenariosTest";

    protected override Guid GuidSentinel
        => new("56D3784D-6F7F-4935-B7F6-E77DC6E1D91E");

    protected override int IntSentinel
        => 667;

    protected override uint UIntSentinel
        => 667;

    protected override IntKey IntKeySentinel
        => IntKey.SixSixSeven;

    protected override ULongKey ULongKeySentinel
        => ULongKey.Sentinel;

    protected override int? NullableIntSentinel
        => 667;

    protected override string StringSentinel
        => "667";

    protected override DateTime DateTimeSentinel
        => new(1973, 9, 3, 0, 3, 0);

    protected override NeedsConverter NeedsConverterSentinel
        => new(668);

    protected override GeometryCollection GeometryCollectionSentinel
        => GeometryFactory.CreateGeometryCollection(
            [GeometryFactory.CreatePoint(new Coordinate(6, 7))]);

    protected override byte[] TimestampSentinel
        => [1, 1, 1, 1, 1, 1, 1, 1];

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Blog>(
            b =>
            {
                b.Property(e => e.Id).HasSentinel(IntSentinel);
                b.Property(e => e.Name).HasSentinel(StringSentinel);
                b.Property(e => e.CreatedOn).HasSentinel(DateTimeSentinel);
                b.Property(e => e.OtherId).HasSentinel(NullableIntSentinel);
                b.Property(e => e.NeedsConverter).HasSentinel(new NeedsConverter(IntSentinel));
            });

        modelBuilder.Entity<BlogWithSpatial>(
            b =>
            {
                b.Property(e => e.Id).HasSentinel(IntSentinel);
                b.Property(e => e.Name).HasSentinel(StringSentinel);
                b.Property(e => e.GeometryCollection).HasSentinel(GeometryCollectionSentinel);
            });
    }
}
