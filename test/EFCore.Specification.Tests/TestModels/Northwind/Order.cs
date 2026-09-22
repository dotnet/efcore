// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

#nullable disable

public class Order
{
    private int? _orderId;

    public int OrderID
    {
        get => _orderId ?? 0;
        set => _orderId = value;
    }

    [MaxLength(5)]
    public string CustomerID { get; set; }

    public uint? EmployeeID { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public int? ShipVia { get; set; }
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

    public Customer Customer { get; set; } = new(); // Initialized to test #23851

    public virtual ICollection<OrderDetail> OrderDetails { get; set; }

    protected bool Equals(Order other)
        => OrderID == other.OrderID;

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj)
            ? true
            : obj.GetType() == GetType()
            && Equals((Order)obj);
    }

    public override int GetHashCode()
        => OrderID.GetHashCode();

    public override string ToString()
        => "Order " + OrderID;
}
