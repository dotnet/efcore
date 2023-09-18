// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

public class OrderLine
{
    public int OrderLineId { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }
    public bool IsSubjectToTax { get; set; }
    public string SpecialRequests { get; set; }
    public bool IsShipped { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }
}
