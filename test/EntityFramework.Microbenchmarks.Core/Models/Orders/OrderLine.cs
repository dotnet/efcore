// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace EntityFramework.Microbenchmarks.Core.Models.Orders
{
    public class OrderLine
    {
        public int OrderLineId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
