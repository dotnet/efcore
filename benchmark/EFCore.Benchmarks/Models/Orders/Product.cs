// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string SKU { get; set; }
    public double Retail { get; set; }
    public double CurrentPrice { get; set; }
    public int TargetStockLevel { get; set; }
    public int ActualStockLevel { get; set; }
    public int? ReorderStockLevel { get; set; }
    public int QuantityOnOrder { get; set; }
    public DateTime? NextShipmentExpected { get; set; }
    public bool IsDiscontinued { get; set; }

    public ICollection<OrderLine> OrderLines { get; set; }
}
