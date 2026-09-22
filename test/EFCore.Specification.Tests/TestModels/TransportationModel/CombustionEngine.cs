// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

#nullable disable

public abstract class CombustionEngine : Engine
{
    public FuelTank FuelTank { get; set; }

    public override bool Equals(object obj)
        => obj is CombustionEngine other
            && base.Equals(other)
            && Equals(FuelTank, other.FuelTank);

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), FuelTank);
}
