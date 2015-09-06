// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace EntityFramework.Microbenchmarks.Core.Models.AdventureWorks
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
        public Guid rowguid { get; set; }
        public DateTime StartDate { get; set; }
        public string Type { get; set; }

        public virtual ICollection<SpecialOfferProduct> SpecialOfferProduct { get; set; }
    }
}
