// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class ProductInventory
{
    public int ProductID { get; set; }
    public short LocationID { get; set; }
    public byte Bin { get; set; }
    public DateTime ModifiedDate { get; set; }
    public short Quantity { get; set; }
#pragma warning disable IDE1006 // Naming Styles
    public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    public string Shelf { get; set; }

    public virtual Location Location { get; set; }
    public virtual Product Product { get; set; }
}
