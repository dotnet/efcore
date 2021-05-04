// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
