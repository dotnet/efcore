// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model;

#nullable disable

[Table("ProductModelProductDescription", Schema = "SalesLT")]
public class ProductModelProductDescription
{
    public int ProductModelID { get; set; }
    public int ProductDescriptionID { get; set; }

    [MaxLength(6)]
    public string Culture { get; set; }

    public DateTime ModifiedDate { get; set; }
    public Guid rowguid { get; set; }

    [ForeignKey("ProductDescriptionID")]
    [InverseProperty("ProductModelProductDescription")]
    public virtual ProductDescription ProductDescription { get; set; }

    [ForeignKey("ProductModelID")]
    [InverseProperty("ProductModelProductDescription")]
    public virtual ProductModel ProductModel { get; set; }
}
