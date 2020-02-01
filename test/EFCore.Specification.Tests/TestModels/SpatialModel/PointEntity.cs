// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel
{
    public class PointEntity
    {
        public static readonly Guid WellKnownId = Guid.Parse("2F39AADE-4D8D-42D2-88CE-775C84AB83B1");

        public Guid Id { get; set; }
        public Geometry Geometry { get; set; }
        public Point Point { get; set; }
    }
}
