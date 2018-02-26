// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class ProductVendor
    {
        public int ProductID { get; set; }
        public int BusinessEntityID { get; set; }
        public int AverageLeadTime { get; set; }
        public decimal? LastReceiptCost { get; set; }
        public DateTime? LastReceiptDate { get; set; }
        public int MaxOrderQty { get; set; }
        public int MinOrderQty { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? OnOrderQty { get; set; }
        public decimal StandardPrice { get; set; }
        public string UnitMeasureCode { get; set; }

        public virtual Vendor BusinessEntity { get; set; }
        public virtual Product Product { get; set; }
        public virtual UnitMeasure UnitMeasureCodeNavigation { get; set; }
    }
}
