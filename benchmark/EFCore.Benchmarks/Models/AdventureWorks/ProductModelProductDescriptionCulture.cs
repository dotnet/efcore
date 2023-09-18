// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class ProductModelProductDescriptionCulture
{
    public int ProductModelID { get; set; }
    public int ProductDescriptionID { get; set; }
    public string CultureID { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual Culture Culture { get; set; }
    public virtual ProductDescription ProductDescription { get; set; }
    public virtual ProductModel ProductModel { get; set; }
}
