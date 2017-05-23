// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model
{
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
}
