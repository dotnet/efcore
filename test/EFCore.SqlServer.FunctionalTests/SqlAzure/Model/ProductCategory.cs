// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model;

#nullable disable

[Table("ProductCategory", Schema = "SalesLT")]
public class ProductCategory
{
    public ProductCategory()
    {
        Product = new HashSet<Product>();
    }

    public int ProductCategoryID { get; set; }
    public string Name { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int? ParentProductCategoryID { get; set; }
    public Guid rowguid { get; set; }

    [InverseProperty("ProductCategory")]
    public virtual ICollection<Product> Product { get; set; }

    [ForeignKey("ParentProductCategoryID")]
    [InverseProperty("InverseParentProductCategory")]
    public virtual ProductCategory ParentProductCategory { get; set; }

    [InverseProperty("ParentProductCategory")]
    public virtual ICollection<ProductCategory> InverseParentProductCategory { get; set; }
}
