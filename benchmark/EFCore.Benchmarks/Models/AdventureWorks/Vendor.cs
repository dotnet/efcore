// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class Vendor
{
    public Vendor()
    {
        ProductVendor = new HashSet<ProductVendor>();
        PurchaseOrderHeader = new HashSet<PurchaseOrderHeader>();
    }

    public int BusinessEntityID { get; set; }
    public string AccountNumber { get; set; }
    public bool ActiveFlag { get; set; }
    public byte CreditRating { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Name { get; set; }
    public bool PreferredVendorStatus { get; set; }
    public string PurchasingWebServiceURL { get; set; }

    public virtual ICollection<ProductVendor> ProductVendor { get; set; }
    public virtual ICollection<PurchaseOrderHeader> PurchaseOrderHeader { get; set; }
    public virtual BusinessEntity BusinessEntity { get; set; }
}
