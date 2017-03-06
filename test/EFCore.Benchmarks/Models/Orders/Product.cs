// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        public decimal Retail { get; set; }
        public decimal CurrentPrice { get; set; }
        public int TargetStockLevel { get; set; }
        public int ActualStockLevel { get; set; }
        public int? ReorderStockLevel { get; set; }
        public int QuantityOnOrder { get; set; }
        public DateTime? NextShipmentExpected { get; set; }
        public bool IsDiscontinued { get; set; }

        public ICollection<OrderLine> OrderLines { get; set; }
    }
}
