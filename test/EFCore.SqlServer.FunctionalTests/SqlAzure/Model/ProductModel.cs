// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model
{
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
}
