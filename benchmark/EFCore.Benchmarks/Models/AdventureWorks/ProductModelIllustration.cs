// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class ProductModelIllustration
{
    public int ProductModelID { get; set; }
    public int IllustrationID { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual Illustration Illustration { get; set; }
    public virtual ProductModel ProductModel { get; set; }
}
