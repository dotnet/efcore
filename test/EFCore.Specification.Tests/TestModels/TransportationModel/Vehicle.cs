// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
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

        public override int GetHashCode() => HashCode.Combine(Name, SeatingCapacity, Operator);
    }
}
