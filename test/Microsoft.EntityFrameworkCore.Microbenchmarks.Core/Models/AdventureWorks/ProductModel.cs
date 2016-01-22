// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace EntityFramework.Microbenchmarks.Core.Models.AdventureWorks
{
    public class ProductModel
    {
        public ProductModel()
        {
            Product = new HashSet<Product>();
            ProductModelIllustration = new HashSet<ProductModelIllustration>();
            ProductModelProductDescriptionCulture = new HashSet<ProductModelProductDescriptionCulture>();
        }

        public int ProductModelID { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        public Guid rowguid { get; set; }
        public string CatalogDescription { get; set; }
        public string Instructions { get; set; }

        public virtual ICollection<Product> Product { get; set; }
        public virtual ICollection<ProductModelIllustration> ProductModelIllustration { get; set; }
        public virtual ICollection<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCulture { get; set; }
    }
}
