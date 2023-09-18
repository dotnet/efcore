// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class PurchaseOrderDetail
{
    public int PurchaseOrderID { get; set; }
    public int PurchaseOrderDetailID { get; set; }
    public DateTime DueDate { get; set; }
    public decimal LineTotal { get; set; }
    public DateTime ModifiedDate { get; set; }
    public short OrderQty { get; set; }
    public int ProductID { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal RejectedQty { get; set; }
    public decimal StockedQty { get; set; }
    public decimal UnitPrice { get; set; }

    public virtual Product Product { get; set; }
    public virtual PurchaseOrderHeader PurchaseOrder { get; set; }
}
