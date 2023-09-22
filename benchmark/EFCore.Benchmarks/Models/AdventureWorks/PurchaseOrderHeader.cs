// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class PurchaseOrderHeader
{
    public PurchaseOrderHeader()
    {
        PurchaseOrderDetail = new HashSet<PurchaseOrderDetail>();
    }

    public int PurchaseOrderID { get; set; }
    public int EmployeeID { get; set; }
    public decimal Freight { get; set; }
    public DateTime ModifiedDate { get; set; }
    public DateTime OrderDate { get; set; }
    public byte RevisionNumber { get; set; }
    public DateTime? ShipDate { get; set; }
    public int ShipMethodID { get; set; }
    public byte Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmt { get; set; }
    public decimal TotalDue { get; set; }
    public int VendorID { get; set; }

    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetail { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual ShipMethod ShipMethod { get; set; }
    public virtual Vendor Vendor { get; set; }
}
