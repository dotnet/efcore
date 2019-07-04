// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class SalesOrderDetail
    {
        public int SalesOrderID { get; set; }
        public int SalesOrderDetailID { get; set; }
        public string CarrierTrackingNumber { get; set; }
        public decimal LineTotal { get; set; }
        public DateTime ModifiedDate { get; set; }
        public short OrderQty { get; set; }
        public int ProductID { get; set; }
#pragma warning disable IDE1006 // Naming Styles
        public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        public int SpecialOfferID { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceDiscount { get; set; }

        public virtual SalesOrderHeader SalesOrder { get; set; }
        public virtual SpecialOfferProduct SpecialOfferProduct { get; set; }
    }
}
