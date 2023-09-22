// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class SalesPerson
{
    public SalesPerson()
    {
        SalesOrderHeader = new HashSet<SalesOrderHeader>();
        SalesPersonQuotaHistory = new HashSet<SalesPersonQuotaHistory>();
        SalesTerritoryHistory = new HashSet<SalesTerritoryHistory>();
        Store = new HashSet<Store>();
    }

    public int BusinessEntityID { get; set; }
    public decimal Bonus { get; set; }
    public decimal CommissionPct { get; set; }
    public DateTime ModifiedDate { get; set; }
#pragma warning disable IDE1006 // Naming Styles
    public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    public decimal SalesLastYear { get; set; }
    public decimal? SalesQuota { get; set; }
    public decimal SalesYTD { get; set; }
    public int? TerritoryID { get; set; }

    public virtual ICollection<SalesOrderHeader> SalesOrderHeader { get; set; }
    public virtual ICollection<SalesPersonQuotaHistory> SalesPersonQuotaHistory { get; set; }
    public virtual ICollection<SalesTerritoryHistory> SalesTerritoryHistory { get; set; }
    public virtual ICollection<Store> Store { get; set; }
    public virtual Employee BusinessEntity { get; set; }
    public virtual SalesTerritory Territory { get; set; }
}
