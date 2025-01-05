// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class ProductPhoto
{
    public ProductPhoto()
    {
        ProductProductPhoto = new HashSet<ProductProductPhoto>();
    }

    public int ProductPhotoID { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
    public byte[] LargePhoto { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
    public string LargePhotoFileName { get; set; }
    public DateTime ModifiedDate { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
    public byte[] ThumbNailPhoto { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
    public string ThumbnailPhotoFileName { get; set; }

    public virtual ICollection<ProductProductPhoto> ProductProductPhoto { get; set; }
}
