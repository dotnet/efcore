// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class Engine
    {
        public string VehicleName { get; set; }
        public string Description { get; set; }
        public PoweredVehicle Vehicle { get; set; }

        public override bool Equals(object obj)
            => obj is Engine other
                && VehicleName == other.VehicleName
                && Description == other.Description;

        public override int GetHashCode() => HashCode.Combine(VehicleName, Description);
    }
}
