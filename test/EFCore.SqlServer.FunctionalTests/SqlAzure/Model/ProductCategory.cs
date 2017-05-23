// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model
{
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
}
