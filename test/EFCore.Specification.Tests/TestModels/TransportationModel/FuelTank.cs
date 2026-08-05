// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

#nullable disable

public class FuelTank
{
    public string VehicleName { get; set; }
    public string FuelType { get; set; }
    public int Capacity { get; set; }

    public PoweredVehicle Vehicle { get; set; }
    public CombustionEngine Engine { get; set; }

    public override bool Equals(object obj)
        => obj is FuelTank other
            && VehicleName == other.VehicleName
            && FuelType == other.FuelType
            && Capacity == other.Capacity;

    public override int GetHashCode()
        => HashCode.Combine(VehicleName, FuelType, Capacity);
}
