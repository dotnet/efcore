// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

#nullable disable

public class Engine
{
    public string VehicleName { get; set; }
    public string Description { get; set; }
    public int Computed { get; set; }
    public PoweredVehicle Vehicle { get; set; }

    public override bool Equals(object obj)
        => obj is Engine other
            && VehicleName == other.VehicleName
            && Description == other.Description;

    public override int GetHashCode()
        => HashCode.Combine(VehicleName, Description);
}
