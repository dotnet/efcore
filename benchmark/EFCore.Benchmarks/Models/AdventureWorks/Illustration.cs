// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class Illustration
{
    public Illustration()
    {
        ProductModelIllustration = new HashSet<ProductModelIllustration>();
    }

    public int IllustrationID { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Diagram { get; set; }

    public virtual ICollection<ProductModelIllustration> ProductModelIllustration { get; set; }
}
