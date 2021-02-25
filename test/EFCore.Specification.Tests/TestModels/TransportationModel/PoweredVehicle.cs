// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
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
}
