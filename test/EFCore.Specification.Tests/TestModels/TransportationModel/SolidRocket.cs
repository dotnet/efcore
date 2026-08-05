// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

#nullable disable

public class SolidRocket : ContinuousCombustionEngine
{
    public SolidFuelTank SolidFuelTank { get; set; }

    public override bool Equals(object obj)
        => obj is SolidRocket other
            && base.Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), SolidFuelTank);
}
