// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders
{
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
}
