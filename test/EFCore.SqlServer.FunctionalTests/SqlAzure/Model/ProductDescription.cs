// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model
{
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
}
