// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class LicensedOperator : Operator
    {
        public string LicenseType { get; set; }

        public override bool Equals(object obj)
            => obj is LicensedOperator other
                && base.Equals(other)
                && LicenseType == other.LicenseType;

        public override int GetHashCode()
            => base.GetHashCode();
    }
}
