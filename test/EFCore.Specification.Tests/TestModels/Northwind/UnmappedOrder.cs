// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

#nullable disable

[Table("Orders")]
public class UnmappedOrder
{
    public int OrderID { get; set; }

    [MaxLength(5)]
    public string CustomerID { get; set; }

    public int? EmployeeID { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public int? ShipVia { get; set; }

    [Column(TypeName = "decimal(18,3")]
    public decimal? Freight { get; set; }

    [MaxLength(40)]
    public string ShipName { get; set; }

    [MaxLength(60)]
    public string ShipAddress { get; set; }

    [MaxLength(15)]
    public string ShipCity { get; set; }

    [MaxLength(15)]
    public string ShipRegion { get; set; }

    [MaxLength(10)]
    public string ShipPostalCode { get; set; }

    [MaxLength(15)]
    public string ShipCountry { get; set; }

    public static UnmappedOrder FromOrder(Order order)
        => new()
        {
            OrderID = order.OrderID,
            CustomerID = order.CustomerID,
            EmployeeID = (int)order.EmployeeID,
            OrderDate = order.OrderDate,
            RequiredDate = order.RequiredDate,
            ShippedDate = order.ShippedDate,
            ShipVia = order.ShipVia,
            Freight = order.Freight,
            ShipName = order.ShipName,
            ShipAddress = order.ShipAddress,
            ShipCity = order.ShipCity,
            ShipRegion = order.ShipRegion,
            ShipPostalCode = order.ShipPostalCode,
            ShipCountry = order.ShipCountry,
        };
}
