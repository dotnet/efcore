// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class CompositeVehicle : PoweredVehicle
    {
        public Vehicle AttachedVehicle { get; set; }

        public override bool Equals(object obj)
            => obj is CompositeVehicle other
                && base.Equals(other)
                && Equals(AttachedVehicle, other.AttachedVehicle);

        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), AttachedVehicle);
    }
}
