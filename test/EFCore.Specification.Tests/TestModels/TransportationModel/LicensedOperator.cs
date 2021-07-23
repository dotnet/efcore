// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
