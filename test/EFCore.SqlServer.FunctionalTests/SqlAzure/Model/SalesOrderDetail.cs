// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model
{
    [Table("SalesOrderDetail", Schema = "SalesLT")]
    public class SalesOrderDetail
    {
        public int SalesOrderID { get; set; }
        public int SalesOrderDetailID { get; set; }
        public decimal LineTotal { get; set; }
        public DateTime ModifiedDate { get; set; }
        public short OrderQty { get; set; }
        public int ProductID { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceDiscount { get; set; }
        public Guid rowguid { get; set; }

        [ForeignKey("ProductID")]
        [InverseProperty("OrderDetails")]
        public virtual Product Product { get; set; }

        [ForeignKey("SalesOrderID")]
        [InverseProperty("Details")]
        public virtual SalesOrder SalesOrder { get; set; }
    }
}
