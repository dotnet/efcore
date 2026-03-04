// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

#nullable disable

public class SolidFuelTank : FuelTank
{
    public string GrainGeometry { get; set; }
    public SolidRocket Rocket { get; set; }

    public override bool Equals(object obj)
        => obj is SolidFuelTank other
            && base.Equals(other)
            && GrainGeometry == other.GrainGeometry;

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), GrainGeometry);
}
