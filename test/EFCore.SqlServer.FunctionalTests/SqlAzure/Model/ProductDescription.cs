// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model;

#nullable disable

[Table("ProductDescription", Schema = "SalesLT")]
public class ProductDescription
{
    public ProductDescription()
    {
        ProductModelProductDescription = new HashSet<ProductModelProductDescription>();
    }

    public int ProductDescriptionID { get; set; }

    [Required]
    [MaxLength(400)]
    public string Description { get; set; }

    public DateTime ModifiedDate { get; set; }
    public Guid rowguid { get; set; }

    [InverseProperty("ProductDescription")]
    public virtual ICollection<ProductModelProductDescription> ProductModelProductDescription { get; set; }
}
