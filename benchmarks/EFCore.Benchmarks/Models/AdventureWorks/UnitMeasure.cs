// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class UnitMeasure
    {
        public UnitMeasure()
        {
            BillOfMaterials = new HashSet<BillOfMaterials>();
            Product = new HashSet<Product>();
            ProductNavigation = new HashSet<Product>();
            ProductVendor = new HashSet<ProductVendor>();
        }

        public string UnitMeasureCode { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }

        public virtual ICollection<BillOfMaterials> BillOfMaterials { get; set; }
        public virtual ICollection<Product> Product { get; set; }
        public virtual ICollection<Product> ProductNavigation { get; set; }
        public virtual ICollection<ProductVendor> ProductVendor { get; set; }
    }
}
