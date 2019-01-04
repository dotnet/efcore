// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel
{
    public class SponsorDetails
    {
        public SponsorDetails()
        {
        }

        private SponsorDetails(int days, decimal space)
        {
            Days = days;
            Space = space;
        }

        public int Days { get; set; }
        public decimal Space { get; set; }
    }
}
