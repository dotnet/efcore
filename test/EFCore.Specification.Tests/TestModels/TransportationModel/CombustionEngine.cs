// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public abstract class CombustionEngine : Engine
    {
        public FuelTank FuelTank { get; set; }

        public override bool Equals(object obj)
            => obj is CombustionEngine other
                && base.Equals(other)
                && Equals(FuelTank, other.FuelTank);

        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), FuelTank);
    }
}
