// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model;

#nullable disable

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
