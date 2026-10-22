// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

#nullable disable

public class UnmappedProduct
{
    public int ProductID { get; set; }
    public string ProductName { get; set; }
    public int? SupplierID { get; set; }

    [NotMapped]
    public int? CategoryID { get; set; }

    [Column(TypeName = "decimal(18,3")]
    public decimal? UnitPrice { get; set; }

    public short UnitsInStock { get; set; }
    public bool Discontinued { get; set; }
}
