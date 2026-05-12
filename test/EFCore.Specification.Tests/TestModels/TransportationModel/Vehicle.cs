// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

#nullable disable

public class Vehicle
{
    public string Name { get; set; }
    public int SeatingCapacity { get; set; }
    public Operator Operator { get; set; }

    public override bool Equals(object obj)
        => obj is Vehicle other
            && Name == other.Name
            && SeatingCapacity == other.SeatingCapacity
            && Equals(Operator, other.Operator);

    public override int GetHashCode()
        => HashCode.Combine(Name, SeatingCapacity, Operator);
}
