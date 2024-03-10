// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model;

#nullable disable

[Table("SalesOrderHeader", Schema = "SalesLT")]
public class SalesOrder
{
    public SalesOrder()
    {
        Details = new HashSet<SalesOrderDetail>();
    }

    public int SalesOrderID { get; set; }
    public string AccountNumber { get; set; }
    public int? BillToAddressID { get; set; }
    public string Comment { get; set; }

    [MaxLength(15)]
    public string CreditCardApprovalCode { get; set; }

    public int CustomerID { get; set; }

    public DateTime DueDate { get; set; }
    public decimal Freight { get; set; }
    public DateTime ModifiedDate { get; set; }

    [Column("OnlineOrderFlag")]
    public bool IsOnlineOrder { get; set; }

    public DateTime OrderDate { get; set; }
    public string PurchaseOrderNumber { get; set; }
    public byte RevisionNumber { get; set; }

    [Required]
    [MaxLength(25)]
    public string SalesOrderNumber { get; set; }

    public DateTime? ShipDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string ShipMethod { get; set; }

    public int? ShipToAddressID { get; set; }
    public byte Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmt { get; set; }
    public decimal TotalDue { get; set; }
    public Guid rowguid { get; set; }

    [InverseProperty("SalesOrder")]
    public virtual ICollection<SalesOrderDetail> Details { get; set; }

    [ForeignKey("CustomerID")]
    [InverseProperty("Orders")]
    public virtual Customer Customer { get; set; }

    [ForeignKey("BillToAddressID")]
    public virtual Address BillToAddress { get; set; }

    [ForeignKey("ShipToAddressID")]
    public virtual Address ShipToAddress { get; set; }
}
