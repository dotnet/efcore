// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class OperatorDetails
    {
        public string VehicleName { get; set; }
        public string Type { get; set; }

        public override bool Equals(object obj)
            => obj is OperatorDetails other
                && VehicleName == other.VehicleName
                && Type == other.Type;

        public override int GetHashCode()
            => HashCode.Combine(VehicleName, Type);
    }
}
