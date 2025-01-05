// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

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
