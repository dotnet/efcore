// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel
{
    public class PolygonEntity
    {
        public Guid Id { get; set; }
        public Polygon Polygon { get; set; }
    }
}
