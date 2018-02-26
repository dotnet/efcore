// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class ProductSubcategory
    {
        public ProductSubcategory()
        {
            Product = new HashSet<Product>();
        }

        public int ProductSubcategoryID { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        public int ProductCategoryID { get; set; }
#pragma warning disable IDE1006 // Naming Styles
        public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public virtual ICollection<Product> Product { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }
    }
}
