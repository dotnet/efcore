// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SqlServerValueGenerationScenariosTest : SqlServerValueGenerationScenariosTestBase
{
    protected override string DatabaseName
        => "SqlServerValueGenerationScenariosTest";

    protected override Guid GuidSentinel
        => new();

    protected override int IntSentinel
        => 0;

    protected override uint UIntSentinel
        => 0;

    protected override IntKey IntKeySentinel
        => IntKey.Zero;

    protected override ULongKey ULongKeySentinel
        => ULongKey.Zero;

    protected override int? NullableIntSentinel
        => null;

    protected override string StringSentinel
        => null;

    protected override DateTime DateTimeSentinel
        => new();

    protected override NeedsConverter NeedsConverterSentinel
        => new(0);

    protected override GeometryCollection GeometryCollectionSentinel
        => null;

    protected override byte[] TimestampSentinel
        => null;
}
