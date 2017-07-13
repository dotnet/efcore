// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
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
}
