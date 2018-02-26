// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class SpecialOffer
    {
        public SpecialOffer()
        {
            SpecialOfferProduct = new HashSet<SpecialOfferProduct>();
        }

        public int SpecialOfferID { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal DiscountPct { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxQty { get; set; }
        public int MinQty { get; set; }
        public DateTime ModifiedDate { get; set; }
#pragma warning disable IDE1006 // Naming Styles
        public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        public DateTime StartDate { get; set; }
        public string Type { get; set; }

        public virtual ICollection<SpecialOfferProduct> SpecialOfferProduct { get; set; }
    }
}
