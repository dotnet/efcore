// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class ProductDescription
{
    public ProductDescription()
    {
        ProductModelProductDescriptionCulture = new HashSet<ProductModelProductDescriptionCulture>();
    }

    public int ProductDescriptionID { get; set; }
    public string Description { get; set; }
    public DateTime ModifiedDate { get; set; }
#pragma warning disable IDE1006 // Naming Styles
    public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    public virtual ICollection<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCulture { get; set; }
}
