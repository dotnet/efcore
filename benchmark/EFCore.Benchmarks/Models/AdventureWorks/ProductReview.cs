// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class ProductReview
{
    public int ProductReviewID { get; set; }
    public string Comments { get; set; }
    public string EmailAddress { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int ProductID { get; set; }
    public int Rating { get; set; }
    public DateTime ReviewDate { get; set; }
    public string ReviewerName { get; set; }

    public virtual Product Product { get; set; }
}
