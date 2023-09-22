// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class Location
{
    public Location()
    {
        ProductInventory = new HashSet<ProductInventory>();
        WorkOrderRouting = new HashSet<WorkOrderRouting>();
    }

    public short LocationID { get; set; }
    public decimal Availability { get; set; }
    public decimal CostRate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Name { get; set; }

    public virtual ICollection<ProductInventory> ProductInventory { get; set; }
    public virtual ICollection<WorkOrderRouting> WorkOrderRouting { get; set; }
}
