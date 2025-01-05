// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

public class Order
{
    public int OrderId { get; set; }
    public DateTime Date { get; set; }
    public string SpecialRequests { get; set; }
    public decimal OrderDiscount { get; set; }
    public string DiscountReason { get; set; }
    public decimal Tax { get; set; }

    public string Addressee { get; set; }
    public string AddressLineOne { get; set; }
    public string AddressLineTwo { get; set; }
    public string City { get; set; }
    public string StateOrProvince { get; set; }
    public string ZipOrPostalCode { get; set; }
    public string County { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; }

    public ICollection<OrderLine> OrderLines { get; set; }
}
