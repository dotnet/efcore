// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

#nullable disable

public class Product
{
    private int? _productId;

    public Product()
    {
        OrderDetails = [];
    }

    public int ProductID
    {
        get => _productId ?? 0;
        set => _productId = value;
    }

    [MaxLength(40)]
    [Required]
    public string ProductName { get; set; }

    public int? SupplierID { get; set; }
    public int? CategoryID { get; set; }

    [MaxLength(20)]
    public string QuantityPerUnit { get; set; }

    public decimal? UnitPrice { get; set; }
    public ushort UnitsInStock { get; set; }
    public ushort? UnitsOnOrder { get; set; }
    public ushort? ReorderLevel { get; set; }
    public bool Discontinued { get; set; }

    public virtual List<OrderDetail> OrderDetails { get; set; }

    protected bool Equals(Product other)
        => Equals(ProductID, other.ProductID);

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj)
            ? true
            : obj.GetType() == GetType()
            && Equals((Product)obj);
    }

    public override int GetHashCode()
        => ProductID.GetHashCode();

    public override string ToString()
        => "Product " + ProductID;
}
