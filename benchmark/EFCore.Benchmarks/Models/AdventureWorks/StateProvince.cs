// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class StateProvince
{
    public StateProvince()
    {
        Address = new HashSet<Address>();
        SalesTaxRate = new HashSet<SalesTaxRate>();
    }

    public int StateProvinceID { get; set; }
    public string CountryRegionCode { get; set; }
    public bool IsOnlyStateProvinceFlag { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Name { get; set; }
#pragma warning disable IDE1006 // Naming Styles
    public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    public string StateProvinceCode { get; set; }
    public int TerritoryID { get; set; }

    public virtual ICollection<Address> Address { get; set; }
    public virtual ICollection<SalesTaxRate> SalesTaxRate { get; set; }
    public virtual CountryRegion CountryRegionCodeNavigation { get; set; }
    public virtual SalesTerritory Territory { get; set; }
}
