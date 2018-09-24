// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel
{
    public class AFewBytes
    {
        public Guid Id { get; set; }
        public byte[] Bytes { get; set; }
        public Point Point { get; set; }
        public IGeometry PolygonAsGeometry { get; set; }
    }
}
