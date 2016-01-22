// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace EntityFramework.Microbenchmarks.Core.Models.AdventureWorks
{
    public class ProductInventory
    {
        public int ProductID { get; set; }
        public short LocationID { get; set; }
        public byte Bin { get; set; }
        public DateTime ModifiedDate { get; set; }
        public short Quantity { get; set; }
        public Guid rowguid { get; set; }
        public string Shelf { get; set; }

        public virtual Location Location { get; set; }
        public virtual Product Product { get; set; }
    }
}
