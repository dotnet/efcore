// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class ProductProductPhoto
    {
        public int ProductID { get; set; }
        public int ProductPhotoID { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool Primary { get; set; }

        public virtual Product Product { get; set; }
        public virtual ProductPhoto ProductPhoto { get; set; }
    }
}
