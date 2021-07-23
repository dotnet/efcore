﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class OperatorDetails
    {
        public string VehicleName { get; set; }
        public string Type { get; set; }
        public bool Active { get; set; }

        public override bool Equals(object obj)
            => obj is OperatorDetails other
                && VehicleName == other.VehicleName
                && Type == other.Type
                && Active == other.Active;

        public override int GetHashCode()
            => HashCode.Combine(VehicleName, Type, Active);
    }
}
