// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model;

#nullable disable

[Table("Product", Schema = "SalesLT")]
public class Product
{
    public Product()
    {
        OrderDetails = new HashSet<SalesOrderDetail>();
    }

    public int ProductID { get; set; }

    [Required]
    public string Name { get; set; }

    [MaxLength(15)]
    public string Color { get; set; }

    public DateTime? DiscontinuedDate { get; set; }
    public decimal ListPrice { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int? ProductCategoryID { get; set; }
    public int? ProductModelID { get; set; }

    [Required]
    [MaxLength(25)]
    public string ProductNumber { get; set; }

    public DateTime? SellEndDate { get; set; }
    public DateTime SellStartDate { get; set; }

    [MaxLength(5)]
    public string Size { get; set; }

    public decimal StandardCost { get; set; }
    public byte[] ThumbNailPhoto { get; set; }

    [MaxLength(50)]
    public string ThumbnailPhotoFileName { get; set; }

    public decimal? Weight { get; set; }
    public Guid rowguid { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<SalesOrderDetail> OrderDetails { get; set; }

    [ForeignKey("ProductCategoryID")]
    [InverseProperty("Product")]
    public virtual ProductCategory ProductCategory { get; set; }

    [ForeignKey("ProductModelID")]
    [InverseProperty("Product")]
    public virtual ProductModel ProductModel { get; set; }
}
