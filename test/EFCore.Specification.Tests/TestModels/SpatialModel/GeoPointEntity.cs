// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel;

public class GeoPointEntity
{
    public Guid Id { get; set; }
    public GeoPoint Location { get; set; }
}
