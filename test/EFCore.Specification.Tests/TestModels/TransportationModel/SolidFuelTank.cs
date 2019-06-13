// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class SolidFuelTank : FuelTank
    {
        public string GrainGeometry { get; set; }
        public SolidRocket Rocket { get; set; }

        public override bool Equals(object obj)
            => obj is SolidFuelTank other
               && base.Equals(other)
               && GrainGeometry == other.GrainGeometry;

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), GrainGeometry);
    }
}
