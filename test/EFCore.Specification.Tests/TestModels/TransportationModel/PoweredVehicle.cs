// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

#nullable disable

public class PoweredVehicle : Vehicle
{
    public Engine Engine { get; set; }

    public override bool Equals(object obj)
        => obj is PoweredVehicle other
            && base.Equals(other)
            && Equals(Engine, other.Engine);

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Engine);
}
