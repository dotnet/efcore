// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class Customer
{
    public Customer()
    {
        SalesOrderHeader = new HashSet<SalesOrderHeader>();
    }

    public int CustomerID { get; set; }
    public string AccountNumber { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int? PersonID { get; set; }
#pragma warning disable IDE1006 // Naming Styles
    public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    public int? StoreID { get; set; }
    public int? TerritoryID { get; set; }

    public virtual ICollection<SalesOrderHeader> SalesOrderHeader { get; set; }
    public virtual Person Person { get; set; }
    public virtual Store Store { get; set; }
    public virtual SalesTerritory Territory { get; set; }
}
