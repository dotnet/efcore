// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

#nullable disable

public class Operator
{
    public string VehicleName { get; set; }
    public string Name { get; set; }
    public Vehicle Vehicle { get; set; }
    public OperatorDetails Details { get; set; }

    public override bool Equals(object obj)
        => obj is Operator other
            && VehicleName == other.VehicleName
            && Name == other.Name
            && Equals(Details, other.Details);

    public override int GetHashCode()
        => HashCode.Combine(VehicleName, Name, Details);
}
