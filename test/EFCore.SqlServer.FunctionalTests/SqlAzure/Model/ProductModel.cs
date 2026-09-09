// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model;

#nullable disable

[Table("ProductModel", Schema = "SalesLT")]
public class ProductModel
{
    public ProductModel()
    {
        Product = new HashSet<Product>();
        ProductModelProductDescription = new HashSet<ProductModelProductDescription>();
    }

    public int ProductModelID { get; set; }
    public string Name { get; set; }
    public string CatalogDescription { get; set; }
    public DateTime ModifiedDate { get; set; }
    public Guid rowguid { get; set; }

    [InverseProperty("ProductModel")]
    public virtual ICollection<Product> Product { get; set; }

    [InverseProperty("ProductModel")]
    public virtual ICollection<ProductModelProductDescription> ProductModelProductDescription { get; set; }
}
