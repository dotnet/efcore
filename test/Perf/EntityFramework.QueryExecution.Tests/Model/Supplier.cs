// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QueryExecution.Model
{
    [Table("DefaultContainerStore.Supplier")]
    public class Supplier
    {
        public Supplier()
        {
            BackOrderLines = new HashSet<BackOrderLine>();
            SupplierInfoes = new HashSet<SupplierInfo>();
            Products = new HashSet<Product>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SupplierId { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<BackOrderLine> BackOrderLines { get; set; }

        public virtual ICollection<SupplierInfo> SupplierInfoes { get; set; }

        public virtual SupplierLogo Logo { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
