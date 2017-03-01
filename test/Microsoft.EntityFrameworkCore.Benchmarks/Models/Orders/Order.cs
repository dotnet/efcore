// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders
{
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
        public string Country { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public ICollection<OrderLine> OrderLines { get; set; }
    }
}
