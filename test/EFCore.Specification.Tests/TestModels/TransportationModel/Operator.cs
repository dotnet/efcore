// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
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
}
