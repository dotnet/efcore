// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class SolidRocket : ContinuousCombustionEngine
    {
        public SolidFuelTank SolidFuelTank { get; set; }

        public override bool Equals(object obj)
            => obj is SolidRocket other
               && base.Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), SolidFuelTank);
    }
}
