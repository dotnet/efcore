// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel;

#nullable disable

public class PointEntity
{
    public static readonly Guid WellKnownId = Guid.Parse("2F39AADE-4D8D-42D2-88CE-775C84AB83B1");

    public Guid Id { get; set; }
    public string Group { get; set; }
    public Geometry Geometry { get; set; }
    public Point Point { get; set; }
    public Point PointZ { get; set; }
    public Point PointM { get; set; }
    public Point PointZM { get; set; }
}
