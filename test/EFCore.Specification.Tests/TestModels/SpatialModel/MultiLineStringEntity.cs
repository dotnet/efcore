// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel;

#nullable disable

public class MultiLineStringEntity
{
    public int Id { get; set; }
    public MultiLineString MultiLineString { get; set; }
}
